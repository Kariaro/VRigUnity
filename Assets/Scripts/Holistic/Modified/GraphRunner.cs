using Mediapipe;
using Mediapipe.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace HardCoded.VRigUnity {
	public abstract class GraphRunner : MonoBehaviour {
		public enum ConfigType {
			None,
			CPU,
			GPU,
			OpenGLES,
		}

		protected string TAG => GetType().Name;

		[SerializeField] private TextAsset _cpuConfig = null;
		[SerializeField] private TextAsset _gpuConfig = null;
		[SerializeField] private TextAsset _openGlEsConfig = null;
		[SerializeField] private long _timeoutMicrosec = 0;

		private static readonly GlobalInstanceTable<int, GraphRunner> _InstanceTable = new(5);
		private static readonly Dictionary<IntPtr, int> _NameTable = new();

		private bool _isRunning = false;

		public InferenceMode inferenceMode => configType == ConfigType.CPU ? InferenceMode.Cpu : InferenceMode.Gpu;
		public virtual ConfigType configType {
			get {
				if (GpuManager.IsInitialized) {
#if UNITY_ANDROID
					if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && _openGlEsConfig != null) {
						return ConfigType.OpenGLES;
					}
#endif
					if (_gpuConfig != null) {
						return ConfigType.GPU;
					}
				}
				return _cpuConfig != null ? ConfigType.CPU : ConfigType.None;
			}
		}

		public TextAsset TextConfig => configType switch {
			ConfigType.CPU => _cpuConfig,
			ConfigType.GPU => _gpuConfig,
			ConfigType.OpenGLES => _openGlEsConfig,
			_ => null,
		};

		public long TimeoutMicrosec {
			get => _timeoutMicrosec;
			set => _timeoutMicrosec = (long)Mathf.Max(0, value);
		}

		public long TimeoutMillisec {
			get => TimeoutMicrosec / 1000;
			set => TimeoutMicrosec = value * 1000;
		}

		public RotationAngle Rotation { get; private set; } = 0;

		private Stopwatch _stopwatch;
		protected CalculatorGraph CalculatorGraph { get; private set; }
		protected Timestamp latestTimestamp;

		protected virtual void Start() {
			_InstanceTable.Add(GetInstanceID(), this);
		}

		protected virtual void OnDestroy() {
			Stop();
		}

		public WaitForResult WaitForInitAsync() {
			return new WaitForResult(this, InitializeAsync());
		}

		public virtual IEnumerator InitializeAsync() {
			Logger.Info(TAG, $"Config Type = {configType}");

			InitializeCalculatorGraph().AssertOk();
			_stopwatch = new Stopwatch();
			_stopwatch.Start();

			Logger.Info(TAG, "Loading dependent assets...");
			var assetRequests = RequestDependentAssets();
			yield return new WaitWhile(() => assetRequests.Any((request) => request.keepWaiting));

			var errors = assetRequests.Where((request) => request.isError).Select((request) => request.error).ToList();
			if (errors.Count > 0) {
				foreach (var error in errors) {
					Logger.Error(TAG, error);
				}
				throw new InternalException("Failed to prepare dependent assets");
			}
		}

		public abstract void StartRun(ImageSource imageSource);

		protected void StartRun(SidePacket sidePacket) {
			CalculatorGraph.StartRun(sidePacket).AssertOk();
			_isRunning = true;
		}

		public virtual void Stop() {
			if (CalculatorGraph != null) {
				if (_isRunning) {
					using (var status = CalculatorGraph.CloseAllPacketSources()) {
						if (!status.Ok()) {
							Logger.Error(TAG, status.ToString());
						}
					}

					using (var status = CalculatorGraph.WaitUntilDone()) {
						if (!status.Ok()) {
							Logger.Error(TAG, status.ToString());
						}
					}
				}

				_isRunning = false;
				var _ = _NameTable.Remove(CalculatorGraph.mpPtr);
				CalculatorGraph.Dispose();
				CalculatorGraph = null;
			}

			if (_stopwatch != null && _stopwatch.IsRunning) {
				_stopwatch.Stop();
			}
		}

		protected void AddPacketToInputStream<T>(string streamName, Packet<T> packet) {
			CalculatorGraph.AddPacketToInputStream(streamName, packet).AssertOk();
		}

		protected void AddTextureFrameToInputStream(string streamName, Texture2D texture) {
			latestTimestamp = GetCurrentTimestamp();

			/*
			if (configType == ConfigType.OpenGLES) {
				var gpuBuffer = textureFrame.BuildGpuBuffer(GpuManager.GlCalculatorHelper.GetGlContext());
				AddPacketToInputStream(streamName, new GpuBufferPacket(gpuBuffer, latestTimestamp));
				return;
			}
			*/

			var width = texture.width;
			var height = texture.height;
			var format = texture.format;
			var imageFrame = new ImageFrame(format.ToImageFormat(), width, height, 4 * width, texture.GetRawTextureData<byte>());
			AddPacketToInputStream(streamName, new ImageFramePacket(imageFrame, latestTimestamp));
		}

		protected long GetCurrentTimestampMicrosec() {
			return _stopwatch == null || !_stopwatch.IsRunning ? -1 : _stopwatch.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000);
		}

		protected Timestamp GetCurrentTimestamp() {
			var microsec = GetCurrentTimestampMicrosec();
			return microsec < 0 ? Timestamp.Unset() : new Timestamp(microsec);
		}

		protected Status InitializeCalculatorGraph() {
			CalculatorGraph = new CalculatorGraph();
			_NameTable.Add(CalculatorGraph.mpPtr, GetInstanceID());

			// NOTE: There's a simpler way to initialize CalculatorGraph.
			//
			//		 calculatorGraph = new CalculatorGraph(config.text);
			//
			//	 However, if the config format is invalid, this code does not initialize CalculatorGraph and does not throw exceptions either.
			//	 The problem is that if you call ObserveStreamOutput in this state, the program will crash.
			//	 The following code is not very efficient, but it will return Non-OK status when an invalid configuration is given.
			try {
				var baseConfig = TextConfig == null ? null : CalculatorGraphConfig.Parser.ParseFromTextFormat(TextConfig.text);
				if (baseConfig == null) {
					throw new InvalidOperationException("Failed to get the text config. Check if the config is set to GraphRunner");
				}
				var status = ConfigureCalculatorGraph(baseConfig);
				return !status.Ok() || inferenceMode == InferenceMode.Cpu ? status : CalculatorGraph.SetGpuResources(GpuManager.GpuResources);
			} catch (Exception e) {
				return Status.FailedPrecondition(e.ToString());
			}
		}

		protected abstract Status ConfigureCalculatorGraph(CalculatorGraphConfig config);

		protected void SetImageTransformationOptions(SidePacket sidePacket, ImageSource imageSource, bool expectedToBeMirrored = false) {
			// NOTE: The origin is left-bottom corner in Unity, and right-top corner in MediaPipe.

			// TODO: Check if this code can be removed?
			Rotation = imageSource.Rotation.Reverse();
			var inputRotation = Rotation;
			var isInverted = Mediapipe.Unity.CoordinateSystem.ImageCoordinate.IsInverted(Rotation);
			var shouldBeMirrored = imageSource.IsHorizontallyFlipped ^ expectedToBeMirrored ^ true;
			var inputHorizontallyFlipped = isInverted ^ shouldBeMirrored;
			var inputVerticallyFlipped = !isInverted;

			if ((inputHorizontallyFlipped && inputVerticallyFlipped) || Rotation == RotationAngle.Rotation180) {
				inputRotation = inputRotation.Add(RotationAngle.Rotation180);
				inputHorizontallyFlipped = !inputHorizontallyFlipped;
				inputVerticallyFlipped = !inputVerticallyFlipped;
			}

			Logger.Debug($"input_rotation = {inputRotation}, input_horizontally_flipped = {inputHorizontallyFlipped}, input_vertically_flipped = {inputVerticallyFlipped}");

			sidePacket.Emplace("input_rotation", new IntPacket((int)inputRotation));
			sidePacket.Emplace("input_horizontally_flipped", new BoolPacket(inputHorizontallyFlipped));
			sidePacket.Emplace("input_vertically_flipped", new BoolPacket(inputVerticallyFlipped));
		}

		protected WaitForResult WaitForAsset(string assetName) {
			bool overwrite = false;
			return new WaitForResult(this, SolutionUtils.GetAssetManager().PrepareAssetAsync(assetName, assetName, overwrite));
		}

		protected abstract IList<WaitForResult> RequestDependentAssets();
	}
}
