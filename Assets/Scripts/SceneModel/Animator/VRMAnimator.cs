using UnityEngine;
using System.Collections.Generic;
using VRM;
using UnityEngine.Animations.Rigging;

namespace HardCoded.VRigUnity {
	public class VRMAnimator : MonoBehaviour {
		// Distance offset to remove arm glitches
		public const float ArmJointOffset = 0.01f;

		private HolisticSolution sol;
		private RigAnimator rigger;
		private Animator anim;
		
		public Vector3 LeftElbow;
		public Vector3 LeftHand;
		public Vector3 RightElbow;
		public Vector3 RightHand;

		void Start() {
			sol = SolutionUtils.GetSolution();
			anim = GetComponent<Animator>();
			rigger = GetComponent<RigAnimator>();
			rigger.SetupRigging();
		}

		void PerformRigging() {
			float time = sol.TimeNow;

			{
				Transform shoulder = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
				Transform elbow = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
				Transform hand = anim.GetBoneTransform(HumanBodyBones.LeftHand);
				float se_dist = Vector3.Distance(shoulder.position, elbow.position) + ArmJointOffset;
				float eh_dist = Vector3.Distance(elbow.position, hand.position) + ArmJointOffset;

				Vector3 ai_shoulder = sol.Pose.LeftShoulder.Target;
				Vector3 ai_elbow = sol.Pose.LeftElbow.Target;
				Vector3 ai_hand = sol.Pose.LeftHand.Target;
				
				if (!sol.TrackLeftHand) {
					Vector3 direction = new Vector3(-0.5f, -2, 0).normalized;
					LeftElbow = Vector3.Slerp(LeftElbow, direction * se_dist, Settings.TrackingInterpolation);
					LeftHand = Vector3.Slerp(LeftHand, direction * eh_dist, Settings.TrackingInterpolation);
				} else {
					LeftElbow = Vector3.Slerp(LeftElbow, (ai_shoulder - ai_elbow).normalized * se_dist, Settings.TrackingInterpolation);
					LeftHand = Vector3.Slerp(LeftHand, (ai_elbow - ai_hand).normalized * eh_dist, Settings.TrackingInterpolation);
				}

				Vector3 lElbow = shoulder.position + LeftElbow;
				Vector3 lHand = lElbow + LeftHand;
				
				rigger.leftHandHint.position = lElbow;
				rigger.leftHandTarget.position = lHand;
				rigger.leftHandTarget.rotation = sol.LeftHand.Wrist.Current;
				
				if (!sol.LeftHand.Wrist.HasValue(time) || !BoneSettings.Get(BoneSettings.LEFT_WRIST)) {
					rigger.leftHandTarget.rotation = elbow.rotation;
				}

				if (!BoneSettings.Get(BoneSettings.LEFT_ARM)) {
					rigger.leftHandHint.position = new(-0.5f, -1, 0);
					rigger.leftHandTarget.position = new(-0.5f, -1, 0);
				}

				if (!sol.TrackLeftHand) {
					rigger.leftHandTarget.rotation = BoneSettings.GetDefaultRotation(HumanBodyBones.LeftUpperArm);
				}
			}

			{
				Transform shoulder = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
				Transform elbow = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
				Transform hand = anim.GetBoneTransform(HumanBodyBones.RightHand);
				float se_dist = Vector3.Distance(shoulder.position, elbow.position) + ArmJointOffset;
				float eh_dist = Vector3.Distance(elbow.position, hand.position) + ArmJointOffset;

				Vector3 ai_shoulder = sol.Pose.RightShoulder.Target;
				Vector3 ai_elbow = sol.Pose.RightElbow.Target;
				Vector3 ai_hand = sol.Pose.RightHand.Target;

				if (!sol.TrackRightHand) {
					Vector3 direction = new Vector3(0.5f, -2, 0).normalized;
					RightElbow = Vector3.Slerp(RightElbow, direction * se_dist, Settings.TrackingInterpolation);
					RightHand = Vector3.Slerp(RightHand, direction * eh_dist, Settings.TrackingInterpolation);
				} else {
					RightElbow = Vector3.Slerp(RightElbow, (ai_shoulder - ai_elbow).normalized * se_dist, Settings.TrackingInterpolation);
					RightHand = Vector3.Slerp(RightHand, (ai_elbow - ai_hand).normalized * eh_dist, Settings.TrackingInterpolation);
				}

				Vector3 rElbow = shoulder.position + RightElbow;
				Vector3 rHand = rElbow + RightHand;

				rigger.rightHandHint.position = rElbow;
				rigger.rightHandTarget.position = rHand;
				rigger.rightHandTarget.rotation = sol.RightHand.Wrist.Current;

				if (!sol.RightHand.Wrist.HasValue(time) || !BoneSettings.Get(BoneSettings.RIGHT_WRIST)) {
					rigger.rightHandTarget.rotation = elbow.rotation;
				}

				if (!BoneSettings.Get(BoneSettings.RIGHT_ARM)) {
					rigger.rightHandHint.position = new(0.5f, -1, 0);
					rigger.rightHandTarget.position = new(0.5f, -1, 0);
				}

				if (!sol.TrackRightHand) {
					rigger.rightHandTarget.rotation = BoneSettings.GetDefaultRotation(HumanBodyBones.RightUpperArm);
				}
			}
		}
		
