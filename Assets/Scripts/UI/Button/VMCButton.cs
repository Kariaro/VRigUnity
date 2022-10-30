using UnityEngine;
using TMPro;
using UnityEngine.UI;
using uOSC;
using System;
using VRM;

namespace HardCoded.VRigUnity {
	public class VMCButton : MonoBehaviour {
		public VMCSender vmcSender;

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

		private void InitializeContents() {
			buttonImage.color = toggleOnColor;
			isVMCStarted = false;

			// Setup settings listener (TODO: Remove)
			Settings.VMCPortListener += (value) => {
				// Only display port changes when the VMC is closed
				if (!isVMCStarted) {
					portText.text = "Port " + value;
				}
			};

			portText.text = "Port " + Settings.VMCPort;
			
			toggleButton.onClick.RemoveAllListeners();
			toggleButton.onClick.AddListener(delegate {
				SetVMC(!isVMCStarted);
			});
		}

		private void SetVMC(bool enable) {
			isVMCStarted = enable;
			buttonImage.color = enable ? toggleOffColor : toggleOnColor;
			buttonText.text = enable ? "Stop VMC" : "Start VMC";

			// Start/Stop the VMC instance
			if (enable) {
				vmcSender.SetPort(Settings.VMCPort);	
				vmcSender.StartVMC();
			} else {
				vmcSender.StopVMC();
			}

			// Update port
			portText.text = "Port " + Settings.VMCPort;
		}
	}
}
