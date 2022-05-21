using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class GUIDraggableHandle : MonoBehaviour, IDragHandler {
		[SerializeField] RectTransform _parent;
		[SerializeField] Canvas _canvas;
		[SerializeField] float _border = 50;
		
		void Start() {
			_canvas = gameObject.GetComponentInParent<Canvas>();
		}

		public void OnDrag(PointerEventData eventData) {
			_parent.anchoredPosition += eventData.delta / _canvas.scaleFactor;

			// Make sure the window is within the correct position
			CheckPosition();
		}

		void LateUpdate() {
			CheckPosition();
		}

		private void CheckPosition() {
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
