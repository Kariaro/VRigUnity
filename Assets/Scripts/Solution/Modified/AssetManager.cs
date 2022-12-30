using Mediapipe;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class AssetManager : ResourceManager {
		private static readonly string _TAG = nameof(AssetManager);
		
		public override PathResolver pathResolver => PathToResourceAsFile;
		public override ResourceProvider resourceProvider => GetResourceContents;

		private static string _CachePathRoot;
		private static string _AssetPathRoot;

		public AssetManager() : base() {
			_AssetPathRoot = Path.Combine(Application.streamingAssetsPath, "mediapipe");
			_CachePathRoot = Path.Combine(Application.persistentDataPath, "mediapipe");
		}

		// Check if a file is cached
		public override bool IsPrepared(string assetName) {
			return File.Exists(GetCachePathFor(assetName));
		}

		public override IEnumerator PrepareAssetAsync(string name, string uniqueKey, bool overwrite = true) {
			var destFilePath = GetCachePathFor(uniqueKey);
			if (File.Exists(destFilePath) && !overwrite) {
				// Logger.Info(_TAG, $"{name} will not be copied because it already exists. Destination was {destFilePath}");
				yield break;
			}

			var sourceFilePath = GetAssetPathFor(name);
			if (!File.Exists(sourceFilePath)) {
				Logger.Info(_TAG, $"{name} will not be copied because it was not found. Destination was {destFilePath}");
				throw new Exception($"{name} will not be copied because it was not found. Destination was {destFilePath}");
			}

			if (sourceFilePath == destFilePath) {
				yield break;
			}

			if (!Directory.Exists(_CachePathRoot)) {
				Directory.CreateDirectory(_CachePathRoot);
			}

			Logger.Verbose(_TAG, $"Copying {sourceFilePath} to {destFilePath}...");
			File.Copy(sourceFilePath, destFilePath, overwrite);
			Logger.Verbose(_TAG, $"{sourceFilePath} is copied to {destFilePath}");
		}

		protected static string PathToResourceAsFile(string assetPath) {
			var assetName = GetAssetNameFromPath(assetPath);
			return GetCachePathFor(assetName);
		}

		protected static bool GetResourceContents(string path, IntPtr dst) {
			try {
				// Logger.Debug($"{path} is requested");

				var cachePath = PathToResourceAsFile(path);
				if (!File.Exists(cachePath)) {
					Logger.Error(_TAG, $"{cachePath} is not found");
					return false;
				}

				var asset = File.ReadAllBytes(cachePath);
				using (var srcStr = new StdString(asset)) {
					srcStr.Swap(new StdString(dst, false));
				}

				return true;
			} catch (Exception e) {
				Logger.Exception(e);
				return false;
			}
		}

		private static string GetAssetPathFor(string assetName) {
			return Path.Combine(_AssetPathRoot, assetName);
		}

		private static string GetCachePathFor(string assetName) {
			return Path.Combine(_CachePathRoot, assetName);
		}
	}
}
