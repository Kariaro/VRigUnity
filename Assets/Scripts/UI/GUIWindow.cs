using UnityEngine;

namespace HardCoded.VRigUnity {
	public class GUIWindow : MonoBehaviour {
		public virtual void OnEnable() {
			transform.SetAsLastSibling();
		}
	}
}
