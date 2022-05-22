using UnityEngine;

namespace HardCoded.VRigUnity {
	public class SphereScript : MonoBehaviour {
		void OnMouseDown() {
			Debug.Log(transform.GetSiblingIndex());
		}
	}
}
