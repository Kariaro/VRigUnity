using UnityEngine;

namespace HardCoded.VRigUnity {
	public class HandGroup : MonoBehaviour {
		// These have the same index as the MediaPipe.Hand class
		[SerializeField] private GameObject[] points;
		[SerializeField] private LineHolder lineHolder;

		// Rotation list for all the different points
		private static int[][] RotationList = new int[][] {
			new int[] { MediaPipe.Hand.WRIST },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.THUMB_CMC },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.THUMB_CMC, MediaPipe.Hand.THUMB_MCP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.THUMB_CMC, MediaPipe.Hand.THUMB_MCP, MediaPipe.Hand.THUMB_IP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.THUMB_CMC, MediaPipe.Hand.THUMB_MCP, MediaPipe.Hand.THUMB_IP, MediaPipe.Hand.THUMB_TIP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.INDEX_FINGER_MCP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.INDEX_FINGER_MCP, MediaPipe.Hand.INDEX_FINGER_PIP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.INDEX_FINGER_MCP, MediaPipe.Hand.INDEX_FINGER_PIP, MediaPipe.Hand.INDEX_FINGER_DIP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.INDEX_FINGER_MCP, MediaPipe.Hand.INDEX_FINGER_PIP, MediaPipe.Hand.INDEX_FINGER_DIP, MediaPipe.Hand.INDEX_FINGER_TIP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.MIDDLE_FINGER_MCP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.MIDDLE_FINGER_MCP, MediaPipe.Hand.MIDDLE_FINGER_PIP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.MIDDLE_FINGER_MCP, MediaPipe.Hand.MIDDLE_FINGER_PIP, MediaPipe.Hand.MIDDLE_FINGER_DIP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.MIDDLE_FINGER_MCP, MediaPipe.Hand.MIDDLE_FINGER_PIP, MediaPipe.Hand.MIDDLE_FINGER_DIP, MediaPipe.Hand.MIDDLE_FINGER_TIP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.RING_FINGER_MCP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.RING_FINGER_MCP, MediaPipe.Hand.RING_FINGER_PIP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.RING_FINGER_MCP, MediaPipe.Hand.RING_FINGER_PIP, MediaPipe.Hand.RING_FINGER_DIP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.RING_FINGER_MCP, MediaPipe.Hand.RING_FINGER_PIP, MediaPipe.Hand.RING_FINGER_DIP, MediaPipe.Hand.RING_FINGER_TIP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.PINKY_MCP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.PINKY_MCP, MediaPipe.Hand.PINKY_PIP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.PINKY_MCP, MediaPipe.Hand.PINKY_PIP, MediaPipe.Hand.PINKY_DIP },
			new int[] { MediaPipe.Hand.WRIST, MediaPipe.Hand.PINKY_MCP, MediaPipe.Hand.PINKY_PIP, MediaPipe.Hand.PINKY_DIP, MediaPipe.Hand.PINKY_TIP },
		};

		private void ConnectHand() {
			// Thumb
			lineHolder.AddConnection(points[MediaPipe.Hand.THUMB_CMC], points[MediaPipe.Hand.THUMB_MCP]);
			lineHolder.AddConnection(points[MediaPipe.Hand.THUMB_MCP], points[MediaPipe.Hand.THUMB_IP]);
			lineHolder.AddConnection(points[MediaPipe.Hand.THUMB_IP], points[MediaPipe.Hand.THUMB_TIP]);

			// Index
			lineHolder.AddConnection(points[MediaPipe.Hand.INDEX_FINGER_MCP], points[MediaPipe.Hand.INDEX_FINGER_PIP]);
			lineHolder.AddConnection(points[MediaPipe.Hand.INDEX_FINGER_PIP], points[MediaPipe.Hand.INDEX_FINGER_DIP]);
			lineHolder.AddConnection(points[MediaPipe.Hand.INDEX_FINGER_DIP], points[MediaPipe.Hand.INDEX_FINGER_TIP]);

			// Middle
			lineHolder.AddConnection(points[MediaPipe.Hand.MIDDLE_FINGER_MCP], points[MediaPipe.Hand.MIDDLE_FINGER_PIP]);
			lineHolder.AddConnection(points[MediaPipe.Hand.MIDDLE_FINGER_PIP], points[MediaPipe.Hand.MIDDLE_FINGER_DIP]);
			lineHolder.AddConnection(points[MediaPipe.Hand.MIDDLE_FINGER_DIP], points[MediaPipe.Hand.MIDDLE_FINGER_TIP]);

			// Ring
			lineHolder.AddConnection(points[MediaPipe.Hand.RING_FINGER_MCP], points[MediaPipe.Hand.RING_FINGER_PIP]);
			lineHolder.AddConnection(points[MediaPipe.Hand.RING_FINGER_PIP], points[MediaPipe.Hand.RING_FINGER_DIP]);
			lineHolder.AddConnection(points[MediaPipe.Hand.RING_FINGER_DIP], points[MediaPipe.Hand.RING_FINGER_TIP]);

			// Pinky
			lineHolder.AddConnection(points[MediaPipe.Hand.PINKY_MCP], points[MediaPipe.Hand.PINKY_PIP]);
			lineHolder.AddConnection(points[MediaPipe.Hand.PINKY_PIP], points[MediaPipe.Hand.PINKY_DIP]);
			lineHolder.AddConnection(points[MediaPipe.Hand.PINKY_DIP], points[MediaPipe.Hand.PINKY_TIP]);

			// Palm
			lineHolder.AddConnection(points[MediaPipe.Hand.WRIST], points[MediaPipe.Hand.THUMB_CMC]);
			lineHolder.AddConnection(points[MediaPipe.Hand.WRIST], points[MediaPipe.Hand.INDEX_FINGER_MCP]);
			lineHolder.AddConnection(points[MediaPipe.Hand.WRIST], points[MediaPipe.Hand.PINKY_MCP]);
			lineHolder.AddConnection(points[MediaPipe.Hand.INDEX_FINGER_MCP], points[MediaPipe.Hand.MIDDLE_FINGER_MCP]);
			lineHolder.AddConnection(points[MediaPipe.Hand.MIDDLE_FINGER_MCP], points[MediaPipe.Hand.RING_FINGER_MCP]);
			lineHolder.AddConnection(points[MediaPipe.Hand.RING_FINGER_MCP], points[MediaPipe.Hand.PINKY_MCP]);
		}

		public void Apply(DataGroups.HandPoints handPoints, Vector3 origin, float scale = 1.0f) {
			// Normalize on the wrist position
			for (int i = 0; i < handPoints.Data.Length; i++) {
				points[i].transform.position = (handPoints.Data[i] - handPoints.Wrist) * scale + origin;
			}
		}

		public void Apply(DataGroups.HandData handRotations) {
			// TODO: Does not seem to work?
			for (int i = 0; i < handRotations.Length; i++) {
				points[i].transform.rotation = handRotations[i];
			}
		}

		void Start() {
			ConnectHand();
		}
	}
}