		/*
		void FixedUpdate() {
			if (!sol.IsPaused) {
				PerformRigging();
			}

			sol.UpdateModel();
		}
		*/

		void Update() {
			if (!sol.IsPaused) {
				PerformRigging();
			} else {
				// If the app is paused reset the rig hand positions
				rigger.leftHandHint.SetPositionAndRotation(new(-0.5f, -1, 0), BoneSettings.DefaultLeftArm);
				rigger.leftHandTarget.SetPositionAndRotation(new(-0.5f, -1, 0), BoneSettings.DefaultLeftArm);
				rigger.rightHandHint.SetPositionAndRotation(new(0.5f, -1, 0), BoneSettings.DefaultRightArm);
				rigger.rightHandTarget.SetPositionAndRotation(new(0.5f, -1, 0), BoneSettings.DefaultRightArm);
			}

			UpdateFromReceiver();
			sol.UpdateModel();
			sol.AnimateModel();
		}
		
		void UpdateFromReceiver() {
			// Make sure the hand constraints are not applied when they are turned off
			sol.Model.RigAnimator.UseHandIK(true, BoneSettings.Get(BoneSettings.LEFT_ARM));
			sol.Model.RigAnimator.UseHandIK(false, BoneSettings.Get(BoneSettings.RIGHT_ARM));

			// Apply VMC Receiver
			VMCReceiver receiver = VMCReceiver.Receiver;
			if (receiver.IsRunning()) {
				Dictionary<BlendShapeKey, float> dict = null;
				// First save all blend shapes to make sure we do not overwrite if the face is not applied
				if (!BoneSettings.Get(BoneSettings.FACE)) {
					// Overwrite all blend shape changes	
					dict = new(sol.Model.BlendShapeProxy.GetValues());
				}
				
				receiver.vmcReceiver.Update();

				// Get all the transforms and apply them to the OverwriteTransforms
				foreach (var entry in sol.Model.ModelBones) {
					if (!BoneSettings.CanExternalModify(entry.Key)) {
						continue;
					}

					if (sol.Model.Transforms.TryGetValue(entry.Key, out var trans) && trans != null) {
						// Rotate the transform based on the space
						if (trans.data.space == OverrideTransformData.Space.World) {
							trans.data.rotation = entry.Value.rotation.eulerAngles;
						} else if (trans.data.space == OverrideTransformData.Space.Local) {
							trans.data.rotation = entry.Value.localRotation.eulerAngles;
						}
					}
				}

				if (dict != null) {
					sol.Model.BlendShapeProxy.SetValues(dict);
				}
			}
		}
	}
}
