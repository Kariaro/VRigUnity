using System;
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

		public SettingsFieldTemplate AddToggle(Action<Toggle, bool> action, bool defaultValue = false) {
			GameObject field = Instantiate(toggleTemplate, transform);
			field.name = "ToggleField";
			field.SetActive(true);

			Toggle toggle = field.GetComponent<Toggle>();
			toggle.isOn = defaultValue;
			toggle.onValueChanged.AddListener(delegate { action.Invoke(toggle, toggle.isOn); });
			return this;
		}

		public SettingsFieldTemplate AddInputField(Action<TMP_InputField, string> action, string defaultValue = "") {
			GameObject field = Instantiate(inputFieldTemplate, transform);
			field.name = "InputField";
			field.SetActive(true);

			TMP_InputField inputField = field.GetComponent<TMP_InputField>();
			inputField.text = defaultValue;
			inputField.onValueChanged.AddListener(delegate { action.Invoke(inputField, inputField.text); });
			return this;
		}

		public SettingsFieldTemplate AddNumberInput(Action<TMP_InputField, int> action, int min, int max, int value, int defaultValue, float width = 24) {
			GameObject field = Instantiate(inputFieldTemplate, transform);
			field.name = "NumberInputField";
			field.SetActive(true);

			LayoutElement layoutElement = field.GetComponent<LayoutElement>();
			if (width < 0) {
				layoutElement.flexibleWidth = 1;
				layoutElement.minWidth = -1;
			} else {
				layoutElement.minWidth = width;
			}

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

		public SettingsFieldTemplate AddButton(string name, Action<Button> action, float width = 24) {
			GameObject field = Instantiate(buttonTemplate, transform);
			field.name = "ButtonField";
			field.SetActive(true);

			LayoutElement layoutElement = field.GetComponent<LayoutElement>();
			if (width < 0) {
				layoutElement.flexibleWidth = 1;
				layoutElement.minWidth = -1;
			} else {
				layoutElement.minWidth = width;
			}

			TMP_Text text = field.GetComponentInChildren<TMP_Text>();
			text.text = name;

			Button button = field.GetComponent<Button>();
			button.onClick.AddListener(delegate { action.Invoke(button); });
			return this;
		}

		// This will remove all unused data inside this field
		public SettingsField Build(string name) {
			gameObject.name = "Field(" + name + ")";
			fieldName.text = name;

			SettingsField field = gameObject.AddComponent<SettingsField>();
			
			Destroy(inputFieldTemplate);
			Destroy(toggleTemplate);
			Destroy(buttonTemplate);

			// Remove this field
			Destroy(this);
			return field;
		}
	}
}
