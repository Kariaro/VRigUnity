using UnityEngine;

namespace HardCoded.VRigUnity {
	public class AutoFit : MonoBehaviour {
		[System.Serializable]
		public enum FitMode {
			Expand,
			Shrink,
			FitWidth,
			FitHeight,
		}

		[SerializeField] private FitMode _fitMode;
		private RectTransform rectTransform;
		private RectTransform parentTransform;

		void Start() {
			rectTransform = GetComponent<RectTransform>();
			parentTransform = transform.parent.GetComponent<RectTransform>();
		}

		void LateUpdate() {
			if (rectTransform.rect.width == 0 || rectTransform.rect.height == 0) {
				return;
			}

			var parentRect = parentTransform.rect;
			var (width, height) = GetBoundingBoxSize(rectTransform);

			var ratio = parentRect.width / width;
			var h = height * ratio;

			if (_fitMode == FitMode.FitWidth || (_fitMode == FitMode.Expand && h >= parentRect.height) || (_fitMode == FitMode.Shrink && h <= parentRect.height)) {
				rectTransform.offsetMin *= ratio;
				rectTransform.offsetMax *= ratio;
				return;
			}

			ratio = parentRect.height / height;

			rectTransform.offsetMin *= ratio;
			rectTransform.offsetMax *= ratio;
		}

		private (float, float) GetBoundingBoxSize(RectTransform rectTransform) {
			var rect = rectTransform.rect;
			var center = rect.center;
			var topLeftRel = new Vector2(rect.xMin - center.x, rect.yMin - center.y);
			var topRightRel = new Vector2(rect.xMax - center.x, rect.yMin - center.y);

			var rotatedTopLeftRel = topLeftRel;
			var rotatedTopRightRel = topRightRel;
			var wMax = Mathf.Max(Mathf.Abs(rotatedTopLeftRel.x), Mathf.Abs(rotatedTopRightRel.x));
			var hMax = Mathf.Max(Mathf.Abs(rotatedTopLeftRel.y), Mathf.Abs(rotatedTopRightRel.y));
			return (2 * wMax, 2 * hMax);
		}
	}
}
