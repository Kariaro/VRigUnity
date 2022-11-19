using Mediapipe;
using Mediapipe.Unity;
using System;
using UnityEngine;
using VRM;

namespace HardCoded.VRigUnity {
	public class HolisticTrackingSolutionWithDebug : HolisticTrackingSolution {
		[Header("Debug")]
		[SerializeField] private HandGroup handGroup;
		[SerializeField] private int fps = 60;

		private Groups.HandPoints handPoints = new();
		private bool hasHandData;

		protected override void OnStartRun() {
			base.OnStartRun();
			graphRunner.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
			graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
			graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
			graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
			graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
		}

		private void OnPoseLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {}
		private void OnFaceLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {}
		private void OnLeftHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {}
		private void OnPoseWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs) {}
		
		private void OnRightHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			if (eventArgs.value == null) {
				return;
			}

			int count = eventArgs.value.Landmark.Count;
			for (int i = 0; i < count; i++) {
				NormalizedLandmark mark = eventArgs.value.Landmark[i];
				handPoints.Data[i] = new(mark.X * 2, -mark.Y, -mark.Z);
			}

			hasHandData = true;
		}

		void Update() {
			Application.targetFrameRate = fps;
		}

		[Range(0, 1)]
		public float angleTest = 0;
		public int test;

		public override void ModelUpdate() {
			if (handGroup != null && handPoints != null) {
				handGroup.Apply(handPoints, animator.GetBoneTransform(HumanBodyBones.LeftHand).transform.position, 0.5f);
			}

			// Debug
			if (hasHandData) {
				HandResolver.SolveRightHand(handPoints);
			}
			
			if (Settings.UseWristRotation) {
				{
					Vector3 w_pos = RightHand.Wrist.GetLastPosition();
					Quaternion w_rot = RightHand.Wrist.GetLastRotation();
					Vector3 a_pos = Pose.LeftLowerArm.GetLastPosition();
					Quaternion a_rot = Pose.LeftLowerArm.GetLastRotation();
					MovementUtils.GetArmWristAngle(a_pos, a_rot, w_pos, w_rot);
				}
			
				{
					Vector3 w_pos = LeftHand.Wrist.GetLastPosition();
					Quaternion w_rot = LeftHand.Wrist.GetLastRotation();
					Vector3 a_pos = Pose.RightLowerArm.GetLastPosition();
					Quaternion a_rot = Pose.RightLowerArm.GetLastRotation();
					MovementUtils.GetArmWristAngle(a_pos, a_rot, w_pos, w_rot);
				}
			}

			base.ModelUpdate();
		}
	}
}
