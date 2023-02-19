using UnityEngine;

namespace HardCoded.VRigUnity {
#if UNITY_EDITOR
	[ExecuteAlways]
#endif
	public class LanguageValidator : MonoBehaviour {
#if UNITY_EDITOR
		void LateUpdate() {
			if (!Application.isPlaying) {
				return;
			}

			// Try get the languages to give errors in the editor
			_ = LanguageLoader.Languages;
		}
#endif
	}
}
