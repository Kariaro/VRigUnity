using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class VMCReceiverButton : BaseButton {
		public VMCReceiver vmcReceiver;
		
		[SerializeField] private RectTransform canvasRect;
		[SerializeField] private CanvasGroup canvasGroup;
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
			Localization.OnLocalizationChangeEvent += UpdateLanguage;
		}

		void FixedUpdate() {
			Vector3 pos = canvasRect.localPosition;
			pos.y = Mathf.Lerp(pos.y, Hover ? -24 : 0, 0.2f);
			canvasRect.localPosition = pos;
			canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, Hover ? 1 : 0, 0.2f); 
		}

		private void InitializeContents() {
			buttonImage.color = toggleOnColor;

			Settings.VMCReceiverListener += (ip, port) => {
				// Only display port changes when the VMC is closed
				if (!isVMCStarted) {
					UpdateLanguage();
				}
			};
			
			toggleButton.onClick.RemoveAllListeners();
			toggleButton.onClick.AddListener(delegate {
				SetVMC(!isVMCStarted);
			});
		}

		private void SetVMC(bool enable) {
			isVMCStarted = enable;
			buttonImage.color = enable ? toggleOffColor : toggleOnColor;

			// Start/Stop the VMC instance
			if (enable) {
				vmcReceiver.SetPort(Settings.VMCReceiverPort);
				vmcReceiver.StartVMC();
			} else {
				vmcReceiver.StopVMC();
			}

			// Update port
			UpdateLanguage();
		}

		private void UpdateLanguage() {
			buttonText.text = isVMCStarted
				? Lang.VmcReceiverStop.Get()
				: Lang.VmcReceiverStart.Get();
			portText.text = Lang.VmcPort.Get() + " " + Settings.VMCReceiverPort;
		}
	}
}
