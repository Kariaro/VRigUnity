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

			// Make this element the top sibling
			transform.SetAsLastSibling();

			// TODO: If the Transform is dragged outside the screen it should remember the original drag point
			_parent.anchoredPosition += eventData.delta / canvas.scaleFactor;

			// Make sure the window is within the correct position
			CheckPosition();
		}

		private void CheckPosition() {
			Vector2 size = _parent.rect.size;
			Vector2 screenSize = canvas.pixelRect.size / canvas.scaleFactor;
			Vector2 botLeft = _parent.anchoredPosition - (size / 2f) + (screenSize / 2f);
			Vector2 nudgeValue = Vector2.zero;
			float border = _border;

			if (botLeft.x > screenSize.x - border) {
				nudgeValue.x = screenSize.x - border - botLeft.x;
			}
			if (botLeft.x < border - size.x) {
				nudgeValue.x = border - size.x - botLeft.x;
			}
			if (botLeft.y > screenSize.y - border) {
				nudgeValue.y = screenSize.y - border - botLeft.y;
			}
			if (botLeft.y < 0) {
				nudgeValue.y = 0 - botLeft.y;
			}

			_parent.anchoredPosition += nudgeValue;
		}
	}
}
