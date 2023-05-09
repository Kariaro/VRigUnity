using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity.Visuals {
	public class IrisAnnotation : VisualizationBase {
		private PointListAnnotation pointAnnotation;
		
		private const int LandmarkOffset = 468;
		private const int LandmarkCount = 10;
		
		protected override void Setup() {
			pointAnnotation = gameObject.AddComponent<PointListAnnotation>();

			for (int i = 0; i < LandmarkCount; i++) {
				pointAnnotation.AddIndex(i, parent.pointMaterial, Color.yellow, 10);
			}
		}

		public void Apply(HolisticLandmarks landmarks) {
			if (!IsPrepared) {
				return;
			}

			if (!landmarks.IsPresent || landmarks.Count < LandmarkOffset + LandmarkCount) {
				pointAnnotation.HideAll();
				return;
			}

			for (int i = 0; i < LandmarkCount; i++) {
				Vector3 point = parent.FromNormalized(landmarks.GetRaw(LandmarkOffset + i));
				pointAnnotation.Set(i, point);
			}
		}
	}
}
