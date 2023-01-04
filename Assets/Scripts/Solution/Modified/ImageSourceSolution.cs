using Mediapipe.Unity;
using System.Linq;
using System.Collections;
using UnityEngine;
using System;

namespace HardCoded.VRigUnity {
	public abstract class ImageSourceSolution<T> : Solution where T : GraphRunner {
		[SerializeField] protected T graphRunner;
		[SerializeField] protected TextureFramePool textureFramePool;

		private Coroutine _coroutine;
		private WebCamSource ImageSource => SolutionUtils.GetImageSource();
		private Action<string> _errorListener;

		public override void Play() {
			if (_coroutine != null) {
				Stop();
			}

			base.Play();
			_coroutine = StartCoroutine(Run());
		}

		public override void Stop() {
			base.Stop();
			StopCoroutine(_coroutine);
			ImageSource.Stop();
			graphRunner.Stop();
		}

		// TODO: Make this an event
		public void SetErrorListener(Action<string> action) {
			_errorListener = action;
		}

		private IEnumerator Run() {
			var graphInitRequest = graphRunner.WaitForInitAsync();
			var imageSource = ImageSource;

			// Update image source
			imageSource.SelectSourceFromName(Settings.CameraName);
			imageSource.SelectResolutionFromString(Settings.CameraResolution);
			imageSource.IsHorizontallyFlipped = Settings.CameraFlipped;

			Exception wrapped = null;
			yield return CorutineUtils.HandleExceptions(imageSource.Play(), error => wrapped = error);
			
			if (wrapped != null || !imageSource.IsPrepared) {
				Logger.Error(TAG, "Failed to start ImageSource, exiting...");
				_errorListener?.Invoke($"Failed to start ImageSource '{Settings.CameraName}', exiting...");
				yield break;
			}

			// Use RGBA32 as the input format.
			// TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so the following code must be fixed.
			textureFramePool.ResizeTexture(imageSource.TextureWidth, imageSource.TextureHeight, TextureFormat.RGBA32);
			SetupScreen(imageSource);

			yield return graphInitRequest;
			if (graphInitRequest.isError) {
				Logger.Error(TAG, graphInitRequest.error);
				_errorListener?.Invoke($"Failed to start graph '{graphInitRequest.error}', exiting...");
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
				// For some reason, when the image is coiped on GPU, latency tends to be high.
				// So even when OpenGL ES is available, use CPU to copy images.
				WebCamTexture tex = imageSource.GetCurrentTexture() as WebCamTexture;

				// This is much faster if the type is (WebCamTexture)
				textureFrame.ReadTextureFromOnCPU(tex);
				AddTextureFrameToInputStream(textureFrame);
				yield return new WaitForEndOfFrame();

				RenderCurrentFrame(textureFrame);
			}
		}

		protected abstract void SetupScreen(ImageSource imageSource);

		protected abstract void RenderCurrentFrame(TextureFrame textureFrame);

		protected abstract void OnStartRun();

		protected abstract void AddTextureFrameToInputStream(TextureFrame textureFrame);
	}
}
