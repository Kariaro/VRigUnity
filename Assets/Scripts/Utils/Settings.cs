using System;
using UnityEngine;
using static HardCoded.VRigUnity.SettingsTypes;

namespace HardCoded.VRigUnity {
	public class Settings {
		private const string _TAG = nameof(Settings);

		// This must be called to initialize the system
		public static void Init() {
			foreach (IField field in SettingsTypes.DefinedSettings) {
				field.Init();
				Logger.Info(_TAG, $"{field.Name()} '{field.RawValue()}'");
			}
		}

		// Camera Settings
		public static SettingsTypes.String _CameraName = new("camera.name", "");
		public static SettingsTypes.Bool _CameraFlipped = new("camera.flipped", false);
		public static SettingsTypes.String _CameraResolution = new("camera.resolution", "");

		public static string CameraName {
			get => _CameraName.Get();
			set => _CameraName.Set(value);
		}

		public static bool CameraFlipped {
			get => _CameraFlipped.Get();
			set => _CameraFlipped.Set(value);
		}

		public static string CameraResolution {
			get => _CameraResolution.Get();
			set => _CameraResolution.Set(value);
		}

		// Bone Settings
		public static SettingsTypes.Int _BoneMask = new("bone.mask", BoneSettings.Default);
		public static SettingsTypes.Bool _UseWristRotation = new("bone.use.wristik", false);
		public static SettingsTypes.Bool _UseLegRotation = new("boke.use.legs", false);

		public static int BoneMask {
			get => _BoneMask.Get();
			set => _BoneMask.Set(value);
		}

		public static bool UseWristRotation {
			get => _UseWristRotation.Get();
			set => _UseWristRotation.Set(value);
		}

		public static bool UseLegRotation {
			get => _UseLegRotation.Get();
			set => _UseLegRotation.Set(value);
		}

		// Gui Settings
		public static SettingsTypes.String _ModelFile = new("gui.model", "");
		public static SettingsTypes.String _ImageFile = new("gui.image", "");
		public static SettingsTypes.Bool _ShowCustomBackground = new("gui.show.custombackground", false);
		public static SettingsTypes.Bool _ShowBgColor = new("gui.show.bgColor", false);
		public static SettingsTypes.Int _VMCSenderPort = new("vmc.sender.port", 3333);
		public static SettingsTypes.Int _VMCReceiverPort = new("vmc.receiver.port", 39539);
		public static SettingsTypes.Bool _AlwaysShowUI = new("gui.alwaysShowUI", false);
		public static SettingsTypes.Int _GuiScale = new("gui.scale", 1);

		public static string ModelFile {
			get => _ModelFile.Get();
			set => _ModelFile.Set(value);
		}

		public static string ImageFile {
			get => _ImageFile.Get();
			set => _ImageFile.Set(value);
		}

		public static bool ShowCustomBackground {
			get => _ShowCustomBackground.Get();
			set => _ShowCustomBackground.Set(value);
		}

		public static bool ShowCustomBackgroundColor {
			get => _ShowBgColor.Get();
			set => _ShowBgColor.Set(value);
		}

		public static int VMCSenderPort {
			get => _VMCSenderPort.Get();
			set {
				_VMCSenderPort.Set(value);
				VMCSenderPortListener?.Invoke(value);
			}
		}

		public static int VMCReceiverPort {
			get => _VMCReceiverPort.Get();
			set {
				_VMCReceiverPort.Set(value);
				VMCReceiverPortListener?.Invoke(value);
			}
		}

		public static bool AlwaysShowUI {
			get => _AlwaysShowUI.Get();
			set => _AlwaysShowUI.Set(value);
		}

		public static int GuiScale {
			get => _GuiScale.Get();
			set {
				_GuiScale.Set(value);
				GuiScaleListener?.Invoke(value);
			}
		}

		public delegate void IntDelegate (int value);
		public static event IntDelegate VMCSenderPortListener;
		public static event IntDelegate VMCReceiverPortListener;
		public static event IntDelegate GuiScaleListener;

		// Reset
		public static void ResetSettings() {
			PlayerPrefs.DeleteAll();
		}
	}
}
