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
		[SerializeField] TestHolisticTrackingSolution holisticSolution;
		[SerializeField] Vector3 modelTransform = Vector3.zero;
		[SerializeField] Image worldBackgroundColor;
		[SerializeField] RawImage worldBackgroundImage;

		private Solution solution;
		private bool showWebCamImage;
		private WebCamSource webCamSource;

		void Start() {
			solution = SolutionUtils.GetSolution();
			webCamSource = solution.GetComponent<WebCamSource>();
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

		public void SetShowCamera(bool show) {
			worldBackgroundImage.texture = null;
			worldBackgroundImage.color = show ? Color.white : Color.clear;
			showWebCamImage = show;
		}

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
	}
}
