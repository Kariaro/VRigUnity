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

			Vector3 uu = Vector3.up * 0.1f;
			Vector3 ff = Vector3.forward * 0.1f;
			Vector3 rr = Vector3.right * 0.1f;

			//Vector3 l_pos = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
			//Debug.DrawRay(l_pos, m_leftWrist * uu, UnityEngine.Color.green);
			//Debug.DrawRay(l_pos, m_leftWrist * ff, UnityEngine.Color.blue);
			//Debug.DrawRay(l_pos, m_leftWrist * rr, UnityEngine.Color.red);
			
			// First remove Z rotation as it does not change the wrist
			Quaternion q_rWrist = animator.GetBoneTransform(HumanBodyBones.LeftHand).rotation;

			// This is in local
			Vector3 wristE = q_rWrist.eulerAngles;

			{
				Vector3 r_wpos = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
				Quaternion r_wrot = animator.GetBoneTransform(HumanBodyBones.LeftHand).rotation;

				//Debug.DrawRay(r_wpos, r_wrot * uu, UnityEngine.Color.green);
				//Debug.DrawRay(r_wpos, r_wrot * ff, UnityEngine.Color.blue);
				//Debug.DrawRay(r_wpos, r_wrot * rr, UnityEngine.Color.red);


				Vector3 r_apos = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position;
				Quaternion r_arot = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).rotation;
				Debug.DrawRay(r_apos, r_arot * uu, UnityEngine.Color.green);
				Debug.DrawRay(r_apos, r_arot * ff, UnityEngine.Color.blue);
				Debug.DrawRay(r_apos, r_arot * rr, UnityEngine.Color.red);

				
				Quaternion r_wrot2 = animator.GetBoneTransform(HumanBodyBones.LeftHand).localRotation;
				Debug.DrawRay(r_wpos, r_wrot2 * -uu, UnityEngine.Color.green);
				Debug.DrawRay(r_wpos, r_wrot2 * -rr, UnityEngine.Color.red);


				r_wrot = animator.GetBoneTransform(HumanBodyBones.LeftHand).localRotation;
				float wrs_r = r_wrot.eulerAngles.x;
				float arm_r = r_arot.eulerAngles.x;
				float da = Mathf.DeltaAngle(arm_r, wrs_r);
				// Logger.Info($"{wrs_r} {arm_r} {da}");

				// On the line up and forward

				// Debug.DrawRay(r_wpos, r_wrot * uu, UnityEngine.Color.green);
				// Debug.DrawRay(r_wpos, r_wrot * ff, UnityEngine.Color.blue);
				// Debug.DrawRay(r_wpos, r_wrot * rr, UnityEngine.Color.red);

				// animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).rotation = r_arot * Quaternion.Euler(da / 2.0f, 0, 0);
				
				/*
				Vector3 r_tpos = (r_wpos + r_apos) / 2.0f;
				Quaternion r_trot = Quaternion.Slerp(r_wrot, r_arot, angleTest);
				Debug.DrawRay(r_tpos, r_trot * uu, UnityEngine.Color.green);
				Debug.DrawRay(r_tpos, r_trot * ff, UnityEngine.Color.blue);
				Debug.DrawRay(r_tpos, r_trot * rr, UnityEngine.Color.red);
				*/

				// Point the red vector of the wrist towards the red vector of the arm
				/*
				Quaternion test = Quaternion.FromToRotation(r_wrot * rr, r_arot * rr);

				
				r_wrot = q_rWrist * test * Quaternion.Inverse(q_rWrist);

				Debug.DrawRay(r_wpos, r_wrot * uu, UnityEngine.Color.green);
				Debug.DrawRay(r_wpos, r_wrot * ff, UnityEngine.Color.blue);
				Debug.DrawRay(r_wpos, r_wrot * rr, UnityEngine.Color.red);
				*/
			}
			

			// Blue red around arm


			base.FixedUpdate();

			// After update
			
			float time = TimeNow;
			//Quaternion q_rUpperArm = rUpperArm.GetRawUpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightUpperArm), time);
			//Quaternion q_rLowerArm = rLowerArm.GetRawUpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), time);
			Quaternion q_lUpperArm = Pose.LeftUpperArm.GetRawUpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), time);
			Quaternion q_lLowerArm = Pose.LeftLowerArm.GetRawUpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), time);

			float rotAngle = m_rightWrist.eulerAngles.x;

			q_lLowerArm.ToAngleAxis(out float outAngle, out Vector3 outAxis);

			// Debug.DrawRay(r_pos, outAxis, UnityEngine.Color.white);

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



			// animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
			// animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);

			// animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).transform.rotation = q_lUpperArm * Quaternion.Euler(r_maxAngle1, 0, 0);
			
			Vector3 lWristPos = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
			Vector3 lWristFor = (m_rightWrist * Vector3.forward * 0.05f);
			Vector3 lLowerArmPos = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position;
			Vector3 lLowerArmFor = (q_lLowerArm * Vector3.forward * 0.05f);

			Debug.DrawLine(lWristPos, lWristPos + lWristFor);
			Debug.DrawLine(lLowerArmPos, lLowerArmPos + lLowerArmFor);
			Debug.DrawLine(lWristPos + lWristFor, lLowerArmPos + lLowerArmFor);

			// Rotate (lLowerArmFor) such that (lWristPos + lWristFor, lLowerArmPos + lLowerArmFor)
			// is smallest


			// This is localRotation
			//Quaternion g_lLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).rotation * q_lLowerArm;
			//g_lLowerArm.ToAngleAxis(out float lAngle, out Vector3 lAxis);

			//Debug.DrawLine(lLowerArmPos, lLowerArmPos + lAxis * 0.05f, UnityEngine.Color.red);



			// animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).transform.rotation = q_lLowerArm * Quaternion.Euler(r_maxAngle2, 0, 0);
		}
	}
}
