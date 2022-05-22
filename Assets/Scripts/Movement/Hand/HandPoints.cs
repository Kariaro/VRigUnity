using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class HandPoints {
		public static void ComputeFinger(Quaternion rel, Vector3 vMcp, float mul, Vector3 vPip, Vector3 vDip, Vector3 vTip, out Quaternion pip, out Quaternion dip, out Quaternion tip) {
			Vector3 rMcp = rel * (vMcp - vMcp);
			Vector3 rPip = rel * (vPip - vMcp) - rMcp;
			Vector3 rDip = rel * (vDip - vMcp) - rMcp;
			Vector3 rTip = rel * (vTip - vMcp) - rMcp;

			rPip.z = 0.03f;
			rDip.z = 0.06f;
			rTip.z = 0.09f;

			// No x
			rDip.x = rTip.x = 0;

			Vector3 cPip = Quaternion.FromToRotation(Vector3.up, rPip).eulerAngles;
			Vector3 cDip = Quaternion.FromToRotation(rPip, rDip - rPip).eulerAngles;
			Vector3 cTip = Quaternion.FromToRotation(rDip, rTip - rDip).eulerAngles;
			cPip.x = cDip.x = cTip.x = 0; // No yaw
			cDip.y = cTip.y = 0; // No roll
			cPip.y = -cPip.y * mul;
			
			cPip.z = MovementUtils.NormalizeAngle(cPip.z);
			cDip.z = MovementUtils.NormalizeAngle(cDip.z);
			cTip.z = MovementUtils.NormalizeAngle(cTip.z);
			cPip.z = Mathf.Abs(cPip.z);
			cDip.z = Mathf.Abs(cDip.z);
			cTip.z = Mathf.Abs(cTip.z);
			cPip.z = Mathf.Clamp(cPip.z, 0, 120) * mul;
			cDip.z = Mathf.Clamp(cDip.z, 0, 120) * mul;
			cTip.z = Mathf.Clamp(cTip.z, 0, 120) * mul;

			pip = Quaternion.Euler(cPip);
			dip = Quaternion.Euler(cDip);
			tip = Quaternion.Euler(cTip);
		}

		// public static void ComputeFinger2(Quaternion rel, float mul, int id, Vector3 vMcp, Vector3 vPip, Vector3 vDip, Vector3 vTip, out Quaternion pip, out Quaternion dip, out Quaternion tip) {
		// 	ComputeFinger2(rel, mul, id, vMcp, vPip, vDip, vTip, null, out pip, out dip, out tip);
		// }
		
		public static void ComputeFinger2(Quaternion rel, float mul, int id, Vector3 vMcp, Vector3 vPip, Vector3 vDip, Vector3 vTip, Vector3[] _debug_vectors, out Quaternion pip, out Quaternion dip, out Quaternion tip) {
			Vector3 rMcp = rel * (vMcp - vMcp);
			Vector3 rPip = rel * (vPip - vMcp) - rMcp;
			Vector3 rDip = rel * (vDip - vMcp) - rMcp;
			Vector3 rTip = rel * (vTip - vMcp) - rMcp;

			// No x
			rDip.x = rTip.x = rPip.x;

			Vector3 cPip = Quaternion.FromToRotation(Vector3.up, rPip).eulerAngles;
			Vector3 cDip = Quaternion.FromToRotation(rPip, rDip - rPip).eulerAngles;
			Vector3 cTip = Quaternion.FromToRotation(rDip, rTip - rDip).eulerAngles;
			cPip.x = cDip.x = cTip.x = 0; // No yaw
			cDip.y = cTip.y = 0; // No roll
			cPip.y = -cPip.y * mul;
			
			cPip.z = MovementUtils.NormalizeAngle(cPip.z);
			cDip.z = MovementUtils.NormalizeAngle(cDip.z);
			cTip.z = MovementUtils.NormalizeAngle(cTip.z);
			cPip.z = Mathf.Abs(cPip.z);
			cDip.z = Mathf.Abs(cDip.z);
			cTip.z = Mathf.Abs(cTip.z);
			cPip.z = Mathf.Clamp(cPip.z, 0, 120) * mul;
			cDip.z = Mathf.Clamp(cDip.z, 0, 120) * mul;
			cTip.z = Mathf.Clamp(cTip.z, 0, 120) * mul;

			pip = Quaternion.Euler(cPip);
			dip = Quaternion.Euler(cDip);
			tip = Quaternion.Euler(cTip);
			
			if (_debug_vectors != null) {
				// TODO: Debug
				Vector3 off = new(0.05f * (id + 2), 0, 0);
				_debug_vectors[21 + id * 4] = (Vector3.zero) + off;
				_debug_vectors[22 + id * 4] = (rPip) + off;
				_debug_vectors[23 + id * 4] = (rDip) + off;
				_debug_vectors[24 + id * 4] = (rTip) + off;
			}
		}

		// public static void ComputeThumb(Quaternion rel, float mul, int id, Vector3 vMcp, Vector3 vPip, Vector3 vDip, Vector3 vTip, out Quaternion pip, out Quaternion dip, out Quaternion tip) {
		// 	ComputeThumb(rel, mul, id, vMcp, vPip, vDip, vTip, null, out pip, out dip, out tip);
		// }

		public static void ComputeThumb(Quaternion rel, float mul, int id, Vector3 vMcp, Vector3 vPip, Vector3 vDip, Vector3 vTip, Vector3[] _debug_vectors, out Quaternion pip, out Quaternion dip, out Quaternion tip) {
			Vector3 rMcp = rel * (vMcp - vMcp);
			Vector3 rPip = rel * (vPip - vMcp) - rMcp;
			Vector3 rDip = rel * (vDip - vMcp) - rMcp;
			Vector3 rTip = rel * (vTip - vMcp) - rMcp;
			
			Vector3 thumbVectorDefault = new(0.5f, 0, 0.5f * -mul);
			Vector3 cPip = Quaternion.FromToRotation(thumbVectorDefault, rPip).eulerAngles;
			Vector3 cDip = Quaternion.FromToRotation(rPip, rDip - rPip).eulerAngles;
			Vector3 cTip = Quaternion.FromToRotation(rDip, rTip - rDip).eulerAngles;
			cDip = Vector3.zero;
			cTip = Vector3.zero;
			
			cPip.x = cPip.z = 0;
			cPip.y *= mul;
			cPip.y = MovementUtils.NormalizeAngle(cPip.y);

			if (mul > 0) {
				cPip.y *= -1;
			}

			// Debug.Log(cPip.y);
			if (mul < 0) {
				cPip.y = Mathf.Clamp(cPip.y, -2, 15) * 2;
			} else {
				cPip.y = Mathf.Clamp(cPip.y - 10, -15, 10) * 2;
			}
			cDip.y = cPip.y;
			cTip.y = cPip.y;
			// TODO: Calculate this value
			//cPip = Quaternion.FromToRotation(thumbVectorDefault, rPip);
			//if (cPip.y < -10) cPip.y = -10;
			//cDip.y = cPip.y + 5;
			//cTip.y = cPip.y + 5;

			/*cDip.x = cDip.z = 0;
			cDip.y *= mul;

			cTip.x = cTip.z = 0;
			cTip.y *= mul;
			*/

			//cDip.x = cTip.x = 0;
			//cDip.z = cTip.z = 0;

			//float pipX = cPip.x;
			//float pipY = cPip.y;
			//cPip.x = 0;
			//cPip.y = pipX * 3;
			//cPip.z = 0;

			//cPip.x = cDip.x = cTip.x = 0; // No yaw
			//cDip.y = cTip.y = 0; // No roll
			
			/*
			cPip.z = Mathf.Abs(cPip.z);
			cDip.z = Mathf.Abs(cDip.z);
			cTip.z = Mathf.Abs(cTip.z);
			cPip.z = Mathf.Clamp(cPip.z, 0, 120) * mul;
			cDip.z = Mathf.Clamp(cDip.z, 0, 120) * mul;
			cTip.z = Mathf.Clamp(cTip.z, 0, 120) * mul;
			*/

			pip = Quaternion.Euler(cPip);
			dip = Quaternion.Euler(cDip);
			tip = Quaternion.Euler(cTip);
			
			if (_debug_vectors != null) {
				// TODO: Debug
				Vector3 off = new(0.05f * (id + 2), 0, 0);
				_debug_vectors[21 + id * 4] = (Vector3.zero) + off;
				_debug_vectors[22 + id * 4] = (rPip) + off;
				_debug_vectors[23 + id * 4] = (rDip) + off;
				_debug_vectors[24 + id * 4] = (rTip) + off;
			}
		}
	}
}

