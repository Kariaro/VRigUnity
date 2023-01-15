using Mediapipe;
using Mediapipe.Unity;
using System;
using UnityEngine;
using VRM;

namespace HardCoded.VRigUnity {
	public class HolisticSolution : Solution {
		[Header("Rig")]
		[SerializeField] protected GameObject defaultVrmPrefab;
		[SerializeField] protected GameObject vrmModel;
		[SerializeField] protected RuntimeAnimatorController vrmController;
		[SerializeField] protected VRMAnimator vrmAnimator;
		[SerializeField] protected VRMBlendShapeProxy blendShapeProxy;
		[SerializeField] protected Animator animator;

		[Header("UI")]
		public GUIScript guiScript;
		public CustomizableCanvas Canvas => guiScript.customizableCanvas;
		public TrackingResizableBox TrackingBox => guiScript.trackingBox;

		// Pose values
		public readonly PoseValues Pose = new();
		public readonly HandValues RightHand = new();
		public readonly HandValues LeftHand = new();
		
		private float mouthOpen = 0;

		public FaceData.RollingAverage lEyeOpen = new(FaceConfig.EAR_FRAMES);
		public FaceData.RollingAverage rEyeOpen = new(FaceConfig.EAR_FRAMES);

		public FaceData.RollingAverageVector2 lEyeIris = new(FaceConfig.EAR_FRAMES);
		public FaceData.RollingAverageVector2 rEyeIris = new(FaceConfig.EAR_FRAMES);

		public bool TrackRightHand = true;
		public bool TrackLeftHand = true;

		// API Getters
		private readonly long StartTicks = DateTime.Now.Ticks;
		public GameObject VrmModel => vrmModel;
		public float TimeNow => (float)((DateTime.Now.Ticks - StartTicks) / (double)TimeSpan.TicksPerSecond);

		void Awake() {
			SetVRMModel(vrmModel);
		}

		public void ResetVRMModel() {
			SetVRMModel(Instantiate(defaultVrmPrefab));
		}

		public bool SetVRMModel(GameObject gameObject) {
			var blendShapeProxy = gameObject.GetComponent<VRMBlendShapeProxy>();
			var animator = gameObject.GetComponent<Animator>();

			if (animator == null || blendShapeProxy == null) {
				return false;
			}

			if (vrmModel != null && vrmModel != gameObject) {
				Destroy(vrmModel);
			}
			
			this.vrmAnimator = gameObject.AddComponent<VRMAnimator>();
			this.vrmAnimator.controller = vrmController;
			this.vrmModel = gameObject;
			this.blendShapeProxy = blendShapeProxy;
			this.animator = animator;

			DefaultVRMAnimator();

			if (!Settings.ShowModel) {
				foreach (var transform in vrmModel.GetComponentsInChildren<Transform>()) {
					transform.gameObject.layer = LayerMask.NameToLayer("HiddenModel");
				}
			}

			return true;
		}

		// Called when a bone is selected or deselected
		public void OnBoneUpdate(int index, bool set) {
			// Our program should now track the bone
			// Make sure all parts are cleared
			foreach (HumanBodyBones bone in BoneSettings.GetBones(index)) {
				Transform trans = animator.GetBoneTransform(bone);
				if (trans != null) {
					trans.localRotation = BoneSettings.GetDefaultRotation(bone);
				}
			}
		}

		protected override void OnStartRun() {
			// (a, b) => { try { 
			// (a, b); } catch (Exception e) {} };
			graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
			graphRunner.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
			graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
			graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
			graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;

			Canvas.SetupAnnotations();
		}

