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

		private readonly DataGroups.HandPoints rightHandPoints = new();
		private bool hasHandData;

		// Used by 'FaceGizmos'
		public HolisticLandmarks facePoints;
		
		// Used for custom mesh
		public GameObject meshObject;

		public override void OnLandmarks(HolisticLandmarks face,
			HolisticLandmarks leftHand,
			HolisticLandmarks rightHand,
			HolisticLandmarks pose,
			HolisticLandmarks poseWorld,
			int flags) {
			base.OnLandmarks(face, leftHand, rightHand, pose, poseWorld, flags);
			
			if (face.IsPresent) {
				facePoints = face;
			}

			if (rightHand.IsPresent) {
				for (int i = 0; i < rightHand.Count; i++) {
					rightHandPoints.Data[i] = -rightHand[i];
				}

				hasHandData = true;
			}
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
