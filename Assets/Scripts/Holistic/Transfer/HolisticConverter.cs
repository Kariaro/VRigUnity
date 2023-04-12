using Mediapipe;
using Mediapipe.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class HolisticConverter {
		public static void Connect(HolisticGraph graph, IHolisticCallback callback) {
			// Face
			graph.OnFaceLandmarksOutput += (_, eventArgs) => {
				callback.OnFaceLandmarks(FromMediapipe(eventArgs));
			};

			// Hands
			graph.OnLeftHandLandmarksOutput += (_, eventArgs) => {
				callback.OnLeftHandLandmarks(FromMediapipe(eventArgs));
			};
			graph.OnRightHandLandmarksOutput += (_, eventArgs) => {
				callback.OnRightHandLandmarks(FromMediapipe(eventArgs));
			};

			// Pose
			graph.OnPoseLandmarksOutput += (_, eventArgs) => {
				callback.OnPoseLandmarks(FromMediapipe(eventArgs, false));
			};
			graph.OnPoseWorldLandmarksOutput += (_, eventArgs) => {
				callback.OnPoseWorldLandmarks(FromMediapipe(eventArgs));
			};
		}

		public static HolisticLandmarks FromMediapipe(OutputEventArgs<NormalizedLandmarkList> eventArgs, bool modify = true) {
			var landmarkList = eventArgs?.value;
			if (landmarkList == null) {
				return HolisticLandmarks.NotPresent;
			}
			
			List<Vector4> list = new();
			List<Vector4> raw = new();
			int count = landmarkList.Landmark.Count;
			for (int i = 0; i < count; i++) {
				var mark = landmarkList.Landmark[i];
				if (modify) {
					list.Add(new(mark.X * 2, mark.Y, mark.Z * 2, mark.Visibility));
				} else {
					list.Add(new(mark.X, mark.Y, mark.Z, mark.Visibility));
				}
				raw.Add(new(mark.X, mark.Y, mark.Z, mark.Visibility));
			}

			return new(list, raw);
		}

		public static HolisticLandmarks FromMediapipe(OutputEventArgs<LandmarkList> eventArgs) {
			var landmarkList = eventArgs?.value;
			if (landmarkList == null) {
				return HolisticLandmarks.NotPresent;
			}
			
			List<Vector4> list = new();
			int count = landmarkList.Landmark.Count;
			for (int i = 0; i < count; i++) {
				var mark = landmarkList.Landmark[i];
				list.Add(new(mark.X, mark.Y, mark.Z, mark.Visibility));
			}

			return new(list, list);
		}
	}
}
