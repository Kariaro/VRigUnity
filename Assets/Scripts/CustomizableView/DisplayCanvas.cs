using Mediapipe.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	[RequireComponent(typeof(Canvas))]
	public class DisplayCanvas : MonoBehaviour {
		// API properties
		public bool IsShowingWebcam => webcamImage != null && m_showWebcam;
		public bool IsShowingBackground => backgroundImage != null && m_showBackground;

		// If this is set it will output data from the webcam
		[SerializeField] private RawImage webcamImage;
		[SerializeField] private RawImage backgroundImage;
		[SerializeField] private RawImage backgroundColor;

		// Private values for what to show
		private Canvas m_canvas;
		private bool m_showWebcam;
		private bool m_showBackground;

		void Awake() {
			m_canvas = GetComponent<Canvas>();

			// By default these should be false
			ShowBackground(false);
			ShowWebcam(false);
		}

		// Set if the background should be enabled or not
		public void ShowBackground(bool enable) {
			m_showBackground = enable;

			if (backgroundImage != null) {
				backgroundImage.gameObject.SetActive(enable);
				backgroundImage.color = enable ? Color.white : Color.clear;
			}
		}

		// Set if the webcam should be enabled or not
		public void ShowWebcam(bool enable) {
			m_showWebcam = enable;

			if (webcamImage != null) {
				webcamImage.gameObject.SetActive(enable);
				webcamImage.color = Color.white;
			}
		}

		// Set the camera of the canvas
		public void SetCamera(Camera camera) {
			m_canvas.worldCamera = camera;
		}

		// Image setters
		public void SetBackgroundImage(Texture texture) {
			if (backgroundImage != null) {
				backgroundImage.texture = texture;
			}
		}

		public void ReadSync(Texture2D texture) {
			if (!IsShowingWebcam) {
				webcamImage.color = Color.clear;
				return;
			}

			float w = (Screen.width / (float) Screen.height);
			float d = (texture.height / (float) texture.width) * w * 0.5f;

			if (d < 0.5) {
				webcamImage.rectTransform.anchorMin = new(0, 0.5f - d);
				webcamImage.rectTransform.anchorMax = new(1, 0.5f + d);
			} else {
				d = ((texture.width / (float) texture.height) / w) * 0.5f;
				webcamImage.rectTransform.anchorMin = new(0.5f - d, 0);
				webcamImage.rectTransform.anchorMax = new(0.5f + d, 1);
			}

			// Flip UV rect
			if (Settings.CameraFlipped) {
				webcamImage.uvRect = new(0, 0, 1, 1);
			} else {
				webcamImage.uvRect = new(1, 0, -1, 1);
			}

			webcamImage.color = Color.white;
			webcamImage.texture = texture;
		}
	}
}
