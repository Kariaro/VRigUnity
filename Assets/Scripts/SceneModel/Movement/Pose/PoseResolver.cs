using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class PoseResolver {
		public static DataGroups.PoseData SolvePose(HolisticLandmarks landmarks) {
			DataGroups.PoseData pose = new();

			{
				pose.rShoulder = landmarks[MediaPipe.Pose.RIGHT_SHOULDER];
				pose.lShoulder = landmarks[MediaPipe.Pose.LEFT_SHOULDER];
				Vector4 rHip = landmarks[MediaPipe.Pose.RIGHT_HIP];
				Vector4 lHip = landmarks[MediaPipe.Pose.LEFT_HIP];

				{
					Vector3 vRigA = Vector3.left;
					Vector3 vRigB = pose.rShoulder - pose.lShoulder;
					Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
					pose.chestRotation = rot;

					vRigA = Vector3.left;
					vRigB = rHip - lHip;
					rot = Quaternion.FromToRotation(vRigA, vRigB);
					pose.hipsRotation = rot;

					float mul = 1000;
					pose.hipsPosition = new Vector3(
						(rHip.y + lHip.y) * 0.5f * mul,
						0, // -(rHip.z + lHip.z) * 0.5f * mul,
						0 // (rHip.y + lHip.y) * 0.5f * mul
					);

					/*
					float bodyRotation = 1.0f;
					bodyRotation = Mathf.Abs(Mathf.Cos(rot.eulerAngles.y * 1.6f));
					*/
				}

				{
					pose.rElbow = landmarks[MediaPipe.Pose.RIGHT_ELBOW];
					pose.rHand = landmarks[MediaPipe.Pose.RIGHT_WRIST];
					Vector3 vRigB = pose.rElbow - pose.rShoulder;

					if (pose.rHand.w < Settings.HandTrackingThreshold) {
						pose.rHand = (Vector3) pose.rElbow + vRigB;
					}
				}

				{
					pose.lElbow = landmarks[MediaPipe.Pose.LEFT_ELBOW];
					pose.lHand = landmarks[MediaPipe.Pose.LEFT_WRIST];
					Vector3 vRigB = pose.lElbow - pose.lShoulder;

					if (pose.lHand.w < Settings.HandTrackingThreshold) {
						pose.lHand = (Vector3) pose.lElbow + vRigB;
					}
				}

				// Legs
				{
					Vector4 rKnee = landmarks[MediaPipe.Pose.RIGHT_KNEE];
					Vector4 rAnkle = landmarks[MediaPipe.Pose.RIGHT_ANKLE];
					Vector3 vRigA = Vector3.up;
					Vector3 vRigB = rKnee - rHip;
					Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
					pose.rUpperLeg = rot;

					Vector3 vRigC = rAnkle - rKnee;
					rot = Quaternion.FromToRotation(vRigA, vRigC);
					pose.rLowerLeg = rot;
					
					pose.hasRightLeg = rHip.w > 0.5 && rKnee.w > 0.5;
				}

				{
					Vector4 lKnee = landmarks[MediaPipe.Pose.LEFT_KNEE];
					Vector4 lAnkle = landmarks[MediaPipe.Pose.LEFT_ANKLE];
					Vector3 vRigA = Vector3.up;
					Vector3 vRigB = lKnee - lHip;
					Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
					pose.lUpperLeg = rot;

					Vector3 vRigC = lAnkle - lKnee;
					rot = Quaternion.FromToRotation(vRigA, vRigC);
					pose.lLowerLeg = rot;

					pose.hasLeftLeg = lHip.w > 0.5 && lKnee.w > 0.5;
				}
			}

			return pose;
		}
	}
}

