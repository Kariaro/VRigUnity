using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public abstract class ImageSourceSolution<T> : Solution where T : GraphRunner {
		[SerializeField] protected T graphRunner;
		[SerializeField] protected TextureFramePool textureFramePool;

		private Coroutine _coroutine;

		public long TimeoutMillisec {
			get => graphRunner.TimeoutMillisec;
			set => graphRunner.TimeoutMillisec = value;
		}

		public WebCamSource ImageSource => SolutionUtils.GetImageSource();

		public override void Play() {
			if (_coroutine != null) {
				Stop();
			}

			base.Play();
			_coroutine = StartCoroutine(Run());
		}

		public override void Pause() {
			base.Pause();
			ImageSource.Pause();
		}

		public override void Resume() {
			base.Resume();
			var _ = StartCoroutine(ImageSource.Resume());
		}

		public override void Stop() {
			base.Stop();
			StopCoroutine(_coroutine);
			ImageSource.Stop();
			graphRunner.Stop();
		}

		private IEnumerator Run() {
			var graphInitRequest = graphRunner.WaitForInitAsync();
			var imageSource = ImageSource;

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

		protected abstract void SetupScreen(ImageSource imageSource);

		protected abstract void RenderCurrentFrame(TextureFrame textureFrame);

		protected abstract void OnStartRun();

		protected abstract void AddTextureFrameToInputStream(TextureFrame textureFrame);
	}
}
