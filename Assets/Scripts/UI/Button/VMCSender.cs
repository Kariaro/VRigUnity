using UnityEngine;
using uOSC;
using VRM;

namespace HardCoded.VRigUnity {
	[RequireComponent(typeof(uOscClient))]
	public class VMCSender : MonoBehaviour {
		private uOscClient uClient;

		public string GetAddress() {
			return uClient.address;
		}

		public void SetAddress(string address) {
			uClient.address = address;
		}

		public int GetPort() {
			return uClient.port;
		}

		public void SetPort(int port) {
			uClient.port = port;
		}

		public void StartVMC() {
			uClient.StartClient();
		}

		public void StopVMC() {
			uClient.StopClient();
		}

		void Start() {
			uClient = GetComponent<uOscClient>();
		}

		void Update() {
			var holisticModel = SolutionUtils.GetSolution().Model;

			// Check if we should be sending
			if (!uClient.isRunning || !holisticModel.IsPrepared) {
				return;
			}

			// Bones
			var boneBundle = new Bundle(Timestamp.Now);
			foreach (var pair in holisticModel.ModelBones) {
				Transform tr = pair.Value;
				boneBundle.Add(new Message("/VMC/Ext/Bone/Pos",
					pair.Key.ToString(),
					tr.localPosition.x, tr.localPosition.y, tr.localPosition.z,
					tr.localRotation.x, tr.localRotation.y, tr.localRotation.z, tr.localRotation.w
				));
			}
			uClient.Send(boneBundle);

			// BlendShape
            var blendShapeBundle = new Bundle(Timestamp.Now);
            foreach (var b in holisticModel.BlendShapeProxy.GetValues()) {
                blendShapeBundle.Add(new Message("/VMC/Ext/Blend/Val",
                    b.Key.ToString(),
                    (float) b.Value
                ));
            }
            blendShapeBundle.Add(new Message("/VMC/Ext/Blend/Apply"));
            uClient.Send(blendShapeBundle);

            // Available
            uClient.Send("/VMC/Ext/OK", 1);

			// Timestamp
			uClient.Send("/VMC/Ext/T", Time.time);
		}
	}
}
