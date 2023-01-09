using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class TrackingResizableBox : MonoBehaviour {
		// Unity fields
		[SerializeField] ResizableBox box;
		[SerializeField] RectTransform cameraBox;
		
		void Start() {
			CanvasScaler canvasScaler = GetComponentInParent<CanvasScaler>();
			canvasScaler.scaleFactor = SettingsUtil.GetUIScaleValue(Settings.GuiScale);
			Settings.GuiScaleListener += (value) => {
				canvasScaler.scaleFactor = SettingsUtil.GetUIScaleValue(value);
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
	}
}
