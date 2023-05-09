using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class VisualsButton : AbstractBaseButton {
		private bool isDebugShowing;

		// on  = #909090
		// off = #ffffff

		protected override void InitializeContent() {
			isDebugShowing = false;
			buttonText.color = toggleOn;
			Localization.OnLocalizationChangeEvent += UpdateLanguage;
		}

		protected override void OnClick() {
			isDebugShowing = !isDebugShowing;
			buttonText.color = isDebugShowing ? toggleOff : toggleOn;
			UpdateLanguage();

			HolisticSolution solution = SolutionUtils.GetSolution();
			solution.Canvas.ShowAnnotations(isDebugShowing);
		}

		private void UpdateLanguage() {
			buttonText.text = isDebugShowing
				? Lang.VisualsOn.Get()
				: Lang.VisualsOff.Get();
		}
	}
}
