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
		private WebCamSource m_webCamSource;

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

		public void DrawImage(TextureFrame textureFrame) {
			if (!IsShowingWebcam) {
				webcamImage.color = Color.clear;
				return;
			}

			if (m_webCamSource == null) {
				m_webCamSource = SolutionUtils.GetImageSource();
				return;
			}

			WebCamTexture texture = m_webCamSource.GetCurrentTexture() as WebCamTexture;
			Texture2D tex = webcamImage.texture as Texture2D;
			
			if (!(tex is Texture2D)) {
				if (tex == null || tex.width != texture.width || tex.height != texture.height) {
					tex = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
					webcamImage.texture = tex;
				}
			}
				
			float w = (UnityEngine.Screen.width / (float) UnityEngine.Screen.height);
			float d = (texture.height / (float) texture.width) * w * 0.5f;

			if (d < 0.5) {
				webcamImage.rectTransform.anchorMin = new(0, 0.5f - d);
				webcamImage.rectTransform.anchorMax = new(1, 0.5f + d);
			} else {
				d = ((texture.width / (float) texture.height) / w) * 0.5f;
				webcamImage.rectTransform.anchorMin = new(0.5f - d, 0);
				webcamImage.rectTransform.anchorMax = new(0.5f + d, 1);
			}

			webcamImage.color = Color.white;

			textureFrame.CopyTexture(tex);
		}
	}
}
