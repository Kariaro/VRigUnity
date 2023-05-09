using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

using Google.Protobuf;
using Mediapipe.Unity;
using Mediapipe;

namespace HardCoded.VRigUnity {
	public class HolisticGraph : GraphRunner {
		public enum ModelComplexity {
			Lite = 0,
			Full = 1,
			Heavy = 2,
		}

		public ModelComplexity modelComplexity = ModelComplexity.Full;

		private float _minDetectionConfidence = 0.5f;
		public float MinDetectionConfidence {
			get => _minDetectionConfidence;
			set => _minDetectionConfidence = Mathf.Clamp01(value);
		}

		private float _minTrackingConfidence = 0.5f;
		public float MinTrackingConfidence {
			get => _minTrackingConfidence;
			set => _minTrackingConfidence = Mathf.Clamp01(value);
		}

		public event EventHandler<OutputEventArgs<NormalizedLandmarkList>> OnPoseLandmarksOutput {
			add => _poseLandmarksStream.AddListener(value);
			remove => _poseLandmarksStream.RemoveListener(value);
		}

		public event EventHandler<OutputEventArgs<NormalizedLandmarkList>> OnFaceLandmarksOutput {
			add => _faceLandmarksStream.AddListener(value);
			remove => _faceLandmarksStream.RemoveListener(value);
		}

		public event EventHandler<OutputEventArgs<NormalizedLandmarkList>> OnLeftHandLandmarksOutput {
			add => _leftHandLandmarksStream.AddListener(value);
			remove => _leftHandLandmarksStream.RemoveListener(value);
		}

		public event EventHandler<OutputEventArgs<NormalizedLandmarkList>> OnRightHandLandmarksOutput {
			add => _rightHandLandmarksStream.AddListener(value);
			remove => _rightHandLandmarksStream.RemoveListener(value);
		}

		public event EventHandler<OutputEventArgs<LandmarkList>> OnPoseWorldLandmarksOutput {
			add => _poseWorldLandmarksStream.AddListener(value);
			remove => _poseWorldLandmarksStream.RemoveListener(value);
		}

		private const string _InputStreamName = "input_video";
		private const string _PoseLandmarksStreamName = "pose_landmarks";
		private const string _FaceLandmarksStreamName = "face_landmarks";
		private const string _LeftHandLandmarksStreamName = "left_hand_landmarks";
		private const string _RightHandLandmarksStreamName = "right_hand_landmarks";
		private const string _PoseWorldLandmarksStreamName = "pose_world_landmarks";

		private OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList> _poseLandmarksStream;
		private OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList> _faceLandmarksStream;
		private OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList> _leftHandLandmarksStream;
		private OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList> _rightHandLandmarksStream;
		private OutputStream<LandmarkListPacket, LandmarkList> _poseWorldLandmarksStream;

		public override void StartRun(ImageSource imageSource) {
			StartRun(BuildSidePacket(imageSource));
		}

		public override void Stop() {
			_poseLandmarksStream?.RemoveAllListeners();
			_faceLandmarksStream?.RemoveAllListeners();
			_leftHandLandmarksStream?.RemoveAllListeners();
			_rightHandLandmarksStream?.RemoveAllListeners();
			_poseWorldLandmarksStream?.RemoveAllListeners();
			_poseLandmarksStream = null;
			_faceLandmarksStream = null;
			_leftHandLandmarksStream = null;
			_rightHandLandmarksStream = null;
			_poseWorldLandmarksStream = null;

			base.Stop();
		}

		public void AddTextureFrameToInputStream(Texture2D texture) {
			AddTextureFrameToInputStream(_InputStreamName, texture);
		}

		protected override IList<WaitForResult> RequestDependentAssets() {
			return new List<WaitForResult> {
				WaitForAsset("face_detection_short_range.bytes"),
				WaitForAsset("face_landmark_with_attention.bytes"),
				WaitForAsset("iris_landmark.bytes"),
				WaitForAsset("hand_landmark_full.bytes"),
				WaitForAsset("hand_recrop.bytes"),
				WaitForAsset("handedness.txt"),
				WaitForAsset("palm_detection_full.bytes"),
				WaitForAsset("pose_detection.bytes"),
				WaitForPoseLandmarkModel(),
			};
		}

		private WaitForResult WaitForPoseLandmarkModel() {
			switch (modelComplexity) {
				case ModelComplexity.Lite: return WaitForAsset("pose_landmark_lite.bytes");
				case ModelComplexity.Full: return WaitForAsset("pose_landmark_full.bytes");
				case ModelComplexity.Heavy: return WaitForAsset("pose_landmark_heavy.bytes");
				default: throw new InternalException($"Invalid model complexity: {modelComplexity}");
			}
		}

