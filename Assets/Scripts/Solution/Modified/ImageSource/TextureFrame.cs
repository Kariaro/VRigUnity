// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Mediapipe.Unity {
	public class TextureFrame {
		public class ReleaseEvent : UnityEvent<TextureFrame> { }

		private const string _TAG = nameof(TextureFrame);

		private static readonly GlobalInstanceTable<Guid, TextureFrame> _InstanceTable = new(100);
		/// <summary>
		///   A dictionary to look up which native texture belongs to which <see cref="TextureFrame" />.
		/// </summary>
		/// <remarks>
		///   Not all the <see cref="TextureFrame" /> instances are registered.
		///   Texture names are queried only when necessary, and the corresponding data will be saved then.
		/// </remarks>
		private static readonly Dictionary<uint, Guid> _NameTable = new();

		private readonly Texture2D _texture;
		private IntPtr _nativeTexturePtr = IntPtr.Zero;
		private GlSyncPoint _glSyncToken;

		private Color32[] _pixelsBuffer; // for WebCamTexture
		private Color32[] pixelsBuffer {
			get {
				if (_pixelsBuffer == null) {
					_pixelsBuffer = new Color32[width * height];
				}
				return _pixelsBuffer;
			}
		}

		private readonly Guid _instanceId;
		// NOTE: width and height can be accessed from a thread other than Main Thread.
		public readonly int width;
		public readonly int height;
		public readonly TextureFormat format;

		private ImageFormat.Types.Format _format = ImageFormat.Types.Format.Unknown;
		public ImageFormat.Types.Format imageFormat {
			get {
				if (_format == ImageFormat.Types.Format.Unknown) {
					_format = format.ToImageFormat();
				}
				return _format;
			}
		}

		public GpuBufferFormat gpuBufferformat => GpuBufferFormat.kBGRA32;
		
		/// <summary>
		///   The event that will be invoked when the TextureFrame is released.
		/// </summary>
		public readonly ReleaseEvent OnRelease;

		private TextureFrame(Texture2D texture) {
			_texture = texture;
			width = texture.width;
			height = texture.height;
			format = texture.format;
			OnRelease = new ReleaseEvent();
			_instanceId = Guid.NewGuid();
			_InstanceTable.Add(_instanceId, this);
		}

		public TextureFrame(int width, int height, TextureFormat format) : this(new Texture2D(width, height, format, false)) { }
		public TextureFrame(int width, int height) : this(width, height, TextureFormat.RGBA32) { }

		public void CopyTexture(Texture dst) {
			Graphics.CopyTexture(_texture, dst);
		}

		/// <summary>
		///   Copy texture data from <paramref name="src" />.
		/// </summary>
		/// <param name="src">
		///   The texture from which the pixels are read.
		///   Its width and height must match that of the TextureFrame.
		/// </param>
		/// <remarks>
		///   This operation is slow.
		///   If CPU won't access the pixel data, use <see cref="ReadTextureFromOnGPU" /> instead.
		/// </remarks>
		public void ReadTextureFromOnCPU(WebCamTexture src) {
			SetPixels32(src.GetPixels32(pixelsBuffer));
		}

		public void SetPixels32(Color32[] pixels) {
			_texture.SetPixels32(pixels);
			_texture.Apply();

			if (!RevokeNativeTexturePtr()) {
				// If this line was executed, there must be a bug.
				Logger.LogError("Failed to revoke the native texture.");
			}
		}

		public NativeArray<byte> GetRawTextureByteData() {
			return _texture.GetRawTextureData<byte>();
		}

		/// <returns>The texture's native pointer</returns>
		public IntPtr GetNativeTexturePtr() {
			if (_nativeTexturePtr == IntPtr.Zero) {
				_nativeTexturePtr = _texture.GetNativeTexturePtr();
				var name = (uint)_nativeTexturePtr;

				lock (((ICollection)_NameTable).SyncRoot) {
					if (!AcquireName(name, _instanceId)) {
						throw new InvalidProgramException($"Another instance (id={_instanceId}) is using the specified name ({name}) now");
					}
					_NameTable.Add(name, _instanceId);
				}
			}
			return _nativeTexturePtr;
		}

		public uint GetTextureName() {
			return (uint)GetNativeTexturePtr();
		}

		public Guid GetInstanceID() {
			return _instanceId;
		}

		public ImageFrame BuildImageFrame() {
			return new ImageFrame(imageFormat, width, height, 4 * width, GetRawTextureByteData());
		}

		public GpuBuffer BuildGpuBuffer(GlContext glContext) {
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX || UNITY_ANDROID
			var glTextureBuffer = new GlTextureBuffer(GetTextureName(), width, height, gpuBufferformat, OnReleaseTextureFrame, glContext);
			return new GpuBuffer(glTextureBuffer);
#else
			throw new NotSupportedException("This method is only supported on Linux or Android");
#endif
		}

		public void RemoveAllReleaseListeners() {
			OnRelease.RemoveAllListeners();
		}

		// TODO: stop invoking OnRelease when it's already released
		public void Release(GlSyncPoint token = null) {
			if (_glSyncToken != null) {
				_glSyncToken.Dispose();
			}
			_glSyncToken = token;
			OnRelease.Invoke(this);
		}

		/// <summary>
		///   Waits until the GPU has executed all commands up to the sync point.
		///   This blocks the CPU, and ensures the commands are complete from the point of view of all threads and contexts.
		/// </summary>
		public void WaitUntilReleased() {
			if (_glSyncToken == null) {
				return;
			}
			_glSyncToken.Wait();
			_glSyncToken.Dispose();
			_glSyncToken = null;
		}

		[AOT.MonoPInvokeCallback(typeof(GlTextureBuffer.DeletionCallback))]
		public static void OnReleaseTextureFrame(uint textureName, IntPtr syncTokenPtr) {
			var isIdFound = _NameTable.TryGetValue(textureName, out var _instanceId);

			if (!isIdFound) {
				Logger.LogError(_TAG, $"nameof (name={textureName}) is released, but the owner TextureFrame is not found");
				return;
			}

			var isTextureFrameFound = _InstanceTable.TryGetValue(_instanceId, out var textureFrame);

			if (!isTextureFrameFound) {
				Logger.LogWarning(_TAG, $"nameof owner TextureFrame of the released texture (name={textureName}) is already garbage collected");
				return;
			}

			var _glSyncToken = syncTokenPtr == IntPtr.Zero ? null : new GlSyncPoint(syncTokenPtr);
			textureFrame.Release(_glSyncToken);
		}

		/// <summary>
		///   Remove <paramref name="name" /> from <see cref="_NameTable" /> if it's stale.
		///   If <paramref name="name" /> does not exist in <see cref="_NameTable" />, do nothing.
		/// </summary>
		/// <remarks>
		///   If the instance whose id is <paramref name="ownerId" /> owns <paramref name="name" /> now, it still removes <paramref name="name" />.
		/// </remarks>
		/// <returns>Return if name is available</returns>
		private static bool AcquireName(uint name, Guid ownerId) {
			if (_NameTable.TryGetValue(name, out var id)) {
				if (ownerId != id && _InstanceTable.TryGetValue(id, out var _)) {
					// if instance is found, the instance is using the name.
					Logger.LogVerbose($"{id} is using {name} now");
					return false;
				}
				var _ = _NameTable.Remove(name);
			}
			return true;
		}

		/// <summary>
		///   Remove the texture name from <see cref="_NameTable" /> and empty <see cref="_nativeTexturePtr" />.
		///   This method needs to be called when an operation is performed that may change the internal texture.
		/// </summary>
		private bool RevokeNativeTexturePtr() {
			if (_nativeTexturePtr == IntPtr.Zero) {
				return true;
			}

			var currentName = GetTextureName();
			if (!_NameTable.Remove(currentName)) {
				return false;
			}
			_nativeTexturePtr = IntPtr.Zero;
			return true;
		}
	}
}
