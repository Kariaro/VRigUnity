using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace HardCoded.VRigUnity {
	public class RigAnimator : MonoBehaviour {
		private Animator anim;
		
		[Header("Hands")]
		public Transform leftHandTarget;
		public Transform leftHandHint;
		public Transform rightHandTarget;
		public Transform rightHandHint;

		[Header("Legs")]
		public Transform leftLegTarget;
		public Transform leftLegHint;
		public Transform rightLegTarget;
		public Transform rightLegHint;

		public void SetupRigging() {
			anim = GetComponent<Animator>();

			// Only if we are in editor we should add a bone renderer
#if UNITY_EDITOR
			BoneRenderer boneRenderer = gameObject.AddComponent<BoneRenderer>();
			{
				var renderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
				var bones = new List<Transform>();
				if (renderers != null && renderers.Length > 0) {
					for (int i = 0; i < renderers.Length; i++) {
						var renderer = renderers[i];
						for (int j = 0; j < renderer.bones.Length; j++) {
							var bone = renderer.bones[j];
							if (!bones.Contains(bone)) {
								bones.Add(bone);
								for (int k = 0; k < bone.childCount; k++) {
									if (!bones.Contains(bone.GetChild(k))) {
										bones.Add(bone.GetChild(k));
									}
								}
							}
						}
					}
				} else {
					bones.AddRange(transform.GetComponentsInChildren<Transform>());
				}

				boneRenderer.transforms = bones.ToArray();
			}
#endif

			RigBuilder rigBuilder = gameObject.AddComponent<RigBuilder>();

			GameObject rigObject = new("Rig");
			rigObject.transform.SetParent(transform);

			Rig rig = rigObject.AddComponent<Rig>();
			rigBuilder.layers.Add(new RigLayer(rig));

			SetupHands(rig);
			// SetupLegs(rig);

			rigBuilder.Build();
		}

		public void SetupHands(Rig rig) {
			{ // Left Hand
				GameObject hand = new("Left Hand");
				GameObject target = new("Target");
				GameObject hint = new("Hint");
				hand.transform.SetParent(rig.transform);
				target.transform.SetParent(hand.transform);
				hint.transform.SetParent(hand.transform);
				
				leftHandTarget = target.transform;
				leftHandHint = hint.transform;

				TwoBoneIKConstraint constraint = hand.AddComponent<TwoBoneIKConstraint>();
				constraint.data.root = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
				constraint.data.mid = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
				constraint.data.tip = anim.GetBoneTransform(HumanBodyBones.RightHand);
				constraint.data.target = leftHandTarget;
				constraint.data.hint = leftHandHint;
				constraint.data.targetPositionWeight = 1f;
				constraint.data.targetRotationWeight = 1f;
				constraint.data.hintWeight = 1f;
			}

			{ // Right Hand
				GameObject hand = new("Right Hand");
				GameObject target = new("Target");
				GameObject hint = new("Hint");
				hand.transform.SetParent(rig.transform);
				target.transform.SetParent(hand.transform);
				hint.transform.SetParent(hand.transform);
				
				rightHandTarget = target.transform;
				rightHandHint = hint.transform;

				TwoBoneIKConstraint constraint = hand.AddComponent<TwoBoneIKConstraint>();
				constraint.data.root = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
				constraint.data.mid = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
				constraint.data.tip = anim.GetBoneTransform(HumanBodyBones.LeftHand);
				constraint.data.target = rightHandTarget;
				constraint.data.hint = rightHandHint;
				constraint.data.targetPositionWeight = 1f;
				constraint.data.targetRotationWeight = 1f;
				constraint.data.hintWeight = 1f;
			}
		}

		public void SetupLegs(Rig rig) {
			{ // Left Leg
				GameObject leg = new("Left Leg");
				GameObject target = new("Target");
				GameObject hint = new("Hint");
				leg.transform.SetParent(rig.transform);
				target.transform.SetParent(leg.transform);
				hint.transform.SetParent(leg.transform);
				
				leftLegTarget = target.transform;
				leftLegHint = hint.transform;

				TwoBoneIKConstraint constraint = leg.AddComponent<TwoBoneIKConstraint>();
				constraint.data.root = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
				constraint.data.mid = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
				constraint.data.tip = anim.GetBoneTransform(HumanBodyBones.RightFoot);
				constraint.data.target = leftLegTarget;
				constraint.data.hint = leftLegHint;
				constraint.data.targetPositionWeight = 1f;
				constraint.data.targetRotationWeight = 1f;
				constraint.data.hintWeight = 1f;
			}

			{ // Right Leg
				GameObject leg = new("Right Leg");
				GameObject target = new("Target");
				GameObject hint = new("Hint");
				leg.transform.SetParent(rig.transform);
				target.transform.SetParent(leg.transform);
				hint.transform.SetParent(leg.transform);
				
				rightLegTarget = target.transform;
				rightLegHint = hint.transform;

				TwoBoneIKConstraint constraint = leg.AddComponent<TwoBoneIKConstraint>();
				constraint.data.root = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
				constraint.data.mid = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
				constraint.data.tip = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
				constraint.data.target = rightLegTarget;
				constraint.data.hint = rightLegHint;
				constraint.data.targetPositionWeight = 1f;
				constraint.data.targetRotationWeight = 1f;
				constraint.data.hintWeight = 1f;
			}
		}
	}
}
