using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class MovementUtils {
		public static float NormalizeAngle(float angle) {
			float newAngle = ((angle % 360.0f) + 360.0f) % 360.0f;
			return newAngle > 180.0f ? (newAngle - 360.0f) : newAngle;
		}

		public static float GetTriangleArea(Vector3 A, Vector3 B, Vector3 C) {
			Vector3 AB = new(B.x - A.x, B.y - A.y, B.z - A.z);
			Vector3 AC = new(C.x - B.x, C.y - A.y, C.z - A.z);

			float P1 = (AB.y * AC.z - AB.z * AC.y);
			float P2 = (AB.z * AC.x - AB.x * AC.z);
			float P3 = (AB.x * AC.y - AB.y * AC.x);
			return 0.5f * Mathf.Sqrt(P1 * P1 + P2 * P2 + P3 * P3);
		}

		public static float GetArmWristAngle(Vector3 a_pos, Quaternion a_rot, Vector3 w_pos, Quaternion w_rot) {
			Vector3 uu = Vector3.up * 0.1f;
			Vector3 ff = Vector3.forward * 0.1f;
			Vector3 rr = Vector3.right * 0.1f;

			if (ThreadObject.IsUnityThread()) {
				Debug.DrawRay(a_pos, a_rot * uu, Color.green);
				Debug.DrawRay(a_pos, a_rot * ff, Color.blue);
				Debug.DrawRay(a_pos, a_rot * rr, Color.red);
				
				Debug.DrawRay(w_pos, w_rot * -uu, Color.green);
				Debug.DrawRay(w_pos, w_rot * -rr, Color.red);
			}

			// The green rays should point approximately in the same direction
			Vector3 w_for = w_rot * -uu;
			Vector3 a_for = a_rot * -uu;
			
			if (ThreadObject.IsUnityThread()) {
				Debug.DrawLine(w_pos, w_pos + w_for);
				Debug.DrawLine(a_pos, a_pos + a_for);
			}

			// Rotate a point around the axis r_arot * rr
			Vector3 wristPos = w_pos + w_for;
			Vector3 closestPos = a_pos + a_for;
			float closest = Vector3.Distance(closestPos, wristPos);
			float closestRot = 180;
			for (int i = 0; i < 100; i++) {
				Vector3 point = new(0, Mathf.Cos(Mathf.Deg2Rad * i * 3.6f), Mathf.Sin(Mathf.Deg2Rad * i * 3.6f));
				Vector3 s_point = a_pos + a_rot * point * 0.1f;

				float dst = Vector3.Distance(s_point, wristPos);
				if (dst < closest) {
					closest = dst;
					closestPos = s_point;
					closestRot = i * 3.6f;
					
					if (ThreadObject.IsUnityThread()) {
						Debug.DrawLine(a_pos, s_point, Color.cyan);
					}
				}
			}

			if (ThreadObject.IsUnityThread()) {
				Debug.DrawLine(closestPos, wristPos, Color.yellow);
			}

			return closestRot - 180.0f;
		}
	}
}

