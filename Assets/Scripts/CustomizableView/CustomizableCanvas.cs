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
		// scene to the user.
		[SerializeField] private Camera unityCamera;
		
		[Header("Canvas")]

		// Canvas used for UI
		[SerializeField] private DisplayCanvas streamCanvas;
		[SerializeField] private DisplayCanvas unityCanvas;

		[Header("Annotations")]
		[SerializeField] private GameObject annotationObject;
		[SerializeField] private Mediapipe.Unity.Screen screen;
		[SerializeField] private RectTransform worldAnnotationArea;
		[SerializeField] private DetectionAnnotationController poseDetectionAnnotationController;
		[SerializeField] private HolisticLandmarkListAnnotationController holisticAnnotationController;
		[SerializeField] private PoseWorldLandmarkListAnnotationController poseWorldLandmarksAnnotationController;
		[SerializeField] private NormalizedRectAnnotationController poseRoiAnnotationController;

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
		}

		public void DrawImage(TextureFrame textureFrame) {
			unityCanvas.DrawImage(textureFrame);
		}

		// Annotations
		public void SetupAnnotations() {
			var imageSource = SolutionUtils.GetImageSource();
			SetupAnnotationController(poseDetectionAnnotationController, imageSource);
			SetupAnnotationController(holisticAnnotationController, imageSource);
			SetupAnnotationController(poseWorldLandmarksAnnotationController, imageSource);
			SetupAnnotationController(poseRoiAnnotationController, imageSource);
		}

		protected static void SetupAnnotationController<T>(AnnotationController<T> annotationController, ImageSource imageSource, bool expectedToBeMirrored = false) where T : HierarchicalAnnotation {
			annotationController.isMirrored = expectedToBeMirrored ^ imageSource.isHorizontallyFlipped ^ imageSource.isFrontFacing;
			annotationController.rotationAngle = imageSource.rotation.Reverse();
		}

		public void SetupScreen(ImageSource imageSource) {
			// NOTE: Without this line the screen does not update its size and no annotations are drawn
			screen.Initialize(imageSource);
			worldAnnotationArea.localEulerAngles = imageSource.rotation.Reverse().GetEulerAngles();
		}

		public void OnPoseDetectionOutput(OutputEventArgs<Detection> eventArgs) {
			poseDetectionAnnotationController.DrawLater(eventArgs.value);
		}
		
		public void OnPoseLandmarksOutput(OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			holisticAnnotationController.DrawPoseLandmarkListLater(eventArgs.value);
		}

		public void OnPoseRoiOutput(OutputEventArgs<NormalizedRect> eventArgs) {
			poseRoiAnnotationController.DrawLater(eventArgs.value);
		}

		public void OnFaceLandmarksOutput(OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			holisticAnnotationController.DrawFaceLandmarkListLater(eventArgs.value);
		}

		public void OnLeftHandLandmarksOutput(OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			holisticAnnotationController.DrawLeftHandLandmarkListLater(eventArgs.value);
		}

		public void OnRightHandLandmarksOutput(OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			holisticAnnotationController.DrawRightHandLandmarkListLater(eventArgs.value);
		}

		public void OnPoseWorldLandmarksOutput(OutputEventArgs<LandmarkList> eventArgs) {
			poseWorldLandmarksAnnotationController.DrawLater(eventArgs.value);
		}
	}
}
