using System.Linq;
using UnityEngine;

namespace HardCoded.VRigUnity {
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
