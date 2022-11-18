using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public abstract class Solution : MonoBehaviour {
		protected virtual string TAG => GetType().Name;

		protected Bootstrap bootstrap;
		protected bool isPaused;

		protected virtual IEnumerator Start() {
			bootstrap = FindBootstrap();
			yield return new WaitUntil(() => bootstrap.IsFinished);
			isPaused = true;
		}

		public virtual void Play() {
			isPaused = false;
		}

		public virtual void Pause() {
			isPaused = true;
		}

		public virtual void Resume() {
			isPaused = false;
		}

		public virtual void Stop() {
			isPaused = true;
		}

		public bool IsPaused() {
			return isPaused;
		}

		protected Bootstrap FindBootstrap() {
			var bootstrap = SolutionUtils.GetBootstrap();
			bootstrap.enabled = true;
			return bootstrap;
		}
	}
}
