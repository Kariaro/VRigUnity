using System.Linq;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public struct DiscreteRotStruct {
		public static DiscreteRotStruct identity => new(Quaternion.identity);
		
		// Internal
		private Quaternion m_current;
		private Quaternion m_target;
		private float m_targetTime;
		public Quaternion Current => m_current;

		public DiscreteRotStruct(Quaternion current) {
			m_current = current;
			m_target = Quaternion.identity;
			m_targetTime = 0;
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
			// Implement spherical bezier spline interpolation?
			if (m_targetTime < time - 1) {
				// We have lost focus of the data point
			}
			
			// 60 fps is the default speed so this should == 1
			// If we have 120 fps this would be == 0.5
			// If we have  30 fps this would be == 2.0
			float td = Time.deltaTime * 60;
			float iv = td * Settings.TrackingInterpolation;
			m_current = Quaternion.Slerp(m_current, m_target, iv);
		}

		public void ApplyLocal(Transform transform) {
			transform.localRotation = m_current;
		}

		public void ApplyGlobal(Transform transform) {
			transform.rotation = m_current;
		}
	}

	public struct DiscretePosStruct {
		public static DiscretePosStruct identity => new(Vector3.zero);
		
		// Internal
		private Vector3 m_current;
		private Vector3 m_target;
		private float m_targetTime;
		public Vector3 Current => m_current;
		
		public DiscretePosStruct(Vector3 current) {
			m_current = current;
			m_target = Vector3.zero;
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
			if (m_targetTime < time - 1) {
				// We have lost focus of the data point
			}

			m_current = Vector3.Lerp(m_current, m_target, Settings.TrackingInterpolation);
		}

		public void Apply(Transform transform) {
			transform.position = m_current;
		}
	}

	public struct RotStruct {
		public static RotStruct identity => new(Quaternion.identity, 0);

		private float lastTime;
		private float currTime;
		private Quaternion curr;

		// Cache values
		private Quaternion lastRotation;
		private Transform lastTransform;
		private Vector3 lastPosition;
		private HumanBodyBones lastBone;

		public RotStruct(Quaternion init, float time) {
			currTime = time;
			lastTime = time;
			curr = init;

			lastTransform = null;
			lastPosition = Vector3.zero;
			lastRotation = Quaternion.identity;
			lastBone = HumanBodyBones.LastBone;
		}

		public void Add(Quaternion value, float time) {
			lastTime = currTime;
			currTime = time;
			curr = value;
		}

		// Used for non main thread IK calculations
		public Quaternion GetLastRotation() {
			return lastRotation;
		}

		// Used for non main thread IK calculations
		public Vector3 GetLastPosition() {
			return lastPosition;
		}

		// Used to check if this value is set
		public bool HasValue(float time) {
			return time - 1 <= currTime;
		}

		private Quaternion GetUpdatedRotation(Quaternion current, Quaternion curr, float time) {
			// 60 fps is the default speed so this should == 1
			// If we have 120 fps this would be == 0.5
			// If we have  30 fps this would be == 2.0
			float td = Time.deltaTime * 60;
			float iv = td * Settings.TrackingInterpolation;
			return Quaternion.Slerp(current, curr, iv);
		}

		private Transform GetTransform(Animator animator, HumanBodyBones bone) {
			if (lastTransform == null || lastBone != bone) {
				lastBone = bone;
				lastTransform = animator.GetBoneTransform(bone);
			}

			return lastTransform;
		}

		public Transform GetTransform() {
			return lastTransform;
		}

		public Quaternion GetRawUpdateRotation(Transform transform, float time) {
			return GetUpdatedRotation(transform.rotation, curr, time);
		}
		
		// TODO: Remove 'HumanBodyBones' from this call
		//       This should handled in a special way
		public void UpdateRotation(Animator animator, HumanBodyBones bone, float time) {
			Transform transform = GetTransform(animator, bone);
			if (time - 1 > currTime) {
				lastRotation = GetUpdatedRotation(lastRotation, BoneSettings.GetDefaultRotation(bone), time);
				if (!Settings.UseFullIK) {
					transform.localRotation = lastRotation;
				}
			} else {
				lastRotation = GetUpdatedRotation(lastRotation, curr, time);
				if (!Settings.UseFullIK) {
					transform.rotation = lastRotation;
				}
			}
			lastPosition = transform.position;
		}

		public void UpdateRotationWithoutIK(Animator animator, HumanBodyBones bone, float time) {
			Transform transform = GetTransform(animator, bone);
			if (time - 1 > currTime) {
				lastRotation = GetUpdatedRotation(lastRotation, BoneSettings.GetDefaultRotation(bone), time);
				transform.localRotation = lastRotation;
			} else {
				lastRotation = GetUpdatedRotation(lastRotation, curr, time);
				transform.rotation = lastRotation;
			}
			lastPosition = transform.position;
		}

		public void UpdateLocalRotation(Animator animator, HumanBodyBones bone, float time) {
			Transform transform = GetTransform(animator, bone);
			if (time - 1 > currTime) {
				lastRotation = GetUpdatedRotation(lastRotation, BoneSettings.GetDefaultRotation(bone), time);
			} else {
				lastRotation = GetUpdatedRotation(lastRotation, curr, time);
			}
			transform.localRotation = lastRotation;
			lastPosition = transform.position;
		}
	}

	public struct PosStruct {
		public static PosStruct identity => new(Vector3.zero, 0);
		
		private float lastTime;
		private float currTime;
		private Vector3 curr;

		// Cache values
		private Transform lastTransform;
		private HumanBodyBones lastBone;

		public PosStruct(Vector3 init, float time) {
			currTime = time;
			lastTime = time;
			curr = init;

			lastTransform = null;
			lastBone = HumanBodyBones.LastBone;
		}

		public void Add(Vector3 value, float time) {
			lastTime = currTime;
			currTime = time;
			curr = value;
		}

		public Vector3 Get() {
			return curr;
		}

		private Vector3 GetUpdatedPosition(Vector3 current, Vector3 curr, float time) {
			return Vector3.Lerp(current, curr, Settings.TrackingInterpolation);
		}
		
		private Transform GetTransform(Animator animator, HumanBodyBones bone) {
			if (lastTransform == null || lastBone != bone) {
				lastBone = bone;
				lastTransform = animator.GetBoneTransform(bone);
			}

			return lastTransform;
		}
		
		public void UpdatePosition(Animator animator, HumanBodyBones bone, float time) {
			Transform transform = GetTransform(animator, bone);
			if (time - 1 > currTime) {
				transform.position = Vector3.Lerp(transform.position, curr, 0.1f);
			} else {
				transform.position = GetUpdatedPosition(transform.position, curr, time);
			}
		}

		
		public Vector3 GetRawUpdatePosition(Vector3 last, float time) {
			return GetUpdatedPosition(last, curr, time);
		}
	}

	public class HandValues {
		public RotStruct Wrist = RotStruct.identity;
		public RotStruct IndexPip = RotStruct.identity;
		public RotStruct IndexDip = RotStruct.identity;
		public RotStruct IndexTip = RotStruct.identity;
		public RotStruct MiddlePip = RotStruct.identity;
		public RotStruct MiddleDip = RotStruct.identity;
		public RotStruct MiddleTip = RotStruct.identity;
		public RotStruct RingPip = RotStruct.identity;
		public RotStruct RingDip = RotStruct.identity;
		public RotStruct RingTip = RotStruct.identity;
		public RotStruct PinkyPip = RotStruct.identity;
		public RotStruct PinkyDip = RotStruct.identity;
		public RotStruct PinkyTip = RotStruct.identity;
		public RotStruct ThumbPip = RotStruct.identity;
		public RotStruct ThumbDip = RotStruct.identity;
		public RotStruct ThumbTip = RotStruct.identity;

		public void Update(Groups.HandRotation value, float time) {
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
	}

	public class HandValuesTest {
		public DiscreteRotStruct Wrist     = DiscreteRotStruct.identity;
		public DiscreteRotStruct IndexPip  = DiscreteRotStruct.identity;
		public DiscreteRotStruct IndexDip  = DiscreteRotStruct.identity;
		public DiscreteRotStruct IndexTip  = DiscreteRotStruct.identity;
		public DiscreteRotStruct MiddlePip = DiscreteRotStruct.identity;
		public DiscreteRotStruct MiddleDip = DiscreteRotStruct.identity;
		public DiscreteRotStruct MiddleTip = DiscreteRotStruct.identity;
		public DiscreteRotStruct RingPip   = DiscreteRotStruct.identity;
		public DiscreteRotStruct RingDip   = DiscreteRotStruct.identity;
		public DiscreteRotStruct RingTip   = DiscreteRotStruct.identity;
		public DiscreteRotStruct PinkyPip  = DiscreteRotStruct.identity;
		public DiscreteRotStruct PinkyDip  = DiscreteRotStruct.identity;
		public DiscreteRotStruct PinkyTip  = DiscreteRotStruct.identity;
		public DiscreteRotStruct ThumbPip  = DiscreteRotStruct.identity;
		public DiscreteRotStruct ThumbDip  = DiscreteRotStruct.identity;
		public DiscreteRotStruct ThumbTip  = DiscreteRotStruct.identity;

		public void Update(Groups.HandRotation value, float time) {
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
	}

	public class PoseValues {
		public RotStruct Neck = RotStruct.identity;
		public RotStruct Chest = RotStruct.identity;
		public RotStruct Hips = RotStruct.identity;

		public PosStruct HipsPosition = PosStruct.identity;

		public PosStruct RightShoulder = PosStruct.identity;
		public PosStruct RightElbow = PosStruct.identity;
		public PosStruct RightHand = PosStruct.identity;
		
		public PosStruct LeftShoulder = PosStruct.identity;
		public PosStruct LeftElbow = PosStruct.identity;
		public PosStruct LeftHand = PosStruct.identity;
		
		public RotStruct RightUpperArm = RotStruct.identity;
		public RotStruct RightLowerArm = RotStruct.identity;
		public RotStruct LeftUpperArm = RotStruct.identity;
		public RotStruct LeftLowerArm = RotStruct.identity;
		public RotStruct RightUpperLeg = RotStruct.identity;
		public RotStruct RightLowerLeg = RotStruct.identity;
		public RotStruct LeftUpperLeg = RotStruct.identity;
		public RotStruct LeftLowerLeg = RotStruct.identity;
	}

	public class FaceData {
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
}
