using SFB;
using System.IO;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class Settings {
		public static readonly string ModelPath = "gui.modelpath";
		public static readonly string ImagePath = "gui.imagepath";
		public static readonly string ShowImage = "gui.show.imagepath";

		public static string GetModelPath() {
			return PlayerPrefs.GetString(ModelPath, "");
		}

		public static string GetImagePath() {
			return PlayerPrefs.GetString(ModelPath, "");
		}

		public static bool IsShowCustomBackground() {
			return PlayerPrefs.GetInt(ShowImage, 0) != 0;
		}

		public static void SetShowCustomBackground(bool show) {
			PlayerPrefs.SetInt(ShowImage, show ? 1 : 0);
		}

		public static void ResetSettings() {
			PlayerPrefs.DeleteAll();
		}
	}
}
