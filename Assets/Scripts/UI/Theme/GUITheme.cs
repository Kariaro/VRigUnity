using UnityEngine;
using TMPro;
using UnityEngine.UI;
using HardCoded.VRigUnity.Theme;

namespace HardCoded.VRigUnity {
	public class GUITheme : MonoBehaviour {
		public Color[] colors;
		public Color textColor = Color.white;
		public Color tabOutline = Color.white;
		public Color colorA = new(0.4039216f, 0.3098039f, 0.2156863f); // 674F37
		public Color colorB = new(0.3411765f, 0.2549020f, 0.1725490f); // 57412C
		public Color colorC = new(0.2358491f, 0.1715313f, 0.1101371f); // 3C2C1C
		public Color colorLogger = new(0.1429779f, 0.1885245f, 0.1981132f); // 253133

		// colorA = 132623
		// colorB = 314448
		// colorLoggerBright = 253133; old -> 56402A
		// 2A3B3F, 0.3568628

		public GameObject[] buttons;

#if UNITY_EDITOR
		void Update() {
			ApplyColors();
		}

		void ApplyColors() {
			
			foreach (var item in GetComponentsInChildren<TMP_Text>(true)) {
				item.color = textColor;
			}

			foreach (var item in GetComponentsInChildren<ThemeSkin>(true)) {
				item.Apply(this);
			}
		}
#endif
	}
}
