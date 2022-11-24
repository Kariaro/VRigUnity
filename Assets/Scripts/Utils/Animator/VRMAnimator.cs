using UnityEngine;

namespace HardCoded.VRigUnity {
	public class VRMAnimator : MonoBehaviour {
		private Animator anim;
		public RuntimeAnimatorController controller;

		public Vector3 VecC;
		public Vector3 VecD;
		
		void Start() {
			anim = GetComponent<Animator>();
		}

		void OnAnimatorIK(int layerIndex) {
			HolisticTrackingSolution sol = SolutionUtils.GetSolution();

			// By default the IK is always in global
			anim.SetIKRotation(AvatarIKGoal.RightHand, sol.LeftHand.Wrist.GetLastRotation() * Quaternion.Euler(0, 90, 0));
			anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);

			anim.SetIKRotation(AvatarIKGoal.LeftHand, sol.RightHand.Wrist.GetLastRotation() * Quaternion.Euler(0, -90, 0));
			anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);

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

				VecC = Vector3.Lerp(VecC, fit_hand, 0.2f);

				anim.SetIKPosition(AvatarIKGoal.RightHand, VecC);
				anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
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

				VecD = Vector3.Lerp(VecD, fit_hand, 0.2f);

				anim.SetIKPosition(AvatarIKGoal.LeftHand, VecD);
				anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
			}
		}

		void LateUpdate() {
			if (Settings.UseFullIK) {
				anim.runtimeAnimatorController = controller;
			} else {
				anim.runtimeAnimatorController = null;
			}

			// anim.WriteDefaultValues();
			SolutionUtils.GetSolution().ModelUpdate();
		}
	}
}
