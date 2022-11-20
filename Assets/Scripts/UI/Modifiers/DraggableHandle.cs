using UnityEngine;
using UnityEngine.EventSystems;

namespace HardCoded.VRigUnity {
	public class DraggableHandle : MonoBehaviour, IDragHandler {
		[SerializeField] RectTransform _parent;
		[SerializeField] float _border = 50;
		
		private Canvas canvas;
		
		void Start() {
			canvas = GetComponentInParent<Canvas>();
		}

		void LateUpdate() {
			CheckPosition();
		}

		public void OnDrag(PointerEventData eventData) {
			if (eventData.used) {
				return;
			}

			// TODO: If the Transform is dragged outside the screen it should remember the original drag point
			_parent.anchoredPosition += eventData.delta / canvas.scaleFactor;

			// Make sure the window is within the correct position
			CheckPosition();
		}

		private void CheckPosition() {
			Vector2 parentSize = _parent.rect.size;
			Vector2 screenSize = canvas.pixelRect.size / canvas.scaleFactor;
			Vector2 center = _parent.anchoredPosition + (screenSize / 2f);
			Vector2 topLeft = center - (parentSize / 2f);
			Vector2 nudgeValue = Vector2.zero;
			float border = _border / canvas.scaleFactor;

			if (topLeft.x > screenSize.x - border) {
				nudgeValue.x = screenSize.x - border - topLeft.x;
			}
			if (topLeft.x < border - parentSize.x) {
				nudgeValue.x = border - parentSize.x - topLeft.x;
			}
			if (topLeft.y > screenSize.y - border) {
				nudgeValue.y = screenSize.y - border - topLeft.y;
			}
			if (topLeft.y < 0) {
				nudgeValue.y = 0 - topLeft.y;
			}

			_parent.anchoredPosition += nudgeValue;
		}
	}
}
