using Mediapipe.Unity;
using System;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class SolutionUtils {
		public static HolisticTrackingSolution GetSolution() {
			GameObject gameObject = GameObject.FindGameObjectWithTag("Solution");
			if (gameObject == null) {
				return null;
			}

			return gameObject.GetComponent<HolisticTrackingSolution>();
		}

		public static AssetManager GetAssetManager() {
			return GetBootstrap().GetAssetManager();
		}

		public static Bootstrap GetBootstrap() {
			return GetSolution().GetComponent<Bootstrap>();
		}

		public static WebCamSource GetImageSource() {
			return GetSolution().GetComponent<WebCamSource>();
		}
	}
}
