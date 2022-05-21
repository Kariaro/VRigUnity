using Mediapipe;
using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class TestBootstrap : MonoBehaviour {
		private const string _TAG = nameof(TestBootstrap);
		
		[SerializeField] private InferenceMode _preferableInferenceMode;

		public InferenceMode inferenceMode { get; private set; }
		public bool IsFinished { get; private set; }
		private CustomAssetManager _assetManager;

		private void OnEnable() {
			var _ = StartCoroutine(Init());
		}

		public CustomAssetManager GetAssetManager() {
			return _assetManager;
		}

		private IEnumerator Init() {
			// TODO: Is this logger needed?
			Protobuf.SetLogHandler(Protobuf.DefaultLogHandler);

			// TODO: Remove global config manager?
			Logger.Info(_TAG, "Setting global flags...");
			GlobalConfigManager.SetFlags();
			
			// %appdata%\..\LocalLow\DefaultCompany\VRigUnity
			// https://github.com/mrayy/UnityCam
			Logger.Info(_TAG, "Initializing AssetManager...");
			_assetManager = new CustomAssetManager();
			
			DecideInferenceMode();
			if (inferenceMode == InferenceMode.GPU) {
				Logger.Info(_TAG, "Initializing GPU resources...");
				yield return GpuManager.Initialize();
			}

			Logger.Info(_TAG, "Preparing ImageSource...");
			ImageSourceProvider.ImageSource = GetImageSource();

			IsFinished = true;
		}

		public ImageSource GetImageSource() {
			return GetComponent<WebCamSource>();
		}

		private void DecideInferenceMode() {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
			if (_preferableInferenceMode == InferenceMode.GPU) {
				Logger.Warning(_TAG, "Current platform does not support GPU inference mode, so falling back to CPU mode");
			}

			inferenceMode = InferenceMode.CPU;
#else
			inferenceMode = _preferableInferenceMode;
#endif
		}

		private void OnApplicationQuit() {
			GpuManager.Shutdown();
			Protobuf.ResetLogHandler();
		}
	}
}
