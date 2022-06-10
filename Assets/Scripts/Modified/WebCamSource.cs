using Mediapipe.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODO: Update this class to make this more unique to this project
namespace HardCoded.VRigUnity {
	public class WebCamSource : ImageSource {
		[Tooltip("For the default resolution, the one whose width is closest to this value will be chosen")]
		[SerializeField] private int _preferableDefaultWidth = 1280;

		private const string _TAG = nameof(WebCamSource);

		[SerializeField]
		private ResolutionStruct[] _defaultAvailableResolutions = new ResolutionStruct[] {
			new ResolutionStruct(1920, 1080, 30),
			new ResolutionStruct(1600, 896, 30),
			new ResolutionStruct(1280, 720, 30),
			new ResolutionStruct(960, 540, 30),
			new ResolutionStruct(848, 480, 30),
			new ResolutionStruct(640, 480, 30),
			new ResolutionStruct(640, 360, 30),
			new ResolutionStruct(424, 240, 30),
			new ResolutionStruct(320, 240, 30),
			new ResolutionStruct(176, 144, 30),
		};

		private static readonly object _PermissionLock = new object();
		private static bool _IsPermitted = false;

		private WebCamTexture _webCamTexture;
		private WebCamTexture webCamTexture {
			get => _webCamTexture;
			set {
				if (_webCamTexture != null) {
					_webCamTexture.Stop();
				}
				_webCamTexture = value;
			}
		}

		public override int textureWidth => !isPrepared ? 0 : webCamTexture.width;
		public override int textureHeight => !isPrepared ? 0 : webCamTexture.height;

		public override bool isVerticallyFlipped => isPrepared && webCamTexture.videoVerticallyMirrored;
		public override bool isFrontFacing => isPrepared && (webCamDevice is WebCamDevice valueOfWebCamDevice) && valueOfWebCamDevice.isFrontFacing;
		public override RotationAngle rotation => !isPrepared ? RotationAngle.Rotation0 : (RotationAngle)webCamTexture.videoRotationAngle;

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
				resolution = GetDefaultResolution();
			}
		}
		public override string sourceName => (webCamDevice is WebCamDevice valueOfWebCamDevice) ? valueOfWebCamDevice.name : null;

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

		public override string[] sourceCandidateNames => availableSources?.Select(device => device.name).ToArray();

		public override ResolutionStruct[] availableResolutions => webCamDevice == null ? null : _defaultAvailableResolutions;
		public override bool isPrepared => webCamTexture != null;
		public override bool isPlaying => webCamTexture != null && webCamTexture.isPlaying;
		private bool _isInitialized;

		private IEnumerator Start() {
			yield return UpdateSources();
		}

		public IEnumerator UpdateSources() {
			yield return GetPermission();

			if (!_IsPermitted) {
				_isInitialized = true;
				yield break;
			}

			availableSources = WebCamTexture.devices;

			if (availableSources != null && availableSources.Length > 0) {
				webCamDevice = availableSources[0];
			}

			_isInitialized = true;
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

		public override void SelectSource(int sourceId) {
			if (sourceId < 0 || sourceId >= availableSources.Length) {
				throw new ArgumentException($"Invalid source ID: {sourceId}");
			}

			webCamDevice = availableSources[sourceId];
		}

		public override IEnumerator Play() {
			yield return new WaitUntil(() => _isInitialized);
			if (!_IsPermitted) {
				throw new InvalidOperationException("Not permitted to access cameras");
			}

			InitializeWebCamTexture();
			webCamTexture.Play();
			yield return WaitForWebCamTexture();
		}

		public override IEnumerator Resume() {
			if (!isPrepared) {
				throw new InvalidOperationException("WebCamTexture is not prepared yet");
			}

			if (!webCamTexture.isPlaying) {
				webCamTexture.Play();
			}

			yield return WaitForWebCamTexture();
		}

		public override void Pause() {
			if (isPlaying) {
				webCamTexture.Pause();
			}
		}

		public override void Stop() {
			if (webCamTexture != null) {
				webCamTexture.Stop();
			}

			webCamTexture = null;
		}

		public override Texture GetCurrentTexture() {
			return webCamTexture;
		}

		private ResolutionStruct GetDefaultResolution() {
			var resolutions = availableResolutions;
			return resolutions == null || resolutions.Length == 0 ? new ResolutionStruct() : resolutions.OrderBy(resolution => resolution, new ResolutionStructComparer(_preferableDefaultWidth)).First();
		}

		private void InitializeWebCamTexture() {
			Stop();
			if (webCamDevice is WebCamDevice valueOfWebCamDevice) {
				webCamTexture = new WebCamTexture(valueOfWebCamDevice.name, resolution.width, resolution.height, (int)resolution.frameRate);
				return;
			}

			throw new InvalidOperationException("Cannot initialize WebCamTexture because WebCamDevice is not selected");
		}

		private IEnumerator WaitForWebCamTexture() {
			const int timeoutFrame = 500;
			var count = 0;
			Logger.Verbose("Waiting for WebCamTexture to start");
			yield return new WaitUntil(() => count++ > timeoutFrame || webCamTexture.width > 16);

			if (webCamTexture.width <= 16) {
				throw new TimeoutException("Failed to start WebCam");
			}
		}

		private class ResolutionStructComparer : IComparer<ResolutionStruct> {
			private readonly int _preferableDefaultWidth;

			public ResolutionStructComparer(int preferableDefaultWidth) {
				_preferableDefaultWidth = preferableDefaultWidth;
			}

			public int Compare(ResolutionStruct a, ResolutionStruct b) {
				var aDiff = Mathf.Abs(a.width - _preferableDefaultWidth);
				var bDiff = Mathf.Abs(b.width - _preferableDefaultWidth);
				if (aDiff != bDiff) {
					return aDiff - bDiff;
				}

				if (a.height != b.height) {
					// prefer smaller height
					return a.height - b.height;
				}

				// prefer smaller frame rate
				return (int)(a.frameRate - b.frameRate);
			}
		}
	}
}
