using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public abstract class TestImageSourceSolution<T> : TestSolution where T : TestGraphRunner {
		[SerializeField] protected Mediapipe.Unity.Screen screen;
		[SerializeField] protected T graphRunner;
		[SerializeField] protected TextureFramePool textureFramePool;

		private Coroutine _coroutine;

		public long TimeoutMillisec {
			get => graphRunner.TimeoutMillisec;
			set => graphRunner.TimeoutMillisec = value;
		}

		public override void Play() {
			if (_coroutine != null) {
				Stop();
			}

			base.Play();
			_coroutine = StartCoroutine(Run());
		}

		public override void Pause() {
			base.Pause();
			ImageSourceProvider.ImageSource.Pause();
		}

		public override void Resume() {
			base.Resume();
			var _ = StartCoroutine(ImageSourceProvider.ImageSource.Resume());
		}

		public override void Stop() {
			base.Stop();
			StopCoroutine(_coroutine);
			ImageSourceProvider.ImageSource.Stop();
			graphRunner.Stop();
		}

		private IEnumerator Run() {
			var graphInitRequest = graphRunner.WaitForInitAsync();
			var imageSource = ImageSourceProvider.ImageSource;

			yield return imageSource.Play();

			if (!imageSource.isPrepared) {
				Logger.Error(TAG, "Failed to start ImageSource, exiting...");
				yield break;
			}

			// Use RGBA32 as the input format.
			// TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so the following code must be fixed.
			textureFramePool.ResizeTexture(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32);
			SetupScreen(imageSource);

			yield return graphInitRequest;
			if (graphInitRequest.isError) {
				Logger.Error(TAG, graphInitRequest.error);
				yield break;
			}

			OnStartRun();
			graphRunner.StartRun(imageSource);

			var waitWhilePausing = new WaitWhile(() => isPaused);

			while (true) {
				if (isPaused) {
					yield return waitWhilePausing;
				}

				if (!textureFramePool.TryGetTextureFrame(out var textureFrame)) {
					yield return new WaitForEndOfFrame();
					continue;
				}

				// Copy current image to TextureFrame
				ReadFromImageSource(imageSource, textureFrame);
				AddTextureFrameToInputStream(textureFrame);
				yield return new WaitForEndOfFrame();

				// I don't care just render the image
				RenderCurrentFrame(textureFrame);
			}
		}

		protected virtual void SetupScreen(ImageSource imageSource) {
			// NOTE: Without this line the screen does not update its size and no annotations are drawn
			screen.Initialize(imageSource);
		}

		protected abstract void RenderCurrentFrame(TextureFrame textureFrame);

		protected abstract void OnStartRun();

		protected abstract void AddTextureFrameToInputStream(TextureFrame textureFrame);

		// TODO: Is this needed?
		protected abstract IEnumerator WaitForNextValue();
	}
}
