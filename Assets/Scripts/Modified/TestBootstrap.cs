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
			// TODO: Is this logger needed?
			Protobuf.SetLogHandler(Protobuf.DefaultLogHandler);

			Logger.Info(_TAG, "Setting global flags...");
			GlobalConfigManager.SetFlags();

			if (_enableGlog) {
				if (Glog.LogDir != null) {
					if (!Directory.Exists(Glog.LogDir)) {
						Directory.CreateDirectory(Glog.LogDir);
					}
					Logger.Verbose(_TAG, $"Glog will output files under {Glog.LogDir}");
				}
				Glog.Initialize("MediaPipeUnityPlugin");
				_isGlogInitialized = true;
			}

			Logger.Info(_TAG, "Initializing AssetLoader...");
			// TODO: Create a custom asset manager without MediapipeUnity
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
					Logger.Error("LocalResourceManager is only supported on UnityEditor");
					yield break;
#endif
				}
				default: {
					Logger.Error($"AssetLoaderType is unknown: {_assetLoaderType}");
					yield break;
				}
			}

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

			if (_isGlogInitialized) {
				Glog.Shutdown();
			}

			Protobuf.ResetLogHandler();
		}
	}
}
