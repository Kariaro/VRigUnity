using System;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class GUIAdvancedSettingsWindow : MonoBehaviour {
		[Header("Template")]
		public GameObject emptySetting;
		public Transform contentTransform;
	
		[Header("Settings")]
		public SettingsField vmcSenderPort;
		public SettingsField vmcReceiverPort;
		public SettingsField boneWindow;
		public SettingsField alwaysShowUI;

		[Header("Fields")]
		public GUIBoneSettingsWindow boneSettingsWindow;
		
		void Start() {
			vmcSenderPort = CreateSetting("VMC Sender Port", builder => {
				return builder.AddNumberInput((_, value) => { Settings.VMCSenderPort = value; }, 0, 65535, Settings.VMCSenderPort, 3333, -1);	
			});
			vmcSenderPort = CreateSetting("VMC Receiver Port", builder => {
				return builder.AddNumberInput((_, value) => { Settings.VMCReceiverPort = value; }, 0, 65535, Settings.VMCReceiverPort, 3333, -1);	
			});
			boneWindow = CreateSetting("Bone Window", builder => {
				return builder.AddButton("Open", (_) => { boneSettingsWindow.gameObject.SetActive(true); }, -1);	
			});
			alwaysShowUI = CreateSetting("Always show UI", builder => {
				return builder.AddToggle((_, value) => { Settings.AlwaysShowUI = value; }, Settings.AlwaysShowUI);
			});
		}

		private SettingsField CreateSetting(string name, Func<SettingsFieldTemplate, SettingsFieldTemplate> builder) {
			GameObject empty = Instantiate(emptySetting);
			empty.transform.SetParent(contentTransform);
			empty.SetActive(true);
			return builder.Invoke(empty.GetComponent<SettingsFieldTemplate>()).Build(name);
		}
	}
}
