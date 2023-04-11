using UnityEngine;

namespace HardCoded.VRigUnity.Visuals {
	public class Visualization : MonoBehaviour {
		public Mesh pointMesh;

		[SerializeField] private RectTransform annotationArea;
		
		private FaceAnnotation face;
		private IrisAnnotation iris;
		private PoseAnnotation pose;
		private HandAnnotation leftHand;
		private HandAnnotation rightHand;
		
		public Material lineMaterial;
		public Material pointMaterial;
		public Color lineColor = Color.white;
		public Color pointColor = new(0, 0.8509804f, 0.9058824f);
		public Color orangeColor = new(1, 0.5411765f, 0);
		public float lineWidth = 1;
		public float pointSize = 0.5f;

		public bool IsPrepared { get; private set; }

		void Start() {
			CreateChild("Face Annotation", out face);
			CreateChild("Iris Annotation", out iris);
			CreateChild("Pose Annotation", out pose);
			CreateChild("LeftHand Annotation", out leftHand);
			CreateChild("RightHand Annotation", out rightHand);

			leftHand.SetHandedness(HandAnnotation.HandType.Left);
			rightHand.SetHandedness(HandAnnotation.HandType.Right);
			IsPrepared = true;
		}

		void CreateChild<T>(string name, out T value) where T : MonoBehaviour {
			GameObject child = new(name);
			child.transform.SetParent(transform, false);
			value = child.AddComponent<T>();
		}

		public Vector3 FromNormalized(Vector3 v) {
			v.y = 1 - v.y;
			var rect = annotationArea.rect;
			return new(
				rect.width * (v.x - 0.5f),
				rect.height * (v.y - 0.5f),
				0
			);
		}

		public Vector3 FromNormalizedToScreen(Vector3 v) {
			v.y = 1 - v.y;
			var rect = annotationArea.rect;
			return new(
				rect.width * v.x + rect.xMin,
				rect.height * v.y + rect.yMin,
				0
			);
		}

		private HolisticLandmarks prevFaceLandmarks;
		private HolisticLandmarks prevLeftHandLandmarks;
		private HolisticLandmarks prevRightHandLandmarks;
		private HolisticLandmarks prevPoseLandmarks;

		public void DrawLandmarks(HolisticLandmarks faceLandmarks,
			HolisticLandmarks leftHandLandmarks,
			HolisticLandmarks rightHandLandmarks,
			HolisticLandmarks poseLandmarks,
			HolisticLandmarks poseWorldLandmarks) {

			const int FACE = 1;
			const int L_HAND = 2;
			const int R_HAND = 4;
			const int POSE = 8;
			int mask = 0;

			mask |= (faceLandmarks == prevFaceLandmarks) ? 0 : FACE;
			mask |= (leftHandLandmarks == prevLeftHandLandmarks) ? 0 : L_HAND;
			mask |= (rightHandLandmarks == prevRightHandLandmarks) ? 0 : R_HAND;
			mask |= (poseLandmarks == prevPoseLandmarks) ? 0 : POSE;
			
			if ((mask & FACE) != 0) {
				face.Apply(faceLandmarks);
				iris.Apply(faceLandmarks);
				prevFaceLandmarks = faceLandmarks;
			}

			if ((mask & L_HAND) != 0) {
				leftHand.Apply(leftHandLandmarks);
				prevLeftHandLandmarks = leftHandLandmarks;
			}

			if ((mask & R_HAND) != 0) {
				rightHand.Apply(rightHandLandmarks);
				prevRightHandLandmarks = rightHandLandmarks;
			}

			if ((mask & (FACE | POSE | L_HAND | R_HAND)) != 0) {
				pose.Apply(poseLandmarks, faceLandmarks, leftHandLandmarks, rightHandLandmarks);
				prevPoseLandmarks = poseLandmarks;
			}
		}
	}
}
