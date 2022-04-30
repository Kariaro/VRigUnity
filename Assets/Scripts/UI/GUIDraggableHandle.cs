using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class GUIDraggableHandle : MonoBehaviour, IDragHandler {
		[SerializeField] RectTransform _parent;
		[SerializeField] Canvas _canvas;
		
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
			// TODO: Make sure the window cannot escape the viewing canvas
		}
	}
}
