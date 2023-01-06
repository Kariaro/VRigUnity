using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class ResizableBox : MonoBehaviour {
		// Unity fields
		[SerializeField] float borderThickness = 50;
		[SerializeField] float borderInset = 10;
		[SerializeField] float borderSeparation = 10;
		[SerializeField] float minWidth = 50;
		[SerializeField] float minHeight = 50;
		[SerializeField] RectTransform targetRect;
		
		// Internal
		private readonly GameObject[] objects = new GameObject[9];
		private Canvas canvas;

		private int SelectedIndex { get; set; }
		private int HoveredIndex { get; set; }
		private RectTransform rect;

		public Vector2 Size = new(0.5f, 0.5f);
		public Vector2 Offset = Vector2.zero;
		public Vector2 LocalSize = Vector2.one;

		public void UpdateScale(Vector2 size) {
			LocalSize = size;
			// transform.localScale = new(size.x, size.y, 1);
		}

		public Vector2 FromScreenToLocal(Vector2 point) {
			Vector2 screen = new Vector2(Screen.width, Screen.height) * LocalSize;
			return point / screen;
		}

		public class HoverBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler {
			public ResizableBox box;
			public int index;

			// Internal
			private Image image;
			private Color color;
			private Vector2 virtualPos;

			// Properties
			private bool IsShift => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			private bool IsColored {
				get {
					int hoveringIdx = box.HoveredIndex;
					int selectedIdx = box.SelectedIndex;
					if (index == selectedIdx || (selectedIdx == -1 && index == hoveringIdx)) {
						return true;
					}

					return IsShift && (selectedIdx != -1 ? selectedIdx : hoveringIdx) == (8 - index);
				}
			}
			
			void Start() {
				image = GetComponent<Image>();
				color = image.color;
			}
			
			public void OnPointerEnter(PointerEventData eventData) {
				box.HoveredIndex = index;
			}

			public void OnPointerExit(PointerEventData eventData) {
				box.HoveredIndex = -1;
			}

			public void OnDrag(PointerEventData eventData) {
				box.SelectedIndex = index;

				// Depending on the index
				int x = index % 3;
				int y = index / 3;
				
				bool isShift = IsShift;

				Vector2 delta = box.FromScreenToLocal(eventData.delta);
				if (x == 1 && y == 1) {
					if (isShift) {
						if (virtualPos.sqrMagnitude == 0) {
							virtualPos = box.Offset;
						}
						virtualPos += delta;
						
						Vector2 pnt = box.FromScreenToLocal(new(30, 30));
						Vector2 pos = new(
							(Mathf.Abs(virtualPos.x) < pnt.x) ? 0 : virtualPos.x,
							(Mathf.Abs(virtualPos.y) < pnt.y) ? 0 : virtualPos.y
						);
						box.Offset = pos;
					} else if (virtualPos.sqrMagnitude != 0) {
						box.Offset = virtualPos + delta;
						virtualPos = Vector2.zero;
					} else {
						Vector2 next = box.Offset + delta;
						Vector2 size = box.Size;
						next.x = Mathf.Clamp(next.x, (size.x - 1) / 2.0f, (1 - size.x) / 2.0f);
						next.y = Mathf.Clamp(next.y, (size.y - 1) / 2.0f, (1 - size.y) / 2.0f);
						box.Offset = next;
					}
				} else {
					Vector2 posDelta = box.FromScreenToLocal(eventData.delta);
					
					if (x == 0) {
						delta.x = -delta.x;
					}
					
					if (y == 2) {
						delta.y = -delta.y;
					}

					if (x == 1) {
						delta.x = 0;
						posDelta.x = 0;
					}

					if (y == 1) {
						delta.y = 0;
						posDelta.y = 0;
					}

					if (isShift) {
						delta *= 2;
					}
					
					Vector2 minSize = box.FromScreenToLocal(new(box.minWidth, box.minHeight));
					Vector2 size = box.Size;
					if (size.x + delta.x < minSize.x) {
						delta.x = minSize.x - size.x;
					}

					if (size.y + delta.y < minSize.y) {
						delta.y = minSize.y - size.y;
					}

					if (!isShift) {
						Vector2 offset = box.Offset;
						if (x == 2) {
							if (offset.x + size.x / 2.0f + delta.x > 0.5f) {
								delta.x += 0.5f - offset.x - size.x / 2.0f - delta.x;
							}
						} else if (x == 0) {
							if (offset.x - size.x / 2.0f - delta.x < -0.5f) {
								delta.x -= (-0.5f - offset.x + size.x / 2.0f + delta.x);
							}
						}

						if (y == 0) {
							if (offset.y + size.y / 2.0f + delta.y > 0.5f) {
								delta.y += 0.5f - offset.y - size.y / 2.0f - delta.y;
							}
						} else if (y == 2) {
							if (offset.y - size.y / 2.0f - delta.y < -0.5f) {
								delta.y -= (-0.5f - offset.y + size.y / 2.0f + delta.y);
							}
						}
					}

					box.Size += delta;
					if (!isShift) {
						delta.x = x == 0 ? -delta.x : delta.x;
						delta.y = y == 2 ? -delta.y : delta.y;
						box.Offset += delta / 2.0f;
					}
				}
			}

			public void OnEndDrag(PointerEventData eventData) {
				box.SelectedIndex = -1;
				virtualPos = Vector2.zero;
			}

			void Update() {
				Color col = IsColored ? Color.gray : color;
				col.a = 0.25f;
				image.color = col;
			}
		}

		void Start() {
			canvas = GetComponentInParent<Canvas>();
			rect = GetComponent<RectTransform>();
			SelectedIndex = -1;

			// Add all components
			for (int i = 0; i < 9; i++) {
				GameObject obj = new(GetIndexName(i), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				objects[i] = obj;

				RectTransform rect = obj.GetComponent<RectTransform>();
				rect.SetParent(transform, true);
				
				int x = i % 3;
				int y = i / 3;
				if (x != y && (x == 1 || y == 1)) {
					Image image = obj.GetComponent<Image>();
					image.color = new(0.8396226f, 0.8396226f, 0.8396226f, 0.05f);
				}

				// Add the hover box test component
				HoverBox box = obj.AddComponent<HoverBox>();
				box.box = this;
				box.index = i;
			}
		}

		string GetIndexName(int index) {
			return index switch {
				0 => "Top Left",
				1 => "Top Middle",
				2 => "Top Right",
				3 => "Mid Left",
				4 => "Middle",
				5 => "Mid Right",
				6 => "Bot Left",
				7 => "Bot Middle",
				8 => "Bot Right",
				_ => "invalid"
			};
		}

		void UpdateRect() {
			Vector2 screen = new Vector2(Screen.width, Screen.height) * LocalSize;
			rect.sizeDelta = screen * Size;
			rect.anchoredPosition = screen * Offset;
		}

		void UpdateParts() {
			for (int i = 0; i < 9; i++) {
				RectTransform rect = objects[i].GetComponent<RectTransform>();

				int x = i % 3;
				int y = i / 3;

				{ // Fix anchors
					Vector2 anchorMin = new(x / 2.0f, 1 - y / 2.0f);
					Vector2 anchorMax = new(x / 2.0f, 1 - y / 2.0f);
					
					if (x == 1) {
						anchorMin.x = 0;
						anchorMax.x = 1;
					}
					if (y == 1) {
						anchorMin.y = 0;
						anchorMax.y = 1;
					}

					rect.anchorMin = anchorMin;
					rect.anchorMax = anchorMax;
				}

				{ // Fix position
					Vector2 pos = Vector2.zero;
					pos.y = (y - 1) * (borderInset / 2.0f);
					pos.x = (1 - x) * (borderInset / 2.0f);
					rect.anchoredPosition = pos;
				}

				{ // Fix size
					Vector2 size = new(borderThickness, borderThickness);
					if (x == 1) {
						size.x = -(borderThickness + borderInset + borderSeparation * 2);
					}
					if (y == 1) {
						size.y = -(borderThickness + borderInset + borderSeparation * 2);
					}

					rect.sizeDelta = size;
				}
			}
		}

		void Update() {
			UpdateRect();
			UpdateParts();
		}
	}
}
