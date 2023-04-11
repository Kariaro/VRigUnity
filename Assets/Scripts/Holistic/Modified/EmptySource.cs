using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class EmptySource : WebCamSource {
		public override string SourceName => null;
		public override int TextureWidth => 1;
		public override int TextureHeight => 1;
		public override bool IsPrepared => true;
		public override bool IsPlaying => false;
		public override string[] SourceCandidateNames => null;
		public override RotationAngle Rotation => RotationAngle.Rotation0;
		public override bool IsVerticallyFlipped => false;
		public override bool IsHorizontallyFlipped => false;
		public override Texture CurrentTexture => new Texture2D(1, 1);

		public override void SelectSource(int sourceId) {}
		public override void Stop() {}
		
		protected override IEnumerator Start() {
			yield return null;
		}

		public override IEnumerator Play() {
			yield return null;
		}
	}
}
