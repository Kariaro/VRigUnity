using UnityEngine;

namespace HardCoded.VRigUnity {
	public class VRMAnimator : MonoBehaviour {
		private RigAnimator rigger;
		private Animator anim;
		
		public bool slerp;
		public RuntimeAnimatorController controller;
		public Vector3 LeftElbow;
		public Vector3 LeftHand;
		public Vector3 RightElbow;
		public Vector3 RightHand;

		void Start() {
			anim = GetComponent<Animator>();
			rigger = gameObject.AddComponent<RigAnimator>();
			rigger.SetupRigging();
		}

		void PerformRigging() {
			HolisticSolution sol = SolutionUtils.GetSolution();
			float time = sol.TimeNow;
			// TODO: Fix hands when full body moves

			{
				Transform shoulder = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
				Transform elbow = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
				Transform hand = anim.GetBoneTransform(HumanBodyBones.RightHand);
				float se_dist = Vector3.Distance(shoulder.position, elbow.position);
				float eh_dist = Vector3.Distance(elbow.position, hand.position);

				Vector3 ai_shoulder = sol.Pose.LeftShoulder.Get();
				Vector3 ai_elbow = sol.Pose.LeftElbow.Get();
				Vector3 ai_hand = sol.Pose.LeftHand.Get();
				
				if (!sol.TrackRightHand) {
					Vector3 direction = new Vector3(0.5f, -2, 0).normalized;
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
				rigger.leftHandTarget.rotation = sol.LeftHand.Wrist.GetLastRotation();
				
				if (!sol.LeftHand.Wrist.HasValue(time) || !BoneSettings.Get(BoneSettings.RIGHT_WRIST)) {
					rigger.leftHandTarget.rotation = elbow.rotation;
				}

				if (!BoneSettings.Get(BoneSettings.RIGHT_ARM)) {
					rigger.leftHandHint.position = new(0.5f, -1, 0);
					rigger.leftHandTarget.position = new(0.5f, -1, 0);
				}

				if (!sol.TrackRightHand) {
					rigger.leftHandTarget.rotation = Quaternion.identity;
				}
			}

			{
				Transform shoulder = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
				Transform elbow = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
				Transform hand = anim.GetBoneTransform(HumanBodyBones.LeftHand);
				float se_dist = Vector3.Distance(shoulder.position, elbow.position);
				float eh_dist = Vector3.Distance(elbow.position, hand.position);

				Vector3 ai_shoulder = sol.Pose.RightShoulder.Get();
				Vector3 ai_elbow = sol.Pose.RightElbow.Get();
				Vector3 ai_hand = sol.Pose.RightHand.Get();

				if (!sol.TrackLeftHand) {
					Vector3 direction = new Vector3(-0.5f, -2, 0).normalized;
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
				rigger.rightHandTarget.rotation = sol.RightHand.Wrist.GetLastRotation();

				if (!sol.RightHand.Wrist.HasValue(time) || !BoneSettings.Get(BoneSettings.LEFT_WRIST)) {
					rigger.rightHandTarget.rotation = elbow.rotation;
				}

				if (!BoneSettings.Get(BoneSettings.LEFT_ARM)) {
					rigger.rightHandHint.position = new(-0.5f, -1, 0);
					rigger.rightHandTarget.position = new(-0.5f, -1, 0);
				}

				if (!sol.TrackLeftHand) {
					rigger.rightHandTarget.rotation = Quaternion.identity;
				}
			}
		}

		void LateUpdate() {
			if (Settings.UseFullIK) {
				anim.runtimeAnimatorController = controller;
				PerformRigging();
			} else {
				anim.runtimeAnimatorController = null;
				anim.WriteDefaultValues();
			}
			
			SolutionUtils.GetSolution().ModelUpdate();

			// Apply VMC Receiver
			VMCReceiver receiver = VMCReceiver.Receiver;
			if (receiver.IsRunning()) {
				receiver.vmcReceiver.ModelUpdate();
			}
		}
	}
}
