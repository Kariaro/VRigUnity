using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static HardCoded.VRigUnity.FileDialogUtils;

namespace HardCoded.VRigUnity {
	public class GUIBackgroundConfigWindow : MonoBehaviour {
		private const string BackgroundColorTogglePath = "Contents/Background/Toggle";
		private const string BackgroundColorInputPath  = "Contents/Background/InputField";
		private const string BackgroundColorButtonPath = "Contents/Background/Button";
		private const string ShowCameraTogglePath      = "Contents/ShowCamera/Toggle";
		private const string CustomImageTogglePath     = "Contents/Image/Toggle";
		private const string CustomImageTextFieldPath  = "Contents/Image/InputField";
		private const string CustomImageButtonPath     = "Contents/Image/Button";
		
		[SerializeField] private GUIScript settings;
		[SerializeField] private GUIColorPickerWindow _colorPicker;
		private TMP_InputField _bgColorInput;
		private Toggle _bgColorToggle;
		private Button _bgColorButton;
		
		private Toggle _showCameraToggle;
		
		private Toggle _bgImageToggle;
		private Button _bgImageButton;

		void Start() {
			InitializeContents();
		}

		void OnEnable() {
			InitializeContents();
		}

		private void InitializeContents() {
			InitializeBackgroundColor();
			InitializeShowCamera();
			InitializeCustomImage();
		}

		private void InitializeBackgroundColor() {
			_bgColorInput = transform.Find(BackgroundColorInputPath).GetComponent<TMP_InputField>();
			_bgColorInput.onValueChanged.RemoveAllListeners();
			_bgColorInput.text = "";
			_bgColorInput.onValueChanged.AddListener(delegate {
				if (GUIColorPickerWindow.ToColor(_bgColorInput.text, out Color outColor)) {
					settings.SetBackgroundColor(outColor);
				}
			});

			
			_bgColorToggle = transform.Find(BackgroundColorTogglePath).GetComponent<Toggle>();
			_bgColorToggle.onValueChanged.RemoveAllListeners();
			_bgColorToggle.isOn = Settings.ShowCustomBackgroundColor;
			_bgColorToggle.onValueChanged.AddListener(delegate {
				settings.SetShowBackgroundColor(_bgColorToggle.isOn);
			});

			_bgColorButton = transform.Find(BackgroundColorButtonPath).GetComponent<Button>();
			_bgColorButton.onClick.RemoveAllListeners();
		}

		private void InitializeShowCamera() {
			_showCameraToggle = transform.Find(ShowCameraTogglePath).GetComponent<Toggle>();
			_showCameraToggle.onValueChanged.RemoveAllListeners();
			_showCameraToggle.onValueChanged.AddListener(delegate {
				settings.SetShowCamera(_showCameraToggle.isOn);
			});
		}

		private void InitializeCustomImage() {
			_bgImageToggle = transform.Find(CustomImageTogglePath).GetComponent<Toggle>();
			_bgImageToggle.isOn = Settings.ShowCustomBackground;
			_bgImageToggle.onValueChanged.RemoveAllListeners();
			_bgImageToggle.onValueChanged.AddListener(delegate {
				settings.SetShowBackgroundImage(_bgImageToggle.isOn);
			});

			_bgImageButton = transform.Find(CustomImageButtonPath).GetComponent<Button>();
			_bgImageButton.onClick.RemoveAllListeners();
			_bgImageButton.onClick.AddListener(SelectCustomImage);
		}
		
		public void SelectCustomImage() {
			var extensions = new [] {
				new CustomExtensionFilter("Image Files", new string[] { "png", "jpg", "jpeg" }),
				new CustomExtensionFilter("All Files", "*"),
			};
			
			FileDialogUtils.OpenFilePanel(this, "Open Image", Settings.ImageFile, extensions, false, (paths) => {
				if (paths.Length > 0) {
					string filePath = paths[0];
					settings.LoadCustomImage(filePath);
				}
			});
		}

		public void PickColor() {
			_colorPicker.ShowWindow(_bgColorInput.text, (result) => {
				_bgColorInput.SetTextWithoutNotify(GUIColorPickerWindow.FromColor(result));
				settings.SetBackgroundColor(result);
			});
		}
	}
}
