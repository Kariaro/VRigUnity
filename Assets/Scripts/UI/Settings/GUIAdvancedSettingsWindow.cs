using System;
using System.Collections.Generic;
using UnityEngine;
using static HardCoded.VRigUnity.SettingsFieldTemplate;

namespace HardCoded.VRigUnity {
	public class GUIAdvancedSettingsWindow : MonoBehaviour {
		[Header("Template")]
		public GameObject emptySetting;
		public Transform contentTransform;
	
		[Header("Settings")]
		public List<SettingsField> settings = new();

		[Header("Fields")]
		public GUIBoneSettingsWindow boneSettingsWindow;
		
		void Start() {
			CreateSetting("VMC Sender Port", builder => {
				return builder.AddNumberInput((_, value) => { Settings.VMCSenderPort = value; }, 0, 65535, Settings.VMCSenderPort, 3333, FieldData.None);	
			});
			CreateSetting("VMC Receiver Port", builder => {
				return builder.AddNumberInput((_, value) => { Settings.VMCReceiverPort = value; }, 0, 65535, Settings.VMCReceiverPort, 3333, FieldData.None);	
			});
			CreateSetting("Bone Window", builder => {
				return builder.AddButton("Open", (_) => { boneSettingsWindow.gameObject.SetActive(true); }, FieldData.None);
			});
			CreateSetting("Always show UI", builder => {
				return builder.AddToggle((_, value) => { Settings.AlwaysShowUI = value; }, Settings.AlwaysShowUI, FieldData.None);
			});
			CreateSetting("(E) Wrist rotation", builder => {
				return builder.AddToggle((_, value) => { Settings.UseWristRotation = value; }, Settings.UseWristRotation, FieldData.None);
			});
			CreateSetting("(E) Leg rotation", builder => {
				return builder.AddToggle((_, value) => { Settings.UseLegRotation = value; }, Settings.UseLegRotation, FieldData.None);
			});
			CreateSetting("(E) Gui Scale", builder => {
				return builder.AddIntSlider((_, value) => { Settings.GuiScale = value; }, 1, 10, Settings.GuiScale, FieldData.None);
			});
			CreateSetting("(E) Hand threshold", builder => {
				return builder.AddFloatTickSlider((_, value) => { Settings.HandTrackingThreshold = value; }, 0f, 1f, 10, Settings.HandTrackingThreshold, FieldData.None);
			});
			CreateSetting($"(E) Interpolation ({Settings._TrackingInterpolation.Default():0.00})", builder => {
				return builder.AddFloatTickSlider((_, value) => { Settings.TrackingInterpolation = value; }, 0.05f, 1f, 19, Settings.TrackingInterpolation, FieldData.None);
			});
		}

		private SettingsField CreateSetting(string name, Func<SettingsFieldTemplate, SettingsFieldTemplate> builder) {
			GameObject empty = Instantiate(emptySetting);
			empty.transform.localScale = Vector3.one;
			empty.transform.SetParent(contentTransform, false);
			empty.SetActive(true);

			SettingsField field = builder.Invoke(empty.GetComponent<SettingsFieldTemplate>()).Build(name);
			settings.Add(field);
			return field;
		}
	}
}
