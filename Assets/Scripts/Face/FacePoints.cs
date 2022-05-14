using Mediapipe;
using Mediapipe.Unity;
using System.Collections;
using System.IO;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class FacePoints {
		// Left Eye
		public static readonly int[] LeftEye = { 263, 466, 388, 387, 386, 385, 384, 398, 362, 382, 381, 380, 374, 373, 390, 249 };
		
		// Right Eye
		public static readonly int[] RightEye = { 133, 173, 157, 158, 159, 160, 161, 246, 33, 7, 163, 144, 145, 153, 154, 155 };

		// Iris position
		public static readonly int IrisCenter = 0;
		public static readonly int IrisLeft   = 1;
		public static readonly int IrisUp     = 2;
		public static readonly int IrisDown   = 3;
		public static readonly int IrisRight  = 4;

		// Right Eye Iris (center, left, up, right, down)
		public static readonly int[] RightEyeIris = { 468, 469, 470, 471, 472 };
		
		// Left Eye Iris (center, left, up, right, down)
		public static readonly int[] LeftEyeIris = { 473, 474, 475, 476, 477 };
		
		// Left
		//  p1 = 263, p2 = 387, p3 = 384,
		//  p4 = 463, p5 = 380, p6 = 373
		public static readonly int[] LeftEyeEAR = { 263, 387, 384, 463, 380, 373 };
		
		// Right
		//  p1 = 133, p2 = 157, p3 = 160,
		//  p4 =  33, p5 = 144, p6 = 153
		public static readonly int[] RightEyeEAR = { 133, 157, 160, 33, 144, 153 };
		
		// Eye Aspect Ratio
		// https://pyimagesearch.com/2017/04/24/eye-blink-detection-opencv-python-dlib/
		public static float CalculateEyeAspectRatio(Vector3[] points) {
			Vector3 p1 = points[0];
			Vector3 p2 = points[1];
			Vector3 p3 = points[2];
			Vector3 p4 = points[3];
			Vector3 p5 = points[4];
			Vector3 p6 = points[5];

			return ((p2 - p6).magnitude + (p3 - p5).magnitude) / (2 * (p1 - p4).magnitude);
		}

		
		// Iris Points
		public static readonly int[] LeftEyeIrisPoint = { LeftEyeEAR[0], LeftEyeEAR[3], LeftEyeIris[0] };
		public static readonly int[] RightEyeIrisPoint = { RightEyeEAR[0], RightEyeEAR[3], RightEyeIris[0] };

		public static Vector2 CalculateIrisPosition(Vector3[] points) {
			// /-------\
			// L   I   R
			// \-------/

			Vector3 leftPoint = points[0];
			Vector3 rightPoint = points[1];
			Vector3 irisPoint = points[2];
			
			// Find the index (0, 1) where I is on the line LR
			Vector3 point = GetClosestPointOnLine(leftPoint, rightPoint, irisPoint);
			bool clockwise = IsClockwise(leftPoint, rightPoint, irisPoint);
			float d = Vector3.Distance(leftPoint, rightPoint);
			float x = Vector3.Distance(leftPoint, point) / d;
			float y = (Vector3.Distance(point, irisPoint) * (clockwise ? 1 : -1)) / d;
			
			return new(x - 0.5f, y);
		}

		static bool IsClockwise(Vector3 a, Vector3 b, Vector3 c) {
			return (
				((b.x - a.x) * (a.y + b.y)) +
				((c.x - b.x) * (b.y + c.y)) +
				((a.x - c.x) * (c.y + a.y))
			) > 0;
		}

		static Vector3 GetClosestPointOnLine(Vector3 a, Vector3 b, Vector3 point) {
			Vector3 heading = b - a;
			float headingLength = heading.magnitude;
			heading.Normalize();
			float project_length = Mathf.Clamp(Vector3.Dot(point - a, heading), 0f, headingLength);
			return a + heading * project_length;
		}
	}
}
