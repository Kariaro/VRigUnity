using UnityEngine;

namespace HardCoded.VRigUnity {
	public class SettingsFieldUtil {
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
	}
}
