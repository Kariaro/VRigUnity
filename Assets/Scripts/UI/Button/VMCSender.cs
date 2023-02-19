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

		void LateUpdate() {
			var holisticModel = SolutionUtils.GetSolution().Model;

			// Check if we should be sending
			if (!uClient.isRunning || !holisticModel.IsPrepared) {
				return;
			}

			// Bones
			var boneBundle = new Bundle(Timestamp.Now);
			foreach (var pair in holisticModel.ModelBones) {
				Transform tr = pair.Value;
				var pos = tr.localPosition;
				var rot = tr.localRotation;
				boneBundle.Add(new Message("/VMC/Ext/Bone/Pos",
					pair.Key.ToString(),
					pos.x, pos.y, pos.z,
					rot.x, rot.y, rot.z, rot.w
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
