using Mediapipe.Unity;
using System.Collections;
using System.Collections.Generic;
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

		private TestHolisticTrackingSolution holisticSolution;
		private bool showWebCamImage;
		private WebCamSource webCamSource;

		void Start() {
			// There is only one instance of the holistic solution
			holisticSolution = SolutionUtils.GetSolution() as TestHolisticTrackingSolution;
			webCamSource = holisticSolution.GetComponent<WebCamSource>();
		}

		public void LoadVrmModel(string path) {
			Debug.Log("Load VRM Model: '" + path + "'");

			var data = new GlbFileParser(path).Parse();
			var vrm = new VRMData(data);
			using (var context = new VRMImporterContext(vrm)) {
				var loaded = context.Load();
				loaded.EnableUpdateWhenOffscreen();
				loaded.ShowMeshes();

				holisticSolution.SetVrmModel(loaded.gameObject);
			}
		}

		public void LoadCustomImage(string path) {
			if (File.Exists(path)) {
				Texture2D tex = new(2, 2);
				tex.LoadImage(File.ReadAllBytes(path));
				foreach (RawImage image in customBackgroundImages) {
					image.texture = tex;
				}
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

		public void SetShowBackgroundImage(bool show) {
			foreach (RawImage image in customBackgroundImages) {
				image.color = show ? Color.white : Color.clear;
			}
		}

		public void DrawImage(TextureFrame textureFrame) {
			if (showWebCamImage) {
				WebCamTexture texture = webCamSource.GetCurrentTexture() as WebCamTexture;
				Texture2D tex = worldBackgroundImage.texture as Texture2D;

				if (!(tex is Texture2D)) {
					if (tex == null || tex.width != texture.width || tex.height != texture.height) {
						tex = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
						worldBackgroundImage.texture = tex;
					}
				}
				
				float w = (UnityEngine.Screen.width / (float)UnityEngine.Screen.height);
				float d = (texture.height / (float)texture.width) * w * 0.5f;

				if (d < 0.5) {
					worldBackgroundImage.rectTransform.anchorMin = new Vector2(0, 0.5f - d);
					worldBackgroundImage.rectTransform.anchorMax = new Vector2(1, 0.5f + d);
				} else {
					d = ((texture.width / (float)texture.height) / w) * 0.5f;
					worldBackgroundImage.rectTransform.anchorMin = new Vector2(0.5f - d, 0);
					worldBackgroundImage.rectTransform.anchorMax = new Vector2(0.5f + d, 1);
				}

				textureFrame.CopyTexture(tex);
			}
		}

		/*
		void LateUpdate() {
			if (showWebCamImage) {
				WebCamTexture texture = webCamSource.GetCurrentTexture() as WebCamTexture;
				Texture2D tex = worldBackgroundImage.texture as Texture2D;

				if (texture == null) {
					return;
				}

				if (tex == null || tex.width != texture.width || tex.height != texture.height) {
					tex = new Texture2D(texture.width, texture.height);
					worldBackgroundImage.texture = tex;
				}

				if (tex != null) {
					tex.SetPixels32(texture.GetPixels32());
				}
			}
		}
		*/
	}
}
