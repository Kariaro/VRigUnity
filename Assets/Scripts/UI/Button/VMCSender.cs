using UnityEngine;
using uOSC;
using System;
using VRM;

namespace HardCoded.VRigUnity {
	[RequireComponent(typeof(uOscClient))]
	public class VMCSender : MonoBehaviour {
		public GameObject vrmModel;
		public Animator vrmAnimator;
		public VRMBlendShapeProxy vrmBlendShapeProxy;
		private uOscClient uClient = null;

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
			// Check if the vrmModel exists
			if (vrmModel == null) {
				vrmModel = SolutionUtils.GetSolution().VrmModel;
				vrmAnimator = vrmModel.GetComponent<Animator>();
				vrmBlendShapeProxy = vrmModel.GetComponent<VRMBlendShapeProxy>();
				return;
			}

			// Bones
			if (vrmAnimator != null) {
				var boneBundle = new Bundle(Timestamp.Now);
				foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones))) {
					if (bone != HumanBodyBones.LastBone) {
						Transform tr = vrmAnimator.GetBoneTransform(bone);
						if (tr != null) {
							boneBundle.Add(new Message("/VMC/Ext/Bone/Pos",
								bone.ToString(),
								tr.localPosition.x, tr.localPosition.y, tr.localPosition.z,
								tr.localRotation.x, tr.localRotation.y, tr.localRotation.z, tr.localRotation.w
							));
						}
					}
				}
				uClient.Send(boneBundle);
			}

			// BlendShape
            if (vrmBlendShapeProxy != null) {
                var blendShapeBundle = new Bundle(Timestamp.Now);

                foreach (var b in vrmBlendShapeProxy.GetValues()) {
                    blendShapeBundle.Add(new Message("/VMC/Ext/Blend/Val",
                        b.Key.ToString(),
                        (float) b.Value
                    ));
                }
                blendShapeBundle.Add(new Message("/VMC/Ext/Blend/Apply"));
                uClient.Send(blendShapeBundle);
            }

            // Available
            uClient.Send("/VMC/Ext/OK", 1);

			// Timestamp
			uClient.Send("/VMC/Ext/T", Time.time);
		}
	}
}
