using System;
using System.IO;
using UniGLTF;
using UnityEngine;
using UnityEngine.UI;
using VRM;

namespace HardCoded.VRigUnity {
	public class GUITab : MonoBehaviour {
		protected RectTransform rectTransform;
		protected GUITabSettings tabSettings;
		protected GUIMain guiMain;
		public virtual void Awake() {
			rectTransform = GetComponent<RectTransform>();
			tabSettings = GetComponentInParent<GUITabSettings>();
			guiMain = GetComponentInParent<GUIMain>();
		}
	}
}
