using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class MovementUtils {
		public static float NormalizeAngle(float angle) {
			float newAngle = ((angle % 360.0f) + 360.0f) % 360.0f;
			return newAngle > 180.0f ? (newAngle - 360.0f) : newAngle;
		}
	}
}

