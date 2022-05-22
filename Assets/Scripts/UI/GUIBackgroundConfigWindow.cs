using SFB;
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
		private const string _CustomImageTogglePath     = "Contents/Image/Toggle";
		private const string _CustomImageTextFieldPath  = "Contents/Image/InputField";
		private const string _CustomImageButtonPath     = "Contents/Image/Button";
		
		[SerializeField] private GUIScript settings;
		[SerializeField] private GUIColorPickerWindow _colorPicker;
		private TMP_InputField _backgroundInput;
		private Toggle _showCameraToggle;
		private Button _backgroundButton;
		
		private Toggle _customImageToggle;
		private Button _customImageButton;

		void Start() {
			InitializeContents();
		}

		private void InitializeContents() {
			InitializeBackground();
			InitializeImage();
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

		private void InitializeImage() {
			_customImageToggle = gameObject.transform.Find(_CustomImageTogglePath).gameObject.GetComponent<Toggle>();
			_customImageToggle.onValueChanged.RemoveAllListeners();
			_customImageToggle.onValueChanged.AddListener(delegate {
				settings.SetShowBackgroundImage(_customImageToggle.isOn);
			});

			_customImageButton = gameObject.transform.Find(_CustomImageButtonPath).gameObject.GetComponent<Button>();
			_customImageButton.onClick.RemoveAllListeners();
			_customImageButton.onClick.AddListener(SelectCustomImage);
		}
		
		public void SelectCustomImage() {
			var extensions = new [] {
				new ExtensionFilter("Image Files", new string[] { "png", "jpg", "jpeg" }),
				new ExtensionFilter("All Files", "*"),
			};

			var paths = FileDialogUtils.OpenFilePanelRemember("gui.backgroundconfig", "Open Image", extensions, false);
			if (paths.Length > 0) {
				string filePath = paths[0];
				settings.LoadCustomImage(filePath);
			}
		}

		public void PickColor() {
			_colorPicker.ShowWindow(_backgroundInput.text, (result) => {
				_backgroundInput.SetTextWithoutNotify(GUIColorPickerWindow.FromColor(result));
				settings.SetBackgroundColor(result);
			});
		}
	}
}
