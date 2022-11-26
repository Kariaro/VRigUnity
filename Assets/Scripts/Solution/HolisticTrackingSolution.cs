using Mediapipe;
using Mediapipe.Unity;
using System;
using UnityEngine;
using VRM;

namespace HardCoded.VRigUnity {
	public class HolisticTrackingSolution : HolisticSolutionBase {
		[Header("Rig")]
		[SerializeField] protected GameObject defaultVrmPrefab;
		[SerializeField] protected GameObject vrmModel;
		[SerializeField] protected RuntimeAnimatorController vrmController;
		[SerializeField] protected VRMAnimator vrmAnimator;
		[SerializeField] protected VRMBlendShapeProxy blendShapeProxy;
		[SerializeField] protected Animator animator;

		[Header("UI")]
		[SerializeField] protected GUIScript guiScript;
		[SerializeField] public CustomizableCanvas canvas;

		// Pose values
		public readonly PoseValues Pose = new();
		public readonly HandValues RightHand = new();
		public readonly HandValues LeftHand = new();
		
		private float mouthOpen = 0;

		public FaceData.RollingAverage lEyeOpen = new(FaceConfig.EAR_FRAMES);
		public FaceData.RollingAverage rEyeOpen = new(FaceConfig.EAR_FRAMES);

		public FaceData.RollingAverageVector2 lEyeIris = new(FaceConfig.EAR_FRAMES);
		public FaceData.RollingAverageVector2 rEyeIris = new(FaceConfig.EAR_FRAMES);
		
		private readonly long StartTicks = DateTime.Now.Ticks;
		protected float TimeNow => (float)((DateTime.Now.Ticks - StartTicks) / (double)TimeSpan.TicksPerSecond);

		void Awake() {
			SetVRMModel(vrmModel);
		}

		public void ResetVRMModel() {
			SetVRMModel(Instantiate(defaultVrmPrefab));
		}

		public bool SetVRMModel(GameObject gameObject) {
			VRMBlendShapeProxy blendShapeProxy = gameObject.GetComponent<VRMBlendShapeProxy>();
			Animator animator = gameObject.GetComponent<Animator>();

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

			return true;
		}

		public GameObject GetVRMModel() {
			return vrmModel;
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
			graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
			graphRunner.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
			graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
			graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
			graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;

			canvas.SetupAnnotations();
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

		private Vector4 ConvertPoint(LandmarkList list, int idx) {
			Landmark mark = list.Landmark[idx];
			return new(-mark.X, mark.Y, mark.Z, mark.Visibility);
		}

		private Vector4 ConvertPoint(NormalizedLandmarkList list, int idx) {
			NormalizedLandmark mark = list.Landmark[idx];
			return new(-mark.X * 2, mark.Y, mark.Z, mark.Visibility);
		}

		protected override void SetupScreen(ImageSource imageSource) {
			canvas.SetupScreen(imageSource);
		}

		protected override void RenderCurrentFrame(TextureFrame textureFrame) {
			canvas.ReadSync(textureFrame);
		}

		private void OnFaceLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			canvas.OnFaceLandmarksOutput(eventArgs);
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

				// Eyes
				lEyeOpen = FacePoints.CalculateEyeAspectRatio(
					Array.ConvertAll(FacePoints.LeftEyeEAR, i => (Vector3) ConvertPoint(eventArgs.value, i))
				);

				rEyeOpen = FacePoints.CalculateEyeAspectRatio(
					Array.ConvertAll(FacePoints.RightEyeEAR, i => (Vector3) ConvertPoint(eventArgs.value, i))
				);

				lEyeIris = FacePoints.CalculateIrisPosition(
					Array.ConvertAll(FacePoints.LeftEyeIrisPoint, i => (Vector3) ConvertPoint(eventArgs.value, i))
				);

				rEyeIris = FacePoints.CalculateIrisPosition(
					Array.ConvertAll(FacePoints.RightEyeIrisPoint, i => (Vector3) ConvertPoint(eventArgs.value, i))
				);
			}

