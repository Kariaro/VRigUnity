using HardCoded.VRigUnity.Visuals;
using Mediapipe.Unity;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class CustomizableCanvas : MonoBehaviour, IHolisticCallback {
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
		
		public HolisticLandmarks faceLandmarks      = HolisticLandmarks.NotPresent;
		public HolisticLandmarks leftHandLandmarks  = HolisticLandmarks.NotPresent;
		public HolisticLandmarks rightHandLandmarks = HolisticLandmarks.NotPresent;
		public HolisticLandmarks poseLandmarks      = HolisticLandmarks.NotPresent;
		public HolisticLandmarks poseWorldLandmarks = HolisticLandmarks.NotPresent;

		public void OnFaceLandmarks(HolisticLandmarks landmarks) {
			faceLandmarks = landmarks;
		}

		public void OnLeftHandLandmarks(HolisticLandmarks landmarks) {
			leftHandLandmarks = landmarks;
		}

		public void OnRightHandLandmarks(HolisticLandmarks landmarks) {
			rightHandLandmarks = landmarks;
		}

		public void OnPoseLandmarks(HolisticLandmarks landmarks) {
			poseLandmarks = landmarks;
		}

		public void OnPoseWorldLandmarks(HolisticLandmarks landmarks) {
			poseWorldLandmarks = landmarks;
		}

		void LateUpdate() {
			if (!m_showAnnotations || !visualization.IsPrepared) {
				return;
			}

			visualization.DrawLandmarks(
				faceLandmarks,
				leftHandLandmarks,
				rightHandLandmarks,
				poseLandmarks,
				poseWorldLandmarks
			);
		}
	}
}
