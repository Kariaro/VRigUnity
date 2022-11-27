using UnityEngine;

namespace HardCoded.VRigUnity {
	public class GUIWindow : MonoBehaviour {
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
