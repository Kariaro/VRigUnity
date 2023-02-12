using Mediapipe.Unity;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace HardCoded.VRigUnity {
	// We only have one type of image source
	// Make this class webcam specific
	public abstract class ImageSource : MonoBehaviour {
		// The current resolution of this image source
		public ResolutionStruct Resolution { get; protected set; }
		public abstract string SourceName { get; }

		// Required texture dimensions
		public virtual int TextureWidth => Resolution.width;
		public virtual int TextureHeight => Resolution.height;

		// Image source information
		public virtual bool IsHorizontallyFlipped { get; set; } = false;
		public virtual bool IsVerticallyFlipped { get; } = false;
		public virtual bool IsFrontFacing { get; } = false;
		public virtual RotationAngle Rotation { get; } = RotationAngle.Rotation0;

		/// <remarks>
		///   Once <see cref="Play" /> finishes successfully, it will become true.
		/// </remarks>
		/// <returns>
		///   Returns if the image source is prepared.
		/// </returns>
		public abstract bool IsPrepared { get; }
		public abstract bool IsPlaying { get; }

		/// <remarks>
		///   If <see cref="type" /> does not support frame rate, it returns zero.
		/// </remarks>
		public virtual double FrameRate => Resolution.frameRate;

		/// <remarks>
		///   To call this method, the image source must be prepared.
		/// </remarks>
		/// <returns>
		///   <see cref="UnityEngine.TextureFormat" /> that is compatible with the current texture.
		/// </returns>
		public TextureFormat TextureFormat => IsPrepared ? TextureFormatFor(GetCurrentTexture()) : throw new InvalidOperationException("ImageSource is not prepared");
		
		public float FocalLengthPx { get; } = 2.0f; // TODO: calculate at runtime

		public abstract string[] SourceCandidateNames { get; }
		public abstract ResolutionStruct[] AvailableResolutions { get; }

		/// <summary>
		///   Choose the source from <see cref="SourceCandidateNames" />.
		/// </summary>
		/// <remarks>
		///   You need to call <see cref="Play" /> for the change to take effect.
		/// </remarks>
		/// <param name="sourceId">The index of <see cref="SourceCandidateNames" /></param>
		public abstract void SelectSource(int sourceId);

		/// <summary>
		///   Choose the resolution from <see cref="AvailableResolutions" />.
		/// </summary>
		/// <remarks>
		///   You need to call <see cref="Play" /> for the change to take effect.
		/// </remarks>
		/// <param name="resolutionId">The index of <see cref="AvailableResolutions" /></param>
		public void SelectResolution(int resolutionId) {
			var resolutions = AvailableResolutions;

			if (resolutionId < 0 || resolutionId >= resolutions.Length) {
				throw new ArgumentException($"Invalid resolution ID: {resolutionId}");
			}

			Resolution = resolutions[resolutionId];
		}

		/// <summary>
		///   Prepare image source.
		///   If <see cref="IsPlaying" /> is true, it will reset the image source.
		/// </summary>
		/// <remarks>
		///   When it finishes successfully, <see cref="IsPrepared" /> will return true.
		/// </remarks>
		/// <exception cref="InvalidOperation" />
		public abstract IEnumerator Play();

		/// <summary>
		///   Stop image source.
		/// </summary>
		/// <remarks>
		///   When it finishes successfully, <see cref="IsPrepared" /> will return false.
		/// </remarks>
		public abstract void Stop();

		/// <remarks>
		///   To call this method, the image source must be prepared.
		/// </remarks>
		/// <returns>
		///   <see cref="Texture" /> that contains the current image.
		/// </returns>
		public abstract Texture GetCurrentTexture();

		protected static TextureFormat TextureFormatFor(Texture texture) {
			return GraphicsFormatUtility.GetTextureFormat(texture.graphicsFormat);
		}
	}
}
