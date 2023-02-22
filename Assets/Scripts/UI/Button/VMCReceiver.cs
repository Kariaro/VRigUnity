using UnityEngine;
using uOSC;
using EVMC4U;

namespace HardCoded.VRigUnity {
	public class VMCReceiver : MonoBehaviour {
		private static VMCReceiver _receiver;
		public static VMCReceiver Receiver => _receiver;

		public GameObject vrmModel;
		public ExternalReceiver vmcReceiver;
		private uOscServer uServer = null;

		public int GetPort() {
			return uServer.port;
		}

		public void SetPort(int port) {
			uServer.port = port;
		}

		public bool IsRunning() {
			return uServer.isRunning;
		}

		public void StartVMC() {
			vmcReceiver.Model = SolutionUtils.GetSolution().Model.VrmModel;
			vmcReceiver.gameObject.SetActive(true);
			uServer.StartServer();
		}

		public void StopVMC() {
			vmcReceiver.gameObject.SetActive(false);
			uServer.StopServer();
			SolutionUtils.GetSolution().Model.ResetVRMAnimator();
		}

		void Start() {
			uServer = GetComponentInChildren<uOscServer>(true);
			_receiver = this;
		}

		void Update() {
			// Check if the vrmModel exists
			if (vrmModel == null) {
				vrmModel = SolutionUtils.GetSolution().Model.VrmModel;
				vmcReceiver.Model = vrmModel;
			}
		}
	}
}
