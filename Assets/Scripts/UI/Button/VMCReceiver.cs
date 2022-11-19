using UnityEngine;
using uOSC;
using EVMC4U;

namespace HardCoded.VRigUnity {
	public class VMCReceiver : MonoBehaviour {
		public GameObject vrmModel;
		public ExternalReceiver vrmReceiver;
		private uOscServer uServer = null;

		public int GetPort() {
			return uServer.port;
		}

		public void SetPort(int port) {
			uServer.port = port;
		}

		public void StartVMC() {
			vrmReceiver.Model = SolutionUtils.GetSolution().GetVRMModel();
			vrmReceiver.gameObject.SetActive(true);
			uServer.StartServer();
		}

		public void StopVMC() {
			vrmReceiver.gameObject.SetActive(false);
			uServer.StopServer();
			SolutionUtils.GetSolution().ResetVRMAnimator();
		}

		void Start() {
			uServer = GetComponentInChildren<uOscServer>(true);
		}

		void Update() {
			// Check if the vrmModel exists
			if (vrmModel == null) {
				vrmModel = SolutionUtils.GetSolution().GetVRMModel();
				vrmReceiver.Model = vrmModel;
			}
		}
	}
}
