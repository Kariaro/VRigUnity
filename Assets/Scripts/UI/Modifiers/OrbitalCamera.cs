using UnityEngine;
using UnityEngine.EventSystems;

namespace HardCoded.VRigUnity {
	public class OrbitalCamera : MonoBehaviour {
		[SerializeField] Vector3 defaultPosition;
		[SerializeField] Quaternion defaultRotation;
		[SerializeField] float defaultDepth = 0.8f;
		[SerializeField] float dragSpeed = 0.01f;
		[SerializeField] float minDepth = 0.8f;
		[SerializeField] float maxDepth = 10.0f;
		
		public bool IsShiftDown => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		public bool IsControlDown => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

		public enum Type {
			None,
			Rotating,
			Panning,
		}
		
		private float currentDepth;
		private Vector3 lastMouseDrag;
		private Type dragType = Type.None;

		public float Depth {
			set {
				currentDepth = Mathf.Clamp(value, minDepth, maxDepth);

				foreach (Transform child in transform) {
					Vector3 pos = child.localPosition;
					pos.z = currentDepth;
					child.localPosition = pos;
				}
			}
			get => currentDepth;
		}

		private bool IsMouseInGameWindow {
			get {
				Vector2 mouse = Input.mousePosition;
				return mouse.x >= 0 && mouse.y >= 0 && mouse.x <= Screen.width && mouse.y <= Screen.height;
			}
		}

		void Start() {
			Depth = defaultDepth;
		}
		
		public void ResetCamera() {
			transform.SetPositionAndRotation(defaultPosition, defaultRotation);
			Depth = defaultDepth;
		}
		
		void LateUpdate() {
			bool cameraEvent = !EventSystem.current.IsPointerOverGameObject();
			if (cameraEvent && IsMouseInGameWindow) {
				Depth -= Input.mouseScrollDelta.y / 10.0f;

				if (Input.GetMouseButtonDown(0)) {
					if (IsControlDown) {
						dragType = Type.Rotating;
					} else if (IsShiftDown) {
						dragType = Type.Panning;
					} else {
						dragType = Type.None;
					}

					lastMouseDrag = Input.mousePosition;
				}
			}

			if (Input.GetMouseButtonUp(0)) {
				dragType = Type.None;
			}

			if (!Input.GetMouseButton(0) || dragType == Type.None) {
				return;
			}

			Vector2 mouse = (lastMouseDrag - Input.mousePosition);
			lastMouseDrag = Input.mousePosition;

			switch (dragType) {
				case Type.Rotating: {
					Vector3 moved = transform.rotation.eulerAngles - new Vector3(mouse.y, mouse.x, 0);
					float x = MovementUtils.NormalizeAngle(moved.x);
					moved.x = Mathf.Clamp(x, -90, 90);
					moved.z = 0;
					transform.rotation = Quaternion.Euler(moved);
					break;
				}
				case Type.Panning: {
					// Pan in the forward direction of the rotation
					float scale = dragSpeed * Depth;
					Vector3 offset = transform.rotation * new Vector3(-mouse.x, mouse.y, 0) * scale * 0.4f;
					transform.position = transform.position + offset;
					break;
				}
			}
		}
	}
}
