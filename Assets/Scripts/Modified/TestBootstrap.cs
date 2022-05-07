// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe;
using Mediapipe.Unity;
using System.Collections;
using System.IO;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class TestBootstrap : MonoBehaviour {
		[System.Serializable]
		public enum AssetLoaderType {
			StreamingAssets,
			AssetBundle,
			Local,
		}

		private const string _TAG = nameof(TestBootstrap);

		[SerializeField] private InferenceMode _preferableInferenceMode;
		[SerializeField] private AssetLoaderType _assetLoaderType;
		[SerializeField] private bool _enableGlog = true;

		public InferenceMode inferenceMode { get; private set; }
		public bool IsFinished { get; private set; }
		private bool _isGlogInitialized;

		private void OnEnable() {
			var _ = StartCoroutine(Init());
		}

		private IEnumerator Init() {
			Mediapipe.Unity.Logger.SetLogger(new MemoizedLogger(100));
			Mediapipe.Unity.Logger.minLogLevel = Mediapipe.Unity.Logger.LogLevel.Debug;

			Protobuf.SetLogHandler(Protobuf.DefaultLogHandler);

			Mediapipe.Unity.Logger.LogInfo(_TAG, "Setting global flags...");
			GlobalConfigManager.SetFlags();

			if (_enableGlog) {
				if (Glog.LogDir != null) {
					if (!Directory.Exists(Glog.LogDir)) {
						Directory.CreateDirectory(Glog.LogDir);
					}
					Mediapipe.Unity.Logger.LogVerbose(_TAG, $"Glog will output files under {Glog.LogDir}");
				}
				Glog.Initialize("MediaPipeUnityPlugin");
				_isGlogInitialized = true;
			}

			Mediapipe.Unity.Logger.LogInfo(_TAG, "Initializing AssetLoader...");
			switch (_assetLoaderType) {
				case AssetLoaderType.AssetBundle: {
					AssetLoader.Provide(new AssetBundleResourceManager("mediapipe"));
					break;
				}
				case AssetLoaderType.StreamingAssets: {
					AssetLoader.Provide(new StreamingAssetsResourceManager());
					break;
				}
				case AssetLoaderType.Local: {
#if UNITY_EDITOR
					AssetLoader.Provide(new LocalResourceManager());
					break;
#else
					Mediapipe.Unity.Logger.LogError("LocalResourceManager is only supported on UnityEditor");
					yield break;
#endif
				}
				default: {
					Mediapipe.Unity.Logger.LogError($"AssetLoaderType is unknown: {_assetLoaderType}");
					yield break;
				}
			}

			DecideInferenceMode();
			if (inferenceMode == InferenceMode.GPU) {
				Mediapipe.Unity.Logger.LogInfo(_TAG, "Initializing GPU resources...");
				yield return GpuManager.Initialize();
			}

			Mediapipe.Unity.Logger.LogInfo(_TAG, "Preparing ImageSource...");
			ImageSourceProvider.ImageSource = GetImageSource();

			IsFinished = true;
		}

		public ImageSource GetImageSource() {
			return GetComponent<WebCamSource>();
		}

		private void DecideInferenceMode() {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
			if (_preferableInferenceMode == InferenceMode.GPU) {
				Mediapipe.Unity.Logger.LogWarning(_TAG, "Current platform does not support GPU inference mode, so falling back to CPU mode");
			}
			inferenceMode = InferenceMode.CPU;
#else
			inferenceMode = _preferableInferenceMode;
#endif
		}

		private void OnApplicationQuit() {
			GpuManager.Shutdown();

			if (_isGlogInitialized) {
				Glog.Shutdown();
			}

			Protobuf.ResetLogHandler();
			Mediapipe.Unity.Logger.SetLogger(null);
		}
	}
}
