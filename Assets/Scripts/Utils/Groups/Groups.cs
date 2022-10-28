using UnityEngine;

namespace HardCoded.VRigUnity {
	public class Groups {
		public class HandPoints {
			// This uses the indexs MediaPipe.Hand
			public readonly Vector3[] Data = new Vector3[21];
			
			// Helper properties
			public Vector3 Wrist { get => Data[0]; set => Data[0] = value; }
			public Vector3 ThumbCMC { get => Data[1]; set => Data[1] = value; }
			public Vector3 ThumbMCP { get => Data[2]; set => Data[2] = value; }
			public Vector3 ThumbIP { get => Data[3]; set => Data[3] = value; }
			public Vector3 ThumbTIP { get => Data[4]; set => Data[4] = value; }
			public Vector3 IndexFingerMCP { get => Data[5]; set => Data[5] = value; }
			public Vector3 IndexFingerPIP { get => Data[6]; set => Data[6] = value; }
			public Vector3 IndexFingerDIP { get => Data[7]; set => Data[7] = value; }
			public Vector3 IndexFingerTIP { get => Data[8]; set => Data[8] = value; }
			public Vector3 MiddleFingerMCP { get => Data[9]; set => Data[9] = value; }
			public Vector3 MiddleFingerPIP { get => Data[10]; set => Data[10] = value; }
			public Vector3 MiddleFingerDIP { get => Data[11]; set => Data[11] = value; }
			public Vector3 MiddleFingerTIP { get => Data[12]; set => Data[12] = value; }
			public Vector3 RingFingerMCP { get => Data[13]; set => Data[13] = value; }
			public Vector3 RingFingerPIP { get => Data[14]; set => Data[14] = value; }
			public Vector3 RingFingerDIP { get => Data[15]; set => Data[15] = value; }
			public Vector3 RingFingerTIP { get => Data[16]; set => Data[16] = value; }
			public Vector3 PinkyMCP { get => Data[17]; set => Data[17] = value; }
			public Vector3 PinkyPIP { get => Data[18]; set => Data[18] = value; }
			public Vector3 PinkyDIP { get => Data[19]; set => Data[19] = value; }
			public Vector3 PinkyTIP { get => Data[20]; set => Data[20] = value; }
		}

		public class HandRotation {
			// Easy setters
			public Quaternion Wrist; 
			public Quaternion ThumbMCP; 
			public Quaternion ThumbIP;
			public Quaternion ThumbTIP; 
			public Quaternion IndexFingerPIP; 
			public Quaternion IndexFingerDIP; 
			public Quaternion IndexFingerTIP; 
			public Quaternion MiddleFingerPIP;
			public Quaternion MiddleFingerDIP;
			public Quaternion MiddleFingerTIP;
			public Quaternion RingFingerPIP;
			public Quaternion RingFingerDIP;
			public Quaternion RingFingerTIP;
			public Quaternion PinkyPIP;
			public Quaternion PinkyDIP;
			public Quaternion PinkyTIP;

			// This uses the indexs MediaPipe.Hand
			public Quaternion[] Data {
				get {
					Quaternion[] data = new Quaternion[21];
					data[MediaPipe.Hand.WRIST] = Wrist;
					data[MediaPipe.Hand.THUMB_MCP] = ThumbMCP;
					data[MediaPipe.Hand.THUMB_IP] =	ThumbIP;
					data[MediaPipe.Hand.THUMB_TIP] = ThumbTIP;
					data[MediaPipe.Hand.INDEX_FINGER_PIP] =	IndexFingerPIP;
					data[MediaPipe.Hand.INDEX_FINGER_DIP] =	IndexFingerDIP;
					data[MediaPipe.Hand.INDEX_FINGER_TIP] =	IndexFingerTIP;
					data[MediaPipe.Hand.MIDDLE_FINGER_PIP] = MiddleFingerPIP;
					data[MediaPipe.Hand.MIDDLE_FINGER_DIP] = MiddleFingerDIP;
					data[MediaPipe.Hand.MIDDLE_FINGER_TIP] = MiddleFingerTIP;
					data[MediaPipe.Hand.RING_FINGER_PIP] = RingFingerPIP;
					data[MediaPipe.Hand.RING_FINGER_DIP] = RingFingerDIP;
					data[MediaPipe.Hand.RING_FINGER_TIP] = RingFingerTIP;
					data[MediaPipe.Hand.PINKY_PIP] = PinkyPIP;
					data[MediaPipe.Hand.PINKY_DIP] = PinkyDIP;
					data[MediaPipe.Hand.PINKY_TIP] = PinkyTIP;
					return data;
				}
			}
		}
	}
}
