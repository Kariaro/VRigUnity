using System;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class GUIAdvancedSettingsWindow : MonoBehaviour {
		[Header("Template")]
		public GameObject emptySetting;
		public Transform contentTransform;
	
		[Header("Settings")]
		public SettingsField vmcPort;
		
		void Start() {
			vmcPort = CreateSetting("VMC Port", builder => {
				return builder.AddNumberInput((_, value) => { Settings.VMCPort = value; }, 0, 65535, Settings.VMCPort, 3333, -1);	
			});
		}

		private SettingsField CreateSetting(string name, Func<SettingsFieldTemplate, SettingsFieldTemplate> builder) {
			GameObject empty = Instantiate(emptySetting);
			empty.transform.SetParent(contentTransform);
			empty.SetActive(true);
			return builder.Invoke(empty.GetComponent<SettingsFieldTemplate>())
				.Build(name);
		}
	}
}
