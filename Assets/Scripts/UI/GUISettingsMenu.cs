using SFB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class GUISettingsMenu : MonoBehaviour {
		[SerializeField] GUIScript settings;

		public void ShowMenu() {
			gameObject.SetActive(true);
		}

		public void HideMenu() {
			gameObject.SetActive(false);
		}

		public void SelectModel() {
			var extensions = new [] {
				new ExtensionFilter("VRM Files", "vrm"),
				new ExtensionFilter("All Files", "*" ),
			};
			var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

			if (paths.Length > 0) {
				string filePath = paths[0];
				settings.LoadVrmModel(filePath);
			}
		}
	}
}