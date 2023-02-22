using TMPro;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;

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
			UpdateTitle();
#endif
		}

#if UNITY_STANDALONE_WIN
		[DllImport("user32.dll", EntryPoint = "SetWindowText")]
		static extern bool SetWindowText(IntPtr hwnd, string lpString);

		// Add the version information to the window title
		void UpdateTitle() {
			IntPtr windowHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
			SetWindowText(windowHandle, Application.productName + " " + Application.version);
		}
#else
		void UpdateTitle() {
		}
#endif

#if UNITY_EDITOR
		private readonly FileWatcher watcher = new(".version");

		void Update() {
			if (watcher.IsUpdated) {
				ValidateVersion();
			}
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
			
			if (!process.Start() || !process.WaitForExit(1000) || process.ExitCode != 0) {
				Debug.LogWarning("Could not get current tag from 'git' command");
				return;
			}

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
