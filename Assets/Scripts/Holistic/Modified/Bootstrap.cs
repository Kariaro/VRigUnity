using Mediapipe;
using Mediapipe.Unity;
using System;
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

		void Awake() {
			// Set target frame rate	
			Application.targetFrameRate = 60;

			// Log application version
			Logger.Info(_TAG, $"Application version: {Application.version}");

			// Init settings
			Settings.Init();

			// Init quality settings
			QualitySettings.antiAliasing = SettingsUtil.GetQualityValue(Settings.AntiAliasing);

			// Init tracking box
			FindObjectOfType<TrackingResizableBox>(true).Init();

			// Init language
			try {
				Localization.SetLanguage(LanguageLoader.FromCode(Settings.Language));
			} catch {
				Logger.Warning("Failed to apply previously loaded language. Swiching to default");
				Settings.Language = Settings._Language.Default();
			}
		}

		private IEnumerator Init() {
			// Initialize mediapipe
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
#if UNITY_ANDROID || UNITY_IOS
			InferenceMode = InferenceMode.GPU;
#else
			InferenceMode = InferenceMode.CPU;
#endif
		}

		private void OnApplicationQuit() {
			Protobuf.ResetLogHandler();
		}
	}
}
