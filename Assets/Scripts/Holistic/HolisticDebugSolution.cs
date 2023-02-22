using Mediapipe;
using Mediapipe.Unity;
using System;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class HolisticDebugSolution : HolisticSolution {
		[Header("Debug")]
		[SerializeField] private HandGroup handGroup;
		[SerializeField] private int fps = 60;
		[SerializeField] private bool renderUpdate;

		private readonly Groups.HandPoints rightHandPoints = new();
		private bool hasHandData;

		// Used by 'FaceGizmos'
		public NormalizedLandmarkList facePoints;
		
		// Used for custom mesh
		public GameObject meshObject;

		protected override void OnStartRun() {
			base.OnStartRun();
			graphRunner.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
			graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
			graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
			graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
			graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
		}

		private void OnPoseLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {}
		private void OnFaceLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			if (eventArgs.value == null) {
				return;
			}

			facePoints = eventArgs.value;
		}

		private void OnLeftHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {}
		private void OnPoseWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs) {}
		
		private void OnRightHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			if (eventArgs.value == null) {
				return;
			}

			int count = eventArgs.value.Landmark.Count;
			for (int i = 0; i < count; i++) {
				NormalizedLandmark mark = eventArgs.value.Landmark[i];
				rightHandPoints.Data[i] = new(-mark.X * 2, -mark.Y, -mark.Z * 2);
			}

			hasHandData = true;
		}

		public override void Update() {
			base.Update();

			if (Application.targetFrameRate != fps) {
				Application.targetFrameRate = fps;
			}
		}

		public override void UpdateModel() {
			if (!renderUpdate) {	
				base.UpdateModel();
			}
		}

		public override void AnimateModel() {
			if (handGroup != null && rightHandPoints != null) {
				handGroup.Apply(rightHandPoints, model.ModelBones[HumanBodyBones.RightHand].transform.position, 0.5f);
			}

			if (renderUpdate) {
				float time = TimeNow;
				RightHand.Update(time);
				LeftHand.Update(time);
				Pose.Update(time);
			}

			// Debug
			if (hasHandData) {
				HandResolver.SolveRightHand(rightHandPoints);
			}

			base.AnimateModel();
		}
	}
}
