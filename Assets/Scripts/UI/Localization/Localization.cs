using System;
using System.Collections.Generic;
using UnityEngine;
using static HardCoded.VRigUnity.LanguageLoader;

namespace HardCoded.VRigUnity {
	public class Localization : MonoBehaviour {
		private static Localization _instance;
		public static Localization Instance {
			get {
				if (_instance == null) {
					_instance = FindObjectOfType<Localization>(true);
				}

				return _instance;
			}
		}

		/// <summary>
		/// Update the current language
		/// </summary>
		public static bool SetLanguage(Language lang) {
			try {
				Dictionary<string, string> language = LanguageLoader.LoadLanguage(lang);

				Instance._language.Clear();
				foreach (var entry in language) {
					Instance._language.Add(entry.Key, entry.Value);
				}
				_localizationChangeEvent?.Invoke();
			} catch (Exception e) {
				Logger.Error("Localization", $"Failed to change to language '{lang.Code}'. {e.Message}");
				return false;
			}
			
			return true;
		}

		public delegate void LocalizationChangeEvent();

#pragma warning disable IDE1006 // Naming Styles
		private static event LocalizationChangeEvent _localizationChangeEvent;
#pragma warning restore IDE1006 // Naming Styles
		public static event LocalizationChangeEvent OnLocalizationChangeEvent {
			add {
				// When a value is added they will always be updated
				value.Invoke();
				_localizationChangeEvent += value;
			}
			remove => _localizationChangeEvent -= value;
		}

		private readonly Dictionary<string, string> _language = new();
		public string Get(string id, string fallback) {
			if (_language.TryGetValue(id, out string value) && value.Length != 0) {
				return value;
			}

			return fallback;
		}
	}

	public class Lang {
		private static readonly Dictionary<string, Lang> _localizationMap = new();
		private static readonly List<Lang> _localizationList = new();
		public static readonly Lang
				CameraStart = new("camera.start", "Start Camera"),
				CameraStop = new("camera.stop", "Stop Camera"),
				MocapRecordingStart = new("mocap_recording.start", "Record Mocap"),
				MocapRecordingStop = new("mocap_recording.stop", "Stop Recording"),
				VisualsOn = new("visuals.on", "Visuals On"),
				VisualsOff = new("visuals.off", "Visuals Off"),
				VmcReceiverStart = new("vmc_receiver.start", "Start Receiver VMC"),
				VmcReceiverStop = new("vmc_receiver.stop", "Stop Receiver VMC"),
				VmcSenderStart = new("vmc_sender.start", "Start Sender VMC"),
				VmcSenderStop = new("vmc_sender.stop", "Stop Sender VMC"),
				VmcPort = new("vmc.port", "Port"),
				HelpText = new("ui.help_text", "Shift to Pan, Ctrl to Rotate"),
				SettingsButton = new("ui.settings", "Settings"),

				DialogImageFiles = new("dialog.image_files", "Image Files"),
				DialogVrmFiles = new("dialog.vrm_files", "VRM Files"),
				DialogAllFiles = new("dialog.all_files", "All Files"),
				DialogOpenImage = new("dialog.open_image", "Open Image"),
				DialogOpenFile = new("dialog.open_file", "Open File"),

				IpHidden = new("text.ip_hidden", "Ip Hidden"),
				ModelHidden = new("text.model_hidden", "Model Is Hidden"),

				ModelTabLabel = new("tab.model.label", "Model"),
				ModelTabAboutMeTitle = new("tab.model.about_me_title", "About me"),
				ModelTabAboutMeBody = new("tab.model.about_me_body", "I'm a small developer that likes to do stuff on my spare time\nPlease check me out."),
				ModelTabTitle = new("tab.model.title", "Model Settings"),
				ModelTabSelectModel = new("tab.model.select_model", "Select Model"),
				ModelTabResetModel = new("tab.model.reset_model", "Reset Model"),
				ModelTabModelPosition = new("tab.model.model_position", "Model position"),
				ModelTabResetLayout = new("tab.model.reset_layout", "Reset Layout"),
				
				CameraTabLabel = new("tab.camera.label", "Camera"),
				CameraTabDividerCamera = new("tab.camera.divider.camera", "Camera"),
				CameraTabDividerEffects = new("tab.camera.divider.effects", "Effects"),
				CameraTabSource = new("tab.camera.source", "Source"),
				CameraTabResolution = new("tab.camera.resolution", "Resolution"),
				CameraTabCustomResolution = new("tab.camera.custom_resolution", "Custom Res"),
				CameraTabIsHorizontallyFlipped = new("tab.camera.is_horizontally_flipped", "Is Horizontally Flipped"),
				CameraTabVirtualCamera = new("tab.camera.virtual_camera", "Virtual Camera"),
				CameraTabVirtualCameraInstall = new("tab.camera.virtual_camera.install", "Install"),
				CameraTabVirtualCameraUninstall = new("tab.camera.virtual_camera.uninstall", "Uninstall"),
				CameraTabCustomBackground = new("tab.camera.custom_background", "Custom Background"),
				CameraTabCustomBackgroundSelectImage = new("tab.camera.custom_background.select_image", "Select Image"),
				CameraTabReloadSources = new("tab.camera.reload_sources", "Reload Sources"),
				
