using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class CameraButton : AbstractBaseButton {
		private bool isCameraShowing;

		// on  = #14AD58
		// off = #B30009

		protected override void InitializeContent() {
			buttonImage.color = toggleOn;
			isCameraShowing = false;

			Localization.OnLocalizationChangeEvent += UpdateLanguage;
		}

		protected override void OnClick() {
			SetCamera(!isCameraShowing);
		}

		private void SetCamera(bool enable) {
			buttonImage.color = enable ? toggleOff : toggleOn;
			isCameraShowing = enable;
			UpdateLanguage();

			if (enable) {
				SolutionUtils.GetSolution().Play((_, _) => {
					// Error handling
					SetCamera(false);
				});
			} else {
				SolutionUtils.GetSolution().Model.ResetVRMAnimator();
				SolutionUtils.GetSolution().Stop();
			}
		}

		private void UpdateLanguage() {
			buttonText.text = isCameraShowing
				? Lang.CameraStop.Get()
				: Lang.CameraStart.Get();
		}
	}
}
