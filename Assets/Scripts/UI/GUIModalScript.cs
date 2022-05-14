using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniGLTF;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public abstract class GUIModalScript : MonoBehaviour {
		protected bool IsVisible => gameObject.activeSelf;

		public void Show() {
			gameObject.SetActive(true);
		}

		public void Hide() {
			gameObject.SetActive(false);
		}

		public void Toggle() {
			gameObject.SetActive(!IsVisible);
		}

		// Should this element block its parent
		public abstract bool IsBlockingParent();
	}
}
