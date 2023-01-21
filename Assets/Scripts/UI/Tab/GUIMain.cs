using System.IO;
using UniGLTF;
using UnityEngine;
using UnityEngine.UI;
using VRM;

namespace HardCoded.VRigUnity {
	/// <summary>
	/// Main script of the UI classes
	/// </summary>
	public class GUIMain : MonoBehaviour {
		[Header("Settings")]
		[SerializeField] private GameObject settingsMenu;
		[SerializeField] private OrbitalCamera orbitalCamera;
		[SerializeField] private CanvasScaler[] canvasScalers;
		public CustomizableCanvas customizableCanvas;
		public TrackingResizableBox trackingBox;

		public Vector3 ModelTransform { get; set; }

		void Start() {
			// Configure scene with settings
			LoadVrmModel(Settings.ModelFile);
			LoadCustomImage(Settings.ImageFile);
			SetShowBackgroundImage(Settings.ShowCustomBackground);
			
			UpdateCanvasScale(SettingsUtil.GetUIScaleValue(Settings.GuiScale));
			Settings.GuiScaleListener += (value) => {
				UpdateCanvasScale(SettingsUtil.GetUIScaleValue(value));
			};
		}

		private void UpdateCanvasScale(float scaleFactor) {
			// Update position of UI windows
			float positionMultiplier = canvasScalers[0].scaleFactor / scaleFactor;
			Transform parent = settingsMenu.transform.parent;
			foreach (Transform child in parent) {
				RectTransform rect = child.GetComponent<RectTransform>();
				rect.anchoredPosition *= positionMultiplier;
			}

			foreach (CanvasScaler canvas in canvasScalers) {
				canvas.scaleFactor = scaleFactor;
			}
		}

		public void ResetModel() {
			Settings.ModelFile = "";
			SolutionUtils.GetSolution().ResetVRMModel();
		}

		public void LoadVrmModel(string path) {
			if (path == "") {
				return;
			}

			if (!File.Exists(path)) {
				Logger.Log($"Failed to load vrm model '{path}'");
				return;
			}

			Logger.Log($"Load VRM Model: '{path}'");

			var data = new GlbFileParser(path).Parse();
			var vrm = new VRMData(data);
			using (var context = new VRMImporterContext(vrm)) {
				var loaded = context.Load();
				loaded.EnableUpdateWhenOffscreen();
				loaded.ShowMeshes();
				
				Settings.ModelFile = path;
				SolutionUtils.GetSolution().SetVRMModel(loaded.gameObject);
			}
		}

		public void LoadCustomImage(string path) {
			if (path == "") {
				return;
			}

			if (!File.Exists(path)) {
				Logger.Log($"Failed to load background image '{path}'");
				return;
			}

			Settings.ImageFile = path;
			Texture2D tex = new(2, 2);
			tex.LoadImage(File.ReadAllBytes(path));

			customizableCanvas.SetBackgroundImage(tex);
		}
		
		public void SetShowBackgroundImage(bool show) {
			Settings.ShowCustomBackground = show;
			customizableCanvas.ShowBackground(show);
		}

		public void SetBackgroundColor(Color color) {
			customizableCanvas.SetBackgroundColor(color);
		}

		public void ResetCamera() {
			orbitalCamera.ResetCamera();
		}

		public void SetShowCamera(bool show) {
			customizableCanvas.ShowWebcam(show);
		}

		public void SetShowBackgroundColor(bool show) {
			Settings.ShowCustomBackgroundColor = show;
		}
	}
}
