using Mediapipe;
using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class Bootstrap : MonoBehaviour {
		private const string _TAG = nameof(Bootstrap);
		
		public bool IsFinished { get; private set; }
		public InferenceMode InferenceMode { get; private set; }
		private AssetManager _assetManager;

		private void OnEnable() {
			var _ = StartCoroutine(Init());
		}

		public AssetManager GetAssetManager() {
			return _assetManager;
		}

		private IEnumerator Init() {
			// Loading libraries needs to be done first
			Logger.Info(_TAG, "Initializing Libraries...");
			if (!LoadLibrary.TryLoadLibrary()) {
				Logger.Error("Failed to try load library");
			}

			Protobuf.SetLogHandler(Protobuf.DefaultLogHandler);
			
			Logger.Info(_TAG, "Initializing AssetManager...");
			_assetManager = new AssetManager();

			DecideInferenceMode();
			if (InferenceMode == InferenceMode.GPU) {
				Logger.Info(_TAG, "Initializing GPU resources...");
				yield return GpuManager.Initialize();

				if (!GpuManager.IsInitialized) {
					Logger.Warning("If your native library is built for CPU, change 'Preferable Inference Mode' to CPU from the Inspector Window for Bootstrap");
				}
			}

			Logger.Info(_TAG, "Preparing ImageSource...");
			SolutionUtils.GetImageSource().enabled = true;

			IsFinished = true;
		}

		private void DecideInferenceMode() {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
			InferenceMode = InferenceMode.CPU;
#else
			InferenceMode = InferenceMode.GPU;
#endif
		}

		private void OnApplicationQuit() {
			Protobuf.ResetLogHandler();
		}
	}
}
