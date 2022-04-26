using Mediapipe;
using Mediapipe.Unity;
using System.Collections;
using UnityEngine;
using VRM;

namespace HardCoded.VRigUnity {
	public abstract class HolisticSolutionBase : ImageSourceSolution<TestHolisticTrackingGraph> {
		[SerializeField] protected RectTransform _worldAnnotationArea;
		[SerializeField] protected DetectionAnnotationController _poseDetectionAnnotationController;
		[SerializeField] protected HolisticLandmarkListAnnotationController _holisticAnnotationController;
		[SerializeField] protected PoseWorldLandmarkListAnnotationController _poseWorldLandmarksAnnotationController;
		[SerializeField] protected NormalizedRectAnnotationController _poseRoiAnnotationController;

		public TestHolisticTrackingGraph.ModelComplexity ModelComplexity {
			get => graphRunner.modelComplexity;
			set => graphRunner.modelComplexity = value;
		}

		public bool SmoothLandmarks {
			get => graphRunner.smoothLandmarks;
			set => graphRunner.smoothLandmarks = value;
		}

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
			Detection poseDetection = null;
			NormalizedLandmarkList faceLandmarks = null;
			NormalizedLandmarkList poseLandmarks = null;
			NormalizedLandmarkList leftHandLandmarks = null;
			NormalizedLandmarkList rightHandLandmarks = null;
			LandmarkList poseWorldLandmarks = null;
			NormalizedRect poseRoi = null;

			if (false) {
				yield return new WaitUntil(() => { return true; });
			}
			/*
			if (runningMode == RunningMode.Sync) {
				var _ = graphRunner.TryGetNext(out poseDetection, out poseLandmarks, out faceLandmarks, out leftHandLandmarks, out rightHandLandmarks, out poseWorldLandmarks, out poseRoi, true);
			} else if (runningMode == RunningMode.NonBlockingSync) {
				yield return new WaitUntil(() =>
					graphRunner.TryGetNext(out poseDetection, out poseLandmarks, out faceLandmarks, out leftHandLandmarks, out rightHandLandmarks, out poseWorldLandmarks, out poseRoi, false));
			}
			*/

			_poseDetectionAnnotationController.DrawNow(poseDetection);
			_holisticAnnotationController.DrawNow(faceLandmarks, poseLandmarks, leftHandLandmarks, rightHandLandmarks);
			_poseWorldLandmarksAnnotationController.DrawNow(poseWorldLandmarks);
			_poseRoiAnnotationController.DrawNow(poseRoi);
		}
	}
}
