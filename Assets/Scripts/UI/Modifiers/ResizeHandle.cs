using UnityEngine;
using UnityEngine.EventSystems;

namespace HardCoded.VRigUnity {
	public class ResizeHandle : MonoBehaviour {
		[SerializeField]
		private RectTransform parent;

		[SerializeField]
		private RectTransform corner;
		private Canvas canvas;
		private DragCorner dragCorner;

		[Header("Settings")]
		[SerializeField] private int minWidth;
		[SerializeField] private int maxWidth;
		[SerializeField] private int minHeight;
		[SerializeField] private int maxHeight;

		void Start() {
			canvas = GetComponentInParent<Canvas>();
			dragCorner = corner.gameObject.AddComponent<DragCorner>();
			dragCorner.resizeHandle = this;
		}

		public class DragCorner : MonoBehaviour, IDragHandler {
			public ResizeHandle resizeHandle;

			public void OnDrag(PointerEventData eventData) {
				resizeHandle.OnDrag(eventData);
			}
		}

		public void OnDrag(PointerEventData eventData) {
			Vector2 delta = eventData.delta;
			delta = new Vector2(delta.x, -delta.y) / canvas.scaleFactor;

			Vector2 newSize = parent.sizeDelta + delta;
			if (minWidth != 0 && newSize.x < minWidth) {
				newSize.x = minWidth;
			}

			if (minHeight != 0 && newSize.y < minHeight) {
				newSize.y = minHeight;
			}

			if (maxWidth != 0 && newSize.x > maxWidth) {
				newSize.x = maxWidth;
			}

			if (maxHeight != 0 && newSize.y > maxHeight) {
				newSize.y = maxHeight;
			}
			
			Vector2 size = parent.rect.size;
			Vector2 screenSize = canvas.pixelRect.size / canvas.scaleFactor;
			if (newSize.y > parent.sizeDelta.y + parent.anchoredPosition.y - (size.y / 2f) + (screenSize.y / 2f)) {
				newSize.y = parent.sizeDelta.y + parent.anchoredPosition.y - (size.y / 2f) + (screenSize.y / 2f);
			}

			Vector2 posAdj = (newSize - parent.sizeDelta) / 2f;
			posAdj.y = -posAdj.y;

			// TODO: Make sure we cant resize larger than the screen
			parent.anchoredPosition += posAdj;
			parent.sizeDelta = newSize;
		}
	}
}
