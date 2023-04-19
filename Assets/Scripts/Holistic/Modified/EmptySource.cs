using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class EmptySource : ImageSource {
		public override Texture CurrentTexture => new Texture2D(1, 1);
		public override int TextureWidth => 1;
		public override int TextureHeight => 1;
		public override bool IsPrepared => true;
		public override bool IsPlaying => false;
		public override RotationAngle Rotation => RotationAngle.Rotation0;
		public override string SourceName => null;
		public override string[] SourceCandidateNames => null;

		public override ResolutionStruct[] GetResolutions() {
			return new ResolutionStruct[] {};
		}

		public override IEnumerator Play() {
			yield return null;
		}

		public override void Stop() {
			// Empty
		}

		public override void UpdateFromSettings() {
			// Empty
		}
	}
}
