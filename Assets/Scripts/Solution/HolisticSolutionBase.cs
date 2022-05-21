using Mediapipe;
using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public abstract class HolisticSolutionBase : TestImageSourceSolution<TestHolisticTrackingGraph> {
		[SerializeField] protected RectTransform _worldAnnotationArea;
		[SerializeField] protected DetectionAnnotationController _poseDetectionAnnotationController;
		[SerializeField] protected HolisticLandmarkListAnnotationController _holisticAnnotationController;
		[SerializeField] protected PoseWorldLandmarkListAnnotationController _poseWorldLandmarksAnnotationController;
		[SerializeField] protected NormalizedRectAnnotationController _poseRoiAnnotationController;

		// Always 'Full'
		public TestHolisticTrackingGraph.ModelComplexity ModelComplexity {
			get => graphRunner.modelComplexity;
			set => graphRunner.modelComplexity = value;
		}

		// Always 'true'
		public bool SmoothLandmarks {
			get => graphRunner.smoothLandmarks;
			set => graphRunner.smoothLandmarks = value;
		}

		// Always 'true'
		public bool RefineFaceLandmarks {
			get => graphRunner.refineFaceLandmarks;
			set => graphRunner.refineFaceLandmarks = value;
		}

		public float MinDetectionConfidence {
			get => graphRunner.MinDetectionConfidence;
			set => graphRunner.MinDetectionConfidence = value;
		}

		public float MinTrackingConfidence {
			get => graphRunner.MinTrackingConfidence;
			set => graphRunner.MinTrackingConfidence = value;
		}

		protected override void SetupScreen(ImageSource imageSource) {
			base.SetupScreen(imageSource);
			_worldAnnotationArea.localEulerAngles = imageSource.rotation.Reverse().GetEulerAngles();
		}

		protected abstract override void OnStartRun();

		protected override void AddTextureFrameToInputStream(TextureFrame textureFrame) {
			graphRunner.AddTextureFrameToInputStream(textureFrame);
		}

		protected override IEnumerator WaitForNextValue() {
			//Detection poseDetection = null;
			//NormalizedLandmarkList faceLandmarks = null;
			//NormalizedLandmarkList poseLandmarks = null;
			//NormalizedLandmarkList leftHandLandmarks = null;
			//NormalizedLandmarkList rightHandLandmarks = null;
			//LandmarkList poseWorldLandmarks = null;
			//NormalizedRect poseRoi = null;

			//_poseDetectionAnnotationController.DrawNow(poseDetection);
			//_holisticAnnotationController.DrawNow(faceLandmarks, poseLandmarks, leftHandLandmarks, rightHandLandmarks);
			//_poseWorldLandmarksAnnotationController.DrawNow(poseWorldLandmarks);
			//_poseRoiAnnotationController.DrawNow(poseRoi);
			yield return null;
		}
	}
}
