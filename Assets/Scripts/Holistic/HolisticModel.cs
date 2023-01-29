using System;
using System.Collections.Generic;
using UnityEngine;
using VRM;

namespace HardCoded.VRigUnity {
	/// <summary>
	/// A simplification of the model handling. This class contains methods to change and update the model
	/// </summary>
	public class HolisticModel {
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

		public HolisticModel(GameObject model, GameObject defaultModelPrefab, RuntimeAnimatorController defaultController) {
			if (model == null || defaultModelPrefab == null || defaultController == null) {
				throw new ArgumentNullException("HolisticModel requires non null parameters");
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
			VrmAnimator.controller = m_defaultController;
			RigAnimator = gameObject.AddComponent<RigAnimator>();
			BlendShapeProxy = blendShapeProxy;
			Animator = animator;
			
			UpdateCaches();
			UpdateVisibility();
			DefaultVRMAnimator();

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

		public void DefaultVRMAnimator() {
			foreach (var entry in boneTransformCache) {
				entry.Value.localRotation = BoneSettings.GetDefaultRotation(entry.Key);
			}
		}

		// Called when a bone is selected or deselected
		public void OnBoneUpdate(int index, bool set) {
			// Our program should now track the bone
			// Make sure all parts are cleared
			foreach (HumanBodyBones bone in BoneSettings.GetBones(index)) {
				if (!boneTransformCache.ContainsKey(bone)) {
					continue;
				}

				Transform trans = boneTransformCache[bone];
				if (trans != null) {
					trans.localRotation = BoneSettings.GetDefaultRotation(bone);
				}
			}
		}

		public void ResetVRMAnimator() {
			// TODO: What does rebind do?
			Animator.Rebind();
			foreach (var item in blendShapeCache) {
				BlendShapeProxy.ImmediatelySetValue(item.Value, 0);
			}
		}
	}
}
