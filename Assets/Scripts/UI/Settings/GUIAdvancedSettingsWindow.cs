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
		public SettingsField expUseWrist;
		public SettingsField expUseLegs;
		public SettingsField guiScale;

		[Header("Fields")]
		public GUIBoneSettingsWindow boneSettingsWindow;
		
		void Start() {
			vmcSenderPort = CreateSetting("VMC Sender Port", builder => {
				return builder.AddNumberInput((_, value) => { Settings.VMCSenderPort = value; }, 0, 65535, Settings.VMCSenderPort, 3333);	
			});
			vmcSenderPort = CreateSetting("VMC Receiver Port", builder => {
				return builder.AddNumberInput((_, value) => { Settings.VMCReceiverPort = value; }, 0, 65535, Settings.VMCReceiverPort, 3333);	
			});
			boneWindow = CreateSetting("Bone Window", builder => {
				return builder.AddButton("Open", (_) => { boneSettingsWindow.gameObject.SetActive(true); });
			});
			alwaysShowUI = CreateSetting("Always show UI", builder => {
				return builder.AddToggle((_, value) => { Settings.AlwaysShowUI = value; }, Settings.AlwaysShowUI);
			});
			expUseWrist = CreateSetting("(Exp) Wrist rotation", builder => {
				return builder.AddToggle((_, value) => { Settings.UseWristRotation = value; }, Settings.UseWristRotation);
			});
			expUseLegs = CreateSetting("(Exp) Leg rotation", builder => {
				return builder.AddToggle((_, value) => { Settings.UseLegRotation = value; }, Settings.UseLegRotation);
			});
			guiScale = CreateSetting("(Exp) Gui Scale", builder => {
				return builder.AddIntSlider((_, value) => { Settings.GuiScale = value; }, 1, 10, Settings.GuiScale);
			});
		}

		private SettingsField CreateSetting(string name, Func<SettingsFieldTemplate, SettingsFieldTemplate> builder) {
			GameObject empty = Instantiate(emptySetting);
			empty.transform.localScale = Vector3.one;
			empty.transform.SetParent(contentTransform, false);
			empty.SetActive(true);
			return builder.Invoke(empty.GetComponent<SettingsFieldTemplate>()).Build(name);
		}
	}
}
