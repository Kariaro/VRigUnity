using System.Threading;
using UnityEngine;

namespace HardCoded.VRigUnity {
	// This class is used to check if the current thread is the unity thread
	public class ThreadObject : MonoBehaviour {
		private static Thread MainThread;
		
		void Start() {
			MainThread = Thread.CurrentThread;
		}

		// Returns if the current thread is the unity thread
		public static bool IsUnityThread() {
			return MainThread != null && MainThread == Thread.CurrentThread;
		}
	}
}
