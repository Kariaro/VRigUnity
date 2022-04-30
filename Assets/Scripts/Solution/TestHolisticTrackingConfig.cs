using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.UI;
using Mediapipe.Unity;

namespace HardCoded.VRigUnity {
	public class TestHolisticTrackingConfig : ModalContents {
		private const string _ModelComplexityPath = "Scroll View/Viewport/Contents/Model Complexity/Dropdown";
		private const string _SmoothLandmarksPath = "Scroll View/Viewport/Contents/Smooth Landmarks/Toggle";
		private const string _RefineFaceLandmarksPath = "Scroll View/Viewport/Contents/Refine Face Landmarks/Toggle";
		private const string _MinDetectionConfidencePath = "Scroll View/Viewport/Contents/Min Detection Confidence/InputField";
		private const string _MinTrackingConfidencePath = "Scroll View/Viewport/Contents/Min Tracking Confidence/InputField";
		private const string _TimeoutMillisecPath = "Scroll View/Viewport/Contents/Timeout Millisec/InputField";

		private TestHolisticTrackingSolution _solution;
		private Dropdown _modelComplexityInput;
		private Toggle _smoothLandmarksInput;
		private Toggle _refineFaceLandmarksInput;
		private InputField _minDetectionConfidenceInput;
		private InputField _minTrackingConfidenceInput;
		private InputField _timeoutMillisecInput;

		private bool _isChanged;

		private void Start() {
			_solution = GameObject.Find("Solution").GetComponent<TestHolisticTrackingSolution>();
			InitializeContents();
		}

		public override void Exit() {
			GetModal().CloseAndResume(_isChanged);
		}

		public void SwitchModelComplexity() {
			_solution.ModelComplexity = (TestHolisticTrackingGraph.ModelComplexity)_modelComplexityInput.value;
			_isChanged = true;
		}

		public void ToggleSmoothLandmarks() {
			_solution.SmoothLandmarks = _smoothLandmarksInput.isOn;
			_isChanged = true;
		}

		public void ToggleRefineFaceLandmarks() {
			_solution.RefineFaceLandmarks = _refineFaceLandmarksInput.isOn;
			_isChanged = true;
		}

		public void SetMinDetectionConfidence() {
			if (float.TryParse(_minDetectionConfidenceInput.text, out var value)) {
				_solution.MinDetectionConfidence = value;
				_isChanged = true;
			}
		}

		public void SetMinTrackingConfidence() {
			if (float.TryParse(_minTrackingConfidenceInput.text, out var value)) {
				_solution.MinTrackingConfidence = value;
				_isChanged = true;
			}
		}

		public void SetTimeoutMillisec() {
			if (int.TryParse(_timeoutMillisecInput.text, out var value)) {
				_solution.timeoutMillisec = value;
				_isChanged = true;
			}
		}

		private void InitializeContents() {
			InitializeModelComplexity();
			InitializeSmoothLandmarks();
			InitializeRefineFaceLandmarks();
			InitializeMinDetectionConfidence();
			InitializeMinTrackingConfidence();
			InitializeTimeoutMillisec();

			_solution.runningMode = RunningMode.Async;
		}

		private void InitializeModelComplexity() {
			_modelComplexityInput = gameObject.transform.Find(_ModelComplexityPath).gameObject.GetComponent<Dropdown>();
			_modelComplexityInput.ClearOptions();

			var options = new List<string>(Enum.GetNames(typeof(TestHolisticTrackingGraph.ModelComplexity)));
			_modelComplexityInput.AddOptions(options);

			var currentModelComplexity = _solution.ModelComplexity;
			var defaultValue = options.FindIndex(option => option == currentModelComplexity.ToString());

			if (defaultValue >= 0) {
				_modelComplexityInput.value = defaultValue;
			}

			_modelComplexityInput.onValueChanged.AddListener(delegate { SwitchModelComplexity(); });
		}

		private void InitializeSmoothLandmarks() {
			_smoothLandmarksInput = gameObject.transform.Find(_SmoothLandmarksPath).gameObject.GetComponent<Toggle>();
			_smoothLandmarksInput.isOn = _solution.SmoothLandmarks;
			_smoothLandmarksInput.onValueChanged.AddListener(delegate { ToggleSmoothLandmarks(); });
		}

		private void InitializeRefineFaceLandmarks() {
			_refineFaceLandmarksInput = gameObject.transform.Find(_RefineFaceLandmarksPath).gameObject.GetComponent<Toggle>();
			_refineFaceLandmarksInput.isOn = _solution.RefineFaceLandmarks;
			_refineFaceLandmarksInput.onValueChanged.AddListener(delegate { ToggleRefineFaceLandmarks(); });
		}

		private void InitializeMinDetectionConfidence() {
			_minDetectionConfidenceInput = gameObject.transform.Find(_MinDetectionConfidencePath).gameObject.GetComponent<InputField>();
			_minDetectionConfidenceInput.text = _solution.MinDetectionConfidence.ToString();
			_minDetectionConfidenceInput.onValueChanged.AddListener(delegate { SetMinDetectionConfidence(); });
		}

		private void InitializeMinTrackingConfidence() {
			_minTrackingConfidenceInput = gameObject.transform.Find(_MinTrackingConfidencePath).gameObject.GetComponent<InputField>();
			_minTrackingConfidenceInput.text = _solution.MinTrackingConfidence.ToString();
			_minTrackingConfidenceInput.onValueChanged.AddListener(delegate { SetMinTrackingConfidence(); });
		}

		private void InitializeTimeoutMillisec() {
			_timeoutMillisecInput = gameObject.transform.Find(_TimeoutMillisecPath).gameObject.GetComponent<InputField>();
			_timeoutMillisecInput.text = _solution.timeoutMillisec.ToString();
			_timeoutMillisecInput.onValueChanged.AddListener(delegate { SetTimeoutMillisec(); });
		}
	}
}
