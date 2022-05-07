using Mediapipe.Unity;
using System;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class SolutionUtils {
		public static TestSolution GetSolution() {
			GameObject gameObject = GameObject.FindGameObjectWithTag("Solution");
			if (gameObject == null) {
				return null;
			}

			return gameObject.GetComponent<TestSolution>();
		}

		public static TestBootstrap GetBootstrap() {
			return GetSolution().GetComponent<TestBootstrap>();
		}
	}
}