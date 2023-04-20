using System.Collections;
using UnityEngine;
using System;

namespace HardCoded.VRigUnity {
	public abstract class Solution : MonoBehaviour {
		protected virtual string TAG => GetType().Name;
		[SerializeField] protected HolisticGraph graphRunner;
		
		private Coroutine runtimeCoroutine;
		private ImageSource imageSource;
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
			imageSource.UpdateFromSettings();

			Exception wrapped = null;
			yield return CorutineUtils.HandleExceptions(imageSource.Play(), error => wrapped = error);
			
			if (wrapped != null || !imageSource.IsPrepared) {
				Logger.Error(TAG, "Failed to start ImageSource, exiting...");
				errorCallback?.Invoke($"Failed to start ImageSource '{Settings.CameraName}', exiting...", wrapped);
				yield break;
			}

			SetupScreen(imageSource);

			yield return graphInitRequest;
			if (graphInitRequest.isError) {
				Logger.Error(TAG, graphInitRequest.error);
				errorCallback?.Invoke($"Failed to start graph '{graphInitRequest.error}', exiting...", null);
				yield break;
			}

			OnStartRun();
			graphRunner.StartRun(imageSource);
			
			Texture2D texture2D = null;
			var waitWhilePausing = new WaitWhile(() => IsPaused);
			while (true) {
				if (IsPaused) {
					yield return waitWhilePausing;
				}
				
				Texture tex = imageSource.CurrentTexture;
				if (texture2D == null || texture2D.width != tex.width || texture2D.height != tex.height) {
					texture2D = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
				}
				Graphics.CopyTexture(tex, texture2D);
				graphRunner.AddTextureFrameToInputStream(texture2D);
				
				yield return new WaitForEndOfFrame();
				RenderCurrentFrame(texture2D);
			}
		}

		protected abstract void SetupScreen(ImageSource imageSource);

		protected abstract void RenderCurrentFrame(Texture2D texture);

		protected abstract void OnStartRun();
	}
}
