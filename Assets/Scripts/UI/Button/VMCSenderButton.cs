using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HardCoded.VRigUnity {
	public class VMCSenderButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
		public VMCSender vmcSender;
		
		[SerializeField] private RectTransform canvasRect;
		[SerializeField] private CanvasGroup canvasGroup;
		private bool m_canvasVisible;

		[SerializeField] private TMP_Text buttonText;
		[SerializeField] private TMP_Text portText;
		private Button toggleButton;
		private Image buttonImage;
		private bool isVMCStarted;

		[SerializeField] Color toggleOnColor  = new(0.08009967f, 0.6792453f, 0.3454931f); // 0x14AD58
		[SerializeField] Color toggleOffColor = new(0.6981132f, 0, 0.03523935f); // 0xB30009
		
		void Start() {
			buttonImage = GetComponent<Image>();
			toggleButton = GetComponent<Button>();
			InitializeContents();
		}

		public void OnPointerEnter(PointerEventData data) {
			m_canvasVisible = true;
		}

		public void OnPointerExit(PointerEventData data) {
			m_canvasVisible = false;
		}

		void FixedUpdate() {
			Vector3 pos = canvasRect.localPosition;
			pos.y = Mathf.Lerp(pos.y, m_canvasVisible ? -24 : 0, 0.2f);
			canvasRect.localPosition = pos;
			canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, m_canvasVisible ? 1 : 0, 0.2f); 
		}

		private void InitializeContents() {
			buttonImage.color = toggleOnColor;
			isVMCStarted = false;

			// Setup settings listener (TODO: Remove)
			Settings.VMCSenderPortListener += (value) => {
				// Only display port changes when the VMC is closed
				if (!isVMCStarted) {
					portText.text = "Port " + value;
				}
			};

			portText.text = "Port " + Settings.VMCSenderPort;
			
			toggleButton.onClick.RemoveAllListeners();
			toggleButton.onClick.AddListener(delegate {
				SetVMC(!isVMCStarted);
			});
		}

		private void SetVMC(bool enable) {
			isVMCStarted = enable;
			buttonImage.color = enable ? toggleOffColor : toggleOnColor;
			buttonText.text = enable ? "Stop Sender VMC" : "Start Sender VMC";

			// Start/Stop the VMC instance
			if (enable) {
				vmcSender.SetPort(Settings.VMCSenderPort);	
				vmcSender.StartVMC();
			} else {
				vmcSender.StopVMC();
			}

			// Update port
			portText.text = "Port " + Settings.VMCSenderPort;
		}
	}
}
