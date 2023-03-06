using System.IO;
using UnityEngine;

namespace HardCoded.VRigUnity {
	[RequireComponent(typeof(Camera))]
	public class CameraCapture : MonoBehaviour {
#if UNITY_STANDALONE_WIN
		public const bool IsVirtualCameraSupported = true;
		private UnityCapture unityCapture;
#elif UNITY_STANDALONE_LINUX
		public const bool IsVirtualCameraSupported = false;
#  warning Virtual Camera has not been added to Linux yet
#elif UNITY_STANDALONE_OSX
		public const bool IsVirtualCameraSupported = false;
#  warning Virtual Camera has not been added to OSX yet
#else
		public const bool IsVirtualCameraSupported = false;
#  warning Virtual Camera is not supported on this system
#endif

		// Internal fields
		private Camera m_mainCamera;

		public int cameraWidth = 1280;
		public int cameraHeight = 720;

		void Start() {
			// Find the game object with the main camera tag
			m_mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

#if UNITY_STANDALONE_WIN
			// Create an instance of unity capture
			unityCapture = gameObject.AddComponent<UnityCapture>();
			unityCapture.ResizeMode = UnityCapture.EResizeMode.LinearResize;
			unityCapture.CaptureDevice = UnityCapture.ECaptureDevice.CaptureDevice1;
			unityCapture.MirrorMode = UnityCapture.EMirrorMode.MirrorHorizontally;
			unityCapture.HideWarnings = true;
			unityCapture.mainCamera = m_mainCamera;
			unityCapture.cameraWidth = cameraWidth;
			unityCapture.cameraHeight = cameraHeight;
#elif UNITY_STANDALONE_LINUX
			// TODO: Linux
#elif UNITY_STANDALONE_OSX
			// TODO: OSX
#endif
		}

		void Update() {
#if UNITY_STANDALONE_WIN
			unityCapture.cameraWidth = cameraWidth;
			unityCapture.cameraHeight = cameraHeight;
#endif
		}

		// Call this function to install the virtual camera
#if UNITY_STANDALONE_WIN
		public static void InstallVirtualCamera() {
			System.Diagnostics.Process.Start(Path.Combine(Application.streamingAssetsPath, "unitycapture", "Install.bat"));
		}

		public static void UninstallVirtualCamera() {
			System.Diagnostics.Process.Start(Path.Combine(Application.streamingAssetsPath, "unitycapture", "Uninstall.bat"));
		}
#elif UNITY_STANDALONE_LINUX
		public static void InstallVirtualCamera() {
			System.Diagnostics.Process.Start(Path.Combine(Application.streamingAssetsPath, "v4l2loopback", "Install.sh"));
		}

		public static void UninstallVirtualCamera() {
			System.Diagnostics.Process.Start(Path.Combine(Application.streamingAssetsPath, "v4l2loopback", "Uninstall.sh"));
		}
#elif UNITY_STANDALONE_OSX
		// TODO: OSX
		public static void InstallVirtualCamera() {
			// TODO: Implement
		}

		public static void UninstallVirtualCamera() {
			// TODO: Implement
		}
#endif
	}
}
