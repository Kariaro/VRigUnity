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
		[SerializeField] Image worldBackgroundColor;
		[SerializeField] RawImage worldBackgroundImage;
		[SerializeField] RawImage[] customBackgroundImages;

		private bool showWebCamImage;
		private WebCamSource webCamSource;

		void Start() {
			// There is only one instance of the holistic solution
			webCamSource = SolutionUtils.GetImageSource();

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
			foreach (RawImage image in customBackgroundImages) {
				image.texture = tex;
			}
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
			worldBackgroundColor.color = color;
		}

		public void ResetCamera() {
			orbitalCamera.ResetCamera();
		}

		public void SetShowCamera(bool show) {
			worldBackgroundImage.texture = null;
			worldBackgroundImage.color = show ? new Color(1, 1, 1, 0.5f) : Color.clear;
			showWebCamImage = show;
		}

		public void UpdateShowCamera() {
			SetShowCamera(showWebCamImage);
		}

		public void SetShowBackgroundImage(bool show) {
			Settings.ShowCustomBackground = show;

			foreach (RawImage image in customBackgroundImages) {
				image.color = show ? Color.white : Color.clear;
			}
		}

		public void DrawImage(TextureFrame textureFrame) {
			if (!showWebCamImage) {
				return;
			}

			if (webCamSource == null) {
				webCamSource = SolutionUtils.GetImageSource();
				return;
			}

			WebCamTexture texture = webCamSource.GetCurrentTexture() as WebCamTexture;
			Texture2D tex = worldBackgroundImage.texture as Texture2D;
			
			if (!(tex is Texture2D)) {
				if (tex == null || tex.width != texture.width || tex.height != texture.height) {
					tex = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
					worldBackgroundImage.texture = tex;
				}
			}
				
			float w = (UnityEngine.Screen.width / (float) UnityEngine.Screen.height);
			float d = (texture.height / (float) texture.width) * w * 0.5f;

			if (d < 0.5) {
				worldBackgroundImage.rectTransform.anchorMin = new(0, 0.5f - d);
				worldBackgroundImage.rectTransform.anchorMax = new(1, 0.5f + d);
			} else {
				d = ((texture.width / (float) texture.height) / w) * 0.5f;
				worldBackgroundImage.rectTransform.anchorMin = new(0.5f - d, 0);
				worldBackgroundImage.rectTransform.anchorMax = new(0.5f + d, 1);
			}

			textureFrame.CopyTexture(tex);
		}
	}
}
