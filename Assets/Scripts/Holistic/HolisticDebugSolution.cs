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

		public override void OnFaceLandmarks(HolisticLandmarks landmarks) {
			base.OnFaceLandmarks(landmarks);

			if (landmarks.IsPresent) {
				facePoints = landmarks;
			}
		}

		public override void OnRightHandLandmarks(HolisticLandmarks landmarks) {
			base.OnRightHandLandmarks(landmarks);
			
			if (landmarks.IsPresent) {
				for (int i = 0; i < landmarks.Count; i++) {
					rightHandPoints.Data[i] = -landmarks[i];
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
