using UnityEngine;

namespace HardCoded.VRigUnity {
	public class VRMAnimator : MonoBehaviour {
		private RigAnimator rigger;
		private Animator anim;
		
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
			HolisticTrackingSolution sol = SolutionUtils.GetSolution();

			{
				Transform shoulder = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
				Transform elbow = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
				Transform hand = anim.GetBoneTransform(HumanBodyBones.RightHand);
				float se_dist = Vector3.Distance(shoulder.position, elbow.position);
				float eh_dist = Vector3.Distance(elbow.position, hand.position);

				Vector3 ai_shoulder = sol.Pose.LeftShoulder.Get();
				Vector3 ai_elbow = sol.Pose.LeftElbow.Get();
				Vector3 ai_hand = sol.Pose.LeftHand.Get();
				
				Vector3 fit_shoulder = shoulder.position;
				Vector3 fit_elbow = fit_shoulder + (ai_shoulder - ai_elbow).normalized * se_dist;
				Vector3 fit_hand = fit_elbow + (ai_elbow - ai_hand).normalized * eh_dist;

				LeftElbow = Vector3.Lerp(LeftElbow, fit_elbow, 0.2f);
				LeftHand = Vector3.Lerp(LeftHand, fit_hand, 0.2f);

				rigger.leftHandHint.position = LeftElbow;
				rigger.leftHandTarget.position = LeftHand;
				rigger.leftHandTarget.rotation = sol.LeftHand.Wrist.GetLastRotation();
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
				
				Vector3 fit_shoulder = shoulder.position;
				Vector3 fit_elbow = fit_shoulder + (ai_shoulder - ai_elbow).normalized * se_dist;
				Vector3 fit_hand = fit_elbow + (ai_elbow - ai_hand).normalized * eh_dist;

				RightElbow = Vector3.Lerp(RightElbow, fit_elbow, 0.2f);
				RightHand = Vector3.Lerp(RightHand, fit_hand, 0.2f);

				rigger.rightHandHint.position = RightElbow;
				rigger.rightHandTarget.position = RightHand;
				rigger.rightHandTarget.rotation = sol.RightHand.Wrist.GetLastRotation();
			}
		}

		void LateUpdate() {
			if (Settings.UseFullIK) {
				anim.runtimeAnimatorController = controller;
				PerformRigging();
			} else {
				if (anim.runtimeAnimatorController != null) {
					anim.WriteDefaultValues();
				}
				anim.runtimeAnimatorController = null;
			}

			// anim.WriteDefaultValues();
			SolutionUtils.GetSolution().ModelUpdate();
		}
	}
}
