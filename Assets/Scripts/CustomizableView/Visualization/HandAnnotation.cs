using System.Collections.Generic;
using UnityEngine;
using static HardCoded.VRigUnity.Visuals.ConnectionListAnnotation;

namespace HardCoded.VRigUnity.Visuals {
	public class HandAnnotation : VisualizationBase {
		private ConnectionListAnnotation connectionAnnotation;
		private PointListAnnotation pointAnnotation;
		private List<Line> lines;
		private HandType handedness;

		public enum HandType {
			Left,
			Right
		}

		private const int LandmarkCount = 21;
		private readonly List<(int, int)> Connections = new() {
			// Thumb
			(1, 2), (2, 3), (3, 4),
			
			// Index
			(5, 6), (6, 7), (7, 8),

			// Middle
			(9, 10), (10, 11), (11, 12),

			// Ring
			(13, 14), (14, 15), (15, 16),

			// Pinky
			(17, 18), (18, 19), (19, 20),

			// Palm
			(1, 0), (0, 5), (5, 9), (9, 13), (13, 17), (17, 0)
		};

		protected override void Setup() {
			connectionAnnotation = gameObject.AddComponent<ConnectionListAnnotation>();
			pointAnnotation = gameObject.AddComponent<PointListAnnotation>();
			
			int offset = 0;
			lines = CreateConnections(Connections, ref offset);

			foreach (var item in lines) {
				connectionAnnotation.AddIndex(item.index, parent.lineMaterial, parent.lineColor, parent.lineWidth);
			}

			Color pointColor = handedness == HandType.Left ? parent.orangeColor : parent.pointColor;
			for (int i = 0; i < LandmarkCount; i++) {
				pointAnnotation.AddIndex(i, parent.pointMaterial, pointColor, 15);
			}
		}

		public void SetHandedness(HandType type) {
			handedness = type;
		}

		public void Apply(HolisticLandmarks landmarks) {
			if (!IsPrepared) {
				return;
			}

			if (!landmarks.IsPresent) {
				connectionAnnotation.HideAll();
				pointAnnotation.HideAll();
				return;
			}

			foreach (var line in lines) {
				Vector3 start = parent.FromNormalizedToScreen(landmarks.GetRaw(line.start));
				Vector3 end = parent.FromNormalizedToScreen(landmarks.GetRaw(line.end));
				connectionAnnotation.Set(line.index, start, end);
			}

			for (int i = 0; i < landmarks.Count; i++) {
				pointAnnotation.Set(i, parent.FromNormalized(landmarks.GetRaw(i)));
			}
		}
	}
}
