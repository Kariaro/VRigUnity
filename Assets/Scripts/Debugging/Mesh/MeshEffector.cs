using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class MeshEffector : MonoBehaviour {
		[SerializeField] private Color color = Color.white;
		[SerializeField] private bool lookAtCamera = false;
		private Renderer rend;

		void Start() {
			rend = GetComponent<Renderer>();
		}

		void Update() {
			var mat = rend.material;
			if (mat != null && mat.color != color) {
				mat.color = color;
			}

			if (lookAtCamera) {
				Camera mainCamera = Camera.main;
				transform.LookAt(mainCamera.transform.position, mainCamera.transform.up);
			}
		}
	}
}
