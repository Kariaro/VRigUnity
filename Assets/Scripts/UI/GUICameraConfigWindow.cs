using Mediapipe.Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class GUICameraConfigWindow : MonoBehaviour {
		private const string SourcePath                = "Contents/Source/Dropdown";
		private const string ResolutionPath            = "Contents/Resolution/Dropdown";
		private const string IsHorizontallyFlippedPath = "Contents/IsHorizontallyFlipped/Toggle";
		private const string VirtualCameraInstall      = "Contents/Virtual/Panel/Install";
		private const string VirtualCameraUninstall    = "Contents/Virtual/Panel/Uninstall";

		private Solution _solution;
		private TMP_Dropdown _sourceInput;
		private TMP_Dropdown _resolutionInput;
		private Toggle _isHorizontallyFlippedInput;
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

			var currentSourceName = imageSource.sourceName;
			var defaultValue = options.FindIndex(option => option == currentSourceName);

			if (defaultValue >= 0) {
				_sourceInput.value = defaultValue;
			}

			_sourceInput.onValueChanged.AddListener(delegate {
				imageSource.SelectSource(_sourceInput.value);
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

			var currentResolutionStr = imageSource.resolution.ToString();
			var defaultValue = options.FindIndex(option => option == currentResolutionStr);

			if (defaultValue >= 0) {
				_resolutionInput.value = defaultValue;
			}

			_resolutionInput.onValueChanged.AddListener(delegate {
				imageSource.SelectResolution(_resolutionInput.value);
				if (!_solution.IsPaused()) {
					_solution.Play();
				}
			});
		}

		private void InitializeIsHorizontallyFlipped() {
			_isHorizontallyFlippedInput = transform.Find(IsHorizontallyFlippedPath).GetComponent<Toggle>();

			var imageSource = SolutionUtils.GetImageSource();
			_isHorizontallyFlippedInput.isOn = imageSource.isHorizontallyFlipped;
			_isHorizontallyFlippedInput.onValueChanged.AddListener(delegate {
				imageSource.isHorizontallyFlipped = _isHorizontallyFlippedInput.isOn;
			});
		}

		private void InitializeVirtualCamera() {
			_virtualCameraInstall = transform.Find(VirtualCameraInstall).GetComponent<Button>();
			_virtualCameraUninstall = transform.Find(VirtualCameraUninstall).GetComponent<Button>();
			
			#if !UNITY_STANDALONE_WIN
			#  error Virtual Camera won't work on non linux systems
			#endif

			_virtualCameraInstall.onClick.RemoveAllListeners();
			_virtualCameraInstall.onClick.AddListener(delegate {
				System.Diagnostics.Process.Start(Path.Combine(Application.streamingAssetsPath, "unitycapture", "Install.bat"));
			});

			_virtualCameraUninstall.onClick.RemoveAllListeners();
			_virtualCameraUninstall.onClick.AddListener(delegate {
				System.Diagnostics.Process.Start(Path.Combine(Application.streamingAssetsPath, "unitycapture", "Uninstall.bat"));
			});
		}
	}
}
