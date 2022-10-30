using UnityEngine;

namespace HardCoded.VRigUnity {
	public class HandPoints {
		public static void ComputeFinger(Quaternion rel, float mul, Vector3 vMcp, Vector3 vPip, Vector3 vDip, Vector3 vTip, out Quaternion pip, out Quaternion dip, out Quaternion tip) {
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
		}

		public static void ComputeThumb(Quaternion rel, float mul, Vector3 vMcp, Vector3 vPip, Vector3 vDip, Vector3 vTip, out Quaternion pip, out Quaternion dip, out Quaternion tip) {
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

			if (mul < 0) {
				cPip.y = Mathf.Clamp(cPip.y, -2, 15) * 2;
			} else {
				cPip.y = Mathf.Clamp(cPip.y - 10, -15, 10) * 2;
			}
			cDip.y = cPip.y;
			cTip.y = cPip.y;

			pip = Quaternion.Euler(cPip);
			dip = Quaternion.Euler(cDip);
			tip = Quaternion.Euler(cTip);
		}
	}
}

