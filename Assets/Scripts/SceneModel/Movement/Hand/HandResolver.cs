using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class HandResolver {
		public static readonly int[][] Fingers = new int[][] {
			new int[] { MediaPipe.Hand.THUMB_CMC,         MediaPipe.Hand.THUMB_TIP },         // Thumb
			new int[] { MediaPipe.Hand.INDEX_FINGER_MCP,  MediaPipe.Hand.INDEX_FINGER_TIP },  // Index
			new int[] { MediaPipe.Hand.MIDDLE_FINGER_MCP, MediaPipe.Hand.MIDDLE_FINGER_TIP }, // Middle
			new int[] { MediaPipe.Hand.RING_FINGER_MCP,   MediaPipe.Hand.RING_FINGER_TIP },   // Ring
			new int[] { MediaPipe.Hand.PINKY_MCP,         MediaPipe.Hand.PINKY_TIP },         // Pinky
		};

		public struct FingerAngle {
			public int idx;
			public Vector3 angle;

			public FingerAngle(int idx, Vector3 angle) {
				this.idx = idx;
				this.angle = angle;
			}
		}

		public static DataGroups.HandData SolveRightHand(HolisticLandmarks landmarks) {
			DataGroups.HandPoints handPoints = new();
			int count = landmarks.Count;
			for (int i = 0; i < count; i++) {
				handPoints.Data[i] = landmarks[i];
			}

			return SolveRightHand(handPoints);
		}

		public static DataGroups.HandData SolveLeftHand(HolisticLandmarks landmarks) {
			DataGroups.HandPoints handPoints = new();
			int count = landmarks.Count;
			for (int i = 0; i < count; i++) {
				handPoints.Data[i] = landmarks[i];
			}

			return SolveLeftHand(handPoints);
		}
		
		private static DataGroups.HandPoints SetGlobalOrigin(DataGroups.HandPoints hand) {
			DataGroups.HandPoints result = new();

			for (int i = 0; i < hand.Data.Length; i++) {
				result.Data[i] = hand.Data[i] - hand.Wrist;
			}

			return result;
		}

		public static DataGroups.HandData SolveLeftHand(DataGroups.HandPoints hand) {
			hand = SetGlobalOrigin(hand);

			List<FingerAngle> angles = FingerAngles(hand, HandType.Left);
			
			{
				Plane plane = new(hand.Wrist, hand.IndexFingerMCP, hand.PinkyMCP);
				Vector3 handUpDir = hand.Wrist - hand.MiddleFingerMCP;
				Vector3 handForwardDir = plane.normal;

				Quaternion rot = Quaternion.LookRotation(handForwardDir, handUpDir) * Quaternion.Euler(0, 90, -90);
				angles.Add(new(MediaPipe.Hand.WRIST, rot.eulerAngles));
			}

			return ConvertData(angles);
		}

		public static DataGroups.HandData SolveRightHand(DataGroups.HandPoints hand) {
			hand = SetGlobalOrigin(hand);

			List<FingerAngle> angles = FingerAngles(hand, HandType.Right);
			
			{
				Plane plane = new(hand.Wrist, hand.IndexFingerMCP, hand.PinkyMCP);
				Vector3 handUpDir = hand.Wrist - hand.MiddleFingerMCP;
				Vector3 handForwardDir = plane.normal;

				Quaternion rot = Quaternion.LookRotation(handForwardDir, handUpDir) * Quaternion.Euler(0, 90, 90);
				angles.Add(new(MediaPipe.Hand.WRIST, rot.eulerAngles));
			}

			return ConvertData(angles);
		}

		private static DataGroups.HandData ConvertData(List<FingerAngle> angles) {
			DataGroups.HandData rotation = new();
			foreach (FingerAngle angle in angles) {
				rotation[angle.idx] = Quaternion.Euler(angle.angle);
			}

			return rotation;
		}

		private static List<FingerAngle> FingerAngles(DataGroups.HandPoints hand, HandType type) {
			List<FingerAngle> data = new();

			if (hand == null) {
				return data;
			}

			float[] xAngles = GetXAngles(hand);
			float[] zAngles = GetZAngles(hand);
			for (int i = 0; i < 20; i++) {
				float xAngle = xAngles[i] * 1.1f; // Multiplier
				float zAngle = zAngles[i];
				if (xAngle == 0 && zAngle == 0) {
					continue;
				}

				if (i >= MediaPipe.Hand.THUMB_CMC && i <= MediaPipe.Hand.THUMB_TIP) {
					// Thumb should move more than the fingers
					xAngle *= 1.1f;

					// The thumb needs to rotate around Y for X
					if (i == MediaPipe.Hand.THUMB_CMC) {
						// By default it has around 20 degree offset from the next segment
						xAngle -= 25;
					}

					if (type == HandType.Right) {
						xAngle = -xAngle;
					}

					data.Add(new(i, new(0, -xAngle, type == HandType.Right ? -10 : 10)));
				} else {
					// xAngles can never go more than 120 degrees
					xAngle = Mathf.Clamp(xAngle, -10, 110);

					// If the x_angle is greather than 45 we will lerp the z_angle towards zero
					if (xAngle > 45) {
						// When the x_angle is greater than 90 the z_angle will be zero
						zAngle = Mathf.Max(0, Mathf.Lerp(zAngle, 0, (xAngle - 45) / 45.0f));
					}

					if (type == HandType.Right) {
						xAngle = -xAngle;
						zAngle = -zAngle;
					}

					data.Add(new(i, new(0, zAngle, xAngle)));
				}
			}

			return data;
		}

		/// <summary>
		/// Calculate the X angles for each finger on the hand
		/// </summary>
		private static float[] GetXAngles(DataGroups.HandPoints hand) {
			// Create an array that contains the position of all finger joints
			Vector3[][] fingers = new Vector3[Fingers.Length][];
			for (int i = 0; i < Fingers.Length; i++) {
				int s = Fingers[i][0];
				int e = Fingers[i][1];
				fingers[i] = new Vector3[e - s + 2];
				fingers[i][0] = hand.Wrist;
				for (int j = s; j <= e; j++) {
					fingers[i][j - s + 1] = hand.Data[j];
				}
			}

			// Place finger vectors the same plane
			for (int i = 0; i < Fingers.Length; i++) {
				Vector3[] finger = fingers[i];

				// Create the plane ( wrist, fingerStart, fingerEnd )
				Vector3[] plane = { finger[0], finger[1], finger[4] };
				for (int j = 0; j < finger.Length; j++) {
					finger[j] = HandResolverUtil.ProjectVecOnPlane(plane, finger[j]);
				}
			}

			// For each joint in the finger calculate the angles between them
			float[][] xFingerAngles = new float[fingers.Length][];
			for (int i = 0; i < xFingerAngles.Length; i++) {
				xFingerAngles[i] = HandResolverUtil.JointAngles(fingers[i]);
			}

			float[] data = new float[20];
			for (int i = 0; i < xFingerAngles.Length; i++) {
				float[] angles = xFingerAngles[i];

				// Apply the finger angles to the data array
				float average = 0;
				int mcp = Fingers[i][0];
				int tip = Fingers[i][1];
				for (int j = 0, k = mcp; k < tip; j++, k++) {
					average += angles[j];
					
#if UNITY_EDITOR
					// Debug visualization code
					if (ThreadObject.IsUnityThread()) {
						UnityEngine.Color[] color = { UnityEngine.Color.white, UnityEngine.Color.red, UnityEngine.Color.green, UnityEngine.Color.blue, UnityEngine.Color.cyan };
						
						if (j + 1 < fingers[i].Length) {
							Vector3 a = fingers[i][j];
							Vector3 b = fingers[i][j + 1];
							Debug.DrawLine(a, b, color[i]);
						}
					}
#endif
				}

				for (int j = 0, k = mcp; k < tip; j++, k++) {
					data[k] = average / (tip - mcp);
				}
			}

			return data;
		}

		private static float[] GetZAngles(DataGroups.HandPoints hand) {
			float[] data = new float[20];

			// Calculate the tangent of the hand
			Vector3 tangent = hand.PinkyMCP - hand.IndexFingerMCP;

			// Get pips and mcps (mcps projected on tangent)
			Vector3[] mcps = new Vector3[Fingers.Length - 1];
			Vector3[] pips = new Vector3[Fingers.Length - 1];
			for (int i = 0; i < Fingers.Length - 1; i++) {
				mcps[i] = HandResolverUtil.ProjectPointOnVector(hand.Data[Fingers[i + 1][0]], hand.IndexFingerMCP, hand.PinkyMCP);
				pips[i] = hand.Data[Fingers[i + 1][1] - 2];
			}

			// Direction vector
			Vector3 forwardVector = hand.IndexFingerMCP - hand.ThumbCMC;

			// For each non thumb finger
			for (int i = 0; i < 4; i++) {
				// Construct a plane were the normal is (tangent), up is (forwardVector) and the center is (mcps[i])
				Plane plane = HandResolverUtil.CreatePlaneAroundVector(tangent, mcps[i], forwardVector);
				
				// Calculate the distance to the plane and angle between the (forwardVector) and MCP, PIP
				float dist = plane.GetDistanceToPoint(pips[i]);
				float angle;

				{
					Vector3 pt = plane.ClosestPointOnPlane(pips[i]);
					Vector3 np = plane.normal * dist + pt;
					angle = Vector3.Angle(pt - mcps[i], np - mcps[i]);
				}

				// If the distance to the plane is negative we are rotating against the normal of the plane
				if (dist < 0) {
					angle = -angle;
				}

				// These angles should be between -25 and 25 degrees
				angle = Mathf.Clamp(angle, -25, 25);

#if UNITY_EDITOR
				// Debug visualization code
				if (ThreadObject.IsUnityThread()) {
					UnityEngine.Color[] color = { UnityEngine.Color.red, UnityEngine.Color.green, UnityEngine.Color.blue, UnityEngine.Color.cyan };
					
					Vector3 pt = plane.ClosestPointOnPlane(pips[i]);
					Vector3 np = plane.normal * dist + pt;
					Vector3[] tri = { 
						mcps[i],
						pt,
						np
					};

					// Draw the distance a finger is from up
					for (int j = 0; j < 3; j++) {
						Vector3 a = tri[j];
						Vector3 b = tri[(j + 1) % 3];
						Debug.DrawLine(a, b, color[i]);
					}
					
					// Draw the finger forward triangle
					Vector3[] triangle = HandResolverUtil.CreateTriangleAroundVector(tangent, mcps[i], forwardVector);
					for (int j = 0; j < triangle.Length; j++) {
						float triDist = Vector3.Distance(mcps[i], pips[i]);

						Vector3 a = triangle[j];
						Vector3 b = triangle[(j + 1) % triangle.Length];
						a = (a - mcps[i]) * triDist + mcps[i];
						b = (b - mcps[i]) * triDist + mcps[i];
						Debug.DrawLine(a, b, color[i]);
					}
				}
#endif

				data[Fingers[i + 1][0]] = angle;
			}
			
			return data;
		}

		enum HandType {
			Left,
			Right
		}
	}

	// Helper methods for the hand calculations
	class HandResolverUtil {
		public static Vector3 ProjectVecOnPlane(Vector3[] triangle, Vector3 vec) {
			Vector3 normal = Vector3.Cross(triangle[1] - triangle[0], triangle[2] - triangle[0]);
			return vec - (Vector3.Dot(vec, normal) / normal.sqrMagnitude) * normal;
		}

		public static Vector3 ProjectPointOnVector(Vector3 p, Vector3 a, Vector3 b) {
			Vector3 ap = p - a;
			Vector3 ab = b - a;
			return a + (Vector3.Dot(ap, ab) / Vector3.Dot(ab, ab)) * ab;
		}

		public static float[] JointAngles(Vector3[] vertices) {
			float[] angles = new float[vertices.Length - 2];
			for (int i = 0; i < vertices.Length - 2; i++) {
				angles[i] = Vector3.Angle(
					vertices[1 + i] - vertices[0 + i],
					vertices[2 + i] - vertices[1 + i]
				);
			}
			return angles;
		}

		public static Plane CreatePlaneAroundVector(Vector3 vector, Vector3 C, Vector3 U) {
			Vector3 V = Vector3.Cross(vector, U);
			U = U.normalized;
			V = V.normalized;
			return new Plane(
				C + (-0.5f * U) + (-0.866025403784f * V),
				C + (-0.5f * U) + ( 0.866025403784f * V),
				C + (U)
			);
		}

		public static Vector3[] CreateTriangleAroundVector(Vector3 vector, Vector3 C, Vector3 U) {
			Vector3 V = Vector3.Cross(vector, U);
			U = U.normalized;
			V = V.normalized;

			//return new Vector3[] {
			//C + (Mathf.Cos(Mathf.PI * 2 / 1.5f) * U) + (Mathf.Sin(Mathf.PI * 2 / 1.5f) * V),
			//C + (Mathf.Cos(Mathf.PI * 1 / 1.5f) * U) + (Mathf.Sin(Mathf.PI * 1 / 1.5f) * V),
			//C + (Mathf.Cos(Mathf.PI * 0 / 1.5f) * U) + (Mathf.Sin(Mathf.PI * 0 / 1.5f) * V),
			//};

			return new Vector3[] {
				C + (-0.5f * U) + (-0.866025403784f * V),
				C + (-0.5f * U) + ( 0.866025403784f * V),
				C + (U)
			};
		}
	}

	public static class Vector3Extensions {
		public static bool IsFinite(this Vector3 vector) {
			return float.IsFinite(vector.x) && float.IsFinite(vector.y) && float.IsFinite(vector.z);
		}
	}
}

