using TMPro;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class HiddenModel : MonoBehaviour {
		private TMP_Text text;

		[SerializeField] private float outlineWidth = 0.1f;
		[SerializeField] private Color32 outlineColor = new(65, 65, 65, 255);

		void Start() {
			text = GetComponent<TMP_Text>();
			text.outlineColor = outlineColor;
			text.outlineWidth = outlineWidth;
		}

		void Update() {
			if (Settings.ShowModel == text.enabled) {
				text.enabled = !Settings.ShowModel;
			}
		}
	}
}
