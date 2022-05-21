using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class GUICameraButton : MonoBehaviour {
		private TMP_Text text;
		private Button toggleButton;
		private Image buttonImage;
		private bool isCameraShowing;

		[SerializeField] Color toggleOnColor  = new(0, 0.7830189f, 0.35044f);    // 0x00C859
		[SerializeField] Color toggleOffColor = new(0.6981132f, 0, 0.03523935f); // 0xB30009

		void Start() {
			text = GetComponentInChildren<TMP_Text>();
			buttonImage = GetComponent<Image>();
			toggleButton = GetComponent<Button>();

			InitializeContents();
		}

		private void InitializeContents() {
			buttonImage.color = toggleOnColor;
			isCameraShowing = false;

			toggleButton.onClick.RemoveAllListeners();
			toggleButton.onClick.AddListener(delegate {
				SetCamera(!isCameraShowing);
			});
		}

		private void SetCamera(bool enable) {
			isCameraShowing = enable;
			buttonImage.color = enable ? toggleOffColor : toggleOnColor;
			text.text = enable ? "Stop Camera" : "Start Camera";

			if (enable) {
				SolutionUtils.GetSolution().Play();
			} else {
				SolutionUtils.GetSolution().Stop();
			}
		}
	}
}
