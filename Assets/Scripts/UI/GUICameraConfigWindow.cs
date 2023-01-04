using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static HardCoded.VRigUnity.SettingsFieldTemplate;

namespace HardCoded.VRigUnity {
	public class GUICameraConfigWindow : GUISettingsBase {
		private Solution _solution;
		private SettingsField sourceField;
		private SettingsField resolutionField;
		private SettingsField customResolutionField;

		protected override void InitializeSettings() {
			var imageSource = SolutionUtils.GetImageSource();
			_solution = SolutionUtils.GetSolution();
			
			AddDivider("Camera");
			sourceField = CreateSetting("Source", builder => {
				return builder
					.AddDropdown((elm, value) => {
						Settings.CameraName = elm.options[value].text;
						imageSource.SelectSource(value);
						if (!_solution.IsPaused()) {
							_solution.Play();
						}
					}, new(), 0, FieldData.None);
			});
			resolutionField = CreateSetting("Resolution", builder => {
				return builder
					.AddDropdown((elm, value) => {
						Settings.CameraResolution = elm.options[value].text;
						imageSource.SelectResolution(value);
						UpdateCustomResolution();
						if (!_solution.IsPaused()) {
							_solution.Play();
						}
					}, new(), 0, FieldData.None);
			});
			customResolutionField = CreateSetting("Custom Res", builder => {
				return builder
					.AddToggle((_, value) => UpdateCustomResolution(true), false, new(24))
					.AddNumberInput((_, value) => UpdateCustomResolutionTest(), 1, 1920, 176, 176, FieldData.None)
					.AddNumberInput((_, value) => UpdateCustomResolutionTest(), 1, 1080, 144, 144, FieldData.None)
					.AddNumberInput((_, value) => UpdateCustomResolutionTest(), 1, 30, 30, 30, FieldData.None);
			});
			CreateSetting("Is Horizontally Flipped", builder => {
				return builder.AddToggle((_, value) => {
					Settings.CameraFlipped = value;
					imageSource.isHorizontallyFlipped = value;
					if (!_solution.IsPaused()) {
						_solution.Play();
					}
				}, Settings.CameraFlipped, FieldData.None);
			});
			CreateSetting("Virtual Camera", builder => {
				return builder
					.AddToggle((_, value) => Settings.VirtualCamera = value, Settings.VirtualCamera, new(24))
					.AddButton("Install", (_) => CameraCapture.InstallVirtualCamera(), FieldData.None)
					.AddButton("Uninstall", (_) => CameraCapture.UninstallVirtualCamera(), FieldData.None);
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
			var sourceNames = imageSource.sourceCandidateNames;
			var sourceId = sourceNames.ToList().FindIndex(source => source == Settings.CameraName);
			if (sourceId >= 0 && sourceId < sourceNames.Length) {
				imageSource.SelectSource(sourceId);
			}

			var options = new List<string>(sourceNames);
			sourceField[0].Dropdown.ClearOptions();
			sourceField[0].Dropdown.AddOptions(options);
			sourceField[0].Dropdown.SetValueWithoutNotify(sourceId >= 0 ? sourceId : 0);
		}

		private void UpdateResolutions() {
			var imageSource = SolutionUtils.GetImageSource();
			var resolutions = imageSource.availableResolutions;
			var resolutionId = resolutions.ToList().FindIndex(option => option.ToString() == Settings.CameraResolution);
			if (resolutionId >= 0) {
				imageSource.SelectResolution(resolutionId);
			}

			var options = resolutions.ToList().Select(option => option.ToString()).ToList();
			resolutionField[0].Dropdown.ClearOptions();
			resolutionField[0].Dropdown.AddOptions(options);
			resolutionField[0].Dropdown.SetValueWithoutNotify(resolutionId >= 0 ? resolutionId : 0);
		}

		private void UpdateCustomResolution(bool custom = false) {
			var res = SettingsUtil.GetResolution(Settings.CameraResolution);
			var widthField = customResolutionField[1].InputField;
			var heightField = customResolutionField[2].InputField;
			var fpsField = customResolutionField[3].InputField;

			widthField.SetTextWithoutNotify(res.width.ToString());
			heightField.SetTextWithoutNotify(res.height.ToString());
			fpsField.SetTextWithoutNotify(res.frameRate.ToString());

			bool active = customResolutionField[0].Toggle.isOn;
			widthField.interactable = active;
			heightField.interactable = active;
			fpsField.interactable = active;
			resolutionField[0].Dropdown.interactable = !active;

			if (active) {
				string test = $"{widthField.text}x{heightField.text} ({fpsField.text}Hz)";

				// The resolution field should contain the custom resolution
				resolutionField[0].Dropdown.ClearOptions();
				resolutionField[0].Dropdown.AddOptions(new List<string> { test });
				resolutionField[0].Dropdown.SetValueWithoutNotify(0);
			} else if (custom) {
				UpdateResolutions();
			}
		}

		private void UpdateCustomResolutionTest() {
			string test = $"{customResolutionField[1].InputField.text}x{customResolutionField[2].InputField.text} ({customResolutionField[3].InputField.text}Hz)";

			// The resolution field should contain the custom resolution
			resolutionField[0].Dropdown.ClearOptions();
			resolutionField[0].Dropdown.AddOptions(new List<string> { test });
			resolutionField[0].Dropdown.SetValueWithoutNotify(0);
		}

		private void UpdateFlipped() {
			var imageSource = SolutionUtils.GetImageSource();
			imageSource.isHorizontallyFlipped = Settings.CameraFlipped;
		}
	}
}
