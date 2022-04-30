using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HardCoded.VRigUnity {
	public class TabNavigation : MonoBehaviour {
		[SerializeField] private List<Selectable> elements;
		[SerializeField] private bool wrapAround;

		void Update() {
			// Only if this object is selected we should calculate navigation
			GameObject currentObject = EventSystem.current.currentSelectedGameObject;

			int idx = elements?.FindIndex(0, elements.Count, e => (e != null ? e.gameObject : null) == currentObject) ?? -1;
			if (idx != -1 && Input.GetKeyDown(KeyCode.Tab)) {
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
