using UnityEngine;

namespace HardCoded.VRigUnity {
	public class OverlayAlpha : MonoBehaviour {
		private CanvasGroup canvasGroup;
		[SerializeField] float timeMultiplier = 1;

		void Start() {
			canvasGroup = GetComponent<CanvasGroup>();
		}

		void FixedUpdate() {
			bool focus = Settings.AlwaysShowUI || Application.isFocused;
			float target = focus ? 1 : 0;
			float alpha = canvasGroup.alpha;
			float time = Time.fixedDeltaTime * timeMultiplier;
			canvasGroup.alpha = (time * alpha) + ((1 - time) * target);
		}
	}
}
