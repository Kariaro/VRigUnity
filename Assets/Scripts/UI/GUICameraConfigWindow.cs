using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static HardCoded.VRigUnity.SettingsFieldTemplate;

namespace HardCoded.VRigUnity {
	public class GUICameraConfigWindow : GUISettingsBase {
		private Solution _solution;
		private SettingsField sourceField;
		private SettingsField resolutionField;

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
						if (!_solution.IsPaused()) {
							_solution.Play();
						}
					}, new(), 0, FieldData.None);
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
					.AddToggle((_, value) => { Settings.VirtualCamera = value; }, Settings.VirtualCamera, new(24))
					.AddButton("Install", (_) => { CameraCapture.InstallVirtualCamera(); }, FieldData.None)
					.AddButton("Uninstall", (_) => { CameraCapture.UninstallVirtualCamera(); }, FieldData.None);
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
			sourceField[0].Dropdown.SetValueWithoutNotify(sourceId < 0 ? 0 : sourceId);
		}

		private void UpdateResolutions() {
			var imageSource = SolutionUtils.GetImageSource();
			var resolutionNames = imageSource.availableResolutions;
			var resolutionId = resolutionNames.ToList().FindIndex(option => option.ToString() == Settings.CameraResolution);
			if (resolutionId >= 0 && resolutionId < resolutionNames.Length) {
				imageSource.SelectResolution(resolutionId);
			}

			var options = resolutionNames.ToList().Select(option => option.ToString()).ToList();
			resolutionField[0].Dropdown.ClearOptions();
			resolutionField[0].Dropdown.AddOptions(options);
			resolutionField[0].Dropdown.SetValueWithoutNotify(resolutionId < 0 ? 0 : resolutionId);
		}

		private void UpdateFlipped() {
			var imageSource = SolutionUtils.GetImageSource();
			imageSource.isHorizontallyFlipped = Settings.CameraFlipped;
		}
	}
}
