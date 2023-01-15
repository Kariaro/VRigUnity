using Mediapipe;
using Mediapipe.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class PoseResolver {
		private static Vector4 ConvertPoint(LandmarkList list, int idx) {
			Landmark mark = list.Landmark[idx];
			return new(mark.X, mark.Y, mark.Z, mark.Visibility);
		}

		public static Groups.PoseRotation SolvePose(OutputEventArgs<LandmarkList> eventArgs) {
			Groups.PoseRotation pose = new();

			{
				pose.rShoulder = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_SHOULDER);
				pose.lShoulder = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_SHOULDER);
				Vector4 rHip = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_HIP);
				Vector4 lHip = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_HIP);

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
					pose.rElbow = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_ELBOW);
					pose.rHand = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_WRIST);
					Vector3 vRigA = Vector3.left;
					Vector3 vRigB = pose.rElbow - pose.rShoulder;
					Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
					pose.rUpperArm = rot;

					Vector3 vRigC = pose.rHand - pose.rElbow;
					rot = Quaternion.FromToRotation(vRigA, vRigC);
					pose.rLowerArm = rot;

					if (pose.rHand.w < Settings.HandTrackingThreshold) {
						pose.rLowerArm = pose.rUpperArm;
						pose.rHand = (Vector3) pose.rElbow + vRigB;
					}
				}

				{
					pose.lElbow = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_ELBOW);
					pose.lHand = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_WRIST);
					Vector3 vRigA = Vector3.right;
					Vector3 vRigB = pose.lElbow - pose.lShoulder;
					Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
					pose.lUpperArm = rot;

					Vector3 vRigC = pose.lHand - pose.lElbow;
					rot = Quaternion.FromToRotation(vRigA, vRigC);
					pose.lLowerArm = rot;

					if (pose.lHand.w < Settings.HandTrackingThreshold) {
						pose.lLowerArm = pose.lUpperArm;
						pose.lHand = (Vector3) pose.lElbow + vRigB;
					}
				}

				// Legs
				{
					Vector4 rKnee = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_KNEE);
					Vector4 rAnkle = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_ANKLE);
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
					Vector4 lKnee = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_KNEE);
					Vector4 lAnkle = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_ANKLE);
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

