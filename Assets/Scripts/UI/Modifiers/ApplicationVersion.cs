using TMPro;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class ApplicationVersion : MonoBehaviour {
		public TMP_Text text;

		void Start() {
			text.text = Application.version;
		}
	}
}