		protected override Status ConfigureCalculatorGraph(CalculatorGraphConfig config) {
			_poseLandmarksStream = new(CalculatorGraph, _PoseLandmarksStreamName, true, TimeoutMicrosec);
			_faceLandmarksStream = new(CalculatorGraph, _FaceLandmarksStreamName, true, TimeoutMicrosec);
			_leftHandLandmarksStream = new(CalculatorGraph, _LeftHandLandmarksStreamName, true, TimeoutMicrosec);
			_rightHandLandmarksStream = new(CalculatorGraph, _RightHandLandmarksStreamName, true, TimeoutMicrosec);
			_poseWorldLandmarksStream = new(CalculatorGraph, _PoseWorldLandmarksStreamName, true, TimeoutMicrosec);

			using (var validatedGraphConfig = new ValidatedGraphConfig()) {
				var status = validatedGraphConfig.Initialize(config);

				if (!status.Ok()) {
					return status;
				}

				var extensionRegistry = new ExtensionRegistry() { TensorsToDetectionsCalculatorOptions.Extensions.Ext, ThresholdingCalculatorOptions.Extensions.Ext };
				var cannonicalizedConfig = validatedGraphConfig.Config(extensionRegistry);

				var poseDetectionCalculatorPattern = new Regex("__posedetection[a-z]+__TensorsToDetectionsCalculator$");
				var tensorsToDetectionsCalculators = cannonicalizedConfig.Node.Where(node => poseDetectionCalculatorPattern.Match(node.Name).Success).ToList();

				var poseTrackingCalculatorPattern = new Regex("tensorstoposelandmarksandsegmentation__ThresholdingCalculator$");
				var thresholdingCalculators = cannonicalizedConfig.Node.Where(node => poseTrackingCalculatorPattern.Match(node.Name).Success).ToList();

				foreach (var calculator in tensorsToDetectionsCalculators) {
					if (calculator.Options.HasExtension(TensorsToDetectionsCalculatorOptions.Extensions.Ext)) {
						var options = calculator.Options.GetExtension(TensorsToDetectionsCalculatorOptions.Extensions.Ext);
						options.MinScoreThresh = MinDetectionConfidence;
						Logger.Info(TAG, $"Min Detection Confidence = {MinDetectionConfidence}");
					}
				}

				foreach (var calculator in thresholdingCalculators) {
					if (calculator.Options.HasExtension(ThresholdingCalculatorOptions.Extensions.Ext)) {
						var options = calculator.Options.GetExtension(ThresholdingCalculatorOptions.Extensions.Ext);
						options.Threshold = MinTrackingConfidence;
						Logger.Info(TAG, $"Min Tracking Confidence = {MinTrackingConfidence}");
					}
				}

				return CalculatorGraph.Initialize(cannonicalizedConfig);
			}
		}

		private SidePacket BuildSidePacket(ImageSource imageSource) {
			var sidePacket = new SidePacket();

			SetImageTransformationOptions(sidePacket, imageSource);
			// The orientation of the output image must match that of the input image.
			var isInverted = Mediapipe.Unity.CoordinateSystem.ImageCoordinate.IsInverted(imageSource.Rotation);
			var outputRotation = imageSource.Rotation;
			var outputHorizontallyFlipped = !isInverted && imageSource.IsHorizontallyFlipped ^ true;
			var outputVerticallyFlipped = (imageSource.IsVerticallyFlipped) ^ (isInverted && imageSource.IsHorizontallyFlipped);

			if ((outputHorizontallyFlipped && outputVerticallyFlipped) || outputRotation == RotationAngle.Rotation180) {
				outputRotation = outputRotation.Add(RotationAngle.Rotation180);
				outputHorizontallyFlipped = !outputHorizontallyFlipped;
				outputVerticallyFlipped = !outputVerticallyFlipped;
			}

			sidePacket.Emplace("output_rotation", new IntPacket((int)outputRotation));
			sidePacket.Emplace("output_horizontally_flipped", new BoolPacket(outputHorizontallyFlipped));
			sidePacket.Emplace("output_vertically_flipped", new BoolPacket(outputVerticallyFlipped));

			Logger.Debug($"output_rotation = {outputRotation}, output_horizontally_flipped = {outputHorizontallyFlipped}, output_vertically_flipped = {outputVerticallyFlipped}");

			sidePacket.Emplace("refine_face_landmarks", new BoolPacket(true));
			sidePacket.Emplace("smooth_landmarks", new BoolPacket(true));
			sidePacket.Emplace("model_complexity", new IntPacket((int)modelComplexity));

			Logger.Info(TAG, $"Model Complexity = {modelComplexity}");

			return sidePacket;
		}
	}
}
