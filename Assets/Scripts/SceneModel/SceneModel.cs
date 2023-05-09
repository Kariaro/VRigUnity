using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using VRM;

namespace HardCoded.VRigUnity {
	/// <summary>
	/// A simplification of the model handling. This class contains methods to change and update the model
	/// </summary>
	public class SceneModel {
		// Cache
		private readonly Dictionary<BlendShapePreset, BlendShapeKey> blendShapeCache = new();
		private readonly Dictionary<HumanBodyBones, Transform> boneTransformCache = new();
		private readonly RuntimeAnimatorController m_defaultController;
		private readonly GameObject m_defaultModelPrefab;
		
		// Internal
		private bool visible = true; // Always visible by default

		// API Getters
		public Dictionary<BlendShapePreset, BlendShapeKey> BlendShapes => blendShapeCache;
		public Dictionary<HumanBodyBones, Transform> ModelBones => boneTransformCache;
		public Dictionary<HumanBodyBones, OverrideTransform> Transforms => RigAnimator.Transforms;
		public bool IsVisible {
			get => visible;
			set {
				if (value != visible) {
					visible = value;
					UpdateVisibility();
				}
			}
		}

		/// <summary>
		/// Returns if the model is prepared and can be modified
		/// </summary>
		public bool IsPrepared
			=> VrmModel != null
			&& VrmAnimator != null
			&& RigAnimator != null
			&& Animator != null
			&& BlendShapeProxy != null;

		public VRMBlendShapeProxy BlendShapeProxy { get; private set; }
		public VRMAnimator VrmAnimator { get; private set; }
		public RigAnimator RigAnimator { get; private set; }
		public Animator Animator { get; private set; }
		public GameObject VrmModel { get; private set; }

		public SceneModel(GameObject model, GameObject defaultModelPrefab, RuntimeAnimatorController defaultController) {
			if (model == null || defaultModelPrefab == null || defaultController == null) {
				throw new ArgumentNullException("SceneModel requires non null parameters");
			}

			// Set default values
			m_defaultModelPrefab = defaultModelPrefab;
			m_defaultController = defaultController;

			// Update the model and compute cache
			SetVRMModel(model);
		}

		/// <summary>
		/// Reset the current model to the default model
		/// </summary>
		public void ResetVRMModel() {
			SetVRMModel(UnityEngine.Object.Instantiate(m_defaultModelPrefab));
		}

		/// <summary>
		/// Update the vrm model to a new model
		/// </summary>
		/// <param name="gameObject"></param>
		/// <returns>If the model was updated sucessfully</returns>
		public bool SetVRMModel(GameObject gameObject) {
			var blendShapeProxy = gameObject.GetComponent<VRMBlendShapeProxy>();
			var animator = gameObject.GetComponent<Animator>();

			if (animator == null || blendShapeProxy == null) {
				return false;
			}

			if (VrmModel != null && VrmModel != gameObject) {
				UnityEngine.Object.Destroy(VrmModel);
			}
			
			VrmModel = gameObject;
			VrmAnimator = gameObject.AddComponent<VRMAnimator>();
			RigAnimator = gameObject.AddComponent<RigAnimator>();
			BlendShapeProxy = blendShapeProxy;
			Animator = animator;
			Animator.runtimeAnimatorController = m_defaultController;
			
			UpdateCaches();
			UpdateVisibility();

			return true;
		}
		
		private void UpdateCaches() {
			// Update bone transform cache
			boneTransformCache.Clear();
			foreach (HumanBodyBones item in Enum.GetValues(typeof(HumanBodyBones))) {
				if (item == HumanBodyBones.LastBone) {
					continue;
				}

				Transform tr = Animator.GetBoneTransform(item);
				if (tr != null) {
					boneTransformCache.Add(item, tr);
				}
			}

			// Update blend shape cache
			blendShapeCache.Clear();
			foreach (BlendShapePreset item in Enum.GetValues(typeof(BlendShapePreset))) {
				blendShapeCache.Add(item, BlendShapeKey.CreateFromPreset(item));
			}
		}

		private void UpdateVisibility() {
			int nextLayer = visible ? 0 : LayerMask.NameToLayer("HiddenModel");
			foreach (var transform in VrmModel.GetComponentsInChildren<Transform>()) {
				transform.gameObject.layer = nextLayer;
			}
		}

		/// <summary>
		/// Used with bone settings
		/// </summary>
		public void OnBoneUpdate(int index, bool set) {
			foreach (HumanBodyBones bone in BoneSettings.GetBones(index)) {
				// If the bone is present in the OverrideTransform list
				if (RigAnimator.Transforms.TryGetValue(bone, out var trans)) {
					trans.data.rotation = BoneSettings.GetDefaultRotation(bone).eulerAngles;
				}
			}

			if (index == BoneSettings.FACE) {
				BlendShapeProxy.ImmediatelySetValue(BlendShapes[BlendShapePreset.O], 0);
				BlendShapeProxy.ImmediatelySetValue(BlendShapes[BlendShapePreset.Blink_L], 0);
				BlendShapeProxy.ImmediatelySetValue(BlendShapes[BlendShapePreset.Blink_R], 0);
			}
		}

		public void ResetVRMAnimator() {
			// TODO: What does rebind do?
			Animator.Rebind();
			foreach (var item in blendShapeCache) {
				BlendShapeProxy.ImmediatelySetValue(item.Value, 0);
			}

			// Reset all bone rotations
			for (int i = 0; i < BoneSettings.Count; i++) {
				OnBoneUpdate(i, true);
			}
		}
	}
}
