using System.Collections.Generic;
using UnityEngine;
using static HardCoded.VRigUnity.Visuals.ConnectionListAnnotation;

namespace HardCoded.VRigUnity.Visuals {
	public class PoseAnnotation : VisualizationBase {
		private ConnectionListAnnotation connectionAnnotation;
		private PointListAnnotation pointAnnotation;
		private List<List<Line>> lines;

		private readonly List<(int, int)>[] Connections = {
			new() { // Draw if face is missing
				(7, 3), (3, 2), (2, 1), (1, 0), (0, 4), (4, 5), (5, 6), (6, 8), (9, 10),
			},
			new() { // Draw if left hand is missing
				(21, 15), (15, 17), (17, 19), (19, 15), (15, 13)
			},
			new() { // Draw if right hand is missing
				(22, 16), (16, 18), (18, 20), (20, 16), (16, 14) 
			},
			new() { // Always draw
				(13, 11), (11, 12), (12, 14),
				
				(27, 31), (31, 29), (29, 27), (27, 25), (25, 23), (23, 24),
				(24, 26), (26, 28), (28, 30), (30, 32), (32, 28),

				(11, 23),
				(12, 24),
			}
		};

		private readonly List<int[]>[] Points = {
			new() { // Draw if face is missing
				new int[] { 7, 3, 2, 1, 9 }, // left
				new int[] { 4, 5, 6, 8, 10 }, // right
				new int[] { 0 }, // either
			},
			new() { // Draw if left hand is missing
				new int[] { 21, 15, 17, 19 },
				new int[] { },
				new int[] { }
			},
			new() { // Draw if right hand is missing
				new int[] { },
				new int[] { 22, 16, 18, 20 },
				new int[] { }
			},
			new() { // Always draw
				new int[] { 13, 11, 23, 25, 27, 31, 29 },
				new int[] { 12, 14, 24, 26, 28, 30, 32 },
				new int[] { }
			}
		};

		private int leftHandConnection;
		private int rightHandConnection;
		
		protected override void Setup() {
			connectionAnnotation = gameObject.AddComponent<ConnectionListAnnotation>();
			pointAnnotation = gameObject.AddComponent<PointListAnnotation>();
			
			int offset = 0;
			lines = new();
			lines.Add(CreateConnections(Connections[0], ref offset));
			lines.Add(CreateConnections(Connections[1], ref offset));
			lines.Add(CreateConnections(Connections[2], ref offset));
			lines.Add(CreateConnections(Connections[3], ref offset));
			leftHandConnection = offset ++;
			rightHandConnection = offset ++;

			foreach (var items in lines) {
				foreach (var item in items) {
					connectionAnnotation.AddIndex(item.index, parent.lineMaterial, parent.lineColor, parent.lineWidth);
				}
			}
			connectionAnnotation.AddIndex(leftHandConnection, parent.lineMaterial, parent.lineColor, parent.lineWidth);
			connectionAnnotation.AddIndex(rightHandConnection, parent.lineMaterial, parent.lineColor, parent.lineWidth);

			foreach (var items in Points) {
				int[] left = items[0]; // cyan
				int[] right = items[1]; // orange
				int[] either = items[2]; // line color

				foreach (var item in left) {
					pointAnnotation.AddIndex(item, parent.pointMaterial, parent.orangeColor, 15);
				}

				foreach (var item in right) {
					pointAnnotation.AddIndex(item, parent.pointMaterial, parent.pointColor, 15);
				}

				foreach (var item in either) {
					pointAnnotation.AddIndex(item, parent.pointMaterial, parent.lineColor, 15);
				}
			}
		}

		public void Apply(HolisticLandmarks landmarks, HolisticLandmarks face, HolisticLandmarks leftHand, HolisticLandmarks rightHand) {
			if (!IsPrepared) {
				return;
			}

			if (!landmarks.IsPresent) {
				connectionAnnotation.HideAll();
				pointAnnotation.HideAll();
				return;
			}

			bool[] skip = {
				face.IsPresent,
				leftHand.IsPresent,
				rightHand.IsPresent,
				false
			};
			for (int i = 0; i < skip.Length; i++) {
				if (skip[i]) {
					foreach (var line in lines[i]) {
						connectionAnnotation.Hide(line.index);
					}

					foreach (var items in Points[i]) {
						foreach (var item in items) {
							pointAnnotation.Hide(item);
						}
					}

					continue;
				}

				foreach (var line in lines[i]) {
					Vector3 start = parent.FromNormalizedToScreen(landmarks.GetRaw(line.start));
					Vector3 end = parent.FromNormalizedToScreen(landmarks.GetRaw(line.end));
					connectionAnnotation.Set(line.index, start, end);
				}

				foreach (var items in Points[i]) {
					foreach (var item in items) {
						pointAnnotation.Set(item, parent.FromNormalized(landmarks.GetRaw(item)));
					}
				}
			}

			if (leftHand.IsPresent) {
				Vector3 wrist = parent.FromNormalizedToScreen(leftHand.GetRaw(0));
				Vector3 elbow = parent.FromNormalizedToScreen(landmarks.GetRaw(13));
				connectionAnnotation.Set(leftHandConnection, wrist, elbow);
			} else {
				connectionAnnotation.Hide(leftHandConnection);
			}

			if (rightHand.IsPresent) {
				Vector3 wrist = parent.FromNormalizedToScreen(rightHand.GetRaw(0));
				Vector3 elbow = parent.FromNormalizedToScreen(landmarks.GetRaw(14));
				connectionAnnotation.Set(rightHandConnection, wrist, elbow);
			} else {
				connectionAnnotation.Hide(rightHandConnection);
			}
		}
	}
}
