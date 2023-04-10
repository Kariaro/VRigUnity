using UnityEngine;

namespace HardCoded.VRigUnity {
	public class DataGroups {
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

		public class HandData {
			public readonly int Length = 21;

			// Easy setters
			public Quaternion Wrist;
			public Quaternion ThumbCMC;
			public Quaternion ThumbMCP;
			public Quaternion ThumbIP;
			public Quaternion ThumbTIP;
			public Quaternion IndexFingerMCP;
			public Quaternion IndexFingerPIP;
			public Quaternion IndexFingerDIP;
			public Quaternion IndexFingerTIP;
			public Quaternion MiddleFingerMCP;
			public Quaternion MiddleFingerPIP;
			public Quaternion MiddleFingerDIP;
			public Quaternion MiddleFingerTIP;
			public Quaternion RingFingerMCP;
			public Quaternion RingFingerPIP;
			public Quaternion RingFingerDIP;
			public Quaternion RingFingerTIP;
			public Quaternion PinkyMCP;
			public Quaternion PinkyPIP;
			public Quaternion PinkyDIP;
			public Quaternion PinkyTIP;
			
			// This uses the indexs MediaPipe.Hand
			public Quaternion this[int index] {
				get {
					return index switch {
						MediaPipe.Hand.WRIST => Wrist,
						MediaPipe.Hand.THUMB_CMC => ThumbCMC,
						MediaPipe.Hand.THUMB_MCP => ThumbMCP,
						MediaPipe.Hand.THUMB_IP => ThumbIP,
						MediaPipe.Hand.THUMB_TIP => ThumbTIP,
						MediaPipe.Hand.INDEX_FINGER_MCP => IndexFingerMCP,
						MediaPipe.Hand.INDEX_FINGER_PIP => IndexFingerPIP,
						MediaPipe.Hand.INDEX_FINGER_DIP => IndexFingerDIP,
						MediaPipe.Hand.INDEX_FINGER_TIP => IndexFingerTIP,
						MediaPipe.Hand.MIDDLE_FINGER_MCP => MiddleFingerMCP,
						MediaPipe.Hand.MIDDLE_FINGER_PIP => MiddleFingerPIP,
						MediaPipe.Hand.MIDDLE_FINGER_DIP => MiddleFingerDIP,
						MediaPipe.Hand.MIDDLE_FINGER_TIP => MiddleFingerTIP,
						MediaPipe.Hand.RING_FINGER_MCP => RingFingerMCP,
						MediaPipe.Hand.RING_FINGER_PIP => RingFingerPIP,
						MediaPipe.Hand.RING_FINGER_DIP => RingFingerDIP,
						MediaPipe.Hand.RING_FINGER_TIP => RingFingerTIP,
						MediaPipe.Hand.PINKY_MCP => PinkyMCP,
						MediaPipe.Hand.PINKY_PIP => PinkyPIP,
						MediaPipe.Hand.PINKY_DIP => PinkyDIP,
						MediaPipe.Hand.PINKY_TIP => PinkyTIP,
						_ => Quaternion.identity,
					};
				}
				set {
					switch (index) {
						case MediaPipe.Hand.WRIST: Wrist = value; return;
						case MediaPipe.Hand.THUMB_CMC: ThumbCMC = value; return;
						case MediaPipe.Hand.THUMB_MCP: ThumbMCP = value; return;
						case MediaPipe.Hand.THUMB_IP: ThumbIP = value; return;
						case MediaPipe.Hand.THUMB_TIP: ThumbTIP = value; return;
						case MediaPipe.Hand.INDEX_FINGER_MCP: IndexFingerMCP = value; return;
						case MediaPipe.Hand.INDEX_FINGER_PIP: IndexFingerPIP = value; return;
						case MediaPipe.Hand.INDEX_FINGER_DIP: IndexFingerDIP = value; return;
						case MediaPipe.Hand.INDEX_FINGER_TIP: IndexFingerTIP = value; return;
						case MediaPipe.Hand.MIDDLE_FINGER_MCP: MiddleFingerMCP = value; return;
						case MediaPipe.Hand.MIDDLE_FINGER_PIP: MiddleFingerPIP = value; return;
						case MediaPipe.Hand.MIDDLE_FINGER_DIP: MiddleFingerDIP = value; return;
						case MediaPipe.Hand.MIDDLE_FINGER_TIP: MiddleFingerTIP = value; return;
						case MediaPipe.Hand.RING_FINGER_MCP: RingFingerMCP = value; return;
						case MediaPipe.Hand.RING_FINGER_PIP: RingFingerPIP = value; return;
						case MediaPipe.Hand.RING_FINGER_DIP: RingFingerDIP = value; return;
						case MediaPipe.Hand.RING_FINGER_TIP: RingFingerTIP = value; return;
						case MediaPipe.Hand.PINKY_MCP: PinkyMCP = value; return;
						case MediaPipe.Hand.PINKY_PIP: PinkyPIP = value; return;
						case MediaPipe.Hand.PINKY_DIP: PinkyDIP = value; return;
						case MediaPipe.Hand.PINKY_TIP: PinkyTIP = value; return;
						default: {
							Debug.LogError("Invalid index " + index);
							return;
						}
					}
				}
			}
		}
		
		public class PoseData {
			public Quaternion chestRotation;
			public Quaternion hipsRotation;
			public Vector3 hipsPosition;

			public Quaternion rUpperLeg;
			public Quaternion rLowerLeg;
			public Quaternion lUpperLeg;
			public Quaternion lLowerLeg;

			// IK Positions
			public Vector4 rShoulder;
			public Vector4 rElbow;
			public Vector4 rHand;
			public Vector4 lShoulder;
			public Vector4 lElbow;
			public Vector4 lHand;

			public bool hasLeftLeg;
			public bool hasRightLeg;
		}

		public class FaceData {
			// Mouth
			public float mouthOpen;

			// Eyes
			public Vector2 lEyeIris;
			public Vector2 rEyeIris;
			public float lEyeOpen;
			public float rEyeOpen;

			// Neck
			public Quaternion neckRotation;
		}
	}
}
