using Mediapipe.Unity;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class CustomizableCanvas : MonoBehaviour {
		[Header("Camera")]

		// This is the camera that will be streamed outwards
		// A value of null means that this class is disabled
		[SerializeField] private Camera streamCamera;

		// This is the default camera unity uses to show the
		// scene to the user.
		[SerializeField] private Camera unityCamera;
		
		[Header("Canvas")]

		// Canvas used for UI
		[SerializeField] private DisplayCanvas streamCanvas;
		[SerializeField] private DisplayCanvas unityCanvas;

		void Start() {
			streamCanvas.SetCamera(streamCamera);
			unityCanvas.SetCamera(unityCamera);
		}

		public void SetBackgroundImage(Texture texture) {
			streamCanvas.SetBackgroundImage(texture);
			unityCanvas.SetBackgroundImage(texture);
		}

		public void SetBackgroundColor(Color color) {
			// TODO: Implement
		}

		public void ShowBackground(bool show) {
			streamCanvas.ShowBackground(show);
			unityCanvas.ShowBackground(show);
		}

		public void ShowWebcam(bool show) {
			unityCanvas.ShowWebcam(show);
		}

		public void DrawImage(TextureFrame textureFrame) {
			unityCanvas.DrawImage(textureFrame);
		}
	}
}
