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
	}
}
