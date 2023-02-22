using Mediapipe.Unity;
using System.Collections;
using UnityEngine;
using System;

namespace HardCoded.VRigUnity {
	public abstract class Solution : MonoBehaviour {
		protected virtual string TAG => GetType().Name;
		[SerializeField] protected HolisticGraph graphRunner;
		[SerializeField] protected TextureFramePool textureFramePool;
		
		private Coroutine runtimeCoroutine;
		private WebCamSource imageSource;
		private Bootstrap bootstrap;

		public bool IsPaused { get; private set; } = true;

		protected IEnumerator Start() {
			imageSource = GetComponent<WebCamSource>();
			bootstrap = GetComponent<Bootstrap>();
			bootstrap.enabled = true;

			yield return new WaitUntil(() => bootstrap.IsFinished);
		}

		public void Play(Action<string, Exception> errorCallback = null) {
			if (runtimeCoroutine != null) {
				Stop();
			}

			IsPaused = false;
			runtimeCoroutine = StartCoroutine(Run(errorCallback));
		}

		public void Stop() {
			IsPaused = true;
			if (runtimeCoroutine != null) {
				StopCoroutine(runtimeCoroutine);
			}
			imageSource.Stop();
			graphRunner.Stop();
		}

		private IEnumerator Run(Action<string, Exception> errorCallback) {
			var graphInitRequest = graphRunner.WaitForInitAsync();

			// Update image source
			imageSource.SelectSourceFromName(Settings.CameraName);
			imageSource.SelectResolutionFromString(Settings.CameraResolution);
			imageSource.IsHorizontallyFlipped = Settings.CameraFlipped;

			Exception wrapped = null;
			yield return CorutineUtils.HandleExceptions(imageSource.Play(), error => wrapped = error);
			
			if (wrapped != null || !imageSource.IsPrepared) {
				Logger.Error(TAG, "Failed to start ImageSource, exiting...");
				errorCallback?.Invoke($"Failed to start ImageSource '{Settings.CameraName}', exiting...", wrapped);
				yield break;
			}

			textureFramePool.ResizeTexture(imageSource.TextureWidth, imageSource.TextureHeight, TextureFormat.RGBA32);
			SetupScreen(imageSource);

			yield return graphInitRequest;
			if (graphInitRequest.isError) {
				Logger.Error(TAG, graphInitRequest.error);
				errorCallback?.Invoke($"Failed to start graph '{graphInitRequest.error}', exiting...", null);
				yield break;
			}

			OnStartRun();
			graphRunner.StartRun(imageSource);

			var waitWhilePausing = new WaitWhile(() => IsPaused);
			while (true) {
				if (IsPaused) {
					yield return waitWhilePausing;
				}

				if (!textureFramePool.TryGetTextureFrame(out var textureFrame)) {
					yield return new WaitForEndOfFrame();
					continue;
				}

				// Copy current image to TextureFrame
				WebCamTexture tex = imageSource.GetCurrentTexture() as WebCamTexture;
				textureFrame.ReadTextureFromOnCPU(tex);
				graphRunner.AddTextureFrameToInputStream(textureFrame);
				
				yield return new WaitForEndOfFrame();
				RenderCurrentFrame(textureFrame);
			}
		}

		protected abstract void SetupScreen(ImageSource imageSource);

		protected abstract void RenderCurrentFrame(TextureFrame textureFrame);

		protected abstract void OnStartRun();
	}
}
