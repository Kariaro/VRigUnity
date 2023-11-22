using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class VMCSenderButton : AbstractBaseButton {
		public VMCSender vmcSender;
		
		[SerializeField] private RectTransform canvasRect;
		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private TMP_Text portText;

		private bool isVMCStarted;

		// on  = #14AD58
		// off = #B30009
		
		protected override void InitializeContent() {
			buttonImage.color = toggleOn;

			Settings.VMCSenderListener += (ip, port) => {
				// Only display port changes when the VMC is closed
				if (!isVMCStarted) {
					UpdateLanguage();
				}
			};

			Localization.OnLocalizationChangeEvent += UpdateLanguage;
		}

		protected override void OnClick() {
			SetVMC(!isVMCStarted);
		}

		void FixedUpdate() {
			Vector3 pos = canvasRect.localPosition;
			pos.y = Mathf.Lerp(pos.y, Hover ? -24 : 0, 0.2f);
			canvasRect.localPosition = pos;
			canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, Hover ? 1 : 0, 0.2f); 
		}

		private void SetVMC(bool enable) {
			isVMCStarted = enable;
			buttonImage.color = enable ? toggleOff : toggleOn;

			// Start/Stop the VMC instance
			if (enable) {
				vmcSender.SetAddress(Settings.VMCSenderAddress);
				vmcSender.SetPort(Settings.VMCSenderPort);	
				vmcSender.StartVMC();
			} else {
				vmcSender.StopVMC();
			}

			// Update port
			UpdateLanguage();
		}
		
		private void UpdateLanguage() {
			buttonText.text = isVMCStarted
				? Lang.VmcSenderStop.Get()
				: Lang.VmcSenderStart.Get();
			portText.text = Lang.VmcPort.Get() + " " + Settings.VMCSenderPort;
		}
	}
}
