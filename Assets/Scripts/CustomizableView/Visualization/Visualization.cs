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

		public void DrawLandmarks(HolisticLandmarks faceLandmarks,
			HolisticLandmarks leftHandLandmarks,
			HolisticLandmarks rightHandLandmarks,
			HolisticLandmarks poseLandmarks,
			HolisticLandmarks poseWorldLandmarks) {
			
			face.Apply(faceLandmarks);
			iris.Apply(faceLandmarks);
			leftHand.Apply(leftHandLandmarks);
			rightHand.Apply(rightHandLandmarks);
			pose.Apply(poseLandmarks, faceLandmarks, leftHandLandmarks, rightHandLandmarks);
		}
	}
}
