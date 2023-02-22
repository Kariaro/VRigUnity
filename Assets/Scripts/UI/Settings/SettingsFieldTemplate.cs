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

		// Used to generate the modifiable elements
		private readonly List<object> fieldObjects = new();

		public SettingsFieldTemplate AddToggle(Action<Toggle, bool> action, bool value, FieldData data) {
			GameObject field = CreateInstance(toggleTemplate);
			field.name = "ToggleField";
			field.SetActive(true);

			ApplyLayout(field, data.Width);

			Toggle toggle = field.GetComponent<Toggle>();
			toggle.isOn = value;
			toggle.onValueChanged.AddListener(delegate { action.Invoke(toggle, toggle.isOn); });
			fieldObjects.Add(toggle);
			return this;
		}

		public SettingsFieldTemplate AddInputField(Action<TMP_InputField, string> action, string value, FieldData data) {
			GameObject field = CreateInstance(inputFieldTemplate);
			field.name = "InputField";
			field.SetActive(true);

			ApplyLayout(field, data.Width);

			TMP_InputField inputField = field.GetComponent<TMP_InputField>();
			inputField.text = value;
			inputField.onValueChanged.AddListener(delegate { action.Invoke(inputField, inputField.text); });
			fieldObjects.Add(inputField);
			return this;
		}

		public SettingsFieldTemplate AddIpAddressField(Action<TMP_InputField, string> action, bool hideIp, string defaultValue, Func<string> value, FieldData data) {
			GameObject field = CreateInstance(inputFieldTemplate);
			field.name = "IpAddressInputField";
			field.SetActive(true);

			ApplyLayout(field, data.Width);

			TMP_InputField inputField = field.GetComponent<TMP_InputField>();
			inputField.text = hideIp ? Lang.IpHidden.Get() : SettingsUtil.NormalizeIpAddress(value.Invoke(), defaultValue);
			inputField.onDeselect.AddListener(delegate {
				inputField.SetTextWithoutNotify(hideIp ? Lang.IpHidden.Get() : SettingsUtil.NormalizeIpAddress(value.Invoke(), defaultValue));
			});
			inputField.onSelect.AddListener(delegate {
				inputField.SetTextWithoutNotify(SettingsUtil.NormalizeIpAddress(value.Invoke(), defaultValue));
			});
			inputField.onValueChanged.AddListener(delegate {
				string value = SettingsUtil.NormalizeIpAddress(inputField.text, defaultValue);
				action.Invoke(inputField, value);
			});
			field.AddComponent<CustomLocalization>().Init(() => {
				if (!inputField.isFocused) {
					inputField.text = Lang.IpHidden.Get();
				}
			});
			fieldObjects.Add(inputField);
			return this;
		}

		public SettingsFieldTemplate AddNumberInput(Action<TMP_InputField, int> action, int min, int max, int defaultValue, int value, FieldData data) {
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
					inputField.textComponent.rectTransform.localPosition = Vector3.zero;
					inputField.GetComponentInChildren<TMP_SelectionCaret>().rectTransform.localPosition = Vector3.zero;
				}

				inputField.caretPosition = 0;
				inputField.stringPosition = 0;
			});
			inputField.onValueChanged.AddListener(delegate {
				int value = min;
				int.TryParse(inputField.text, out value);
				
				// If invalid update the text
				if (inputField.text.Length != 0) {
					inputField.text = "" + value;
				}

				action.Invoke(inputField, Mathf.Clamp(value, min, max));
			});
			fieldObjects.Add(inputField);
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
			fieldObjects.Add(dropdown);
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
			fieldObjects.Add(dropdown);
			return this;
		}

		public SettingsFieldTemplate AddButton(Lang loc, Action<Button> action, FieldData data) {
			GameObject field = CreateInstance(buttonTemplate);
			field.name = "ButtonField";
			field.SetActive(true);

			ApplyLayout(field, data.Width);

			TMP_Text text = field.GetComponentInChildren<TMP_Text>();
			text.text = name;
			field.AddComponent<TextLocalization>().Init(loc, text);

			Button button = field.GetComponent<Button>();
			button.onClick.AddListener(delegate { action.Invoke(button); });
			fieldObjects.Add(button);
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
			fieldObjects.Add(sliderField);
			
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
			fieldObjects.Add(sliderField);
			
			field.SetActive(true);
			ApplyLayout(field, data.Width);
			return this;
		}

		public SettingsFieldTemplate AddIntSlider(Action<Slider, int> action, int min, int max, int value, FieldData data) {
			GameObject field = CreateInstance(sliderTemplate);
			field.name = "SliderIntField";

			Slider sliderField = field.GetComponent<Slider>();
			sliderField.wholeNumbers = true;
			sliderField.minValue = min;
			sliderField.maxValue = max;
			sliderField.value = value;
			sliderField.onValueChanged.AddListener(delegate { action.Invoke(sliderField, (int) sliderField.value); });
			fieldObjects.Add(sliderField);
			
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

		public void BuildDivider(Lang data, float height = 24) {
			gameObject.name = "Divider(" + data.id + ")";
			fieldName.text = data.Get();
			fieldName.margin = new(-20, 0, 0, 0);
			fieldName.fontStyle = FontStyles.Bold;
			gameObject.AddComponent<TextLocalization>().Init(data, fieldName);
			LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
			layoutElement.minHeight = height;
			layoutElement = fieldName.GetComponent<LayoutElement>();
			layoutElement.preferredWidth = -1;
			DestroyThis();
		}

		public SettingsField Build(Lang data) {
			gameObject.name = "Field(" + data.id + ")";
			fieldName.text = data.Get();
			gameObject.AddComponent<TextLocalization>().Init(data, fieldName);

			SettingsField field = gameObject.AddComponent<SettingsField>();
			field.AddFieldObjects(fieldObjects);

			// Add tab navigation
			List<Selectable> selectables = new();
			foreach (object item in fieldObjects) {
				if (item is Selectable selectable) {
					selectables.Add(selectable);
				}
			}

			// Only if there are more than two elements add the navigation
			if (selectables.Count > 1) {
				TabNavigation navigation = gameObject.AddComponent<TabNavigation>();
				navigation.elements = selectables;
				navigation.wrapAround = true;
			}

			DestroyThis();
			return field;
		}
		
		// This will remove all unused data inside this field
		private void DestroyThis() {
			Destroy(inputFieldTemplate);
			Destroy(toggleTemplate);
			Destroy(buttonTemplate);
			Destroy(dropdownTemplate);
			Destroy(sliderTemplate);

			// Remove this field
			Destroy(this);
		}

		public struct FieldData {
			public static FieldData None => new(-1, "");

			public float Width { get; }
			public string Description { get; }

			public FieldData(float width) {
				Width = width;
				Description = "";
			}

			public FieldData(float width, string description) {
				Width = width;
				Description = description;
			}
		}
	}
}
