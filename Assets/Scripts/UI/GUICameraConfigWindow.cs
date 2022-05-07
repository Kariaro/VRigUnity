using Mediapipe.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class GUICameraConfigWindow : MonoBehaviour {
		private const string _SourcePath                = "Scroll View/Viewport/Contents/Source/Dropdown";
		private const string _ResolutionPath            = "Scroll View/Viewport/Contents/Resolution/Dropdown";
		private const string _IsHorizontallyFlippedPath = "Scroll View/Viewport/Contents/IsHorizontallyFlipped/Toggle";

		private TestSolution _solution;
		private TMP_Dropdown _sourceInput;
		private TMP_Dropdown _resolutionInput;
		private Toggle _isHorizontallyFlippedInput;

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
			WebCamSource webCamSource = _solution.bootstrap.GetComponent<WebCamSource>();
			ImageSourceProvider.ImageSource = webCamSource;
			yield return webCamSource.UpdateSources();
			
			InitializeSource();
			InitializeResolution();
			InitializeIsHorizontallyFlipped();

			yield return null;
		}

		private void InitializeSource() {
			_sourceInput = gameObject.transform.Find(_SourcePath).gameObject.GetComponent<TMP_Dropdown>();
			_sourceInput.ClearOptions();
			_sourceInput.onValueChanged.RemoveAllListeners();

			var imageSource = ImageSourceProvider.ImageSource;
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
				_solution.Play();
				InitializeResolution();
			});
		}

		private void InitializeResolution() {
			_resolutionInput = gameObject.transform.Find(_ResolutionPath).gameObject.GetComponent<TMP_Dropdown>();
			_resolutionInput.ClearOptions();
			_resolutionInput.onValueChanged.RemoveAllListeners();

			var imageSource = ImageSourceProvider.ImageSource;
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
				_solution.Play();
			});
		}

		private void InitializeIsHorizontallyFlipped() {
			_isHorizontallyFlippedInput = gameObject.transform.Find(_IsHorizontallyFlippedPath).gameObject.GetComponent<Toggle>();

			var imageSource = ImageSourceProvider.ImageSource;
			_isHorizontallyFlippedInput.isOn = imageSource.isHorizontallyFlipped;
			_isHorizontallyFlippedInput.onValueChanged.AddListener(delegate {
				imageSource.isHorizontallyFlipped = _isHorizontallyFlippedInput.isOn;
			});
		}
	}
}
