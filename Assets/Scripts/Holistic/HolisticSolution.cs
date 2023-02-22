using Mediapipe;
using Mediapipe.Unity;
using System;
using UnityEngine;
using VRM;

namespace HardCoded.VRigUnity {
	public class HolisticSolution : Solution {
		[Header("Rig")]
		[SerializeField] private GameObject defaultVrmModel;
		[SerializeField] private GameObject defaultVrmPrefab;
		[SerializeField] private RuntimeAnimatorController defaultController;
		protected HolisticModel model;

		[Header("UI")]
		public GUIMain guiMain;
		public CustomizableCanvas Canvas => guiMain.CustomizableCanvas;
		public TrackingResizableBox TrackingBox => guiMain.TrackingBox;

		// Pose values
		public readonly PoseValues Pose = new();
		public readonly HandValues RightHand = new(false);
		public readonly HandValues LeftHand = new(true);
		
		private float mouthOpen = 0;

		public FaceData.RollingAverage lEyeOpen = new(FaceConfig.EAR_FRAMES);
		public FaceData.RollingAverage rEyeOpen = new(FaceConfig.EAR_FRAMES);

		public FaceData.RollingAverageVector2 lEyeIris = new(FaceConfig.EAR_FRAMES);
		public FaceData.RollingAverageVector2 rEyeIris = new(FaceConfig.EAR_FRAMES);

		public bool TrackRightHand = true;
		public bool TrackLeftHand = true;

		// API Getters
		private readonly long StartTicks = DateTime.Now.Ticks;
		public float TimeNow => (float)((DateTime.Now.Ticks - StartTicks) / (double)TimeSpan.TicksPerSecond);
		public HolisticModel Model => model;

		void Awake() {
			model = new(defaultVrmModel, defaultVrmPrefab, defaultController);
			model.IsVisible = Settings.ShowModel;
		}

		protected override void OnStartRun() {
			graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
			graphRunner.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
			graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
			graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
			graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
			Canvas.SetupAnnotations();
		}

		private Vector4 ConvertPoint(NormalizedLandmarkList list, int idx) {
			NormalizedLandmark mark = list.Landmark[idx];
			return new(mark.X * 2, mark.Y, mark.Z * 2, mark.Visibility);
		}

		protected override void SetupScreen(ImageSource imageSource) {
			Canvas.SetupScreen(imageSource);
		}

		protected override void RenderCurrentFrame(TextureFrame textureFrame) {
			Canvas.ReadSync(textureFrame);
		}

		private void OnFaceLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			Canvas.OnFaceLandmarksOutput(eventArgs);
			if (eventArgs.value == null) {
				return;
			}

			Quaternion neckRotation = Quaternion.identity;
			float mouthOpen = 0;
			float lEyeOpen = 0;
			float rEyeOpen = 0;
			Vector2 lEyeIris = Vector2.zero;
			Vector2 rEyeIris = Vector2.zero;
			
			if (BoneSettings.Get(BoneSettings.FACE)) {
				// Mouth
				Vector3 a = ConvertPoint(eventArgs.value, 324);
				Vector3 b = ConvertPoint(eventArgs.value, 78);
				Vector3 c = ConvertPoint(eventArgs.value, 13);
				Vector3 m = (a + b) / 2.0f;

				float width = Vector3.Distance(a, b);
				float height = Vector3.Distance(c, m);
				float area = MovementUtils.GetTriangleArea(a, b, c);
				float perc = height / width;

				mouthOpen = (perc - 0.25f) * 3;
				mouthOpen = Mathf.Clamp01(mouthOpen);

				Vector3 converter(int i) {
					Vector3 value = ConvertPoint(eventArgs.value, i);
					value.x = -value.x;
					return value;
				}

				// Eyes
				lEyeOpen = FacePoints.CalculateEyeAspectRatio(Array.ConvertAll(FacePoints.LeftEyeEAR, converter));
				rEyeOpen = FacePoints.CalculateEyeAspectRatio(Array.ConvertAll(FacePoints.RightEyeEAR, converter));
				lEyeIris = FacePoints.CalculateIrisPosition(FacePoints.LeftEyeIrisPoint, converter);
				rEyeIris = FacePoints.CalculateIrisPosition(FacePoints.RightEyeIrisPoint, converter);
			}