			{
				Vector3 faceUpDir;
				Vector3 forwardDir;
				{
					Vector3 botHead = ConvertPoint(eventArgs.value, 152);
					Vector3 topHead = ConvertPoint(eventArgs.value, 10);
					
					Plane plane = new(ConvertPoint(eventArgs.value, 109), ConvertPoint(eventArgs.value, 338), botHead);
					forwardDir = plane.normal;

					// Figure out their position on the eye socket plane
					faceUpDir = new Vector3(
						-(topHead.x - botHead.x),
						-(topHead.y - botHead.y),
						-(topHead.z - botHead.z)
					);
				}

				neckRotation = Quaternion.LookRotation(-forwardDir, faceUpDir);
			}

			Pose.Neck.Set(neckRotation, TimeNow);
			this.mouthOpen = mouthOpen;
			this.rEyeOpen.Add(lEyeOpen);
			this.lEyeOpen.Add(rEyeOpen);
			this.rEyeIris.Add(lEyeIris);
			this.lEyeIris.Add(rEyeIris);
		}

		private void OnLeftHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			canvas.OnLeftHandLandmarksOutput(eventArgs);
			if (eventArgs.value == null) {
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
			canvas.OnRightHandLandmarksOutput(eventArgs);
			if (eventArgs.value == null) {
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
			Groups.HandRotation handGroup;
			
			// handGroup = HandResolverOld.SolveLeftHand(hand);
			handGroup = HandResolver.SolveLeftHand(hand);

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
			Groups.HandRotation handGroup;
			
			// handGroup = HandResolverOld.SolveRightHand(hand);
			handGroup = HandResolver.SolveRightHand(hand);
			
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
			canvas.OnPoseLandmarksOutput(eventArgs);
		}

		private void OnPoseWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs) {
			if (eventArgs.value == null) {
				return;
			}

			Quaternion chestRotation;
			Quaternion hipsRotation;
			Quaternion rUpperArm;
			Quaternion rLowerArm;
			Quaternion lUpperArm;
			Quaternion lLowerArm;
			Vector3 hipsPosition;

			Quaternion rUpperLeg;
			Quaternion rLowerLeg;
			Quaternion lUpperLeg;
			Quaternion lLowerLeg;

			// Experimental IK
			Vector4 rShoulder;
			Vector4 rElbow;
			Vector4 rHand;
			Vector4 lShoulder;
			Vector4 lElbow;
			Vector4 lHand;

			bool hasLeftLeg;
			bool hasRightLeg;

			{
				rShoulder = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_SHOULDER);
				lShoulder = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_SHOULDER);
				Vector4 rHip = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_HIP);
				Vector4 lHip = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_HIP);

				{
					Vector3 vRigA = Vector3.left;
					Vector3 vRigB = rShoulder - lShoulder;
					Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
					chestRotation = rot;

					vRigA = Vector3.left;
					vRigB = rHip - lHip;
					rot = Quaternion.FromToRotation(vRigA, vRigB);
					hipsRotation = rot;

					float mul = 1000;
					hipsPosition = new Vector3(
						(rHip.y + lHip.y) * 0.5f * mul,
						0, // -(rHip.z + lHip.z) * 0.5f * mul,
						0 // (rHip.y + lHip.y) * 0.5f * mul
					);

					/*
					float bodyRotation = 1.0f;
					bodyRotation = Mathf.Abs(Mathf.Cos(rot.eulerAngles.y * 1.6f));
					*/
				}

				{
					rElbow = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_ELBOW);
					rHand = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_WRIST);
					Vector3 vRigA = Vector3.left;
					Vector3 vRigB = rElbow - rShoulder;
					Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
					rUpperArm = rot;

					Vector3 vRigC = rHand - rElbow;
					rot = Quaternion.FromToRotation(vRigA, vRigC);
					rLowerArm = rot;

					if (rHand.w < Settings.HandTrackingThreshold) {
						rLowerArm = rUpperArm;
					}
				}

				{
					lElbow = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_ELBOW);
					lHand = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_WRIST);
					Vector3 vRigA = Vector3.right;
					Vector3 vRigB = lElbow - lShoulder;
					Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
					lUpperArm = rot;

					Vector3 vRigC = lHand - lElbow;
					rot = Quaternion.FromToRotation(vRigA, vRigC);
					lLowerArm = rot;

					if (lHand.w < Settings.HandTrackingThreshold) {
						lLowerArm = lUpperArm;
					}
				}

				// Legs
				{
					Vector4 rKnee = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_KNEE);
					Vector4 rAnkle = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_ANKLE);
					Vector3 vRigA = Vector3.up;
					Vector3 vRigB = rKnee - rHip;
					Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
					rUpperLeg = rot;

					Vector3 vRigC = rAnkle - rKnee;
					rot = Quaternion.FromToRotation(vRigA, vRigC);
					rLowerLeg = rot;
					
					hasRightLeg = rHip.w > 0.5 && rKnee.w > 0.5;
				}

				{
					Vector4 lKnee = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_KNEE);
					Vector4 lAnkle = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_ANKLE);
					Vector3 vRigA = Vector3.up;
					Vector3 vRigB = lKnee - lHip;
					Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
					lUpperLeg = rot;

					Vector3 vRigC = lAnkle - lKnee;
					rot = Quaternion.FromToRotation(vRigA, vRigC);
					lLowerLeg = rot;

					hasLeftLeg = lHip.w > 0.5 && lKnee.w > 0.5;
				}
			}

			float time = TimeNow;
			Pose.Chest.Set(chestRotation, time);
			Pose.Hips.Set(hipsRotation, time);
			Pose.HipsPosition.Set(hipsPosition, time);
			
			if (!Settings.UseFullIK) {
				Pose.RightUpperArm.Set(rUpperArm, time);
				Pose.RightLowerArm.Set(rLowerArm, time);
				Pose.LeftUpperArm.Set(lUpperArm, time);
				Pose.LeftLowerArm.Set(lLowerArm, time);
			}

			if (Settings.UseLegRotation) {
				if (hasRightLeg) {
					Pose.RightUpperLeg.Set(rUpperLeg, time);
					Pose.RightLowerLeg.Set(rLowerLeg, time);
				}

				if (hasLeftLeg) {
					Pose.LeftUpperLeg.Set(lUpperLeg, time);
					Pose.LeftLowerLeg.Set(lLowerLeg, time);
				}
			}
			
			Pose.RightShoulder.Set(lShoulder, time);
			Pose.RightElbow.Set(lElbow, time);
			Pose.RightHand.Set(lHand, time);
			Pose.LeftShoulder.Set(rShoulder, time);
			Pose.LeftElbow.Set(rElbow, time);
			Pose.LeftHand.Set(rHand, time);
		}

		public virtual void ModelUpdate() {
			if (!vrmModel.activeInHierarchy) {
				return;
			}

			float time = TimeNow;

			// Apply the model transform
			vrmModel.transform.position = guiScript.GetModelTransform();

			if (isPaused) {
				DefaultVRMAnimator();
				return;
			}

			// All transformations are inverted from left to right because the VMR
			// models do not allow for mirroring
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
				if (BoneSettings.Get(BoneSettings.LEFT_SHOULDER)) {
					Pose.LeftUpperArm.UpdateRotation(animator, HumanBodyBones.LeftUpperArm, time);
				}

				if (BoneSettings.Get(BoneSettings.LEFT_ELBOW)) {
					Pose.LeftLowerArm.UpdateRotation(animator, HumanBodyBones.LeftLowerArm, time);
				}

				if (BoneSettings.Get(BoneSettings.RIGHT_SHOULDER)) {
					Pose.RightUpperArm.UpdateRotation(animator, HumanBodyBones.RightUpperArm, time);
				}

				if (BoneSettings.Get(BoneSettings.RIGHT_ELBOW)) {
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
				LeftHand.Wrist.UpdateRotation(animator, HumanBodyBones.RightHand, time);
			}

			if (BoneSettings.Get(BoneSettings.RIGHT_FINGERS)) {
				LeftHand.IndexPip. UpdateLocalRotation(animator, HumanBodyBones.RightIndexProximal, time);
				LeftHand.IndexDip. UpdateLocalRotation(animator, HumanBodyBones.RightIndexIntermediate, time);
				LeftHand.IndexTip. UpdateLocalRotation(animator, HumanBodyBones.RightIndexDistal, time);
				LeftHand.MiddlePip.UpdateLocalRotation(animator, HumanBodyBones.RightMiddleProximal, time);
				LeftHand.MiddleDip.UpdateLocalRotation(animator, HumanBodyBones.RightMiddleIntermediate, time);
				LeftHand.MiddleTip.UpdateLocalRotation(animator, HumanBodyBones.RightMiddleDistal, time);
				LeftHand.RingPip.  UpdateLocalRotation(animator, HumanBodyBones.RightRingProximal, time);
				LeftHand.RingDip.  UpdateLocalRotation(animator, HumanBodyBones.RightRingIntermediate, time);
				LeftHand.RingTip.  UpdateLocalRotation(animator, HumanBodyBones.RightRingDistal, time);
				LeftHand.PinkyPip. UpdateLocalRotation(animator, HumanBodyBones.RightLittleProximal, time);
				LeftHand.PinkyDip. UpdateLocalRotation(animator, HumanBodyBones.RightLittleIntermediate, time);
				LeftHand.PinkyTip. UpdateLocalRotation(animator, HumanBodyBones.RightLittleDistal, time);
				LeftHand.ThumbPip. UpdateLocalRotation(animator, HumanBodyBones.RightThumbProximal, time);
				LeftHand.ThumbDip. UpdateLocalRotation(animator, HumanBodyBones.RightThumbIntermediate, time);
				LeftHand.ThumbTip. UpdateLocalRotation(animator, HumanBodyBones.RightThumbDistal, time);
			}
			
			if (BoneSettings.Get(BoneSettings.LEFT_WRIST)) {
				RightHand.Wrist.UpdateRotation(animator, HumanBodyBones.LeftHand, time);
			}
			
			if (BoneSettings.Get(BoneSettings.LEFT_FINGERS)) {
				RightHand.IndexPip. UpdateLocalRotation(animator, HumanBodyBones.LeftIndexProximal, time);
				RightHand.IndexDip. UpdateLocalRotation(animator, HumanBodyBones.LeftIndexIntermediate, time);
				RightHand.IndexTip. UpdateLocalRotation(animator, HumanBodyBones.LeftIndexDistal, time);
				RightHand.MiddlePip.UpdateLocalRotation(animator, HumanBodyBones.LeftMiddleProximal, time);
				RightHand.MiddleDip.UpdateLocalRotation(animator, HumanBodyBones.LeftMiddleIntermediate, time);
				RightHand.MiddleTip.UpdateLocalRotation(animator, HumanBodyBones.LeftMiddleDistal, time);
				RightHand.RingPip.  UpdateLocalRotation(animator, HumanBodyBones.LeftRingProximal, time);
				RightHand.RingDip.  UpdateLocalRotation(animator, HumanBodyBones.LeftRingIntermediate, time);
				RightHand.RingTip.  UpdateLocalRotation(animator, HumanBodyBones.LeftRingDistal, time);
				RightHand.PinkyPip. UpdateLocalRotation(animator, HumanBodyBones.LeftLittleProximal, time);
				RightHand.PinkyDip. UpdateLocalRotation(animator, HumanBodyBones.LeftLittleIntermediate, time);
				RightHand.PinkyTip. UpdateLocalRotation(animator, HumanBodyBones.LeftLittleDistal, time);
				RightHand.ThumbPip. UpdateLocalRotation(animator, HumanBodyBones.LeftThumbProximal, time);
				RightHand.ThumbDip. UpdateLocalRotation(animator, HumanBodyBones.LeftThumbIntermediate, time);
				RightHand.ThumbTip. UpdateLocalRotation(animator, HumanBodyBones.LeftThumbDistal, time);
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
