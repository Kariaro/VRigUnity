using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HardCoded.VRigUnity {
	public class TabNavigation : MonoBehaviour {
		public List<Selectable> elements;
		public bool wrapAround;

		void Update() {
			// Only check if the tab key is pressed
			if (elements == null || !Input.GetKeyDown(KeyCode.Tab)) {
				return;
			}

			// Get the index of the currently selected object
			GameObject currentObject = EventSystem.current.currentSelectedGameObject;
			int idx = elements.FindIndex(e => e != null && e.gameObject == currentObject);
			if (idx != -1) {
				bool isDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
				int newIdx = idx - (isDown ? 1 : -1);

				if (wrapAround) {
					newIdx = (newIdx + elements.Count) % elements.Count;
				}

				newIdx = (newIdx < 0 ? 0 : (newIdx >= elements.Count ? elements.Count - 1 : newIdx));
				EventSystem.current.SetSelectedGameObject(elements[newIdx].gameObject);
			}
		}
	}
}
