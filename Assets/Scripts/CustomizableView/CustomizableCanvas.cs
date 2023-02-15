using Mediapipe;
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
		[SerializeField] private RectTransform annotationArea;
		[SerializeField] private HolisticLandmarkListAnnotationController holisticAnnotationController;
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

		public void ReadSync(TextureFrame textureFrame) {
			unityCanvas.ReadSync(textureFrame);
		}

		void Update() {
			if (streamCamera.enabled != Settings.Temporary.VirtualCamera) {
				streamCamera.enabled = Settings.Temporary.VirtualCamera;
			}
		}

		// Annotations
		public void SetupAnnotations() {
			var imageSource = SolutionUtils.GetImageSource();
			SetupAnnotationController(holisticAnnotationController, imageSource, Settings.CameraFlipped);
		}

		protected static void SetupAnnotationController<T>(AnnotationController<T> annotationController, ImageSource imageSource, bool expectedToBeMirrored = false) where T : HierarchicalAnnotation {
			annotationController.isMirrored = expectedToBeMirrored ^ imageSource.IsHorizontallyFlipped ^ imageSource.IsFrontFacing ^ true;
			annotationController.rotationAngle = imageSource.Rotation.Reverse();
		}

		public void SetupScreen(ImageSource imageSource) {
			// NOTE: Without this line the screen does not update its size and no annotations are drawn
			annotationArea.sizeDelta = new Vector2(imageSource.TextureWidth, imageSource.TextureHeight);
			annotationArea.localEulerAngles = imageSource.Rotation.Reverse().GetEulerAngles();
		}

		public void OnPoseLandmarksOutput(OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			if (m_showAnnotations) {
				holisticAnnotationController.DrawPoseLandmarkListLater(eventArgs.value);
			}
		}

		public void OnFaceLandmarksOutput(OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			if (m_showAnnotations) {
				holisticAnnotationController.DrawFaceLandmarkListLater(eventArgs.value);
			}
		}

		public void OnLeftHandLandmarksOutput(OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			if (m_showAnnotations) {
				holisticAnnotationController.DrawLeftHandLandmarkListLater(eventArgs.value);
			}
		}

		public void OnRightHandLandmarksOutput(OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			if (m_showAnnotations) {
				holisticAnnotationController.DrawRightHandLandmarkListLater(eventArgs.value);
			}
		}
	}
}
