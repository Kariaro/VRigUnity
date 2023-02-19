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

			// Try get the languages to give errors in the editor
			// This is required to generate errors in the logger live
			_ = LanguageLoader.Languages;
		}
	}
}
