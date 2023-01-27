using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class TrackingResizableBox : MonoBehaviour {
		// Unity fields
		[SerializeField] ResizableBox box;
		[SerializeField] RectTransform cameraBox;

		// API Getters
		public Vector2 Min => box.Offset - box.Size / 2.0f;
		public Vector2 Max => box.Offset + box.Size / 2.0f;

		public void Init() {
			Vector4 rect = SettingsUtil.GetResizableBox(Settings.TrackingBox);
			box.Offset = new(rect.x, rect.y);
			box.Size = new(rect.z, rect.w);
			box.Callback = (offset, size) => {
				string next = SettingsUtil.GetResizableBox(offset, size);
				if (next != Settings.TrackingBox) {
					Settings.TrackingBox = next;
				}
			};
		}

		void Update() {
			var image = SettingsUtil.GetResolution(Settings.CameraResolution);
			float w = (Screen.width / (float) Screen.height);
			float d = (image.height / (float) image.width) * w * 0.5f;

			if (d < 0.5) {
				cameraBox.anchorMin = new(0, 0.5f - d);
				cameraBox.anchorMax = new(1, 0.5f + d);
			} else {
				d = ((image.width / (float) image.height) / w) * 0.5f;
				cameraBox.anchorMin = new(0.5f - d, 0);
				cameraBox.anchorMax = new(0.5f + d, 1);
			}

			box.LocalSize = cameraBox.anchorMax - cameraBox.anchorMin;
		}

		public bool IsInside(Vector2 point) {
			return IsInside(point.x, point.y);
		}

		public bool IsInside(float x, float y) {
			// The rect is offset by (0.5, 0.5)
			x -= 0.5f;
			y -= 0.5f;

			Vector2 size = box.Size;
			Vector2 offset = box.Offset - size / 2.0f;
			return x >= offset.x
				&& x <= offset.x + size.x
				&& y >= offset.y
				&& y <= offset.y + size.y;
		}
	}
}
