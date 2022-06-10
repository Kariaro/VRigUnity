using Mediapipe.Unity;
using System;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class SolutionUtils {
		private static HolisticTrackingSolution _solution;
		private static AssetManager _assetManager;
		private static WebCamSource _webCamSource;
		private static Bootstrap _bootstrap;

		public static HolisticTrackingSolution GetSolution() {
			if (_solution == null) {
				GameObject gameObject = GameObject.FindGameObjectWithTag("Solution");
				if (gameObject == null) {
					return null;
				}

				_solution = gameObject.GetComponent<HolisticTrackingSolution>();
			}

			return _solution;
		}
		
		public static Bootstrap GetBootstrap() {
			if (_bootstrap == null) {
				_bootstrap = GetSolution().GetComponent<Bootstrap>();
			}

			return _bootstrap;
		}

		public static WebCamSource GetImageSource() {
			if (_webCamSource == null) {
				_webCamSource = GetSolution().GetComponent<WebCamSource>();
			}

			return _webCamSource;
		}

		public static AssetManager GetAssetManager() {
			if (_assetManager == null) {
				_assetManager = GetBootstrap().GetAssetManager();
			}

			return _assetManager;
		}
	}
}
