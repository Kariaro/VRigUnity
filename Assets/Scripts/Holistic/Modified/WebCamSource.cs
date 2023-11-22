using Mediapipe.Unity;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class WebCamSource : ImageSource {
		private const string _TAG = nameof(WebCamSource);
		private static readonly object _PermissionLock = new();
		private static bool _IsPermitted = false;

		/// <summary>
		/// Default resolutions
		/// </summary>
		public readonly ResolutionStruct[] AvailableResolutions = new ResolutionStruct[] {
			new(1920, 1080, 30),
			new(1600, 896, 30),
			new(1280, 720, 30),
			new(960, 540, 30),
			new(848, 480, 30),
			new(640, 480, 30),
			new(640, 360, 30),
			new(424, 240, 30),
			new(320, 180, 30), // 320x240 -> 16:19 better
			new(176, 144, 30),
		};
		public ResolutionStruct DefaultResolution => AvailableResolutions[6];

		public override Texture CurrentTexture => webCamTexture;
		public override int TextureWidth => IsPrepared ? webCamTexture.width : 0;
		public override int TextureHeight => IsPrepared ? webCamTexture.height : 0;
		public override bool IsPrepared => webCamTexture != null;
		public override bool IsPlaying => webCamTexture != null && webCamTexture.isPlaying;
		public override bool IsVerticallyFlipped { get => IsPrepared && webCamTexture.videoVerticallyMirrored; set {} }
		public override RotationAngle Rotation => IsPrepared ? (RotationAngle) webCamTexture.videoRotationAngle : RotationAngle.Rotation0;
		public override string SourceName => (webCamDevice is WebCamDevice valueOfWebCamDevice) ? valueOfWebCamDevice.name : null;
		public override string[] SourceCandidateNames => availableSources?.Select(device => device.name).ToArray();

		private WebCamDevice? _webCamDevice;
		private WebCamDevice? webCamDevice {
			get => _webCamDevice;
			set {
				if (_webCamDevice is WebCamDevice valueOfWebCamDevice) {
					if (value is WebCamDevice valueOfValue && valueOfValue.name == valueOfWebCamDevice.name) {
						// not changed
						return;
					}
				} else if (value == null) {
					// not changed
					return;
				}
				_webCamDevice = value;
				Resolution = DefaultResolution;
			}
		}

		private WebCamDevice[] _availableSources;
		private WebCamDevice[] availableSources {
			get {
				if (_availableSources == null) {
					_availableSources = WebCamTexture.devices;
				}

				return _availableSources;
			}
			set => _availableSources = value;
		}

		private WebCamTexture webCamTexture;
		private bool isInitialized;

		protected virtual IEnumerator Start() {
			yield return UpdateSources();
		}

		public override ResolutionStruct[] GetResolutions() {
			return AvailableResolutions;
		}

		public IEnumerator UpdateSources() {
			yield return GetPermission();

			if (!_IsPermitted) {
				isInitialized = true;
				yield break;
			}

			availableSources = WebCamTexture.devices;
			if (availableSources != null && availableSources.Length > 0) {
				webCamDevice = availableSources[0];
			}

			isInitialized = true;
		}

		private IEnumerator GetPermission() {
			lock (_PermissionLock) {
				if (_IsPermitted) {
					yield break;
				}

				_IsPermitted = true;

				yield return new WaitForEndOfFrame();
			}
		}

		public virtual void SelectSource(int sourceId) {
			if (sourceId < 0 || sourceId >= availableSources.Length) {
				throw new ArgumentException($"Invalid source ID: {sourceId}");
			}

			webCamDevice = availableSources[sourceId];
		}

		public override IEnumerator Play() {
			yield return new WaitUntil(() => isInitialized);
			if (!_IsPermitted) {
				throw new InvalidOperationException("Not permitted to access cameras");
			}

			InitializeWebCamTexture();
			webCamTexture.Play();
			yield return WaitForWebCamTexture();
		}

		public override void UpdateFromSettings() {
			SelectSourceFromName(Settings.CameraName);
			SelectResolutionFromString(Settings.CameraResolution);
			IsHorizontallyFlipped = Settings.CameraFlipped;
		}

		public override void Stop() {
			if (webCamTexture != null) {
				webCamTexture.Stop();
			}
			webCamTexture = null;
		}

		private void InitializeWebCamTexture() {
			Stop();
			if (webCamDevice is WebCamDevice valueOfWebCamDevice) {
				webCamTexture = new WebCamTexture(valueOfWebCamDevice.name, Resolution.width, Resolution.height, (int)Resolution.frameRate);
				return;
			}

			throw new InvalidOperationException("Cannot initialize WebCamTexture because WebCamDevice is not selected");
		}

		private IEnumerator WaitForWebCamTexture() {
			const int timeoutFrame = 30;
			var count = 0;
			Logger.Verbose("Waiting for WebCamTexture to start");
			yield return new WaitUntil(() => count++ > timeoutFrame || webCamTexture.width > 16);

			if (webCamTexture.width <= 16) {
				throw new TimeoutException("Failed to start WebCam");
			}
		}

		// Custom
		public int SelectSourceFromName(string name) {
			int index = SourceCandidateNames.ToList().FindIndex(source => source == name);
			if (index >= 0) {
				SelectSource(index);
			}

			return index;
		}

		public int SelectResolutionFromString(string text, bool allowCustom = true) {
			int index = AvailableResolutions.ToList().FindIndex(option => option.ToString() == text);
			if (index >= 0) {
				SelectResolution(index);
			} else if (allowCustom) {
				Resolution = SettingsUtil.GetResolution(text);
			}

			return index;
		}

		public void SelectResolution(int resolutionId) {
			var resolutions = AvailableResolutions;
			if (resolutionId < 0 || resolutionId >= resolutions.Length) {
				throw new ArgumentException($"Invalid resolution ID: {resolutionId}");
			}

			Resolution = resolutions[resolutionId];
		}
	}
}
