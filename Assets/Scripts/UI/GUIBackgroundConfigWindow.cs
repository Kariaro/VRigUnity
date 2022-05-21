using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class GUIBackgroundConfigWindow : MonoBehaviour {
		private const string _BackgroundColorInputPath  = "Contents/Background/InputField";
		private const string _BackgroundColorButtonPath = "Contents/Background/Button";
		private const string _ShowCameraTogglePath      = "Contents/ShowCamera/Toggle";
		
		[SerializeField] private GUIScript settings;
		[SerializeField] private GUIColorPickerWindow _colorPicker;
		private TMP_InputField _backgroundInput;
		private Toggle _showCameraToggle;
		private Button _backgroundButton;

		void Start() {
			InitializeContents();
		}

		private void InitializeContents() {
			InitializeBackground();
		}

		private void InitializeBackground() {
			_backgroundInput = gameObject.transform.Find(_BackgroundColorInputPath).gameObject.GetComponent<TMP_InputField>();
			_backgroundInput.onValueChanged.RemoveAllListeners();
			_backgroundInput.text = "";

			_backgroundButton = gameObject.transform.Find(_BackgroundColorButtonPath).gameObject.GetComponent<Button>();
			_backgroundButton.onClick.RemoveAllListeners();

			_showCameraToggle = gameObject.transform.Find(_ShowCameraTogglePath).gameObject.GetComponent<Toggle>();
			_showCameraToggle.onValueChanged.RemoveAllListeners();
			_showCameraToggle.isOn = false;


			_backgroundInput.onValueChanged.AddListener(delegate {
				if (GUIColorPickerWindow.ToColor(_backgroundInput.text, out Color outColor)) {
					settings.SetBackgroundColor(outColor);
				}
			});

			_showCameraToggle.onValueChanged.AddListener(delegate {
				settings.SetShowCamera(_showCameraToggle.isOn);
			});
		}

		public void PickColor() {
			_colorPicker.ShowWindow(_backgroundInput.text, (result) => {
				_backgroundInput.SetTextWithoutNotify(GUIColorPickerWindow.FromColor(result));
				settings.SetBackgroundColor(result);
			});
		}
	}
}
