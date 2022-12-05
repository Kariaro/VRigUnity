using UnityEngine;

namespace HardCoded.VRigUnity {
	public class OSCDebugSender : MonoBehaviour {
		[Header("OSC Settings")]
		[SerializeField] private string oscTargetIp = "127.0.0.1";
		[SerializeField] private int oscTargetPort = 9000;

		[Header("Avatar Settings")]
		[SerializeField] private float userHeightInMeters = 1.7f;
		[SerializeField] private float VRChatAvatarHeight = 1.87f;
		[SerializeField] private Transform headReference;
		[SerializeField] private Transform[] trackers = new Transform[8];

		// Internal values
		private OscCore.OscClient oscClient;
		private float scaleFactor;

		void Start() {
			scaleFactor = userHeightInMeters / VRChatAvatarHeight;
			oscClient = new OscCore.OscClient(oscTargetIp, oscTargetPort);
		}

		private void OnValidate() {
			if (trackers.Length > 8) {
				System.Array.Resize(ref trackers, 8);
			}
		}

		void Update() {
			if (oscClient == null) {
				return;
			}

			// TODO: Implement when the vrchat OSC update gets released
			// https://ask.vrchat.com/t/developer-update-17-november-2022/14986

			if (headReference) {
				oscClient.Send("/tracking/trackers/head/position", headReference.position * scaleFactor);
				oscClient.Send("/tracking/trackers/head/rotation", headReference.eulerAngles);
			}

			for (int i = 0; i < trackers.Length; i++) {
				if (trackers[i]) {
					oscClient.Send($"/tracking/trackers/{i + 1}/position", trackers[i].position * scaleFactor);
					oscClient.Send($"/tracking/trackers/{i + 1}/rotation", trackers[i].eulerAngles);
				}
			}
		}
	}
}
