using Mediapipe;
using Mediapipe.Unity;
using System;
using UnityEngine;
using VRM;

namespace HardCoded.VRigUnity {
	public class HolisticTrackingSolutionWithDebug : HolisticTrackingSolution {
		[Header("Debug")]
		[SerializeField] private HandGroup handGroup;
		
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

		[Range(0, 1)]
		public float angleTest = 0;
		public int test;

		new void FixedUpdate() {
			if (handGroup != null && handPoints != null) {
				handGroup.Apply(handPoints, animator.GetBoneTransform(HumanBodyBones.LeftHand).transform.position, 0.5f);
			}

			// Debug
			if (hasHandData) {
				HandResolver.SolveRightHand(handPoints);
			}
			
			base.FixedUpdate();

			/*
			{
				Vector3 w_pos = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
				Quaternion w_rot = animator.GetBoneTransform(HumanBodyBones.LeftHand).rotation;
				Vector3 a_pos = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position;
				Quaternion a_rot = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).rotation;
				float angle = MovementUtils.GetArmWristAngle(a_pos, a_rot, w_pos, w_rot);
				angle = Mathf.Clamp(angle / 4.0f, -45, 45);
				animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).localRotation *= Quaternion.Euler(angle, 0, 0);
			}
			
			{
				Vector3 w_pos = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
				Quaternion w_rot = animator.GetBoneTransform(HumanBodyBones.RightHand).rotation;
				Vector3 a_pos = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position;
				Quaternion a_rot = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).rotation;
				float angle = MovementUtils.GetArmWristAngle(a_pos, a_rot, w_pos, w_rot);
				angle = Mathf.Clamp(angle / 4.0f, -45, 45);
				animator.GetBoneTransform(HumanBodyBones.RightLowerArm).localRotation *= Quaternion.Euler(angle, 0, 0);
			}
			*/
		}
	}
}
