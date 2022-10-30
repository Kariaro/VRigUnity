using Mediapipe.Unity;
using System.IO;
using UniGLTF;
using UnityEngine;
using UnityEngine.UI;
using VRM;

namespace HardCoded.VRigUnity {
	public class GUIScript : MonoBehaviour {
		[SerializeField] GUISettingsMenu settingsMenu;
		[SerializeField] OrbitalCamera orbitalCamera;
		[SerializeField] Vector3 modelTransform = Vector3.zero;
		[SerializeField] CustomizableCanvas customizableCanvas;

		private bool showWebCamImage;

		void Start() {
			// Configure scene with settings
			LoadVrmModel(Settings.ModelFile);
			LoadCustomImage(Settings.ImageFile);
			SetShowBackgroundImage(Settings.ShowCustomBackground);
		}

		public void ResetModel() {
			Settings.ModelFile = "";
			SolutionUtils.GetSolution().ResetVrmModel();
		}

		public void LoadVrmModel(string path) {
			if (!File.Exists(path)) {
				Logger.Log("Failed to load vrm model '" + path + "'");
				return;
			}

			Logger.Log("Load VRM Model: '" + path + "'");

			var data = new GlbFileParser(path).Parse();
			var vrm = new VRMData(data);
			using (var context = new VRMImporterContext(vrm)) {
				var loaded = context.Load();
				loaded.EnableUpdateWhenOffscreen();
				loaded.ShowMeshes();
				
				Settings.ModelFile = path;
				SolutionUtils.GetSolution().SetVrmModel(loaded.gameObject);
			}
		}

		public void LoadCustomImage(string path) {
			if (!File.Exists(path)) {
				Logger.Log("Failed to load background image '" + path + "'");
				return;
			}

			Settings.ImageFile = path;
			Texture2D tex = new(2, 2);
			tex.LoadImage(File.ReadAllBytes(path));

			customizableCanvas.SetBackgroundImage(tex);
		}

		public Vector3 GetModelTransform() {
			return modelTransform;
		}

		public void SetModelTransform(float x, float y, float z) {
			modelTransform.x = x;
			modelTransform.y = y;
			modelTransform.z = z;
		}

		public void SetBackgroundColor(Color color) {
			customizableCanvas.SetBackgroundColor(color);
		}

		public void ResetCamera() {
			orbitalCamera.ResetCamera();
		}

		public void SetShowCamera(bool show) {
			showWebCamImage = show;
			customizableCanvas.ShowWebcam(show);
		}

		public void SetShowBackgroundImage(bool show) {
			Settings.ShowCustomBackground = show;
			customizableCanvas.ShowBackground(show);
		}

		public void SetShowBackgroundColor(bool show) {
			Settings.ShowCustomBackgroundColor = show;
		}

		public void UpdateShowCamera() {
			SetShowCamera(showWebCamImage);
		}

		public void DrawImage(TextureFrame textureFrame) {
			customizableCanvas.DrawImage(textureFrame);
		}
	}
}
