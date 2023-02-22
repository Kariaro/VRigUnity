using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static HardCoded.VRigUnity.FileDialogUtils;
using static HardCoded.VRigUnity.SettingsFieldTemplate;

namespace HardCoded.VRigUnity {
	public class GUITabCameraConfig : GUITabConfig {
		[Header("Buttons")]
		[SerializeField] private Button reloadContentButton;

		private HolisticSolution _solution;
		private SettingsField sourceField;
		private SettingsField resolutionField;
		private SettingsField customResolutionField;

		protected override void InitializeSettings() {
			var imageSource = SolutionUtils.GetImageSource();
			_solution = SolutionUtils.GetSolution();
			
			reloadContentButton.onClick.RemoveAllListeners();
			reloadContentButton.onClick.AddListener(ReloadContents);

			AddDivider(Lang.CameraTabDividerCamera); 
			sourceField = CreateSetting(Lang.CameraTabSource, builder => {
				return builder
					.AddDropdown((elm, value) => {
						Settings.CameraName = elm.options[value].text;
						imageSource.SelectSource(value);
						if (!_solution.IsPaused) {
							_solution.Play();
						}
					}, new(), 0, FieldData.None);
			});
			resolutionField = CreateSetting(Lang.CameraTabResolution, builder => {
				return builder
					.AddDropdown((elm, value) => {
						Settings.CameraResolution = elm.options[value].text;
						imageSource.SelectResolution(value);
						UpdateCustomResolution();
						if (!_solution.IsPaused) {
							_solution.Play();
						}
					}, new(), 0, FieldData.None);
			});
			customResolutionField = CreateSetting(Lang.CameraTabCustomResolution, builder => {
				return builder
					.AddToggle((_, value) => { Settings.CameraCustomResolution = value; UpdateCustomResolution(true); }, Settings.CameraCustomResolution, new(24))
					.AddNumberInput((_, value) => UpdateCustomResolutionTest(value, 0, 0), 1, 1920, 176, 640, FieldData.None)
					.AddNumberInput((_, value) => UpdateCustomResolutionTest(0, value, 0), 1, 1080, 144, 360, FieldData.None)
					.AddNumberInput((_, value) => UpdateCustomResolutionTest(0, 0, value), 1, 30, 30, 30, FieldData.None);
			});
			CreateSetting(Lang.CameraTabIsHorizontallyFlipped, builder => {
				return builder.AddToggle((_, value) => {
					Settings.CameraFlipped = value;
					imageSource.IsHorizontallyFlipped = value;
					if (!_solution.IsPaused) {
						_solution.Play();
					}
				}, Settings.CameraFlipped, FieldData.None);
			});
			CreateSetting(Lang.CameraTabVirtualCamera, builder => {
				return builder
					.AddToggle((_, value) => Settings.Temporary.VirtualCamera = value, Settings.Temporary.VirtualCamera, new(24))
					.AddButton(Lang.CameraTabVirtualCameraInstall, (_) => CameraCapture.InstallVirtualCamera(), FieldData.None)
					.AddButton(Lang.CameraTabVirtualCameraUninstall, (_) => CameraCapture.UninstallVirtualCamera(), FieldData.None);
			});

			AddDivider(Lang.CameraTabDividerEffects);
			CreateSetting(Lang.CameraTabCustomBackground, builder => {
				return builder
					.AddToggle((_, value) => guiMain.SetShowBackgroundImage(value), Settings.ShowCustomBackground, new(24))
					.AddButton(Lang.CameraTabCustomBackgroundSelectImage, (_) => {
						var extensions = new [] {
							new CustomExtensionFilter(Lang.DialogImageFiles.Get(), new string[] { "png", "jpg", "jpeg" }),
							new CustomExtensionFilter(Lang.DialogAllFiles.Get(), "*"),
						};
			
						FileDialogUtils.OpenFilePanel(this, Lang.DialogOpenImage.Get(), Settings.ImageFile, extensions, false, (paths) => {
							if (paths.Length > 0) {
								string filePath = paths[0];
								guiMain.LoadCustomImage(filePath);
							}
						});
					}, FieldData.None);
			});

			ReloadContents();
		}

		public void ReloadContents() {
			StartCoroutine(UpdateContents());
		}

		private IEnumerator UpdateContents() {
			var imageSource = SolutionUtils.GetImageSource();
			yield return imageSource.UpdateSources();
			
			UpdateSources();
			UpdateResolutions();
			UpdateCustomResolution();
			UpdateFlipped();

			yield return null;
		}

		private void UpdateSources() {
			var imageSource = SolutionUtils.GetImageSource();
			var sourceNames = imageSource.SourceCandidateNames;
			int sourceId = imageSource.SelectSourceFromName(Settings.CameraName);

			var options = new List<string>(sourceNames);
			sourceField[0].Dropdown.ClearOptions();
			sourceField[0].Dropdown.AddOptions(options);
			sourceField[0].Dropdown.SetValueWithoutNotify(sourceId >= 0 ? sourceId : 0);
		}

		private void UpdateResolutions() {
			var imageSource = SolutionUtils.GetImageSource();
			var resolutions = imageSource.AvailableResolutions;
			int resolutionId = imageSource.SelectResolutionFromString(Settings.CameraResolution);

			var options = resolutions.ToList().Select(option => option.ToString()).ToList();
			resolutionField[0].Dropdown.ClearOptions();
			resolutionField[0].Dropdown.AddOptions(options);
			if (resolutionId >= 0) {
				resolutionField[0].Dropdown.SetValueWithoutNotify(resolutionId);
			} else {
				resolutionField[0].Dropdown.value = 6;
			}
		}

		private void UpdateCustomResolution(bool custom = false) {
			var widthField = customResolutionField[1].InputField;
			var heightField = customResolutionField[2].InputField;
			var fpsField = customResolutionField[3].InputField;

			bool active = customResolutionField[0].Toggle.isOn;
			widthField.interactable = active;
			heightField.interactable = active;
			fpsField.interactable = active;
			resolutionField[0].Dropdown.interactable = !active;

			if (!active && custom) {
				UpdateResolutions();
			}

			var res = SettingsUtil.GetResolution(Settings.CameraResolution);
			widthField.SetTextWithoutNotify(res.width.ToString());
			heightField.SetTextWithoutNotify(res.height.ToString());
			fpsField.SetTextWithoutNotify(((int) res.frameRate).ToString());

			if (active) {
				// Values of zero means uninitialized
				UpdateCustomResolutionTest(0, 0, 0);
			}
		}

		private void UpdateCustomResolutionTest(int width, int height, int frameRate) {
			var widthField = customResolutionField[1].InputField;
			var heightField = customResolutionField[2].InputField;
			var fpsField = customResolutionField[3].InputField;
			if (width == 0) int.TryParse(widthField.text, out width);
			if (height == 0) int.TryParse(heightField.text, out height);
			if (frameRate == 0) int.TryParse(fpsField.text, out frameRate);
			
			string resolutionText = $"{width}x{height} ({frameRate}Hz)";

			// The resolution field should contain the custom resolution
			resolutionField[0].Dropdown.ClearOptions();
			resolutionField[0].Dropdown.AddOptions(new List<string> { resolutionText });
			resolutionField[0].Dropdown.SetValueWithoutNotify(0);

			// Update the camera resolution
			Settings.CameraResolution = resolutionText;
		}

		private void UpdateFlipped() {
			var imageSource = SolutionUtils.GetImageSource();
			imageSource.IsHorizontallyFlipped = Settings.CameraFlipped;
		}
	}
}
