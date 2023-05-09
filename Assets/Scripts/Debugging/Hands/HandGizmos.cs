using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class HandGizmos : MonoBehaviour {
		private readonly HumanBodyBones[] LEFT_HAND = {
			HumanBodyBones.LeftHand,
			HumanBodyBones.LeftIndexProximal,
			HumanBodyBones.LeftIndexIntermediate,
			HumanBodyBones.LeftIndexDistal,
			HumanBodyBones.LeftMiddleProximal,
			HumanBodyBones.LeftMiddleIntermediate,
			HumanBodyBones.LeftMiddleDistal,
			HumanBodyBones.LeftRingProximal,
			HumanBodyBones.LeftRingIntermediate,
			HumanBodyBones.LeftRingDistal,
			HumanBodyBones.LeftLittleProximal,
			HumanBodyBones.LeftLittleIntermediate,
			HumanBodyBones.LeftLittleDistal,
			HumanBodyBones.LeftThumbProximal,
			HumanBodyBones.LeftThumbIntermediate,
			HumanBodyBones.LeftThumbDistal,
		};

		private readonly HumanBodyBones[] RIGHT_HAND = {
			HumanBodyBones.RightHand,
			HumanBodyBones.RightIndexProximal,
			HumanBodyBones.RightIndexIntermediate,
			HumanBodyBones.RightIndexDistal,
			HumanBodyBones.RightMiddleProximal,
			HumanBodyBones.RightMiddleIntermediate,
			HumanBodyBones.RightMiddleDistal,
			HumanBodyBones.RightRingProximal,
			HumanBodyBones.RightRingIntermediate,
			HumanBodyBones.RightRingDistal,
			HumanBodyBones.RightLittleProximal,
			HumanBodyBones.RightLittleIntermediate,
			HumanBodyBones.RightLittleDistal,
			HumanBodyBones.RightThumbProximal,
			HumanBodyBones.RightThumbIntermediate,
			HumanBodyBones.RightThumbDistal,
		};

		[SerializeField] [Range(0.0001f, 0.03f)] private float lineLength = 0.015f;
		[SerializeField] [Range(0, 1)] private float alpha = 0.25f;
		[SerializeField] private bool rightHand = true;
		[SerializeField] private bool leftHand = true;
		private SceneModel vrmModel;

		void Start() {
			vrmModel = SolutionUtils.GetSolution().Model;
		}

		void Update() {
			// Make sure the model is semi transparent
			SkinnedMeshRenderer[] array = vrmModel.VrmModel.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer rend in array) {
				rend.material.SetOverrideTag("RenderType", "Transparent");
				rend.material.DisableKeyword("_ALPHATEST_ON");
				rend.material.EnableKeyword("_ALPHABLEND_ON");
				rend.material.SetFloat("_AlphaToMask", 0);
				rend.material.SetFloat("_BlendMode", 2);
				rend.material.SetFloat("_DstBlend", 10);
				rend.material.SetFloat("_SrcBlend", 5);
				rend.material.SetFloat("_ZWrite", 0);
				rend.material.SetColor("_Color", new Color(1, 1, 1, alpha));
				rend.material.renderQueue = 3000;
			}

			Animator animator = vrmModel.Animator;
			if (rightHand) DrawHandGizmo(animator, RIGHT_HAND);
			if (leftHand) DrawHandGizmo(animator, LEFT_HAND);
		}

		void DrawHandGizmo(Animator animator, HumanBodyBones[] handBones) {
			foreach (HumanBodyBones bone in handBones) {
				Transform trans = animator.GetBoneTransform(bone);
				Debug.DrawLine(trans.position, trans.position + (trans.rotation * Vector3.up) * lineLength, Color.green);
				Debug.DrawLine(trans.position, trans.position + (trans.rotation * Vector3.forward) * lineLength, Color.blue);
				Debug.DrawLine(trans.position, trans.position + (trans.rotation * Vector3.right) * lineLength, Color.red);
			}
		}
	}
}