using HardCoded.VRigUnity.SettingsTypes;
using System;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class Settings {
		private const string _TAG = nameof(Settings);

		// This must be called to initialize the system
		public static void Init() {
			foreach (IField field in IField.DefinedSettings) {
				field.Init();
				Logger.Info(_TAG, $"{field.Name()} '{field.RawValue()}'");
			}
		}

		public static class Temporary {
			public static bool VirtualCamera = false;
		}

		// Language Settings
		public static Text _Language = new("language", "en_US");

		public static string Language {
			get => _Language.Get();
			set => _Language.Set(value);
		}

		// Camera Settings
		public static Text _CameraName = new("camera.name", "");
		public static Bool _CameraFlipped = new("camera.flipped", false);
		public static Text _CameraResolution = new("camera.resolution", "");
		public static Bool _CameraCustomResolution = new("camera.resolution.custom", false);

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

		public static bool CameraCustomResolution {
			get => _CameraCustomResolution.Get();
			set => _CameraCustomResolution.Set(value);
		}

		// Tracking Settings
		public static Text _TrackingBox = new("tracking.box", "0,0,5000,5000");
		public static Bool _TrackingBoxEnabled = new("tracking.box.enable", false);

		public static string TrackingBox {
			get => _TrackingBox.Get();
			set => _TrackingBox.Set(value);
		}
		
		public static bool UseTrackingBox {
			get => _TrackingBoxEnabled.Get();
			set => _TrackingBoxEnabled.Set(value);
		}

		// Bone Settings
		public static Int _BoneMask = new("bone.mask", BoneSettings.Default);
		public static Bool _UseLegRotation = new("bone.use.legs", false);
		public static Float _HandTrackingThreshold = new("tracking.threshold.hand", 0f);
		public static Float _TrackingInterpolation = new("tracking.interpolation", 0.1f);

		public static int BoneMask {
			get => _BoneMask.Get();
			set => _BoneMask.Set(value);
		}

		public static bool UseLegRotation {
			get => _UseLegRotation.Get();
			set => _UseLegRotation.Set(value);
		}

		public static float HandTrackingThreshold {
			get => _HandTrackingThreshold.Get();
			set => _HandTrackingThreshold.Set(value);
		}

		public static float TrackingInterpolation {
			get => _TrackingInterpolation.Get();
			set => _TrackingInterpolation.Set(value);
		}

		// Gui Settings
		public static Text _ModelFile = new("gui.model", "");
		public static Text _ImageFile = new("gui.image", "");
		public static Bool _ShowCustomBackground = new("gui.show.custombackground", false);
		public static Bool _ShowBgColor = new("gui.show.bgColor", false);
		public static Bool _AlwaysShowUI = new("gui.alwaysShowUI", false);
		public static Int _GuiScale = new("gui.scale", 1, value => GuiScaleListener?.Invoke(value));
		public static SafeEnumOf<FlagScript.Flag> _Flag = new("gui.flag", FlagScript.Flag.None);
		public static Int _AntiAliasing = new("view.antialiasing", 0);
		public static Bool _ShowModel = new("view.showmodel", true);

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

		public static bool AlwaysShowUI {
			get => _AlwaysShowUI.Get();
			set => _AlwaysShowUI.Set(value);
		}

		public static int GuiScale {
			get => _GuiScale.Get();
			set => _GuiScale.Set(value);
		}

		public static FlagScript.Flag Flag {
			get => _Flag.Get();
			set => _Flag.Set(value);
		}

		public static int AntiAliasing {
			get => _AntiAliasing.Get();
			set => _AntiAliasing.Set(value);
		}

		public static bool ShowModel {
			get => _ShowModel.Get();
			set => _ShowModel.Set(value);
		}

		public delegate void IntDelegate (int value);
		public static event IntDelegate GuiScaleListener;

		// VMC Settings
		public static SafeText _VMCSenderAddress = new("vmc.sender.ip", "127.0.0.1", value => VMCSenderListener?.Invoke(value, VMCSenderPort));
		public static Int _VMCSenderPort = new("vmc.sender.port", 3333, value => VMCSenderListener?.Invoke(VMCSenderAddress, value));
		public static Int _VMCReceiverPort = new("vmc.receiver.port", 39539, value => VMCReceiverListener?.Invoke(null, value));

		public static string VMCSenderAddress {
			get => _VMCSenderAddress.Get();
			set => _VMCSenderAddress.Set(value);
		}

		public static int VMCSenderPort {
			get => _VMCSenderPort.Get();
			set => _VMCSenderPort.Set(value);
		}

		public static int VMCReceiverPort {
			get => _VMCReceiverPort.Get();
			set => _VMCReceiverPort.Set(value);
		}
		
		public delegate void IpDelegate (string ip, int port);
		public static event IpDelegate VMCSenderListener;
		public static event IpDelegate VMCReceiverListener;

		// Reset
		public static void ResetSettings() {
			PlayerPrefs.DeleteAll();
		}
	}
}
