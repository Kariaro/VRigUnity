using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static HardCoded.VRigUnity.FileDialogUtils;
using System.Globalization;

namespace HardCoded.VRigUnity {
	public class GUITabModelConfig : GUITab {
		[Header("Buttons")]
		[SerializeField] private Button selectModelButton;
		[SerializeField] private Button resetModelButton;

		// Model position
		[Header("Model")]
		[SerializeField] private TMP_InputField modelX;
		[SerializeField] private TMP_InputField modelY;
		[SerializeField] private TMP_InputField modelZ;
		[SerializeField] private Button resetLayoutButton;

		// Links
		[Header("Links")]
		[SerializeField] private Button githubButton;
		[SerializeField] private Button discordButton;
		[SerializeField] private Button twitterButton;

		void Start() {
			selectModelButton.onClick.RemoveAllListeners();
			resetModelButton.onClick.RemoveAllListeners();
			resetLayoutButton.onClick.RemoveAllListeners();
			selectModelButton.onClick.AddListener(SelectModel);
			resetModelButton.onClick.AddListener(ResetModel);
			resetLayoutButton.onClick.AddListener(ResetCamera);

			modelX.onValueChanged.RemoveAllListeners();
			modelY.onValueChanged.RemoveAllListeners();
			modelZ.onValueChanged.RemoveAllListeners();
			modelX.onValueChanged.AddListener(delegate { SetModelTransform(); });
			modelY.onValueChanged.AddListener(delegate { SetModelTransform(); });
			modelZ.onValueChanged.AddListener(delegate { SetModelTransform(); });

			githubButton.onClick.RemoveAllListeners();
			discordButton.onClick.RemoveAllListeners();
			twitterButton.onClick.RemoveAllListeners();
			githubButton.onClick.AddListener(delegate { Application.OpenURL("https://github.com/Kariaro/VRigUnity"); });
			discordButton.onClick.AddListener(delegate { Application.OpenURL("https://discord.com/invite/Enaup9TJPd"); });
			twitterButton.onClick.AddListener(delegate { Application.OpenURL("https://twitter.com/HardCodedTwitch"); });
		}

		private void ResetModel() {
			guiMain.ResetModel();
		}

		private void SelectModel() {
			var extensions = new [] {
				new CustomExtensionFilter(Lang.DialogVrmFiles.Get(), "vrm"),
				new CustomExtensionFilter(Lang.DialogAllFiles.Get(), "*"),
			};

			FileDialogUtils.OpenFilePanel(this, Lang.DialogOpenFile.Get(), Settings.ModelFile, extensions, false, (paths) => {
				if (paths.Length > 0) {
					string filePath = paths[0];
					guiMain.LoadVrmModel(filePath);
				}
			});
		}

		private void ResetCamera() {
			guiMain.ResetCamera();
		}

		private bool TryParseFloat(string s, out float value) {
			return float.TryParse(
				string.IsNullOrEmpty(s) ? "0" : s,
				NumberStyles.Number,
				CultureInfo.GetCultureInfo("en-US"),
				out value
			);
		}

		private void SetModelTransform() {
			if (!TryParseFloat(modelX.text, out float x)
			|| !TryParseFloat(modelY.text, out float y)
			|| !TryParseFloat(modelZ.text, out float z)) {
				return;
			}

			guiMain.ModelTransform = new(x, y, z);
		}
	}
}
