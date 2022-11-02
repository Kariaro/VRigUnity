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
			graphRunner.OnPoseDetectionOutput += OnPoseDetectionOutput;
			graphRunner.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
			graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
			graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
			graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
			graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
			graphRunner.OnPoseRoiOutput += OnPoseRoiOutput;
		}

		private void OnPoseDetectionOutput(object stream, OutputEventArgs<Detection> eventArgs) {}
		private void OnPoseLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {}
		private void OnPoseRoiOutput(object stream, OutputEventArgs<NormalizedRect> eventArgs) {}
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

			Vector3 uu = Vector3.up * 0.1f;
			Vector3 ff = Vector3.forward * 0.1f;
			Vector3 rr = Vector3.right * 0.1f;

			//Vector3 l_pos = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
			//Debug.DrawRay(l_pos, m_leftWrist * uu, UnityEngine.Color.green);
			//Debug.DrawRay(l_pos, m_leftWrist * ff, UnityEngine.Color.blue);
			//Debug.DrawRay(l_pos, m_leftWrist * rr, UnityEngine.Color.red);
			
			// First remove Z rotation as it does not change the wrist
			Vector3 r_pos = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;

			Quaternion wrist = m_rightWrist;
			Vector3 wristE = m_rightWrist.eulerAngles;
			wrist = Quaternion.Euler(0, wristE.y, 0);

			if (test == 1) {
				wrist = m_rightWrist;
			}

			Debug.DrawRay(r_pos, wrist * uu, UnityEngine.Color.green);
			Debug.DrawRay(r_pos, wrist * ff, UnityEngine.Color.blue);
			Debug.DrawRay(r_pos, wrist * rr, UnityEngine.Color.red);

			float y_rotation = m_rightWrist.eulerAngles.y;


			// Blue red around arm


			base.FixedUpdate();

			// After update
			
			float time = TimeNow;
			//Quaternion q_rUpperArm = rUpperArm.GetRawUpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightUpperArm), time);
			//Quaternion q_rLowerArm = rLowerArm.GetRawUpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), time);
			Quaternion q_lUpperArm = lUpperArm.GetRawUpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), time);
			Quaternion q_lLowerArm = lLowerArm.GetRawUpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), time);

			float rotAngle = m_rightWrist.eulerAngles.x;

			q_lLowerArm.ToAngleAxis(out float outAngle, out Vector3 outAxis);

			Debug.DrawRay(r_pos, outAxis, UnityEngine.Color.white);

			// We need to rotate rotAngkle around the lower arms Vector3.right axis
			float r_maxAngle1 = 0 * Mathf.Clamp(-wristE.y / 40.0f, -10, 10);
			float r_maxAngle2 = Mathf.Clamp(-wristE.y / 10.0f, -23, 23);

			// Using the ring and index finger we get a left and right side
			// The right of the arm is always closest to the index finger
			// And the left of the arm is always closest to the ring finger

			// The trick is to get the right of the arm to point in the same direction
			// as (ring -> index) because they are always in that order.

			// Problem. If the hand is tilted in any forward direction

			// To solve this we need to get the arm's right to go through the palm plane

			// Problem. If both are used both tilted and up down.

			// To solve this imagine a lego Technic U-Join

			// The ring  finger is always on the left side of the arm
			// The index finger is always on the right side of the arm

			// Imagine taking the ray of the wrist to the middle finger
			// Then applying that same ray to the ring and index finger.
			// We will get two points on the right line of the wrist

			// Then imagine drawing two lines on the arm. One for right and one
			// for left. These lines should connect to the two points on the wrist line
			// To achieve this we would need a rotation to best fit them



			animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).transform.rotation = q_lUpperArm * Quaternion.Euler(r_maxAngle1, 0, 0);
			animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).transform.rotation = q_lLowerArm * Quaternion.Euler(r_maxAngle2, 0, 0);
		}
	}
}