				AdvancedTabLabel = new("tab.advanced.label", "Advanced"),
				AdvancedTabDividerLanguage = new("tab.advanced.divider.language", "Language"),
				AdvancedTabDividerVmc = new("tab.advanced.divider.vmc", "VMC Settings"),
				AdvancedTabDividerTracking = new("tab.advanced.divider.tracking", "Tracking Settings"),
				AdvancedTabDividerUI = new("tab.advanced.divider.ui", "UI Settings"),
				AdvancedTabDividerExperimental = new("tab.advanced.divider.experimental", "Experimental Settings"),
				AdvancedTabLanguage = new("tab.advanced.language", "Language"),
				AdvancedTabVmcSender = new("tab.advanced.vmc_sender", "VMC Sender"),
				AdvancedTabVmcReceiverPort = new("tab.advanced.vmc_receiver_port", "VMC Receiver Port"),
				AdvancedTabHandArea = new("tab.advanced.hand_area", "Hand Area"),
				AdvancedTabHandAreaSelectArea = new("tab.advanced.hand_area.select_area", "Select Area"),
				AdvancedTabHandAreaClose = new("tab.advanced.hand_area.close", "Close"),
				AdvancedTabAlwaysShowUI = new("tab.advanced.always_show_ui", "Always show UI"),
				AdvancedTabAntiAliasing = new("tab.advanced.antialiasing", "Anti Aliasing"),
				AdvancedTabShowWebcam = new("tab.advanced.show_webcam", "Show Webcam"),
				AdvancedTabShowModel = new("tab.advanced.show_model", "Show Model"),
				AdvancedTabGuiScale = new("tab.advanced.gui_scale", "Gui Scale"),
				AdvancedTabFlag = new("tab.advanced.flag", "Flag"),
				AdvancedTabExpLegRotation = new("tab.advanced.leg_rotation", "(E) Leg rotation"),
				AdvancedTabExpHandTreshold = new("tab.advanced.hand_threshold", "(E) Hand threshold"),
				AdvancedTabExpInterpolation = new("tab.advanced.interpolation", "(E) Interpolation"),
				
				BonesTabLabel = new("tab.bones.label", "Bones"),
				BonesTabTextTitle = new("tab.bones.text_title", "Bone Settings"),
				BonesTabTextBody = new("tab.bones.text_body", "Press the toggles to enable/disable joints in the model.\n\nHover over elements to view their name"),
				BonesTabFace = new("tab.bones.face", "Face"),
				BonesTabNeck = new("tab.bones.neck", "Neck"),
				BonesTabLeftArm = new("tab.bones.left_arm", "Left Arm"),
				BonesTabLeftWrist = new("tab.bones.left_wrist", "Left Wrist"),
				BonesTabLeftFingers = new("tab.bones.left_fingers", "Left Fingers"),
				BonesTabRightArm = new("tab.bones.right_arm", "Right Arm"),
				BonesTabRightWrist = new("tab.bones.right_wrist", "Right Wrist"),
				BonesTabRightFingers = new("tab.bones.right_fingers", "Right Fingers"),
				BonesTabChest = new("tab.bones.chest", "Chest"),
				BonesTabHips = new("tab.bones.hips", "Hips"),
				BonesTabLeftLeg = new("tab.bones.left_leg", "Left Leg"),
				BonesTabLeftAnkle = new("tab.bones.left_ankle", "Left Ankle"),
				BonesTabRightLeg = new("tab.bones.right_leg", "Right Leg"),
				BonesTabRightAnkle = new("tab.bones.right_ankle", "Right Ankle"),

				LoggerTabLabel = new("tab.logger.label", "Logger"),
				LoggerTabOpenLogs = new("tab.logger.open_logs", "Open Logs"),

				WarningDialogCancel = new("ui.warning.cancel", "Cancel"),
				WarningDialogContinue = new("ui.warning.continue", "Continue"),
				WarningDialogLargeModelSize = new("ui.warning.large_model", "Model size is above {0}, loading this model could take a while")

		;
		
		public static Lang FromId(string id) {
			if (!_localizationMap.TryGetValue(id, out var data)) {
				throw new Exception("Invalid localization id '" + id + "'");
			}

			return data;
		}

		public static Lang Extend(Lang text, Func<Lang, string> func) {
			return new LangExtend(text, func);
		}

		public static Lang Format(Lang text, params object[] args) {
			return new LangFormated(text, args);
		}

		public static List<Lang> Elements => _localizationList;

		public readonly string id;
		public readonly string fallback;
		public Lang(string id, string fallback) {
			this.id = id;
			this.fallback = fallback;

			if (_localizationMap.ContainsKey(id)) {
				Debug.LogErrorFormat("localization id '{0}' has already been defined", id);
			}
			_localizationMap.Add(id, this);
			_localizationList.Add(this);
		}

		protected Lang(Lang parent) {
			id = parent.id;
			fallback = parent.fallback;
		}

		/// <summary>
		/// Returns the string if this localization
		/// </summary>
		public virtual string Get() {
			return Localization.Instance.Get(id, fallback);
		}

		class LangExtend : Lang {
			private readonly Func<Lang, string> func;
			private readonly Lang parent;

			public LangExtend(Lang parent, Func<Lang, string> func) : base(parent) {
				this.func = func;
				this.parent = parent;
			}

			public override string Get() {
				return func.Invoke(parent);
			}
		}

		class LangFormated : Lang {
			private readonly Lang parent;
			private readonly object[] args;

			public LangFormated(Lang parent, params object[] args) : base(parent) {
				this.args = args;
				this.parent = parent;
			}

			public override string Get() {
				return String.Format(parent.Get(), args);
			}
		}
	}
}
