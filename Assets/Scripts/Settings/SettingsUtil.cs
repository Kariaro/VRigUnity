using System.Text.RegularExpressions;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class SettingsUtil {
		public static bool IsValidIpAddress(string ip) {
			string[] parts = ip.Split('.');
			if (parts.Length != 4) {
				return false;
			}

			for (int i = 0; i < 4; i++) {
				if (!int.TryParse(parts[i], out int value)) {
					return false;
				}

				if (value < 0 || value > 255) {
					return false;
				}
			}

			return true;
		}

		public static string NormalizeIpAddress(string ip, string defaultIp) {
			if (!IsValidIpAddress(ip)) {
				return defaultIp;
			}

			string[] parts = ip.Split('.');
			return int.Parse(parts[0]) + "." + int.Parse(parts[1]) + "." + int.Parse(parts[2]) + "." + int.Parse(parts[3]);
		}

		public static int GetQualityValue(int index) {
			return index switch {
				3 => 8,
				2 => 4,
				1 => 2,
				_ => 0
			};
		}

		public static float GetUIScaleValue(int value) {
			return 1 + (value - 1) / 9.0f;
		}

		public static ResolutionStruct GetResolution(string text, int width = 640, int height = 360, int frameRate = 30) {
			string[] parts = Regex.Split(text, "[^0-9]+");

			if (parts.Length >= 2) {
				int.TryParse(parts[0], out width);
				int.TryParse(parts[1], out height);
			}

			if (parts.Length >= 3) {
				int.TryParse(parts[2], out frameRate);
			}

			ResolutionStruct r = new(width, height, frameRate);
			return r;
		}

		public static string GetResizableBox(Vector2 offset, Vector2 size) {
			int x = (int)(offset.x * 10000);
			int y = (int)(offset.y * 10000);
			int z = (int)(size.x * 10000);
			int w = (int)(size.y * 10000);
			return $"{x},{y},{z},{w}";
		}

		public static Vector4 GetResizableBox(string text) {
			string[] parts = text.Split(",");
			if (parts.Length != 4) {
				return new();
			}

			int.TryParse(parts[0], out int x);
			int.TryParse(parts[1], out int y);
			int.TryParse(parts[2], out int z);
			int.TryParse(parts[3], out int w);
			return new Vector4(x, y, z, w) / 10000.0f;
		}
	}
}
