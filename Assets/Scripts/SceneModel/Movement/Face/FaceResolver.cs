using Mediapipe;
using Mediapipe.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class FaceResolver {
		private static Vector4 ConvertPoint(NormalizedLandmarkList list, int idx) {
			NormalizedLandmark mark = list.Landmark[idx];
			return new(mark.X * 2, mark.Y, mark.Z * 2, mark.Visibility);
		}

		public static DataGroups.FaceData Solve(OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			DataGroups.FaceData data = new();

			Quaternion neckRotation = Quaternion.identity;
			float mouthOpen = 0;
			float lEyeOpen = 0;
			float rEyeOpen = 0;
			Vector2 lEyeIris = Vector2.zero;
			Vector2 rEyeIris = Vector2.zero;
			
			if (BoneSettings.Get(BoneSettings.FACE)) {
				// Mouth
				Vector3 a = ConvertPoint(eventArgs.value, 324);
				Vector3 b = ConvertPoint(eventArgs.value, 78);
				Vector3 c = ConvertPoint(eventArgs.value, 13);
				Vector3 m = (a + b) / 2.0f;

				float width = Vector3.Distance(a, b);
				float height = Vector3.Distance(c, m);
				float area = MovementUtils.GetTriangleArea(a, b, c);
				float perc = height / width;

				mouthOpen = (perc - 0.25f) * 3;
				mouthOpen = Mathf.Clamp01(mouthOpen);

				Vector3 converter(int i) {
					Vector3 value = ConvertPoint(eventArgs.value, i);
					value.x = -value.x;
					return value;
				}

				// Eyes
				lEyeOpen = FacePoints.CalculateEyeAspectRatio(Array.ConvertAll(FacePoints.LeftEyeEAR, converter));
				rEyeOpen = FacePoints.CalculateEyeAspectRatio(Array.ConvertAll(FacePoints.RightEyeEAR, converter));
				lEyeIris = FacePoints.CalculateIrisPosition(FacePoints.LeftEyeIrisPoint, converter);
				rEyeIris = FacePoints.CalculateIrisPosition(FacePoints.RightEyeIrisPoint, converter);
			}

			{
				Vector3 botHead = ConvertPoint(eventArgs.value, 152);
				Vector3 topHead = ConvertPoint(eventArgs.value, 10);
				Plane plane = new(ConvertPoint(eventArgs.value, 109), ConvertPoint(eventArgs.value, 338), botHead);

				// Figure out their position on the eye socket plane
				Vector3 forwardDir = plane.normal;
				Vector3 faceUpDir = botHead - topHead;

				neckRotation = Quaternion.LookRotation(forwardDir, faceUpDir);
			}

			data.neckRotation = neckRotation;
			data.mouthOpen = mouthOpen;
			data.rEyeOpen = rEyeOpen;
			data.lEyeOpen = lEyeOpen;
			data.rEyeIris = rEyeIris;
			data.lEyeIris = lEyeIris;

			return data;
		}
	}
}

