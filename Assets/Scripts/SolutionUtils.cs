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

		public static CustomAssetManager GetAssetManager() {
			return GetBootstrap().GetAssetManager();
		}

		public static TestBootstrap GetBootstrap() {
			return GetSolution().GetComponent<TestBootstrap>();
		}
	}
}
