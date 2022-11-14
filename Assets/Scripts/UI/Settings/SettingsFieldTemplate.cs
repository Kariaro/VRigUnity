using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	// SettingsField builder class
	public class SettingsFieldTemplate : MonoBehaviour {
		// Template name
		[SerializeField] private TMP_Text fieldName;

		// Template objects
		[SerializeField] private GameObject inputFieldTemplate;
		[SerializeField] private GameObject toggleTemplate;
		[SerializeField] private GameObject buttonTemplate;
		[SerializeField] private GameObject dropdownTemplate;
		[SerializeField] private GameObject sliderTemplate;

		public SettingsFieldTemplate AddToggle(Action<Toggle, bool> action, bool value, FieldData data) {
			GameObject field = CreateInstance(toggleTemplate);
			field.name = "ToggleField";
			field.SetActive(true);

			ApplyLayout(field, data.Width);

			Toggle toggle = field.GetComponent<Toggle>();
			toggle.isOn = value;
			toggle.onValueChanged.AddListener(delegate { action.Invoke(toggle, toggle.isOn); });
			return this;
		}

		public SettingsFieldTemplate AddInputField(Action<TMP_InputField, string> action, string defaultValue, FieldData data) {
			GameObject field = CreateInstance(inputFieldTemplate);
			field.name = "InputField";
			field.SetActive(true);

			ApplyLayout(field, data.Width);

			TMP_InputField inputField = field.GetComponent<TMP_InputField>();
			inputField.text = defaultValue;
			inputField.onValueChanged.AddListener(delegate { action.Invoke(inputField, inputField.text); });
			return this;
		}

		public SettingsFieldTemplate AddNumberInput(Action<TMP_InputField, int> action, int min, int max, int value, int defaultValue, FieldData data) {
			GameObject field = CreateInstance(inputFieldTemplate);
			field.name = "NumberInputField";
			field.SetActive(true);
			
			ApplyLayout(field, data.Width);

			TMP_InputField inputField = field.GetComponent<TMP_InputField>();
			inputField.characterValidation = TMP_InputField.CharacterValidation.Integer;
			inputField.text = "" + Math.Clamp(value, min, max);
			inputField.onDeselect.AddListener(delegate {
				int value = min;
				bool valid = int.TryParse(inputField.text, out value);
				int result = Math.Clamp(value, min, max);

				// If invalid update the text
				if (result != value || (inputField.text != "" + result)|| !valid) {
					inputField.text = "" + (valid ? result : defaultValue);
				}
			});
			inputField.onValueChanged.AddListener(delegate {
				int value = min;
				int.TryParse(inputField.text, out value);
				int result = Math.Clamp(value, min, max);

				// If invalid update the text
				if (result != value) {
					inputField.text = "" + result;
				}

				action.Invoke(inputField, result);
			});
			return this;
		}

		public SettingsFieldTemplate AddEnumDropdown<T>(Action<TMP_Dropdown, T> action, T value, FieldData data) where T : Enum {
			GameObject field = CreateInstance(dropdownTemplate);
			field.name = "EnumDropdownField";
			field.SetActive(true);
			
			ApplyLayout(field, data.Width);

			int count = 0;
			int index = 0;
			List<string> options = new();
			foreach (Enum item in Enum.GetValues(typeof(T))) {
				options.Add(item.ToString());

				if (item.Equals(value)) {
					index = count;
				}

				count ++;
			}
			
			TMP_Dropdown dropdown = field.GetComponentInChildren<TMP_Dropdown>();
			dropdown.AddOptions(options);
			dropdown.SetValueWithoutNotify(index);
			dropdown.onValueChanged.AddListener(delegate {
				string valueName = options[dropdown.value];
				if (Enum.TryParse(typeof(T), valueName, out object result)) {
					action.Invoke(dropdown, (T) result);
				}
			});
			return this;
		}

		public SettingsFieldTemplate AddDropdown(Action<TMP_Dropdown, int> action, List<string> options, int index, FieldData data) {
			GameObject field = CreateInstance(dropdownTemplate);
			field.name = "DropdownField";
			field.SetActive(true);
			
			ApplyLayout(field, data.Width);

			TMP_Dropdown dropdown = field.GetComponentInChildren<TMP_Dropdown>();
			dropdown.AddOptions(options);
			dropdown.SetValueWithoutNotify(index);
			dropdown.onValueChanged.AddListener(delegate {
				action.Invoke(dropdown, dropdown.value);
			});
			return this;
		}

		public SettingsFieldTemplate AddButton(string name, Action<Button> action, FieldData data) {
			GameObject field = CreateInstance(buttonTemplate);
			field.name = "ButtonField";
			field.SetActive(true);

			ApplyLayout(field, data.Width);

			TMP_Text text = field.GetComponentInChildren<TMP_Text>();
			text.text = name;

			Button button = field.GetComponent<Button>();
			button.onClick.AddListener(delegate { action.Invoke(button); });
			return this;
		}

		public SettingsFieldTemplate AddFloatSlider(Action<Slider, float> action, float min, float max, float value, FieldData data) {
			GameObject field = CreateInstance(sliderTemplate);
			field.name = "SliderFloatField";

			Slider sliderField = field.GetComponent<Slider>();
			sliderField.minValue = min;
			sliderField.maxValue = max;
			sliderField.value = value;
			sliderField.onValueChanged.AddListener(delegate { action.Invoke(sliderField, sliderField.value); });
			
			field.SetActive(true);
			ApplyLayout(field, data.Width);
			return this;
		}

		public SettingsFieldTemplate AddFloatTickSlider(Action<Slider, float> action, float min, float max, int steps, float value, FieldData data) {
			GameObject field = CreateInstance(sliderTemplate);
			field.name = "SliderFloatTickField";

			float size = max - min;
			float normalized = (Mathf.Round(((value - min) / size) * steps) / steps) * size + min;

			Slider sliderField = field.GetComponent<Slider>();
			sliderField.minValue = min;
			sliderField.maxValue = max;
			sliderField.value = value;
			sliderField.onValueChanged.AddListener(delegate {
				float normalized = (Mathf.Round(((sliderField.value - min) / size) * steps) / steps) * size + min;
				sliderField.SetValueWithoutNotify(normalized);
				action.Invoke(sliderField, normalized);
			});
			
			field.SetActive(true);
			ApplyLayout(field, data.Width);
			return this;
		}

		public SettingsFieldTemplate AddIntSlider(Action<Slider, int> action, int min, int max, int value, FieldData data) {
			GameObject field = CreateInstance(sliderTemplate);
			field.name = "SliderIntField";

			Slider sliderField = field.GetComponent<Slider>();
			sliderField.minValue = min;
			sliderField.maxValue = max;
			sliderField.value = value;
			sliderField.wholeNumbers = true;
			sliderField.onValueChanged.AddListener(delegate { action.Invoke(sliderField, (int) sliderField.value); });
			
			field.SetActive(true);
			ApplyLayout(field, data.Width);
			return this;
		}

		private void ApplyLayout(GameObject obj, float width) {
			LayoutElement layoutElement = obj.GetComponent<LayoutElement>();
			if (width < 0) {
				layoutElement.flexibleWidth = 1;
				layoutElement.minWidth = -1;
			} else {
				layoutElement.minWidth = width;
			}
		}

		private GameObject CreateInstance(GameObject obj) {
			GameObject result = Instantiate(obj);
			result.transform.localScale = Vector3.one;
			result.transform.SetParent(transform, false);
			return result;
		}

		// This will remove all unused data inside this field
		public SettingsField Build(string name) {
			gameObject.name = "Field(" + name + ")";
			fieldName.text = name;

			SettingsField field = gameObject.AddComponent<SettingsField>();
			
			Destroy(inputFieldTemplate);
			Destroy(toggleTemplate);
			Destroy(buttonTemplate);
			Destroy(dropdownTemplate);
			Destroy(sliderTemplate);

			// Remove this field
			Destroy(this);
			return field;
		}

		public struct FieldData {
			public static FieldData None => new(-1, "");

			public float Width { get; }
			public string Description { get; }

			public FieldData(float width, string description) {
				Width = width;
				Description = description;
			}
		}
	}
}
