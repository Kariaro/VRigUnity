using UnityEngine;

namespace HardCoded.VRigUnity {
	[RequireComponent(typeof(Camera))]
	public class CameraCapture : MonoBehaviour {
		// WINDOWS
		private UnityCapture unityCapture;

		// LINUX
		// ...

		// iOS
		// ...

		// Internal fields
		private Camera m_mainCamera;

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
#else
#  error Virtual Camera is not supported on this system
#endif
		}
	}
}
