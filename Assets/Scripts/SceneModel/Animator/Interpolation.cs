using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace HardCoded.VRigUnity {
	public struct DiscreteRotStruct {
		// Internal
		private readonly HumanBodyBones m_bone;
		private readonly Quaternion m_lostFocus;

		private Quaternion m_current;
		private Quaternion m_target;
		private float m_targetTime;
		private bool m_hasFocus;

		// Public
		public Quaternion Current => m_current;

		public DiscreteRotStruct(HumanBodyBones bone) {
			m_bone = bone;
			m_lostFocus = BoneSettings.GetDefaultRotation(bone);
			m_current = Quaternion.identity;
			m_target = Quaternion.identity;
			m_hasFocus = false;
			m_targetTime = 0;
		}

		public bool HasValue(float time) {
			return time - 1 < m_targetTime;
		}

		/// <summary>
		/// Add a quaternion rotation with a specified time
		/// </summary>
		/// <param name="value">The rotation</param>
		/// <param name="time">The time</param>
		public void Add(Quaternion value, float time) {
			m_target = value;
			m_targetTime = time;
		}

		public void Update(float time) {
			// Implement spherical B-spline interpolation?
			
			// 60 fps is the default speed so this should == 1
			// If we have 120 fps this would be == 0.5
			// If we have  30 fps this would be == 2.0
			float td = Time.deltaTime * 60;
			float iv = td * Settings.TrackingInterpolation;

			m_hasFocus = HasValue(time);
			m_current = Quaternion.Slerp(m_current, m_hasFocus ? m_target : m_lostFocus, iv);
		}

		public void ApplyLocal(OverrideTransform transform) {
			transform.data.rotation = m_current.eulerAngles;
		}

		public void ApplyLocal(SceneModel model) {
			if (model.RigAnimator.Transforms.TryGetValue(m_bone, out var transform)) {
				ApplyLocal(transform);
			}
		}

		public void ApplyGlobal(OverrideTransform transform, bool localWhenLostFocus = false) {
			transform.data.rotation = m_current.eulerAngles;

			if (localWhenLostFocus && !m_hasFocus) {
				transform.data.space = OverrideTransformData.Space.World;
			} else {
				transform.data.space = OverrideTransformData.Space.Local;
			}
		}

		public void ApplyGlobal(SceneModel model, bool localWhenLostFocus = false) {
			if (model.RigAnimator.Transforms.TryGetValue(m_bone, out var transform)) {
				ApplyGlobal(transform, localWhenLostFocus);
			}
		}
	}

	public struct DiscretePosStruct {
		// Internal
		private Vector3 m_current;
		private Vector3 m_target;
		private Vector3 m_lostFocus;
		private float m_targetTime;
		public Vector3 Target => m_target;
		public Vector3 Current => m_current;
		
		public DiscretePosStruct(Vector3 current, Vector3 lostFocus) {
			m_current = current;
			m_target = Vector3.zero;
			m_lostFocus = lostFocus;
			m_targetTime = 0;
		}

		/// <summary>
		/// Add a position with a specified time
		/// </summary>
		/// <param name="value">The position</param>
		/// <param name="time">The time</param>
		public void Add(Vector3 value, float time) {
			m_target = value;
			m_targetTime = time;
		}

		/// <summary>
		/// Update the internal rotation
		/// </summary>
		/// <param name="time">The time</param>
		public void Update(float time) {
			bool lostFocus = m_targetTime < time - 1;
			var target = lostFocus ? m_lostFocus : m_target;
			m_current = Vector3.Lerp(m_current, target, Settings.TrackingInterpolation);
		}

		public void Apply(Transform transform) {
			transform.position = m_current;
		}
	}

	public class HandValues {
		public DiscreteRotStruct Wrist    ;
		public DiscreteRotStruct IndexPip ;
		public DiscreteRotStruct IndexDip ;
		public DiscreteRotStruct IndexTip ;
		public DiscreteRotStruct MiddlePip;
		public DiscreteRotStruct MiddleDip;
		public DiscreteRotStruct MiddleTip;
		public DiscreteRotStruct RingPip  ;
		public DiscreteRotStruct RingDip  ;
		public DiscreteRotStruct RingTip  ;
		public DiscreteRotStruct PinkyPip ;
		public DiscreteRotStruct PinkyDip ;
		public DiscreteRotStruct PinkyTip ;
		public DiscreteRotStruct ThumbPip ;
		public DiscreteRotStruct ThumbDip ;
		public DiscreteRotStruct ThumbTip ;

		public HandValues(bool isLeft) {
			Wrist     = new(isLeft ? HumanBodyBones.LeftHand               : HumanBodyBones.RightHand              );
			IndexPip  = new(isLeft ? HumanBodyBones.LeftIndexProximal      : HumanBodyBones.RightIndexProximal     );
			IndexDip  = new(isLeft ? HumanBodyBones.LeftIndexIntermediate  : HumanBodyBones.RightIndexIntermediate );
			IndexTip  = new(isLeft ? HumanBodyBones.LeftIndexDistal        : HumanBodyBones.RightIndexDistal       );
			MiddlePip = new(isLeft ? HumanBodyBones.LeftMiddleProximal     : HumanBodyBones.RightMiddleProximal    );
			MiddleDip = new(isLeft ? HumanBodyBones.LeftMiddleIntermediate : HumanBodyBones.RightMiddleIntermediate);
			MiddleTip = new(isLeft ? HumanBodyBones.LeftMiddleDistal       : HumanBodyBones.RightMiddleDistal      );
			RingPip   = new(isLeft ? HumanBodyBones.LeftRingProximal       : HumanBodyBones.RightRingProximal      );
			RingDip   = new(isLeft ? HumanBodyBones.LeftRingIntermediate   : HumanBodyBones.RightRingIntermediate  );
			RingTip   = new(isLeft ? HumanBodyBones.LeftRingDistal         : HumanBodyBones.RightRingDistal        );
			PinkyPip  = new(isLeft ? HumanBodyBones.LeftLittleProximal     : HumanBodyBones.RightLittleProximal    );
			PinkyDip  = new(isLeft ? HumanBodyBones.LeftLittleIntermediate : HumanBodyBones.RightLittleIntermediate);
			PinkyTip  = new(isLeft ? HumanBodyBones.LeftLittleDistal       : HumanBodyBones.RightLittleDistal      );
			ThumbPip  = new(isLeft ? HumanBodyBones.LeftThumbProximal      : HumanBodyBones.RightThumbProximal     );
			ThumbDip  = new(isLeft ? HumanBodyBones.LeftThumbIntermediate  : HumanBodyBones.RightThumbIntermediate );
			ThumbTip  = new(isLeft ? HumanBodyBones.LeftThumbDistal        : HumanBodyBones.RightThumbDistal       );
		}

		public void Update(DataGroups.HandData value, float time) {
			Wrist.Add(value.Wrist, time);
			IndexPip.Add(value.IndexFingerMCP, time);
			IndexDip.Add(value.IndexFingerPIP, time);
			IndexTip.Add(value.IndexFingerDIP, time);
			MiddlePip.Add(value.MiddleFingerMCP, time);
			MiddleDip.Add(value.MiddleFingerPIP, time);
			MiddleTip.Add(value.MiddleFingerDIP, time);
			RingPip.Add(value.RingFingerMCP, time);
			RingDip.Add(value.RingFingerPIP, time);
			RingTip.Add(value.RingFingerDIP, time);
			PinkyPip.Add(value.PinkyMCP, time);
			PinkyDip.Add(value.PinkyPIP, time);
			PinkyTip.Add(value.PinkyDIP, time);
			ThumbPip.Add(value.ThumbCMC, time);
			ThumbDip.Add(value.ThumbMCP, time);
			ThumbTip.Add(value.ThumbIP, time);
		}

		public void Update(float time) {
			Wrist    .Update(time);
			IndexPip .Update(time);
			IndexDip .Update(time);
			IndexTip .Update(time);
			MiddlePip.Update(time);
			MiddleDip.Update(time);
			MiddleTip.Update(time);
			RingPip  .Update(time);
			RingDip  .Update(time);
			RingTip  .Update(time);
			PinkyPip .Update(time);
			PinkyDip .Update(time);
			PinkyTip .Update(time);
			ThumbPip .Update(time);
			ThumbDip .Update(time);
			ThumbTip .Update(time);
		}

		public void ApplyFingers(SceneModel model) {
			IndexPip .ApplyLocal(model);
			IndexDip .ApplyLocal(model);
			IndexTip .ApplyLocal(model);
			MiddlePip.ApplyLocal(model);
			MiddleDip.ApplyLocal(model);
			MiddleTip.ApplyLocal(model);
			RingPip  .ApplyLocal(model);
			RingDip  .ApplyLocal(model);
			RingTip  .ApplyLocal(model);
			PinkyPip .ApplyLocal(model);
			PinkyDip .ApplyLocal(model);
			PinkyTip .ApplyLocal(model);
			ThumbPip .ApplyLocal(model);
			ThumbDip .ApplyLocal(model);
			ThumbTip .ApplyLocal(model);
		}
	}

	public class PoseValues {
		public DiscreteRotStruct Neck  = new(HumanBodyBones.Neck);
		public DiscreteRotStruct Chest = new(HumanBodyBones.Chest);
		public DiscreteRotStruct Hips  = new(HumanBodyBones.Hips);

		// IK pos structures
		public DiscretePosStruct HipsPosition = new(Vector3.zero, Vector3.zero);
		public DiscretePosStruct RightShoulder = new(Vector3.zero, Vector3.zero);
		public DiscretePosStruct RightElbow = new(Vector3.zero, Vector3.zero);
		public DiscretePosStruct RightHand = new(Vector3.zero, Vector3.zero);
		public DiscretePosStruct LeftShoulder = new(Vector3.zero, Vector3.zero);
		public DiscretePosStruct LeftElbow = new(Vector3.zero, Vector3.zero);
		public DiscretePosStruct LeftHand = new(Vector3.zero, Vector3.zero);
		
		public DiscreteRotStruct RightUpperLeg = new(HumanBodyBones.RightUpperLeg);
		public DiscreteRotStruct RightLowerLeg = new(HumanBodyBones.RightLowerLeg);
		public DiscreteRotStruct LeftUpperLeg  = new(HumanBodyBones.LeftUpperLeg);
		public DiscreteRotStruct LeftLowerLeg  = new(HumanBodyBones.LeftLowerLeg);

		public void Update(float time) {
			Neck .Update(time);
			Chest.Update(time);
			Hips .Update(time);
			HipsPosition .Update(time);
			RightShoulder.Update(time);
			RightElbow   .Update(time);
			RightHand    .Update(time);
			LeftShoulder .Update(time);
			LeftElbow    .Update(time);
			LeftHand     .Update(time);
			RightUpperLeg.Update(time);
			RightLowerLeg.Update(time);
			LeftUpperLeg .Update(time);
			LeftLowerLeg .Update(time);
		}
	}

	public class FaceValues {
		// Mouth
		public float mouthOpen = 0;

		// Eyes
		public RollingAverageVector2 lEyeIris = new(FaceConfig.EAR_FRAMES);
		public RollingAverageVector2 rEyeIris = new(FaceConfig.EAR_FRAMES);
		public RollingAverage lEyeOpen = new(FaceConfig.EAR_FRAMES);
		public RollingAverage rEyeOpen = new(FaceConfig.EAR_FRAMES);
	}

	public struct RollingAverage {
		public float[] data;
		private int dataIndex;

		public RollingAverage(int size) {
			data = new float[size];
			dataIndex = 0;
		}

		public void Add(float value) {
			data[dataIndex] = value;
			dataIndex = (dataIndex + 1) % data.Length;
		}

		public float Average() {
			return data.Average();
		}

		public float Min() {
			return data.Min();
		}

		public float Max() {
			return data.Max();
		}
	}

	public struct RollingAverageVector2 {
		public Vector2[] data;
		private int dataIndex;

		public RollingAverageVector2(int size) {
			data = new Vector2[size];
			dataIndex = 0;
		}

		public void Add(Vector2 value) {
			data[dataIndex] = value;
			dataIndex = (dataIndex + 1) % data.Length;
		}

		public Vector2 Average() {
			return data.Aggregate((a, b) => a + b) / (float)data.Length;
		}
	}
}
