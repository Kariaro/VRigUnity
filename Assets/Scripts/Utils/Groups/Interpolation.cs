using UnityEngine;
using System.Linq;

namespace HardCoded.VRigUnity {
	public struct RotStruct {
		public static int TestInterpolationStatic => HolisticTrackingSolution.TestInterpolationStatic;
		public static float TestInterpolationValue => HolisticTrackingSolution.TestInterpolationValue;

		public static RotStruct identity => new(Quaternion.identity, 0);

		private float lastTime;
		private float currTime;
		private Quaternion curr;

		public RotStruct(Quaternion init, float time) {
			currTime = time;
			lastTime = time;
			curr = init;
		}

		public void Set(Quaternion value, float time) {
			lastTime = currTime;
			currTime = time;
			curr = value;
		}

		private Quaternion GetUpdatedRotation(Quaternion current, Quaternion curr, float time) {
			switch (TestInterpolationStatic) {
				default: {
					return Quaternion.Slerp(current, curr, TestInterpolationValue);
				}

				// TODO: Remove these
				case 1: {
					float timeLength = currTime - lastTime;
					float delta = (time - currTime) / timeLength;
					return Quaternion.Lerp(current, curr, Mathf.Clamp01(delta));
				}
				case 2: {
					return curr;
				}
			}
		}
			
		public void UpdateRotation(Transform transform, float time) {
			if (time - 1 > currTime) {
				// If the part was lost we slowly put it back to it's original position
				transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, 0.1f);
			} else {
				transform.rotation = GetUpdatedRotation(transform.rotation, curr, time);
			}
		}

		public Quaternion GetRawUpdateRotation(Transform transform, float time) {
			return GetUpdatedRotation(transform.rotation, curr, time);
		}

		public void UpdateLocalRotation(Transform transform, float time) {
			transform.localRotation = GetUpdatedRotation(transform.localRotation, curr, time);
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
	}

	public class PoseValues {
		public RotStruct Neck = RotStruct.identity;
		public RotStruct Chest = RotStruct.identity;
		public RotStruct Hips = RotStruct.identity;
		public RotStruct HipsPosition = RotStruct.identity;
		public RotStruct RightUpperArm = RotStruct.identity;
		public RotStruct RightLowerArm = RotStruct.identity;
		public RotStruct LeftUpperArm = RotStruct.identity;
		public RotStruct LeftLowerArm = RotStruct.identity;
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
