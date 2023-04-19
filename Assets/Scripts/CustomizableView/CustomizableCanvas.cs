using HardCoded.VRigUnity.Visuals;
using Mediapipe.Unity;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class CustomizableCanvas : MonoBehaviour {
		[Header("Camera")]

		// This is the camera that will be streamed outwards
		// A value of null means that this class is disabled
		[SerializeField] private Camera streamCamera;

		// This is the default camera unity uses to show the
		// scene to the user
		[SerializeField] private Camera unityCamera;
		
		[Header("Canvas")]

		// Canvas used for UI
		[SerializeField] private DisplayCanvas streamCanvas;
		[SerializeField] private DisplayCanvas unityCanvas;

		[Header("Annotations")]
		[SerializeField] private GameObject annotationObject;
		[SerializeField] public RectTransform annotationArea;
		public Visualization visualization;
		private bool m_showAnnotations;

		void Start() {
			streamCanvas.SetCamera(streamCamera);
			unityCanvas.SetCamera(unityCamera);
		}

		public void SetBackgroundImage(Texture texture) {
			streamCanvas.SetBackgroundImage(texture);
			unityCanvas.SetBackgroundImage(texture);
		}

		public void SetBackgroundColor(UnityEngine.Color color) {
			// TODO: Implement
		}

		public void ShowBackground(bool show) {
			streamCanvas.ShowBackground(show);
			unityCanvas.ShowBackground(show);
		}

		public void ShowWebcam(bool show) {
			unityCanvas.ShowWebcam(show);
		}

		public void ShowAnnotations(bool show) {
			annotationObject.SetActive(show);
			m_showAnnotations = show;
		}

		public void ReadSync(Texture2D texture) {
			unityCanvas.ReadSync(texture);
		}

		void Update() {
			if (streamCamera.enabled != Settings.Temporary.VirtualCamera) {
				streamCamera.enabled = Settings.Temporary.VirtualCamera;
			}
		}

		// Annotations
		public void SetupAnnotations() {
			var imageSource = SolutionUtils.GetImageSource();
			SetupAnnotationController(imageSource, Settings.CameraFlipped);
		}

		protected static void SetupAnnotationController(ImageSource imageSource, bool expectedToBeMirrored = false) {
			// bool isMirrored = expectedToBeMirrored ^ imageSource.IsHorizontallyFlipped ^ imageSource.IsFrontFacing ^ true;
			// var rotationAngle = imageSource.Rotation.Reverse();
		}

		public void SetupScreen(ImageSource imageSource) {
			annotationArea.sizeDelta = new Vector2(imageSource.TextureWidth, imageSource.TextureHeight);
			annotationArea.localEulerAngles = imageSource.Rotation.Reverse().GetEulerAngles();
		}

		public void OnLandmarks(HolisticLandmarks face,
			HolisticLandmarks leftHand,
			HolisticLandmarks rightHand,
			HolisticLandmarks pose,
			HolisticLandmarks poseWorld,
			int flags) {
			if (!m_showAnnotations || !visualization.IsPrepared) {
				return;
			}

			visualization.DrawLandmarks(
				face,
				leftHand,
				rightHand,
				pose,
				poseWorld,
				flags
			);
		}
	}
}
