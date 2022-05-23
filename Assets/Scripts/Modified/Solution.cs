using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public abstract class Solution : MonoBehaviour {
		protected virtual string TAG => GetType().Name;

		public Bootstrap bootstrap;
		protected bool isPaused;

		// List of debug transforms
		public Transform[] debugTransforms;

		protected virtual IEnumerator Start() {
			bootstrap = FindBootstrap();
			yield return new WaitUntil(() => bootstrap.IsFinished);
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

		public void SetDebug(bool enable) {
			foreach (Transform t in debugTransforms) {
				t.gameObject.SetActive(enable);
			}
		}

		protected static void SetupAnnotationController<T>(AnnotationController<T> annotationController, ImageSource imageSource, bool expectedToBeMirrored = false) where T : HierarchicalAnnotation {
			annotationController.isMirrored = expectedToBeMirrored ^ imageSource.isHorizontallyFlipped ^ imageSource.isFrontFacing;
			annotationController.rotationAngle = imageSource.rotation.Reverse();
		}

		protected static void ReadFromImageSource(WebCamSource imageSource, TextureFrame textureFrame) {
			// For some reason, when the image is coiped on GPU, latency tends to be high.
			// So even when OpenGL ES is available, use CPU to copy images.
			textureFrame.ReadTextureFromOnCPU(imageSource.GetCurrentTexture());
		}

		protected Bootstrap FindBootstrap() {
			var bootstrap = SolutionUtils.GetBootstrap();
			bootstrap.enabled = true;
			return bootstrap;
		}
	}
}
