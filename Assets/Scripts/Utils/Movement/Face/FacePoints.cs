using System;
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
		public static readonly int[] LeftEyeEAR = { 362, 263, 386, 374 };
		
		// Right
		//  p1 = 133, p2 = 157, p3 = 160,
		//  p4 =  33, p5 = 144, p6 = 153
		public static readonly int[] RightEyeEAR = { 33, 133, 159, 145 };
		
		// Eye Aspect Ratio
		// https://pyimagesearch.com/2017/04/24/eye-blink-detection-opencv-python-dlib/
		// https://www.youtube.com/watch?v=XIJD43rbI-4
		public static float CalculateEyeAspectRatio(Vector3[] points) {
			// Left
			// left/right 33  -> 133
			// top/down   159 -> 145

			// Right
			// left/right 362 -> 263
			// top/down   386 -> 374

			float horizontal = (points[0] - points[1]).magnitude;
			float vertical = (points[2] - points[3]).magnitude;
			return vertical / horizontal;
		}

		
		// Iris Points
		public static readonly int[] LeftEyeIrisPoint = { LeftEyeEAR[0], LeftEyeEAR[1], LeftEyeIris[IrisCenter] };
		public static readonly int[] RightEyeIrisPoint = { RightEyeEAR[0], RightEyeEAR[1], RightEyeIris[IrisCenter] };

		public static Vector2 CalculateIrisPosition(int[] points, Converter<int, Vector3> converter) {
			// /-------\
			// L   I   R
			// \-------/

			Vector3 leftPoint  = converter(points[0]);
			Vector3 rightPoint = converter(points[1]);
			Vector3 irisPoint  = converter(points[2]);

			// Find the index (0, 1) where I is on the line LR
			Vector3 point = GetClosestPointOnLine(leftPoint, rightPoint, irisPoint);
			bool clockwise = IsClockwise(leftPoint, rightPoint, irisPoint);
			float d = Vector3.Distance(leftPoint, rightPoint);
			float x = Vector3.Distance(leftPoint, point) / d;
			float y = (Vector3.Distance(point, irisPoint) * (clockwise ? -1 : 1)) / d;
			
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
