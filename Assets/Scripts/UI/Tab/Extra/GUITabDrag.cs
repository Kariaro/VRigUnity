using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class GUITabDrag : MonoBehaviour, IDragHandler {
		private GUITabSettings tabSettings;
		private RectTransform parent;
		private Canvas canvas;

		[Header("Settings")]
		[SerializeField] private int minWidth;
		[SerializeField] private int maxWidth;
		[SerializeField] private int minHeight;
		[SerializeField] private int maxHeight;

		private class Drag : MonoBehaviour, IDragHandler {
			public RectTransform _parent;
		
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
				Vector2 screenSize = canvas.pixelRect.size / canvas.scaleFactor;
				Vector2 newPos = _parent.anchoredPosition;
				Vector2 size = _parent.sizeDelta;
				newPos.x = Mathf.Clamp(newPos.x, (size.x - screenSize.x) / 2.0f, (screenSize.x - size.x) / 2.0f);
				newPos.y = Mathf.Clamp(newPos.y, (size.y - screenSize.y) / 2.0f, (screenSize.y - size.y) / 2.0f);

				if (size.x > screenSize.x) {
					newPos.x = (size.x - screenSize.x) / 2.0f;
					_parent.sizeDelta = new(screenSize.x, size.y);
					size = _parent.sizeDelta;
				}

				if (size.y > screenSize.y) {
					newPos.y = (screenSize.y - size.y) / 2.0f;
					_parent.sizeDelta = new(size.x, screenSize.y);
				}
				
				_parent.anchoredPosition = newPos;
			}
		}

		void Start() {
			tabSettings = GetComponentInParent<GUITabSettings>(true);
			canvas = GetComponentInParent<Canvas>();
			parent = tabSettings.GetComponent<RectTransform>();
			Drag drag = tabSettings.gameObject.AddComponent<Drag>();
			drag._parent = tabSettings.GetComponent<RectTransform>();
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

			if (newSize.x > screenSize.x) {
				newSize.x = screenSize.x;
			}
			
			if (newSize.y > screenSize.y) {
				newSize.y = screenSize.y;
			}

			// Min height 240 or 430
			Vector2 posAdj = (newSize - parent.sizeDelta) / 2f;
			posAdj.y = -posAdj.y;

			// TODO: Make sure we cant resize larger than the screen
			parent.anchoredPosition += posAdj;
			parent.sizeDelta = newSize;
		}
	}
}
