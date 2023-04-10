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
		protected SceneModel model;

		[Header("UI")]
		public GUIMain guiMain;
		public CustomizableCanvas Canvas => guiMain.CustomizableCanvas;
		public TrackingResizableBox TrackingBox => guiMain.TrackingBox;

		// Pose values
		public readonly PoseValues Pose = new();
		public readonly FaceValues Face = new();
		public readonly HandValues RightHand = new(false);
		public readonly HandValues LeftHand = new(true);
		public bool TrackRightHand = true;
		public bool TrackLeftHand = true;

		// API Getters
		private readonly long StartTicks = DateTime.Now.Ticks;
		public float TimeNow => (float)((DateTime.Now.Ticks - StartTicks) / (double)TimeSpan.TicksPerSecond);
		public SceneModel Model => model;

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

			DataGroups.FaceData face = FaceResolver.Solve(eventArgs);
			Face.mouthOpen = face.mouthOpen;
			Face.lEyeIris.Add(face.lEyeIris);
			Face.rEyeIris.Add(face.rEyeIris);
			Face.lEyeOpen.Add(face.lEyeOpen);
			Face.rEyeOpen.Add(face.rEyeOpen);
			Pose.Neck.Add(face.neckRotation, TimeNow);
		}

		private void OnLeftHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			Canvas.OnLeftHandLandmarksOutput(eventArgs);
			if (eventArgs.value == null || !TrackLeftHand) {
				return;
			}

			DataGroups.HandData handGroup = HandResolver.SolveLeftHand(eventArgs);
			LeftHand.Update(handGroup, TimeNow);
		}

		private void OnRightHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			Canvas.OnRightHandLandmarksOutput(eventArgs);
			if (eventArgs.value == null || !TrackRightHand) {
				return;
			}

			DataGroups.HandData handGroup = HandResolver.SolveRightHand(eventArgs);
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

			DataGroups.PoseData pose = PoseResolver.SolvePose(eventArgs);

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
				model.BlendShapeProxy.ImmediatelySetValue(model.BlendShapes[BlendShapePreset.O], Face.mouthOpen);

				float rEyeTest = model.BlendShapeProxy.GetValue(model.BlendShapes[BlendShapePreset.Blink_R]);
				float lEyeTest = model.BlendShapeProxy.GetValue(model.BlendShapes[BlendShapePreset.Blink_L]);
				float rEyeValue = (Face.rEyeOpen.Max() < FaceConfig.EAR_TRESHHOLD) ? 1 : 0;
				float lEyeValue = (Face.lEyeOpen.Max() < FaceConfig.EAR_TRESHHOLD) ? 1 : 0;
				rEyeValue = (rEyeValue + rEyeTest * 2) / 3.0f;
				lEyeValue = (lEyeValue + lEyeTest * 2) / 3.0f;

				model.BlendShapeProxy.ImmediatelySetValue(model.BlendShapes[BlendShapePreset.Blink_R], rEyeValue);
				model.BlendShapeProxy.ImmediatelySetValue(model.BlendShapes[BlendShapePreset.Blink_L], lEyeValue);

				// TODO: Find a better eye tracking method
				model.RigAnimator.Transforms[HumanBodyBones.LeftEye].data.rotation = new(
					(Face.lEyeIris.Average().y - 0.14f) * -30,
					Face.lEyeIris.Average().x * -30,
					0
				);
				model.RigAnimator.Transforms[HumanBodyBones.RightEye].data.rotation = new(
					(Face.rEyeIris.Average().y - 0.14f) * -30,
					Face.rEyeIris.Average().x * -30,
					0
				);
			}
		}
	}
}
