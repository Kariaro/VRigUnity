using UnityEngine;

public class SphereScript : MonoBehaviour {
	void OnMouseDown() {
		Debug.Log(transform.GetSiblingIndex());
	}
}
