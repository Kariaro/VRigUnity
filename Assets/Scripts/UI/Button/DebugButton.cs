using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class DebugButton : MonoBehaviour {
		private TMP_Text text;
		private Button toggleButton;
		private bool isDebugShowing;

		[SerializeField] Color toggleOnColor  = new(0.5660378f, 0.5660378f, 0.5660378f); // 0x909090
		[SerializeField] Color toggleOffColor = new(1, 1, 1); // 0xffffff

		void Start() {
			text = GetComponentInChildren<TMP_Text>();
			toggleButton = GetComponent<Button>();

			InitializeContents();
		}

		private void InitializeContents() {
			isDebugShowing = false;
			text.color = toggleOnColor;

			toggleButton.onClick.RemoveAllListeners();
			toggleButton.onClick.AddListener(delegate {
				SetDebug(!isDebugShowing);
			});
		}

		private void SetDebug(bool enable) {
			isDebugShowing = enable;
			text.color = enable ? toggleOffColor : toggleOnColor;
			text.text = enable ? "Debug On" : "Debug Off";
			SolutionUtils.GetSolution().SetDebug(enable);
		}
	}
}
