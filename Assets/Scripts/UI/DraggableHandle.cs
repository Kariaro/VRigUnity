using UnityEngine;
using UnityEngine.EventSystems;

namespace HardCoded.VRigUnity {
	public class DraggableHandle : MonoBehaviour, IDragHandler {
		[SerializeField] RectTransform _parent;
		[SerializeField] Canvas _canvas;
		[SerializeField] float _border = 50;
		
		void Start() {
			_canvas = gameObject.GetComponentInParent<Canvas>();
		}

		void LateUpdate() {
			CheckPosition();
		}

		public void OnDrag(PointerEventData eventData) {
			// TODO: If the Transform is dragged outside the screen it should remember the original drag point
			_parent.anchoredPosition += eventData.delta / _canvas.scaleFactor;

			// Make sure the window is within the correct position
			CheckPosition();
		}

		private void CheckPosition() {
			// TODO: This does not use the canvas scale factor
			Vector2 screenSize = _canvas.pixelRect.size;
			Vector2 parentSize = _parent.rect.size;
			Vector2 center = _parent.anchoredPosition + (screenSize / 2f);
			Vector2 topLeft = center - (parentSize / 2f);
			Vector2 bottomRight = center - (parentSize / 2f);
			Vector2 nudgeValue = Vector2.zero;

			if (topLeft.x > screenSize.x - _border) {
				nudgeValue.x = screenSize.x - _border - topLeft.x;
			}
			if (bottomRight.x < _border - parentSize.x) {
				nudgeValue.x = _border - parentSize.x - bottomRight.x;
			}
			if (topLeft.y > screenSize.y - _border) {
				nudgeValue.y = screenSize.y - _border - topLeft.y;
			}
			if (bottomRight.y < _border - parentSize.y) {
				nudgeValue.y = _border - parentSize.y - bottomRight.y;
			}

			_parent.anchoredPosition += nudgeValue;
		}
	}
}
