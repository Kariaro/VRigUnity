using System;
using System.Collections.Generic;
using UnityEngine;
using static HardCoded.VRigUnity.FileDialogUtils;
using static HardCoded.VRigUnity.SettingsFieldTemplate;

namespace HardCoded.VRigUnity {
	public class GUIAdvancedSettingsWindow : GUISettingsBase {
		public static bool ShowCamera { set; get; }

		[Header("Fields")]
		public GUIBoneSettingsWindow boneSettingsWindow;
		
		protected override void InitializeSettings() {
			AddDivider("VMC Settings");
			CreateSetting("VMC Sender", builder => {
				return builder
					.AddIpAddressField((_, value) => Settings.VMCSenderAddress = value, true, "127.0.0.1", () => Settings.VMCSenderAddress, new(136, ""))
					.AddNumberInput((_, value) => Settings.VMCSenderPort = value, 0, 65535, Settings.VMCSenderPort, 3333, FieldData.None);	
			});
			CreateSetting("VMC Receiver Port", builder => {
				return builder.AddNumberInput((_, value) => Settings.VMCReceiverPort = value, 0, 65535, 3333, Settings.VMCReceiverPort, FieldData.None);	
			});

			
			AddDivider("UI Settings");
			CreateSetting("Always show UI", builder => {
				return builder.AddToggle((_, value) => Settings.AlwaysShowUI = value, Settings.AlwaysShowUI, FieldData.None);
			});
			CreateSetting("Anti Aliasing", builder => {
				return builder.AddDropdown((_, value) => {
					Settings.AntiAliasing = value;
					QualitySettings.antiAliasing = SettingsUtil.GetQualityValue(value);
				}, new() { "Disabled", "x2", "x4", "x8" }, Settings.AntiAliasing, FieldData.None);	
			});
			CreateSetting("Show Camera", builder => {
				return builder.AddToggle((_, value) => guiScript.SetShowCamera(value), false, FieldData.None);
			});
			CreateSetting("Show Model", builder => {
				return builder.AddToggle((_, value) => Settings.ShowModel = value, Settings.ShowModel, FieldData.None);
			});
			CreateSetting("Custom Background", builder => {
				return builder
					.AddToggle((_, value) => guiScript.SetShowBackgroundImage(value), Settings.ShowCustomBackground, FieldData.None)
					.AddButton("Select Image", (_) => {
						var extensions = new [] {
							new CustomExtensionFilter("Image Files", new string[] { "png", "jpg", "jpeg" }),
							new CustomExtensionFilter("All Files", "*"),
						};
			
						FileDialogUtils.OpenFilePanel(this, "Open Image", Settings.ImageFile, extensions, false, (paths) => {
							if (paths.Length > 0) {
								string filePath = paths[0];
								guiScript.LoadCustomImage(filePath);
							}
						});
					}, FieldData.None);
			});
			CreateSetting("Gui Scale", builder => {
				return builder.AddIntSlider((_, value) => Settings.GuiScale = value, 1, 10, Settings.GuiScale, FieldData.None);
			});
			CreateSetting($"Flag", builder => {
			 	return builder.AddEnumDropdown((_, value) => Settings.Flag = value, Settings.Flag, FieldData.None);
			});

			AddDivider("Experimental Settings");
			CreateSetting("(E) Bone Window", builder => {
				return builder.AddButton("Open", (_) => boneSettingsWindow.ShowWindow(), FieldData.None);
			});
			CreateSetting("(E) Leg rotation", builder => {
				return builder.AddToggle((_, value) => Settings.UseLegRotation = value, Settings.UseLegRotation, FieldData.None);
			});
			CreateSetting("(E) Full IK", builder => {
				return builder.AddToggle((_, value) => Settings.UseFullIK = value, Settings.UseFullIK, FieldData.None);
			});
			CreateSetting("(E) Hand threshold", builder => {
				return builder.AddFloatTickSlider((_, value) => Settings.HandTrackingThreshold = value, 0f, 1f, 10, Settings.HandTrackingThreshold, FieldData.None);
			});
			CreateSetting($"(E) Interpolation ({Settings._TrackingInterpolation.Default():0.00})", builder => {
				return builder.AddFloatTickSlider((_, value) => Settings.TrackingInterpolation = value, 0.05f, 1f, 19, Settings.TrackingInterpolation, FieldData.None);
			});
		}
	}
}