			{
				Vector3 botHead = ConvertPoint(eventArgs.value, 152);
				Vector3 topHead = ConvertPoint(eventArgs.value, 10);
				Plane plane = new(ConvertPoint(eventArgs.value, 109), ConvertPoint(eventArgs.value, 338), botHead);

				// Figure out their position on the eye socket plane
				Vector3 forwardDir = plane.normal;
				Vector3 faceUpDir = botHead - topHead;

				neckRotation = Quaternion.LookRotation(forwardDir, faceUpDir);
			}

			Pose.Neck.Add(neckRotation, TimeNow);
			this.mouthOpen = mouthOpen;
			this.rEyeOpen.Add(rEyeOpen);
			this.lEyeOpen.Add(lEyeOpen);
			this.rEyeIris.Add(rEyeIris);
			this.lEyeIris.Add(lEyeIris);
		}

		private void OnLeftHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			Canvas.OnLeftHandLandmarksOutput(eventArgs);
			if (eventArgs.value == null || !TrackLeftHand) {
				return;
			}

			Groups.HandPoints handPoints = new();
			int count = eventArgs.value.Landmark.Count;
			for (int i = 0; i < count; i++) {
				handPoints.Data[i] = ConvertPoint(eventArgs.value, i);
			}
			
			Groups.HandRotation handGroup = HandResolver.SolveLeftHand(handPoints);
			LeftHand.Update(handGroup, TimeNow);
		}

		private void OnRightHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			Canvas.OnRightHandLandmarksOutput(eventArgs);
			if (eventArgs.value == null || !TrackRightHand) {
				return;
			}
			
			Groups.HandPoints handPoints = new();
			int count = eventArgs.value.Landmark.Count;
			for (int i = 0; i < count; i++) {
				handPoints.Data[i] = ConvertPoint(eventArgs.value, i);
			}

			Groups.HandRotation handGroup = HandResolver.SolveRightHand(handPoints);
			RightHand.Update(handGroup, TimeNow);
		}

		private void OnPoseLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			Canvas.OnPoseLandmarksOutput(eventArgs);

			bool trackL = true;
			bool trackR = true;

			// Use these fields to get the value
			if (eventArgs.value != null && Settings.UseTrackingBox) {
				var leftWrist = eventArgs.value.Landmark[MediaPipe.Pose.LEFT_WRIST];
				var rightWrist = eventArgs.value.Landmark[MediaPipe.Pose.RIGHT_WRIST];
				trackR = TrackingBox.IsInside(rightWrist.X, 1 - rightWrist.Y);
				trackL = TrackingBox.IsInside(leftWrist.X, 1 - leftWrist.Y);
			}
			
			TrackLeftHand = trackL;
			TrackRightHand = trackR;
		}

		private void OnPoseWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs) {
			if (eventArgs.value == null) {
				return;
			}

			Groups.PoseRotation pose = PoseResolver.SolvePose(eventArgs);

			float time = TimeNow;
			Pose.Chest.Add(pose.chestRotation, time);
			// Pose.Hips.Set(hipsRotation, time);
			Pose.HipsPosition.Add(pose.hipsPosition, time);

			if (Settings.UseLegRotation) {
				if (pose.hasRightLeg) {
					Pose.RightUpperLeg.Add(pose.rUpperLeg, time);
					Pose.RightLowerLeg.Add(pose.rLowerLeg, time);
				}

				if (pose.hasLeftLeg) {
					Pose.LeftUpperLeg.Add(pose.lUpperLeg, time);
					Pose.LeftLowerLeg.Add(pose.lLowerLeg, time);
				}
			}
			
			Pose.RightShoulder.Add(pose.rShoulder, time);
			Pose.RightElbow.Add(pose.rElbow, time);
			Pose.RightHand.Add(pose.rHand, time);
			Pose.LeftShoulder.Add(pose.lShoulder, time);
			Pose.LeftElbow.Add(pose.lElbow, time);
			Pose.LeftHand.Add(pose.lHand, time);
		}

		public virtual void Update() {
			if (Settings.ShowModel != model.IsVisible) {
				model.IsVisible = Settings.ShowModel;
			}
		}

		/// <summary>
		/// This method is called when the model should be updated
		/// </summary>
		public virtual void UpdateModel() {
			float time = TimeNow;
			RightHand.Update(time);
			LeftHand.Update(time);
			Pose.Update(time);
		}

		/// <summary>
		/// This method is called when the model should be animated
		/// </summary>
		public virtual void AnimateModel() {
			if (!model.VrmModel.activeInHierarchy || IsPaused) {
				return;
			}
			
			// Apply the model transform
			model.VrmModel.transform.position = guiMain.ModelTransform;

			if (BoneSettings.Get(BoneSettings.NECK)) {
				Pose.Neck.ApplyGlobal(model);
			}

			if (BoneSettings.Get(BoneSettings.CHEST)) {
				Pose.Chest.ApplyGlobal(model);
			}

			if (BoneSettings.Get(BoneSettings.HIPS)) {
				Pose.Hips.ApplyGlobal(model);
			}

			if (Settings.UseLegRotation) {
				if (BoneSettings.Get(BoneSettings.LEFT_LEG)) {
					Pose.LeftUpperLeg.ApplyGlobal(model);
					Pose.LeftLowerLeg.ApplyGlobal(model);
				}

				if (BoneSettings.Get(BoneSettings.RIGHT_LEG)) {
					Pose.RightUpperLeg.ApplyGlobal(model);
					Pose.RightLowerLeg.ApplyGlobal(model);
				}
			}

			if (BoneSettings.Get(BoneSettings.RIGHT_WRIST)) {
				RightHand.Wrist.ApplyGlobal(model, true);
			}

			if (BoneSettings.Get(BoneSettings.RIGHT_FINGERS)) {
				RightHand.ApplyFingers(model);
			}
			
			if (BoneSettings.Get(BoneSettings.LEFT_WRIST)) {
				LeftHand.Wrist.ApplyGlobal(model, true);
			}
			
			if (BoneSettings.Get(BoneSettings.LEFT_FINGERS)) {
				LeftHand.ApplyFingers(model);
			}

			if (BoneSettings.Get(BoneSettings.FACE)) {
				model.BlendShapeProxy.ImmediatelySetValue(model.BlendShapes[BlendShapePreset.O], mouthOpen);

				float rEyeTest = model.BlendShapeProxy.GetValue(model.BlendShapes[BlendShapePreset.Blink_R]);
				float lEyeTest = model.BlendShapeProxy.GetValue(model.BlendShapes[BlendShapePreset.Blink_L]);
				float rEyeValue = (rEyeOpen.Max() < FaceConfig.EAR_TRESHHOLD) ? 1 : 0;
				float lEyeValue = (lEyeOpen.Max() < FaceConfig.EAR_TRESHHOLD) ? 1 : 0;
				rEyeValue = (rEyeValue + rEyeTest * 2) / 3.0f;
				lEyeValue = (lEyeValue + lEyeTest * 2) / 3.0f;

				model.BlendShapeProxy.ImmediatelySetValue(model.BlendShapes[BlendShapePreset.Blink_R], rEyeValue);
				model.BlendShapeProxy.ImmediatelySetValue(model.BlendShapes[BlendShapePreset.Blink_L], lEyeValue);

				// TODO: Find a better eye tracking method
				model.RigAnimator.Transforms[HumanBodyBones.LeftEye].data.rotation = new(
					(lEyeIris.Average().y - 0.14f) * -30,
					lEyeIris.Average().x * -30,
					0
				);
				model.RigAnimator.Transforms[HumanBodyBones.RightEye].data.rotation = new(
					(rEyeIris.Average().y - 0.14f) * -30,
					rEyeIris.Average().x * -30,
					0
				);
			}
		}
	}
}
