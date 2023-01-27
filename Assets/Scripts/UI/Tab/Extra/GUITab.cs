using System;
using System.IO;
using UniGLTF;
using UnityEngine;
using UnityEngine.UI;
using VRM;

namespace HardCoded.VRigUnity {
	public class GUITab : MonoBehaviour {
		private GUITabSettings _tabSettings;
		public GUITabSettings TabSettings {
			get {
				if (_tabSettings == null) {
					_tabSettings = GetComponentInParent<GUITabSettings>(true);
				}
				return _tabSettings;
			}
		}

		protected RectTransform rectTransform;
		public virtual void Awake() {
			rectTransform = GetComponent<RectTransform>();
		}
	}
}
