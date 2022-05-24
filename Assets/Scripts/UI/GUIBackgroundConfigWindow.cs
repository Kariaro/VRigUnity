using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class GUIBackgroundConfigWindow : MonoBehaviour {
		private const string BackgroundColorInputPath  = "Contents/Background/InputField";
		private const string BackgroundColorButtonPath = "Contents/Background/Button";
		private const string ShowCameraTogglePath      = "Contents/ShowCamera/Toggle";
		private const string CustomImageTogglePath     = "Contents/Image/Toggle";
		private const string CustomImageTextFieldPath  = "Contents/Image/InputField";
		private const string CustomImageButtonPath     = "Contents/Image/Button";
		
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

		void OnEnable() {
			InitializeContents();
		}

		private void InitializeContents() {
			InitializeBackground();
			InitializeImage();
		}

		private void InitializeBackground() {
			//_backgroundInput = transform.Find(BackgroundColorInputPath).GetComponent<TMP_InputField>();
			//_backgroundInput.onValueChanged.RemoveAllListeners();
			//_backgroundInput.text = "";
			//_backgroundInput.onValueChanged.AddListener(delegate {
			//	if (GUIColorPickerWindow.ToColor(_backgroundInput.text, out Color outColor)) {
			//		settings.SetBackgroundColor(outColor);
			//	}
			//});

			_backgroundButton = transform.Find(BackgroundColorButtonPath).GetComponent<Button>();
			_backgroundButton.onClick.RemoveAllListeners();

			_showCameraToggle = transform.Find(ShowCameraTogglePath).GetComponent<Toggle>();
			_showCameraToggle.onValueChanged.RemoveAllListeners();
			_showCameraToggle.isOn = false;
			_showCameraToggle.onValueChanged.AddListener(delegate {
				settings.SetShowCamera(_showCameraToggle.isOn);
			});
		}

		private void InitializeImage() {
			_customImageToggle = transform.Find(CustomImageTogglePath).GetComponent<Toggle>();
			_customImageToggle.isOn = Settings.ShowCustomBackground;
			_customImageToggle.onValueChanged.RemoveAllListeners();
			_customImageToggle.onValueChanged.AddListener(delegate {
				settings.SetShowBackgroundImage(_customImageToggle.isOn);
			});

			_customImageButton = transform.Find(CustomImageButtonPath).GetComponent<Button>();
			_customImageButton.onClick.RemoveAllListeners();
			_customImageButton.onClick.AddListener(SelectCustomImage);
		}
		
		public void SelectCustomImage() {
			var extensions = new [] {
				new ExtensionFilter("Image Files", new string[] { "png", "jpg", "jpeg" }),
				new ExtensionFilter("All Files", "*"),
			};

			var paths = FileDialogUtils.OpenFilePanel("Open Image", Settings.ImageFile, extensions, false);
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
