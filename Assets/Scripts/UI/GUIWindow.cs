using UnityEngine;

namespace HardCoded.VRigUnity {
	public class GUIWindow : MonoBehaviour {
		private GUIScript _guiScript;
		public GUIScript GuiScript {
			get {
				if (_guiScript == null) {
					_guiScript = GetComponentInParent<GUIScript>();
				}

				return _guiScript;
			}
		}

		public virtual void OnEnable() {
			transform.SetAsLastSibling();
		}

		public void ShowWindow() {
			transform.SetAsLastSibling();
			gameObject.SetActive(true);
		}

		public void HideWindow() {
			gameObject.SetActive(false);
		}
	}
}