		public void DefaultVRMAnimator() {
			foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones))) {
				if (bone == HumanBodyBones.LastBone) {
					break;
				}

				Transform trans = animator.GetBoneTransform(bone);
				if (trans != null) {
					trans.localRotation = BoneSettings.GetDefaultRotation(bone);
				}
			}
		}

		public void ResetVRMAnimator() {
			// TODO: What does rebind do?
			animator.Rebind();
			foreach (BlendShapePreset preset in Enum.GetValues(typeof(BlendShapePreset))) {
				// TODO: Remove memory allocation and cache
				blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), 0);
			}
		}

		private Vector4 ConvertPoint(NormalizedLandmarkList list, int idx) {
			NormalizedLandmark mark = list.Landmark[idx];
			return new(mark.X * 2, mark.Y, mark.Z, mark.Visibility);
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

				mouthOpen = perc * 2 - 0.1f;

				Vector3 converter(int i) {
					Vector3 value = ConvertPoint(eventArgs.value, i);
					value.x = -value.x;
					return value;
				}

				// Eyes
				lEyeOpen = FacePoints.CalculateEyeAspectRatio(Array.ConvertAll(FacePoints.LeftEyeEAR, converter));
				rEyeOpen = FacePoints.CalculateEyeAspectRatio(Array.ConvertAll(FacePoints.RightEyeEAR, converter));
				lEyeIris = FacePoints.CalculateIrisPosition(Array.ConvertAll(FacePoints.LeftEyeIrisPoint, converter));
				rEyeIris = FacePoints.CalculateIrisPosition(Array.ConvertAll(FacePoints.RightEyeIrisPoint, converter));
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

			Pose.Neck.Set(neckRotation, TimeNow);
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

			OnLeftHandLandmarks(handPoints);
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

			OnRightHandLandmarks(handPoints);
		}

		private void OnLeftHandLandmarks(Groups.HandPoints hand) {
			Groups.HandRotation handGroup = HandResolver.SolveLeftHand(hand);

			float time = TimeNow;
			LeftHand.Wrist.Set(handGroup.Wrist, time);
			LeftHand.IndexPip.Set(handGroup.IndexFingerMCP, time);
			LeftHand.IndexDip.Set(handGroup.IndexFingerPIP, time);
			LeftHand.IndexTip.Set(handGroup.IndexFingerDIP, time);
			LeftHand.MiddlePip.Set(handGroup.MiddleFingerMCP, time);
			LeftHand.MiddleDip.Set(handGroup.MiddleFingerPIP, time);
			LeftHand.MiddleTip.Set(handGroup.MiddleFingerDIP, time);
			LeftHand.RingPip.Set(handGroup.RingFingerMCP, time);
			LeftHand.RingDip.Set(handGroup.RingFingerPIP, time);
			LeftHand.RingTip.Set(handGroup.RingFingerDIP, time);
			LeftHand.PinkyPip.Set(handGroup.PinkyMCP, time);
			LeftHand.PinkyDip.Set(handGroup.PinkyPIP, time);
			LeftHand.PinkyTip.Set(handGroup.PinkyDIP, time);
			LeftHand.ThumbPip.Set(handGroup.ThumbCMC, time);
			LeftHand.ThumbDip.Set(handGroup.ThumbMCP, time);
			LeftHand.ThumbTip.Set(handGroup.ThumbIP, time);
		}
		
		private void OnRightHandLandmarks(Groups.HandPoints hand) {
			Groups.HandRotation handGroup = HandResolver.SolveRightHand(hand);
			
			float time = TimeNow;
			RightHand.Wrist.Set(handGroup.Wrist, time);
			RightHand.IndexPip.Set(handGroup.IndexFingerMCP, time);
			RightHand.IndexDip.Set(handGroup.IndexFingerPIP, time);
			RightHand.IndexTip.Set(handGroup.IndexFingerDIP, time);
			RightHand.MiddlePip.Set(handGroup.MiddleFingerMCP, time);
			RightHand.MiddleDip.Set(handGroup.MiddleFingerPIP, time);
			RightHand.MiddleTip.Set(handGroup.MiddleFingerDIP, time);
			RightHand.RingPip.Set(handGroup.RingFingerMCP, time);
			RightHand.RingDip.Set(handGroup.RingFingerPIP, time);
			RightHand.RingTip.Set(handGroup.RingFingerDIP, time);
			RightHand.PinkyPip.Set(handGroup.PinkyMCP, time);
			RightHand.PinkyDip.Set(handGroup.PinkyPIP, time);
			RightHand.PinkyTip.Set(handGroup.PinkyDIP, time);
			RightHand.ThumbPip.Set(handGroup.ThumbCMC, time);
			RightHand.ThumbDip.Set(handGroup.ThumbMCP, time);
			RightHand.ThumbTip.Set(handGroup.ThumbIP, time);
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
			Pose.Chest.Set(pose.chestRotation, time);
			// Pose.Hips.Set(hipsRotation, time);
			Pose.HipsPosition.Set(pose.hipsPosition, time);
			
			if (!Settings.UseFullIK) {
				if (TrackRightHand) {
					Pose.RightUpperArm.Set(pose.rUpperArm, time);
					Pose.RightLowerArm.Set(pose.rLowerArm, time);
				}

				if (TrackLeftHand) {
					Pose.LeftUpperArm.Set(pose.lUpperArm, time);
					Pose.LeftLowerArm.Set(pose.lLowerArm, time);
				}
			}

			if (Settings.UseLegRotation) {
				if (pose.hasRightLeg) {
					Pose.RightUpperLeg.Set(pose.rUpperLeg, time);
					Pose.RightLowerLeg.Set(pose.rLowerLeg, time);
				}

				if (pose.hasLeftLeg) {
					Pose.LeftUpperLeg.Set(pose.lUpperLeg, time);
					Pose.LeftLowerLeg.Set(pose.lLowerLeg, time);
				}
			}
			
			Pose.RightShoulder.Set(pose.rShoulder, time);
			Pose.RightElbow.Set(pose.rElbow, time);
			Pose.RightHand.Set(pose.rHand, time);
			Pose.LeftShoulder.Set(pose.lShoulder, time);
			Pose.LeftElbow.Set(pose.lElbow, time);
			Pose.LeftHand.Set(pose.lHand, time);
		}

		void Update() {
			if (Settings.ShowModel != (vrmModel.layer == 0)) {
				int nextLayer = Settings.ShowModel ? 0 : LayerMask.NameToLayer("HiddenModel");
				foreach (var transform in vrmModel.GetComponentsInChildren<Transform>()) {
					transform.gameObject.layer = nextLayer;
				}
			}
		}

		public virtual void ModelUpdate() {
			if (!vrmModel.activeInHierarchy) {
				return;
			}

			float time = TimeNow;

			// Apply the model transform
			vrmModel.transform.position = guiScript.ModelTransform;

			if (IsPaused) {
				DefaultVRMAnimator();
				return;
			}

			if (BoneSettings.Get(BoneSettings.NECK)) {
				Pose.Neck.UpdateRotation2(animator, HumanBodyBones.Neck, time);
			}

			if (BoneSettings.Get(BoneSettings.CHEST)) {
				Pose.Chest.UpdateRotation2(animator, HumanBodyBones.Chest, time);
			}

			if (BoneSettings.Get(BoneSettings.HIPS)) {
				Pose.Hips.UpdateRotation2(animator, HumanBodyBones.Hips, time);
			}
			
			if (!Settings.UseFullIK) { // Arms
				if (BoneSettings.Get(BoneSettings.LEFT_ARM)) {
					Pose.LeftUpperArm.UpdateRotation(animator, HumanBodyBones.LeftUpperArm, time);
					Pose.LeftLowerArm.UpdateRotation(animator, HumanBodyBones.LeftLowerArm, time);
				}

				if (BoneSettings.Get(BoneSettings.RIGHT_ARM)) {
					Pose.RightUpperArm.UpdateRotation(animator, HumanBodyBones.RightUpperArm, time);
					Pose.RightLowerArm.UpdateRotation(animator, HumanBodyBones.RightLowerArm, time);
				}
			}

			if (Settings.UseLegRotation) { // Legs
				if (BoneSettings.Get(BoneSettings.LEFT_HIP)) {
					Pose.LeftUpperLeg.UpdateRotation(animator, HumanBodyBones.LeftUpperLeg, time);
				}

				if (BoneSettings.Get(BoneSettings.LEFT_KNEE)) {
					Pose.LeftLowerLeg.UpdateRotation(animator, HumanBodyBones.LeftLowerLeg, time);
				}

				if (BoneSettings.Get(BoneSettings.RIGHT_HIP)) {
					Pose.RightUpperLeg.UpdateRotation(animator, HumanBodyBones.RightUpperLeg, time);
				}

				if (BoneSettings.Get(BoneSettings.RIGHT_KNEE)) {
					Pose.RightLowerLeg.UpdateRotation(animator, HumanBodyBones.RightLowerLeg, time);
				}
			}

			if (BoneSettings.Get(BoneSettings.RIGHT_WRIST)) {
				RightHand.Wrist.UpdateRotation(animator, HumanBodyBones.RightHand, time);
			}

			if (BoneSettings.Get(BoneSettings.RIGHT_FINGERS)) {
				RightHand.IndexPip. UpdateLocalRotation(animator, HumanBodyBones.RightIndexProximal, time);
				RightHand.IndexDip. UpdateLocalRotation(animator, HumanBodyBones.RightIndexIntermediate, time);
				RightHand.IndexTip. UpdateLocalRotation(animator, HumanBodyBones.RightIndexDistal, time);
				RightHand.MiddlePip.UpdateLocalRotation(animator, HumanBodyBones.RightMiddleProximal, time);
				RightHand.MiddleDip.UpdateLocalRotation(animator, HumanBodyBones.RightMiddleIntermediate, time);
				RightHand.MiddleTip.UpdateLocalRotation(animator, HumanBodyBones.RightMiddleDistal, time);
				RightHand.RingPip.  UpdateLocalRotation(animator, HumanBodyBones.RightRingProximal, time);
				RightHand.RingDip.  UpdateLocalRotation(animator, HumanBodyBones.RightRingIntermediate, time);
				RightHand.RingTip.  UpdateLocalRotation(animator, HumanBodyBones.RightRingDistal, time);
				RightHand.PinkyPip. UpdateLocalRotation(animator, HumanBodyBones.RightLittleProximal, time);
				RightHand.PinkyDip. UpdateLocalRotation(animator, HumanBodyBones.RightLittleIntermediate, time);
				RightHand.PinkyTip. UpdateLocalRotation(animator, HumanBodyBones.RightLittleDistal, time);
				RightHand.ThumbPip. UpdateLocalRotation(animator, HumanBodyBones.RightThumbProximal, time);
				RightHand.ThumbDip. UpdateLocalRotation(animator, HumanBodyBones.RightThumbIntermediate, time);
				RightHand.ThumbTip. UpdateLocalRotation(animator, HumanBodyBones.RightThumbDistal, time);
			}
			
			if (BoneSettings.Get(BoneSettings.LEFT_WRIST)) {
				LeftHand.Wrist.UpdateRotation(animator, HumanBodyBones.LeftHand, time);
			}
			
			if (BoneSettings.Get(BoneSettings.LEFT_FINGERS)) {
				LeftHand.IndexPip. UpdateLocalRotation(animator, HumanBodyBones.LeftIndexProximal, time);
				LeftHand.IndexDip. UpdateLocalRotation(animator, HumanBodyBones.LeftIndexIntermediate, time);
				LeftHand.IndexTip. UpdateLocalRotation(animator, HumanBodyBones.LeftIndexDistal, time);
				LeftHand.MiddlePip.UpdateLocalRotation(animator, HumanBodyBones.LeftMiddleProximal, time);
				LeftHand.MiddleDip.UpdateLocalRotation(animator, HumanBodyBones.LeftMiddleIntermediate, time);
				LeftHand.MiddleTip.UpdateLocalRotation(animator, HumanBodyBones.LeftMiddleDistal, time);
				LeftHand.RingPip.  UpdateLocalRotation(animator, HumanBodyBones.LeftRingProximal, time);
				LeftHand.RingDip.  UpdateLocalRotation(animator, HumanBodyBones.LeftRingIntermediate, time);
				LeftHand.RingTip.  UpdateLocalRotation(animator, HumanBodyBones.LeftRingDistal, time);
				LeftHand.PinkyPip. UpdateLocalRotation(animator, HumanBodyBones.LeftLittleProximal, time);
				LeftHand.PinkyDip. UpdateLocalRotation(animator, HumanBodyBones.LeftLittleIntermediate, time);
				LeftHand.PinkyTip. UpdateLocalRotation(animator, HumanBodyBones.LeftLittleDistal, time);
				LeftHand.ThumbPip. UpdateLocalRotation(animator, HumanBodyBones.LeftThumbProximal, time);
				LeftHand.ThumbDip. UpdateLocalRotation(animator, HumanBodyBones.LeftThumbIntermediate, time);
				LeftHand.ThumbTip. UpdateLocalRotation(animator, HumanBodyBones.LeftThumbDistal, time);
			}

			// Face
			if (BoneSettings.Get(BoneSettings.FACE)) {
				blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.O), mouthOpen);

				float rEyeTest = blendShapeProxy.GetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_R));
				float lEyeTest = blendShapeProxy.GetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_L));
				float rEyeValue = (rEyeOpen.Max() < FaceConfig.EAR_TRESHHOLD) ? 1 : 0;
				float lEyeValue = (lEyeOpen.Max() < FaceConfig.EAR_TRESHHOLD) ? 1 : 0;
				rEyeValue = (rEyeValue + rEyeTest * 2) / 3.0f;
				lEyeValue = (lEyeValue + lEyeTest * 2) / 3.0f;

				blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_R), rEyeValue);
				blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_L), lEyeValue);

				// TODO: Update this code to make it more correct
				animator.GetBoneTransform(HumanBodyBones.LeftEye).localRotation = Quaternion.Euler(
					(lEyeIris.Average().y - 0.14f) * -30,
					lEyeIris.Average().x * -30,
					0
				);
				animator.GetBoneTransform(HumanBodyBones.RightEye).localRotation = Quaternion.Euler(
					(rEyeIris.Average().y - 0.14f) * -30,
					rEyeIris.Average().x * -30,
					0
				);
			} else {
				blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.O), 0);
				blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_L), 0);
				blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_R), 0);
			}
		}
	}
}
