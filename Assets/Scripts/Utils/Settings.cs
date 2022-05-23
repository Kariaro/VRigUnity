using UnityEngine;

namespace HardCoded.VRigUnity {
	public class Settings {
		public static readonly string _ModelFile = "gui.model";
		public static readonly string _ImageFile = "gui.image";
		public static readonly string _ShowCustomBackground = "gui.show.custombackground";

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

		// Reset
		public static void ResetSettings() {
			PlayerPrefs.DeleteAll();
		}

		public static class Data {
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
