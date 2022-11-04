using Mediapipe;
using Mediapipe.Unity;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class LoadLibrary {
		public static bool TryLoadLibrary() {
#if UNITY_EDITOR_LINUX
			var path = Path.Combine("Packages", "com.github.homuler.mediapipe", "Runtime", "Plugins", "libmediapipe_c.so");
#elif UNITY_STANDALONE_LINUX
			var path = Path.Combine(Application.dataPath, "Plugins", "libmediapipe_c.so");
#elif UNITY_EDITOR_OSX
			var path = Path.Combine("Packages", "com.github.homuler.mediapipe", "Runtime", "Plugins", "libmediapipe_c.dylib");
#elif UNITY_STANDALONE_OSX
			var path = Path.Combine(Application.dataPath, "Plugins", "libmediapipe_c.dylib");
#else
			var path = "";
#endif

			if (path == "") {
				// We do not need to load this because it should work
				return true;
			}

			var handle = dlopen(path, 2);

			if (handle != IntPtr.Zero) {
				// Success
				var result = dlclose(handle);

				if (result != 0) {
					Debug.LogError($"Failed to unload {path}");
				} else {
					return true;
				}
			} else {
				Debug.LogError($"Failed to load {path}: {Marshal.GetLastWin32Error()}");
				var error = Marshal.PtrToStringAnsi(dlerror());
				if (error != null) {
					Debug.LogError(error);
				}
			}

			return false;
		}

		[DllImport("dl", SetLastError = true, ExactSpelling = true)]
		private static extern IntPtr dlopen(string name, int flags);

		[DllImport("dl", ExactSpelling = true)]
		private static extern IntPtr dlerror();

		[DllImport("dl", ExactSpelling = true)]
		private static extern int dlclose(IntPtr handle);
	}
}
