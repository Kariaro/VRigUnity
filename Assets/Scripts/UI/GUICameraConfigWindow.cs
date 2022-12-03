using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class GUICameraConfigWindow : GUIWindow {
		private const string SourcePath                = "Contents/Source/Dropdown";
		private const string ResolutionPath            = "Contents/Resolution/Dropdown";
		private const string IsHorizontallyFlippedPath = "Contents/IsHorizontallyFlipped/Toggle";
		private const string VirtualCameraToggle       = "Contents/Virtual/Panel/Toggle";
		private const string VirtualCameraInstall      = "Contents/Virtual/Panel/Install";
		private const string VirtualCameraUninstall    = "Contents/Virtual/Panel/Uninstall";
		
		[SerializeField] private GUIScript settings;
		private Solution _solution;
		private TMP_Dropdown _sourceInput;
		private TMP_Dropdown _resolutionInput;
		private Toggle _isHorizontallyFlippedInput;
		private Toggle _virtualCameraToggle;
		private Button _virtualCameraInstall;
		private Button _virtualCameraUninstall;

		void Start() {
			_solution = SolutionUtils.GetSolution();
			InitializeContents();
		}

		private void InitializeContents() {
			ReloadContents();
		}

		public void ReloadContents() {
			StartCoroutine(UpdateContents());
		}

		private IEnumerator UpdateContents() {
			WebCamSource webCamSource = SolutionUtils.GetImageSource();
			yield return webCamSource.UpdateSources();

			// Updating the webcam source might break the current config
			var sourceId = webCamSource.sourceCandidateNames.ToList().FindIndex(source => source == Settings.CameraName);
			if (sourceId >= 0 && sourceId < webCamSource.sourceCandidateNames.Length) {
				webCamSource.SelectSource(sourceId);
			}
			var resolutionId = webCamSource.availableResolutions.ToList().FindIndex(option => option.ToString() == Settings.CameraResolution);
			if (resolutionId >= 0 && resolutionId < webCamSource.availableResolutions.Length) {
				webCamSource.SelectResolution(resolutionId);
			}
			webCamSource.isHorizontallyFlipped = Settings.CameraFlipped;
			
			// Initialize UI
			InitializeSource();
			InitializeResolution();
			InitializeIsHorizontallyFlipped();
			InitializeVirtualCamera();

			yield return null;
		}

		private void InitializeSource() {
			_sourceInput = transform.Find(SourcePath).GetComponent<TMP_Dropdown>();
			_sourceInput.ClearOptions();
			_sourceInput.onValueChanged.RemoveAllListeners();

			var imageSource = SolutionUtils.GetImageSource();
			var sourceNames = imageSource.sourceCandidateNames;
			
			if (sourceNames == null) {
				_sourceInput.enabled = false;
				return;
			}

			var options = new List<string>(sourceNames);
			_sourceInput.AddOptions(options);

			var currentSourceName = Settings.CameraName;
			var defaultValue = options.FindIndex(option => option == currentSourceName);

			if (defaultValue >= 0) {
				_sourceInput.value = defaultValue;
			}

			_sourceInput.onValueChanged.AddListener(delegate {
				imageSource.SelectSource(_sourceInput.value);
				Settings.CameraName = options[_sourceInput.value];
				if (!_solution.IsPaused()) {
					_solution.Play();
				}

				InitializeResolution();
			});
		}

		private void InitializeResolution() {
			_resolutionInput = transform.Find(ResolutionPath).GetComponent<TMP_Dropdown>();
			_resolutionInput.ClearOptions();
			_resolutionInput.onValueChanged.RemoveAllListeners();

			var imageSource = SolutionUtils.GetImageSource();
			var resolutions = imageSource.availableResolutions;

			if (resolutions == null) {
				_resolutionInput.enabled = false;
				return;
			}

			var options = resolutions.Select(resolution => resolution.ToString()).ToList();
			_resolutionInput.AddOptions(options);

			var currentResolutionStr = Settings.CameraResolution;
			var defaultValue = options.FindIndex(option => option == currentResolutionStr);

			if (defaultValue >= 0) {
				_resolutionInput.value = defaultValue;
			}

			_resolutionInput.onValueChanged.AddListener(delegate {
				imageSource.SelectResolution(_resolutionInput.value);
				Settings.CameraResolution = options[_resolutionInput.value];
				settings.UpdateShowCamera();
				if (!_solution.IsPaused()) {
					_solution.Play();
				}
			});
		}

		private void InitializeIsHorizontallyFlipped() {
			_isHorizontallyFlippedInput = transform.Find(IsHorizontallyFlippedPath).GetComponent<Toggle>();

			var imageSource = SolutionUtils.GetImageSource();
			_isHorizontallyFlippedInput.isOn = Settings.CameraFlipped;
			_isHorizontallyFlippedInput.onValueChanged.AddListener(delegate {
				imageSource.isHorizontallyFlipped = _isHorizontallyFlippedInput.isOn;
				Settings.CameraFlipped = _isHorizontallyFlippedInput.isOn;
				settings.UpdateShowCamera();
				if (!_solution.IsPaused()) {
					_solution.Play();
				}
			});
		}

		private void InitializeVirtualCamera() {
			_virtualCameraToggle = transform.Find(VirtualCameraToggle).GetComponent<Toggle>();
			_virtualCameraToggle.isOn = Settings.VirtualCamera;
			_virtualCameraToggle.onValueChanged.AddListener(delegate {
				Settings.VirtualCamera = _virtualCameraToggle.isOn;
			});

			_virtualCameraInstall = transform.Find(VirtualCameraInstall).GetComponent<Button>();
			_virtualCameraUninstall = transform.Find(VirtualCameraUninstall).GetComponent<Button>();
			_virtualCameraInstall.enabled = CameraCapture.IsVirtualCameraSupported;
			_virtualCameraInstall.onClick.RemoveAllListeners();
			_virtualCameraInstall.onClick.AddListener(delegate {
				CameraCapture.InstallVirtualCamera();
			});
			
			_virtualCameraUninstall.enabled = CameraCapture.IsVirtualCameraSupported;
			_virtualCameraUninstall.onClick.RemoveAllListeners();
			_virtualCameraUninstall.onClick.AddListener(delegate {
				CameraCapture.UninstallVirtualCamera();
			});
		}
	}
}
