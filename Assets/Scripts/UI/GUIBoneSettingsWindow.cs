using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class GUIBoneSettingsWindow : MonoBehaviour {
		[SerializeField] private GUIScript settings;

		public Toggle[] toggles;

		void Start() {
			
		}

		void OnEnable() {
			for (int i = 0; i < toggles.Length; i++) {
				int v = i;
				toggles[i].isOn = BoneSettings.Get(i);
				toggles[i].onValueChanged.RemoveAllListeners();
				toggles[i].onValueChanged.AddListener(delegate {
					BoneSettings.Set(v, toggles[v].isOn);

					// Event
					SolutionUtils.GetSolution().OnBoneUpdate(v, toggles[v].isOn);
				});
			}
		}
	}
}
