using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class SettingsField : MonoBehaviour {
		private FieldObject[] fields;

		public struct FieldObject {
			public TMP_InputField InputField { get; set; }
			public TMP_Dropdown Dropdown { get; set; }
			public Button Button { get; set; }
			public Toggle Toggle { get; set; }
			public Slider Slider { get; set; }
		}

		// Should only be called from SettingsFieldTemplate
		public void AddFieldObjects(List<object> objects) {
			fields = new FieldObject[objects.Count];

			for (int i = 0; i < objects.Count; i++) {
				switch (objects[i]) {
					case TMP_InputField inputField:
						fields[i] = new() { InputField = inputField };
						break;
					case TMP_Dropdown dropdown:
						fields[i] = new() { Dropdown = dropdown };
						break;
					case Button button:
						fields[i] = new() { Button = button };
						break;
					case Toggle toggle:
						fields[i] = new() { Toggle = toggle };
						break;
					case Slider slider:
						fields[i] = new() { Slider = slider };
						break;
				}
			}
		}

		public FieldObject this[int index] {
			get => fields[index];
		}
	}
}
