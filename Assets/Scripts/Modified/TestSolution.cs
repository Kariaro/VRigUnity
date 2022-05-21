using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public abstract class TestSolution : MonoBehaviour {
		protected virtual string TAG => GetType().Name;

		public TestBootstrap bootstrap;
		protected bool isPaused;

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

		protected static void SetupAnnotationController<T>(AnnotationController<T> annotationController, ImageSource imageSource, bool expectedToBeMirrored = false) where T : HierarchicalAnnotation {
			annotationController.isMirrored = expectedToBeMirrored ^ imageSource.isHorizontallyFlipped ^ imageSource.isFrontFacing;
			annotationController.rotationAngle = imageSource.rotation.Reverse();
		}

		protected static void ReadFromImageSource(ImageSource imageSource, TextureFrame textureFrame) {
			// TODO: This is always a webcam
			var sourceTexture = imageSource.GetCurrentTexture();

			// For some reason, when the image is coiped on GPU, latency tends to be high.
			// So even when OpenGL ES is available, use CPU to copy images.
			// var textureType = sourceTexture.GetType();
			
			textureFrame.ReadTextureFromOnCPU((WebCamTexture)sourceTexture);
			
			/*
			if (textureType == typeof(WebCamTexture)) {
				textureFrame.ReadTextureFromOnCPU((WebCamTexture)sourceTexture);
			} else if (textureType == typeof(Texture2D)) {
				textureFrame.ReadTextureFromOnCPU((Texture2D)sourceTexture);
			} else {
				textureFrame.ReadTextureFromOnCPU(sourceTexture);
			}
			*/
		}

		protected TestBootstrap FindBootstrap() {
			var bootstrap = SolutionUtils.GetBootstrap();
			bootstrap.enabled = true;
			return bootstrap;
		}
	}
}
