using UnityEngine;
using UnityEngine.EventSystems;

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
			public GUITabDrag tabDrag;

			public void OnDrag(PointerEventData eventData) {
				if (eventData.used) {
					return;
				}

				tabDrag.OnMoveEvent(eventData.delta);
			}
		}

		void Start() {
			tabSettings = GetComponentInParent<GUITabSettings>(true);
			canvas = GetComponentInParent<Canvas>();
			parent = tabSettings.GetComponent<RectTransform>();
			tabSettings.gameObject.AddComponent<Drag>().tabDrag = this;
		}

		// What is the purpose of this class
		// 1. When dragged the settings should be resized
		// 2. When the settings is moved
		public void OnDrag(PointerEventData eventData) {
			OnSizeEvent(eventData.delta);
		}

		void LateUpdate() {
			// Fix size and position
			OnSizeEvent(Vector2.zero);
			OnMoveEvent(Vector2.zero);
		}

		public void OnMoveEvent(Vector2 delta) {
			float scale = SettingsUtil.GetUIScaleValue(Settings.GuiScale);

			Vector2 screenSize = canvas.pixelRect.size / scale;
			Vector2 newPos = parent.anchoredPosition + (delta / scale);
			Vector2 size = parent.sizeDelta;
			newPos.x = Mathf.Clamp(newPos.x, (size.x - screenSize.x) / 2.0f, (screenSize.x - size.x) / 2.0f);
			newPos.y = Mathf.Clamp(newPos.y, (size.y - screenSize.y) / 2.0f, (screenSize.y - size.y) / 2.0f);

			if (size.x > screenSize.x) {
				newPos.x = 0;
				size.x = screenSize.x;
			}

			if (size.y > screenSize.y) {
				newPos.y = 0;
				size.y = screenSize.y;
			}
			
			parent.anchoredPosition = newPos;
			parent.sizeDelta = size;
		}

		public void OnSizeEvent(Vector2 delta) {
			float scale = SettingsUtil.GetUIScaleValue(Settings.GuiScale);
			delta = new Vector2(delta.x, -delta.y) / scale;

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
			
			Vector2 screenSize = canvas.pixelRect.size / scale;
			if (newSize.y > (screenSize.y + parent.sizeDelta.y) / 2.0f + parent.anchoredPosition.y) {
				newSize.y = (screenSize.y + parent.sizeDelta.y) / 2.0f + parent.anchoredPosition.y;
			}
			
			if (newSize.x > (screenSize.x + parent.sizeDelta.x) / 2.0f - parent.anchoredPosition.x) {
				newSize.x = (screenSize.x + parent.sizeDelta.x) / 2.0f - parent.anchoredPosition.x;
			}

			// Always try to make the tab atleast the minimum size
			if (newSize.x < minWidth) {
				newSize.x = minWidth;
			}

			if (newSize.y < minHeight) {
				newSize.y = minHeight;
			}

			if (newSize.x > screenSize.x) {
				newSize.x = screenSize.x;
			}
			
			if (newSize.y > screenSize.y) {
				newSize.y = screenSize.y;
			}

			Vector2 posAdj = (newSize - parent.sizeDelta) / 2f;
			posAdj.y = -posAdj.y;

			parent.anchoredPosition += posAdj;
			parent.sizeDelta = newSize;
		}
	}
}
