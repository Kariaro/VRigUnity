using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	// This class will be used for testing
	public class EmptySource : ImageSource {
		private const string _TAG = nameof(EmptySource);

		public override int textureWidth => 1;
		public override int textureHeight => 1;
		public override bool isVerticallyFlipped => false;
		public override bool isFrontFacing => false;
		public override RotationAngle rotation => RotationAngle.Rotation0;
		public override string sourceName => null;
		public override string[] sourceCandidateNames => null;
		public override ResolutionStruct[] availableResolutions => null;
		public override bool isPrepared => true;
		public override bool isPlaying => false;

		public override void SelectSource(int sourceId) {}
		public override void Pause() {}
		public override void Stop() {}

		public override IEnumerator Play() {
			yield return null;
		}

		public override IEnumerator Resume() {
			yield return null;
		}

		public override Texture GetCurrentTexture() {
			return new Texture2D(1, 1);
		}

		private ResolutionStruct GetDefaultResolution() {
			return new ResolutionStruct();
		}
	}
}
