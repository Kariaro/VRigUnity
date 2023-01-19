using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class ResizableBox : MonoBehaviour {
		// Unity fields
		[Header("Settings")]
		[SerializeField] float borderThickness = 50;
		[SerializeField] float borderInset = 10;
		[SerializeField] float borderSeparation = 10;
		[SerializeField] float minWidth = 50;
		[SerializeField] float minHeight = 50;

		[Header("Display")]
		[SerializeField] RectTransform targetRect;
		[SerializeField] Sprite cornerImage;
		[SerializeField] float cornerPixelsPerUnit = 1;

		[Header("Shadow")]
		[SerializeField] Color cornerShadowColor = new(0, 0, 0, 0.5f);
		[SerializeField] Vector2 cornerShadowOffset = new(1, 1);
		
		// Internal
		private readonly GameObject[] objects = new GameObject[9];
		private Canvas canvas;

		private Vector2 ScreenSize => new(Screen.width, Screen.height);
		private int SelectedIndex { get; set; } = -1;
		private int HoveredIndex { get; set; } = -1;
		private RectTransform rect;

		public Vector2 Offset { get; set; } = Vector2.zero;
		public Vector2 Size { get; set; } = new(0.5f, 0.5f);
		public Vector2 LocalSize { get; set; } = Vector2.one;
		public Action<Vector2, Vector2> Callback { private get; set; }

		public Vector2 FromScreenToLocal(Vector2 point) {
			Vector2 screen = ScreenSize * LocalSize;
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
					Vector2 size = box.Size;
					Vector2 pos;
					if (isShift) {
						if (virtualPos.sqrMagnitude == 0) {
							virtualPos = box.Offset;
						}

						virtualPos += delta;
						
						Vector2 pnt = box.FromScreenToLocal(new(30, 30));
						pos = new(
							(Mathf.Abs(virtualPos.x) < pnt.x) ? 0 : virtualPos.x,
							(Mathf.Abs(virtualPos.y) < pnt.y) ? 0 : virtualPos.y
						);
					} else if (virtualPos.sqrMagnitude != 0) {
						pos = virtualPos + delta;
						virtualPos = Vector2.zero;
					} else {
						pos = box.Offset + delta;
					}

					pos.x = Mathf.Clamp(pos.x, (size.x - 1) / 2.0f, (1 - size.x) / 2.0f);
					pos.y = Mathf.Clamp(pos.y, (size.y - 1) / 2.0f, (1 - size.y) / 2.0f);
					box.Offset = pos;
				} else {
					Vector2 posDelta = box.FromScreenToLocal(eventData.delta);
					
					if (x == 0) {
						delta.x = -delta.x;
					}
					if (x == 1) {
						delta.x = 0;
						posDelta.x = 0;
					}
					
					if (y == 2) {
						delta.y = -delta.y;
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
					
					Vector2 offset = box.Offset;
					float ax = 0.5f - size.x / 2.0f;
					float ay = 0.5f - size.y / 2.0f;
					if (!isShift) {
						if (x == 2) {
							delta.x = Mathf.Min(delta.x, ax - offset.x);
						} else if (x == 0) {	
							delta.x = Mathf.Min(delta.x, ax + offset.x);
						}

						if (y == 0) {
							delta.y = Mathf.Min(delta.y, ay - offset.y);
						} else if (y == 2) {
							delta.y = Mathf.Min(delta.y, ay + offset.y);
						}
					} else {
						// When shifting the size expands on both sides. 2x
						delta.x = Mathf.Min(delta.x, (ax + offset.x) * 2, (ax - offset.x) * 2);
						delta.y = Mathf.Min(delta.y, (ay + offset.y) * 2, (ay - offset.y) * 2);
					}

					box.Size += delta;
					if (!isShift) {
						delta.x = x == 0 ? -delta.x : delta.x;
						delta.y = y == 2 ? -delta.y : delta.y;
						box.Offset += delta / 2.0f;
					}
				}
				
				// Make sure all values are between 0 and 1
				Vector2 newSize = box.Size;
				newSize.x = Mathf.Clamp01(newSize.x);
				newSize.y = Mathf.Clamp01(newSize.y);
				box.Size = newSize;

				Vector2 newOffset = box.Offset;
				newOffset.x = Mathf.Clamp(newOffset.x, (newSize.x - 1) / 2.0f, (1 - newSize.x) / 2.0f);
				newOffset.y = Mathf.Clamp(newOffset.y, (newSize.y - 1) / 2.0f, (1 - newSize.y) / 2.0f);
				box.Offset = newOffset;
			}

			public void OnEndDrag(PointerEventData eventData) {
				box.SelectedIndex = -1;
				virtualPos = Vector2.zero;
			}

			void Update() {
				Color col = IsColored ? Color.gray : color;
				col.a = 0.15f;
				image.color = col;
			}
		}

		void Start() {
			canvas = GetComponentInParent<Canvas>();
			rect = GetComponent<RectTransform>();
			SelectedIndex = -1;

			// Add all components
			for (int i = 0; i < 9; i++) {
				int x = i % 3;
				int y = i / 3;
				
				GameObject obj = new(GetIndexName(i), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Shadow));
				objects[i] = obj;

				RectTransform rect = obj.GetComponent<RectTransform>();
				rect.SetParent(transform, true);
				rect.localScale = Vector3.one;
				
				Image image = obj.GetComponent<Image>();
				if (x != y && (x == 1 || y == 1)) {
					image.color = new(0.8396226f, 0.8396226f, 0.8396226f);
				} else if (x == 1 && y == 1){
					image.color = new(0.6f, 0.6f, 0.6f);
				}

				image.sprite = cornerImage;
				image.type = Image.Type.Sliced;
				image.pixelsPerUnitMultiplier = cornerPixelsPerUnit;
				
				Shadow shadow = obj.GetComponent<Shadow>();
				shadow.effectColor = cornerShadowColor;
				shadow.effectDistance = cornerShadowOffset;

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
			Vector2 screen = ScreenSize * LocalSize;
			float scaleFactor = SettingsUtil.GetUIScaleValue(Settings.GuiScale);
			rect.sizeDelta = screen * Size / scaleFactor;
			rect.anchoredPosition = screen * Offset / scaleFactor;
		}

		void UpdateParts() {
			for (int i = 0; i < 9; i++) {
				RectTransform rect = objects[i].GetComponent<RectTransform>();
				
				Image image = objects[i].GetComponent<Image>();
				image.sprite = cornerImage;
				image.pixelsPerUnitMultiplier = cornerPixelsPerUnit;

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

		void LateUpdate() {
			UpdateRect();
			UpdateParts();
			Callback?.Invoke(Offset, Size);
		}
	}
}
