using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using static HardCoded.VRigUnity.LanguageLoader;
using static HardCoded.VRigUnity.SettingsFieldTemplate;

namespace HardCoded.VRigUnity {
	public class GUITabAdvancedConfig : GUITabConfig {
		public static bool ShowCamera { set; get; }

		[Header("Fields")]
		public GameObject trackingBox;

		// Fields
		private SettingsField languageField;
		private readonly FileWatcher languageWatcher = new(LanguageLoader.LanguageJsonFile);

		protected override void InitializeSettings() {
			AddDivider(Lang.AdvancedTabDividerLanguage);
			languageField = CreateSetting(Lang.AdvancedTabLanguage, builder => {
				return builder.AddDropdown(ChangeLanguage, new() {}, 0, FieldData.None);	
			});

			AddDivider(Lang.AdvancedTabDividerVmc);
			CreateSetting(Lang.AdvancedTabVmcSender, builder => {
				return builder
					.AddIpAddressField((_, value) => Settings.VMCSenderAddress = value, true, "127.0.0.1", () => Settings.VMCSenderAddress, new(136, ""))
					.AddNumberInput((_, value) => Settings.VMCSenderPort = value, 0, 65535, 3333, Settings.VMCSenderPort, FieldData.None);	
			});
			CreateSetting(Lang.AdvancedTabVmcReceiverPort, builder => {
				return builder.AddNumberInput((_, value) => Settings.VMCReceiverPort = value, 0, 65535, 3333, Settings.VMCReceiverPort, FieldData.None);	
			});

			AddDivider(Lang.AdvancedTabDividerTracking);
			CreateSetting(Lang.AdvancedTabHandArea, builder => {
				return builder
					.AddToggle((_, value) => Settings.UseTrackingBox = value, Settings.UseTrackingBox, FieldData.None)
					.AddButton(Lang.AdvancedTabHandAreaSelectArea, (_) => { trackingBox.SetActive(true); transform.parent.parent.gameObject.SetActive(false); }, FieldData.None);
			});

			AddDivider(Lang.AdvancedTabDividerUI);
			CreateSetting(Lang.AdvancedTabAlwaysShowUI, builder => {
				return builder.AddToggle((_, value) => Settings.AlwaysShowUI = value, Settings.AlwaysShowUI, FieldData.None);
			});
			CreateSetting(Lang.AdvancedTabAntiAliasing, builder => {
				return builder.AddDropdown((_, value) => {
					Settings.AntiAliasing = value;
					QualitySettings.antiAliasing = SettingsUtil.GetQualityValue(value);
				}, new() { "Disabled", "x2", "x4", "x8" }, Settings.AntiAliasing, FieldData.None);	
			});
			CreateSetting(Lang.AdvancedTabShowWebcam, builder => {
				return builder.AddToggle((_, value) => guiMain.SetShowCamera(value), false, FieldData.None);
			});
			CreateSetting(Lang.AdvancedTabShowModel, builder => {
				return builder.AddToggle((_, value) => Settings.ShowModel = value, Settings.ShowModel, FieldData.None);
			});
			CreateSetting(Lang.AdvancedTabGuiScale, builder => {
				return builder.AddIntSlider((_, value) => Settings.GuiScale = value, 1, 10, Settings.GuiScale, FieldData.None);
			});
			CreateSetting(Lang.AdvancedTabFlag, builder => {
			 	return builder.AddEnumDropdown((_, value) => Settings.Flag = value, Settings.Flag, FieldData.None);
			});

			AddDivider(Lang.AdvancedTabDividerExperimental);
			CreateSetting(Lang.AdvancedTabExpLegRotation, builder => {
				return builder.AddToggle((_, value) => Settings.UseLegRotation = value, Settings.UseLegRotation, FieldData.None);
			});
			CreateSetting(Lang.AdvancedTabExpHandTreshold, builder => {
				return builder.AddFloatTickSlider((_, value) => Settings.HandTrackingThreshold = value, 0f, 1f, 10, Settings.HandTrackingThreshold, FieldData.None);
			});
			CreateSetting(Lang.Extend(Lang.AdvancedTabExpInterpolation, lang => $"{lang.Get()} ({Settings._TrackingInterpolation.Default():0.00})"), builder => {
				return builder.AddFloatTickSlider((_, value) => Settings.TrackingInterpolation = value, 0.05f, 1f, 19, Settings.TrackingInterpolation, FieldData.None);
			});

			UpdateLanguages();
		}

		void OnEnable() {
			UpdateLanguages();
		}

		void Update() {
			if (languageWatcher.IsUpdated) {
				UpdateLanguages();
			}
		}

		private List<Language> languages = new();
		private void ChangeLanguage(TMP_Dropdown obj, int value) {
			var lang = languages[value];
			if (Localization.SetLanguage(lang)) {
				Settings.Language = lang.Code;
			}
		}

		private void UpdateLanguages() {
			if (languageField == null) {
				return;
			}

			languages = LanguageLoader.Languages;

			int languageIndex = 0;
			List<string> options = new();
			for (int i = 0; i < languages.Count; i++) {
				var lang = languages[i];
				options.Add(lang.DisplayName);
				
				if (lang.Code == Settings.Language) {
					languageIndex = i;
				}
			}

			var dropdown = languageField[0].Dropdown;
			dropdown.ClearOptions();
			dropdown.AddOptions(options);
			dropdown.SetValueWithoutNotify(languageIndex);
		}
	}
}
