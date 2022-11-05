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

			// UI
			Logger.Info(_TAG, $"ModelFile '{ModelFile}'");
			Logger.Info(_TAG, $"ImageFile '{ImageFile}'");
			Logger.Info(_TAG, $"ShowCustomBackground '{ShowCustomBackground}'");
			Logger.Info(_TAG, $"ShowCustomBackgroundColor '{ShowCustomBackgroundColor}'");

			// Features
			Logger.Info(_TAG, $"VMCPort '{VMCPort}'");
		}

		// Camera Settings
		public static readonly string _CameraName = "camera.name";
		public static readonly string _CameraFlipped = "camera.flipped";
		public static readonly string _CameraResolution = "camera.resolution";

		public static string CameraName {
			get => Data.GetString(_CameraName, "");
			set => Data.SetString(_CameraName, value);
		}

		public static bool CameraFlipped {
			get => Data.GetBool(_CameraFlipped, false);
			set => Data.SetBool(_CameraFlipped, value);
		}

		public static string CameraResolution {
			get => Data.GetString(_CameraResolution, "");
			set => Data.SetString(_CameraResolution, value);
		}

		// Gui Settings
		public static readonly string _ModelFile = "gui.model";
		public static readonly string _ImageFile = "gui.image";
		public static readonly string _ShowCustomBackground = "gui.show.custombackground";
		public static readonly string _ShowBgColor = "gui.show.bgColor";
		public static readonly string _VMCPort = "vmc.port";

		public static string ModelFile {
			get => Data.GetString(_ModelFile, "");
			set => Data.SetString(_ModelFile, value);
		}

		public static string ImageFile {
			get => Data.GetString(_ImageFile, "");
			set => Data.SetString(_ImageFile, value);
		}

		public static bool ShowCustomBackground {
			get => Data.GetBool(_ShowCustomBackground, false);
			set => Data.SetBool(_ShowCustomBackground, value);
		}

		public static bool ShowCustomBackgroundColor {
			get => Data.GetBool(_ShowBgColor, false);
			set => Data.SetBool(_ShowBgColor, value);
		}

		public static int VMCPort {
			get => Data.GetInt(_VMCPort, 3333);
			set {
				Data.SetInt(_VMCPort, value);
				VMCPortListener?.Invoke(value);
			}
		}

		public delegate void VMCPortDelegate (int value);
		public static event VMCPortDelegate VMCPortListener;

		// Reset
		public static void ResetSettings() {
			PlayerPrefs.DeleteAll();
		}

		public static class Data {
			public static void SetInt(string key, int value) {
				PlayerPrefs.SetInt(key, value);
			}
			
			public static int GetInt(string key, int def) {
				return PlayerPrefs.GetInt(key, def);
			}

			public static void SetString(string key, string value) {
				PlayerPrefs.SetString(key, value);
			}
			
			public static string GetString(string key, string def) {
				return PlayerPrefs.GetString(key, def);
			}

			public static void SetBool(string key, bool value) {
				PlayerPrefs.SetInt(key, value ? 1 : 0);
			}
			
			public static bool GetBool(string key, bool def) {
				return PlayerPrefs.GetInt(key, def ? 1 : 0) != 0;
			}

			public static void SetVector3(string key, Vector3 value) {
				PlayerPrefs.SetFloat($"{key}.x", value.x);
				PlayerPrefs.SetFloat($"{key}.y", value.y);
				PlayerPrefs.SetFloat($"{key}.z", value.z);
			}

			public static Vector3 GetVector3(string key, Vector3 def) {
				return new Vector3(
					PlayerPrefs.GetFloat($"{key}.x", def.x),
					PlayerPrefs.GetFloat($"{key}.y", def.y),
					PlayerPrefs.GetFloat($"{key}.z", def.z)
				);
			}
		}
	}
}
