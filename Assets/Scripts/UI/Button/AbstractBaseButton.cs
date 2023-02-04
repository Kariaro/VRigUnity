using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public abstract class AbstractBaseButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
		public bool Hover { private set; get; }
		protected TMP_Text buttonText;
		protected Button button;
		protected Image buttonImage;
		[SerializeField] protected Color toggleOn  = new(0.08009967f, 0.6792453f, 0.3454931f); // 0x14AD58
		[SerializeField] protected Color toggleOff = new(0.6981132f, 0, 0.03523935f); // 0xB30009

		public void Start() {
			buttonText = GetComponentInChildren<TMP_Text>();
			buttonImage = GetComponent<Image>();
			button = GetComponent<Button>();

			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(OnClick);

			InitializeContent();
		}

		public void OnPointerEnter(PointerEventData data) {
			Hover = true;
		}

		public void OnPointerExit(PointerEventData data) {
			Hover = false;
		}

		/// <summary>
		/// This is called during Start()
		/// </summary>
		protected abstract void InitializeContent();

		/// <summary>
		/// This is called when the button is clicked
		/// </summary>
		protected abstract void OnClick();
	}
}
