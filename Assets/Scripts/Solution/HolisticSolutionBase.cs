using Mediapipe.Unity;
using System.Collections;

namespace HardCoded.VRigUnity {
	public abstract class HolisticSolutionBase : ImageSourceSolution<HolisticTrackingGraph> {
		// Always 'Full'
		public HolisticTrackingGraph.ModelComplexity ModelComplexity {
			get => graphRunner.modelComplexity;
			set => graphRunner.modelComplexity = value;
		}

		// Always 'true'
		public bool SmoothLandmarks {
			get => graphRunner.smoothLandmarks;
			set => graphRunner.smoothLandmarks = value;
		}

		// Always 'true'
		public bool RefineFaceLandmarks {
			get => graphRunner.refineFaceLandmarks;
			set => graphRunner.refineFaceLandmarks = value;
		}

		public float MinDetectionConfidence {
			get => graphRunner.MinDetectionConfidence;
			set => graphRunner.MinDetectionConfidence = value;
		}

		public float MinTrackingConfidence {
			get => graphRunner.MinTrackingConfidence;
			set => graphRunner.MinTrackingConfidence = value;
		}

		protected abstract override void OnStartRun();

		protected override void AddTextureFrameToInputStream(TextureFrame textureFrame) {
			graphRunner.AddTextureFrameToInputStream(textureFrame);
		}
	}
}
