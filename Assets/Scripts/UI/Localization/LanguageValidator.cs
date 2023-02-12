using UnityEngine;

namespace HardCoded.VRigUnity {
#if UNITY_EDITOR
	[ExecuteAlways]
#endif
	public class LanguageValidator : MonoBehaviour {
		void LateUpdate() {
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				return;
			}
#endif
			var test = LanguageLoader.Languages;
		}
	}
}
