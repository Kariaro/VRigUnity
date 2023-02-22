using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class SliderField : MonoBehaviour {
		[SerializeField] Slider slider;
		[SerializeField] TMP_Text text;

		void Start() {
			slider = GetComponent<Slider>();
			slider.onValueChanged.AddListener(delegate {
				UpdateText();
			});

			UpdateText();
		}

		void OnEnable() {
			UpdateText();
		}

		void UpdateText() {
			if (slider.wholeNumbers) {
				text.text = $"{(int) slider.value}";
			} else {
				text.text = $"{slider.value:0.00}";
			}
		}
	}
}
