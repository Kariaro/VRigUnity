using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class SolutionUtils {
		public static Solution GetSolution() {
			GameObject gameObject = GameObject.FindGameObjectWithTag("Solution");
			if (gameObject == null) {
				return null;
			}

			return gameObject.GetComponent<Solution>();
		}
	}
}