using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	// This class will be used for testing
	public class EmptySource : ImageSource {
		public override string SourceName => null;
		public override int TextureWidth => 1;
		public override int TextureHeight => 1;
		public override bool IsPrepared => true;
		public override bool IsPlaying => false;
		public override string[] SourceCandidateNames => null;
		public override ResolutionStruct[] AvailableResolutions => null;

		public override void SelectSource(int sourceId) {}
		public override void Stop() {}

		public override IEnumerator Play() {
			yield return null;
		}

		public override Texture GetCurrentTexture() {
			return new Texture2D(1, 1);
		}
	}
}
