using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class GUIColorPickerWindow : MonoBehaviour {
		private const string _InputFieldPath   = "Contents/InputField";
		private const string _DisplayImagePath = "Contents/DisplayImage";
		private const string _RedSliderPath    = "Contents/Red";
		private const string _GreenSliderPath  = "Contents/Green";
		private const string _BlueSliderPath   = "Contents/Blue";
		private const string _AlphaSliderPath  = "Contents/Alpha";
		
		private TMP_InputField _inputField;
		private Image _displayImage;
		private Slider _redSlider;
		private Slider _greenSlider;
		private Slider _blueSlider;
		private Slider _alphaSlider;
		private Action<Color> _callback;

		private void InitializeContents() {
			_inputField = transform.Find(_InputFieldPath).GetComponent<TMP_InputField>();
			_inputField.onValueChanged.RemoveAllListeners();
			_inputField.text = "";
			_displayImage = transform.Find(_DisplayImagePath).GetComponent<Image>();
			_redSlider = transform.Find(_RedSliderPath).GetComponent<Slider>();
			_redSlider.onValueChanged.RemoveAllListeners();
			_greenSlider = transform.Find(_GreenSliderPath).GetComponent<Slider>();
			_greenSlider.onValueChanged.RemoveAllListeners();
			_blueSlider = transform.Find(_BlueSliderPath).GetComponent<Slider>();
			_blueSlider.onValueChanged.RemoveAllListeners();
			_alphaSlider = transform.Find(_AlphaSliderPath).GetComponent<Slider>();
			_alphaSlider.onValueChanged.RemoveAllListeners();

			_inputField.onValueChanged.AddListener(delegate {
				SetHexColor(_inputField.text);
				_displayImage.color = GetCurrentColor();
			});
			_redSlider.onValueChanged.AddListener(delegate {
				_inputField.text = GetColorHexString();
   			});
			_greenSlider.onValueChanged.AddListener(delegate {
				_inputField.text = GetColorHexString();
   			});
			_blueSlider.onValueChanged.AddListener(delegate {
				_inputField.text = GetColorHexString();
   			});
			_alphaSlider.onValueChanged.AddListener(delegate {
				_inputField.text = GetColorHexString();
   			});
		}

		private bool SetHexColor(string hexColor) {
			if (!ToColor(hexColor, out Color color)) {
				return false;
			}

			_redSlider.SetValueWithoutNotify(color.r * 255);
			_greenSlider.SetValueWithoutNotify(color.g * 255);
			_blueSlider.SetValueWithoutNotify(color.b * 255);
			_alphaSlider.SetValueWithoutNotify(color.a * 255);

			return true;
		}

		private string GetColorHexString() {
			return string.Format("#{0:X02}{1:X02}{2:X02}{3:X02}", (int)_redSlider.value, (int)_greenSlider.value, (int)_blueSlider.value, (int)_alphaSlider.value);
		}

		public Color GetCurrentColor() {
			return new Color(
				_redSlider.value / 255.0f,
				_greenSlider.value / 255.0f,
				_blueSlider.value / 255.0f,
				_alphaSlider.value / 255.0f
			);
		}

		public void ShowWindow(string hexColor, Action<Color> callback) {
			gameObject.SetActive(true);

			if (_inputField == null) {
				InitializeContents();
			}

			// Set text with notify
			_inputField.text = hexColor ?? "";
			_callback = callback;
		}

		public static string FromColor(Color color) {
			if (color.a == 1) {
				return string.Format("#{0:X02}{1:X02}{2:X02}", (int)(color.r * 255), (int)(color.g * 255), (int)(color.b * 255));
			} else {
				return string.Format("#{0:X02}{1:X02}{2:X02}{3:X02}", (int)(color.r * 255), (int)(color.g * 255), (int)(color.b * 255), (int)(color.a * 255));
			}
		}

		public static bool ToColor(string hexColor, out Color outColor) {
			if (hexColor.StartsWith("#")) {
				hexColor = hexColor.Substring(1);
			}

			if (!int.TryParse(hexColor, System.Globalization.NumberStyles.HexNumber, null, out int color)) {
				outColor = Color.clear;
				return false;
			}

			int r, g, b, a;
			if (hexColor.Length > 6) {
				r = (color >> 24) & 0xff;
				g = (color >> 16) & 0xff;
				b = (color >> 8) & 0xff;
				a = (color >> 0) & 0xff;
			} else if (hexColor.Length > 4) {
				r = (color >> 16) & 0xff;
				g = (color >> 8) & 0xff;
				b = (color >> 0) & 0xff;
				a = 255;
			} else if (hexColor.Length > 3) {
				r = ((color >> 12) & 0xf) * 0x11;
				g = ((color >> 8) & 0xf) * 0x11;
				b = ((color >> 4) & 0xf) * 0x11;
				a = ((color >> 0) & 0xf) * 0x11;
			} else {
				r = ((color >> 8) & 0xf) * 0x11;
				g = ((color >> 4) & 0xf) * 0x11;
				b = ((color >> 0) & 0xf) * 0x11;
				a = 255;
			}

			outColor = new Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
			return true;
		}

		public void HideWindow() {
			gameObject.SetActive(false);

			if (_callback != null) {
				_callback(GetCurrentColor());
				_callback = null;
			}
		}
	}
}
