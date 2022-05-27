using Mediapipe;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class Bootstrap : MonoBehaviour {
		private const string _TAG = nameof(Bootstrap);
		
		public bool IsFinished { get; private set; }
		private AssetManager _assetManager;

		private void OnEnable() {
			var _ = StartCoroutine(Init());
		}

		public AssetManager GetAssetManager() {
			return _assetManager;
		}

		private IEnumerator Init() {
			// TODO: Is this logger needed?
			Protobuf.SetLogHandler(Protobuf.DefaultLogHandler);
			
			// %appdata%\..\LocalLow\DefaultCompany\VRigUnity
			Logger.Info(_TAG, "Initializing AssetManager...");
			_assetManager = new AssetManager();

			Logger.Info(_TAG, "Preparing ImageSource...");
			SolutionUtils.GetImageSource().enabled = true;

			IsFinished = true;

			// Wait a single frame
			yield return null;
		}

		private void OnApplicationQuit() {
			Protobuf.ResetLogHandler();
		}
	}
}
