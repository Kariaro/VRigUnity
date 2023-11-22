using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

namespace HardCoded.VRigUnity {
	public class MocapRecordingButton : AbstractBaseButton {
		private bool isRecording;
		
		private BVH.BVHExporter.MocapCollection collection = null;

		protected override void InitializeContent() {
			buttonImage.color = toggleOn;
			isRecording = false;

			Localization.OnLocalizationChangeEvent += UpdateLanguage;
		}

		protected override void OnClick() {
			isRecording = !isRecording;
			buttonImage.color = isRecording ? toggleOff : toggleOn;
			UpdateLanguage();

			if (isRecording) {
				collection = new();
			} else {
				BVH.BVHExporter.MocapCollection localCollection = collection;
				FileDialogUtils.SaveFilePanel(this, "Save BVH", "", "bvh", paths => {
					if (paths.Length > 0) {
						string filePath = paths[0];
						string data = BVH.BVHExporter.GenerateData(SolutionUtils.GetSolution().Model.Animator, localCollection);
						System.IO.File.WriteAllBytes(filePath, Encoding.ASCII.GetBytes(data));
					}
				});
				collection = null;
			}
		}

		void FixedUpdate() {
			if (collection != null) {
				collection.Update(0, SolutionUtils.GetSolution().Model.Animator);
			}
		}

		private void UpdateLanguage() {
			buttonText.text = isRecording
				? Lang.MocapRecordingStop.Get()
				: Lang.MocapRecordingStart.Get();
		}
	}
}
