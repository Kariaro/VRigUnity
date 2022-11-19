using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class VRMAnimator : MonoBehaviour {
		private Animator anim;
		public RuntimeAnimatorController controller;

		void Start() {
			anim = GetComponent<Animator>();
			
			// Disable controller
			// anim.runtimeAnimatorController = controller;
			// anim.enabled = false;
		}

		void OnAnimatorIK(int layerIndex) {
			// IK
			// anim.SetIKPosition(AvatarIKGoal.RightFoot, new Vector3(0, Mathf.Sin((Time.time / 100.0f) % 100), 0));
			// anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
		}

		void LateUpdate() {
			// anim.WriteDefaultValues();
			SolutionUtils.GetSolution().ModelUpdate();
		}
	}
}
