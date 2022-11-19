using UnityEngine;
using UnityEngine.EventSystems;

namespace HardCoded.VRigUnity {
	public class BaseButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
		public bool Hover { private set; get; } = false;

		public void OnPointerEnter(PointerEventData data) {
			Hover = true;
		}

		public void OnPointerExit(PointerEventData data) {
			Hover = false;
		}
	}
}
