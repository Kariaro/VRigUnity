using TMPro;
using UnityEngine;
using System;
using System.IO;

namespace HardCoded.VRigUnity {

#if UNITY_EDITOR
	[ExecuteAlways]
#endif
	public class ApplicationVersion : MonoBehaviour {
		public TMP_Text text;

		void Start() {
#if UNITY_EDITOR
			if (Application.isPlaying) {
				text.text = Application.version;
			}
#else
			text.text = Application.version;
#endif
		}

#if UNITY_EDITOR
		[NonSerialized]
		private bool checkOnce;

		[NonSerialized]
		private DateTime fileTimestamp;

		void Update() {
			DateTime time = File.GetLastWriteTime(".version");
			if (checkOnce && (time == fileTimestamp)) {
				return;
			}

			fileTimestamp = time;
			checkOnce = true;

			ValidateVersion();
		}

		private void ValidateVersion() {
			var process = new System.Diagnostics.Process {
				StartInfo = new System.Diagnostics.ProcessStartInfo {
					FileName = "git",
					Arguments = "describe --tags",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};
			process.Start();

			string describedTag = process.StandardOutput.ReadToEnd().Trim();
			if (describedTag.Contains('-')) {
				describedTag = describedTag[..describedTag.LastIndexOf('-')];
			}
			string[] parts = describedTag[1..].Split(".");
			string targetVersion = $"v{int.Parse(parts[0])}.{int.Parse(parts[1]) + 1}.0";

			string resultVersion = targetVersion;
			if (File.Exists(".version")) {
				string customVersion = File.ReadAllText(".version").Trim();
				if (resultVersion.Length > 0) {
					resultVersion = customVersion;
				}
				
				if (customVersion == describedTag) {
					Debug.LogError("The custom version is the same as the current version. Did you forget to remove '.version'?");
				}
			}

			// Check all files dependant on the version
			if (Application.version != resultVersion) {
				Debug.LogError($"Application.version is not '{resultVersion}'");
			}

			string issText = File.ReadAllText("installer.iss");
			if (!issText.Contains($"#define MyAppVersion \"{resultVersion[1..]}\"")) {
				Debug.LogError($"'installer.iss' version is not '{resultVersion[1..]}'");
			}
		}
#endif
	}
}
