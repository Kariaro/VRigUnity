using System;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class Settings {
		private const string _TAG = nameof(Settings);
			
		// Debug function to log all set values
		public static void LogAll() {
			// Camera
			Logger.Info(_TAG, $"CameraName '{CameraName}'");
			Logger.Info(_TAG, $"CameraFlipped '{CameraFlipped}'");
			Logger.Info(_TAG, $"CameraResolution '{CameraResolution}'");

			// Bones
			Logger.Info(_TAG, $"Bones '{BoneMask}'");

			// UI
			Logger.Info(_TAG, $"ModelFile '{ModelFile}'");
			Logger.Info(_TAG, $"ImageFile '{ImageFile}'");
			Logger.Info(_TAG, $"ShowCustomBackground '{ShowCustomBackground}'");
			Logger.Info(_TAG, $"ShowCustomBackgroundColor '{ShowCustomBackgroundColor}'");

			// Features
			Logger.Info(_TAG, $"VMCSenderPort '{VMCSenderPort}'");
			Logger.Info(_TAG, $"VMCReceiverPort '{VMCReceiverPort}'");
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

		public static int BoneMask {
			get => _BoneMask.Get();
			set => _BoneMask.Set(value);
		}

		// Gui Settings
		public static SettingsTypes.String _ModelFile = new("gui.model", "");
		public static SettingsTypes.String _ImageFile = new("gui.image", "");
		public static SettingsTypes.Bool _ShowCustomBackground = new("gui.show.custombackground", false);
		public static SettingsTypes.Bool _ShowBgColor = new("gui.show.bgColor", false);
		public static SettingsTypes.Int _VMCSenderPort = new("vmc.sender.port", 3333);
		public static SettingsTypes.Int _VMCReceiverPort = new("vmc.receiver.port", 39539);

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

		public delegate void VMCPortDelegate (int value);
		public static event VMCPortDelegate VMCSenderPortListener;
		public static event VMCPortDelegate VMCReceiverPortListener;

		// Reset
		public static void ResetSettings() {
			PlayerPrefs.DeleteAll();
		}
	}
}
