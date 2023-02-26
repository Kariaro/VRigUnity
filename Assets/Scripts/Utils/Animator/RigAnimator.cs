using System;
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

		// Hand constraints (First element is IK)
		private readonly IRigConstraint[] leftHandConstraints = new IRigConstraint[4];
		private readonly IRigConstraint[] rightHandConstraints = new IRigConstraint[4];

		// API Getters
		private readonly Dictionary<HumanBodyBones, OverrideTransform> _transforms = new();
		public Dictionary<HumanBodyBones, OverrideTransform> Transforms => _transforms;

		public void SetupRigging() {
			anim = GetComponent<Animator>();

#if UNITY_EDITOR
			// Only if we are in editor we should add a bone renderer
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
			
			GameObject rigOffsetObject = NewObject("RigLayerOffset");
			rigOffsetObject.transform.SetParent(transform);			
			Rig offsetRig = rigOffsetObject.AddComponent<Rig>();
			rigBuilder.layers.Add(new RigLayer(offsetRig));
			
			GameObject rigObject = NewObject("RigLayerHands");
			rigObject.transform.SetParent(transform);
			Rig handRig = rigObject.AddComponent<Rig>();
			rigBuilder.layers.Add(new RigLayer(handRig));
			
			SetupOffsets(offsetRig);
			SetupHands(handRig);
			// SetupLegs(rig);

			// To make this the root transform
			rigBuilder.Build();

			// By default both hands use IK
			UseHandIK(true, false);
			UseHandIK(false, false);

			SolutionUtils.GetSolution().Model.ResetVRMAnimator();
		}

		public void UseHandIK(bool leftHand, bool enable) {
			var constraints = leftHand ? leftHandConstraints : rightHandConstraints;
			for (int i = 0; i < constraints.Length; i++) {
				var constraint = constraints[i];
				constraint.weight = (i > 0) ^ enable ? 1 : 0;
			}
		}

		private void SetupOffsets(Rig rig) {
			_transforms.Clear();

			GameObject offsets = NewObject("Offsets");
			offsets.transform.SetParent(rig.transform);
			
			// Create OverrideTransforms for all bones
			foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones))) {
				if (bone == HumanBodyBones.LastBone) {
					continue;
				}

				CreateBone(offsets, bone);
			}

			// Create constraints map
			leftHandConstraints[1] = _transforms[HumanBodyBones.LeftUpperArm];
			leftHandConstraints[2] = _transforms[HumanBodyBones.LeftLowerArm];
			leftHandConstraints[3] = _transforms[HumanBodyBones.LeftShoulder];
			rightHandConstraints[1] = _transforms[HumanBodyBones.RightUpperArm];
			rightHandConstraints[2] = _transforms[HumanBodyBones.RightLowerArm];
			rightHandConstraints[3] = _transforms[HumanBodyBones.RightShoulder];
		}

		private GameObject NewObject(string name) {
			return new("vrigunity:object:" + name);
		}

		private IRigConstraint CreateBone(GameObject parent, HumanBodyBones bone, bool local = true) {
			GameObject element = NewObject(bone.ToString());
			element.transform.SetParent(parent.transform);

			var offset = element.AddComponent<OverrideTransform>();
			offset.data.constrainedObject = anim.GetBoneTransform(bone);
			offset.data.space = local ? OverrideTransformData.Space.Local : OverrideTransformData.Space.World;
			offset.data.rotationWeight = 1;
			_transforms.Add(bone, offset);

			return offset;
		}

		private void SetupHands(Rig rig) {
			{ // Left Hand
				GameObject hand = NewObject("LeftHandIK");
				GameObject target = NewObject("Left Target");
				GameObject hint = NewObject("Left Hint");
				hand.transform.SetParent(rig.transform);
				target.transform.SetParent(hand.transform);
				hint.transform.SetParent(hand.transform);
				
				leftHandTarget = target.transform;
				leftHandHint = hint.transform;

				TwoBoneIKConstraint constraint = hand.AddComponent<TwoBoneIKConstraint>();
				constraint.data.root = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
				constraint.data.mid = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
				constraint.data.tip = anim.GetBoneTransform(HumanBodyBones.LeftHand);
				constraint.data.target = leftHandTarget;
				constraint.data.hint = leftHandHint;
				constraint.data.targetPositionWeight = 1f;
				constraint.data.targetRotationWeight = 1f;
				constraint.data.hintWeight = 1f;
				leftHandConstraints[0] = constraint;
			}

			{ // Right Hand
				GameObject hand = NewObject("RightHandIK");
				GameObject target = NewObject("Right Target");
				GameObject hint = NewObject("Right Hint");
				hand.transform.SetParent(rig.transform);
				target.transform.SetParent(hand.transform);
				hint.transform.SetParent(hand.transform);
				
				rightHandTarget = target.transform;
				rightHandHint = hint.transform;

				TwoBoneIKConstraint constraint = hand.AddComponent<TwoBoneIKConstraint>();
				constraint.data.root = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
				constraint.data.mid = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
				constraint.data.tip = anim.GetBoneTransform(HumanBodyBones.RightHand);
				constraint.data.target = rightHandTarget;
				constraint.data.hint = rightHandHint;
				constraint.data.targetPositionWeight = 1f;
				constraint.data.targetRotationWeight = 1f;
				constraint.data.hintWeight = 1f;
				rightHandConstraints[0] = constraint;
			}
		}

		private void SetupLegs(Rig rig) {
			{ // Left Leg
				GameObject leg = NewObject("Left Leg");
				GameObject target = NewObject("Target");
				GameObject hint = NewObject("Hint");
				leg.transform.SetParent(rig.transform);
				target.transform.SetParent(leg.transform);
				hint.transform.SetParent(leg.transform);
				
				leftLegTarget = target.transform;
				leftLegHint = hint.transform;

				TwoBoneIKConstraint constraint = leg.AddComponent<TwoBoneIKConstraint>();
				constraint.data.root = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
				constraint.data.mid = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
				constraint.data.tip = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
				constraint.data.target = leftLegTarget;
				constraint.data.hint = leftLegHint;
				constraint.data.targetPositionWeight = 1f;
				constraint.data.targetRotationWeight = 1f;
				constraint.data.hintWeight = 1f;
			}

			{ // Right Leg
				GameObject leg = NewObject("Right Leg");
				GameObject target = NewObject("Target");
				GameObject hint = NewObject("Hint");
				leg.transform.SetParent(rig.transform);
				target.transform.SetParent(leg.transform);
				hint.transform.SetParent(leg.transform);
				
				rightLegTarget = target.transform;
				rightLegHint = hint.transform;

				TwoBoneIKConstraint constraint = leg.AddComponent<TwoBoneIKConstraint>();
				constraint.data.root = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
				constraint.data.mid = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
				constraint.data.tip = anim.GetBoneTransform(HumanBodyBones.RightFoot);
				constraint.data.target = rightLegTarget;
				constraint.data.hint = rightLegHint;
				constraint.data.targetPositionWeight = 1f;
				constraint.data.targetRotationWeight = 1f;
				constraint.data.hintWeight = 1f;
			}
		}
	}
}
