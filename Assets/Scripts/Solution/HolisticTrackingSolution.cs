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
		[SerializeField] protected VRMBlendShapeProxy blendShapeProxy;
		[SerializeField] protected Animator animator;

		[Header("UI")]
		[SerializeField] protected GUIScript guiScript;
		[SerializeField] public CustomizableCanvas canvas;

		// Pose values
		protected readonly PoseValues Pose = new();
		protected readonly HandValues RightHand = new();
		protected readonly HandValues LeftHand = new();
		
		private float mouthOpen = 0;

		public FaceData.RollingAverage lEyeOpen = new(FaceConfig.EAR_FRAMES);
		public FaceData.RollingAverage rEyeOpen = new(FaceConfig.EAR_FRAMES);

		public FaceData.RollingAverageVector2 lEyeIris = new(FaceConfig.EAR_FRAMES);
		public FaceData.RollingAverageVector2 rEyeIris = new(FaceConfig.EAR_FRAMES);
		
		private readonly long StartTicks = DateTime.Now.Ticks;
		protected float TimeNow => (float)((DateTime.Now.Ticks - StartTicks) / (double)TimeSpan.TicksPerSecond);
		
		// Testing values
		[Header("Testing")]
		public int TestInterpolation;
		public float InterpolationValue = 0.2f;
		public static int TestInterpolationStatic;
		public static float TestInterpolationValue;
		
		// Private values
		protected Quaternion m_leftWrist = Quaternion.identity;
		protected Quaternion m_rightWrist = Quaternion.identity;

		public void ResetVRMModel() {
			SetVRMModel(Instantiate(defaultVrmPrefab));
		}

		public bool SetVRMModel(GameObject gameObject) {
			VRMBlendShapeProxy blendShapeProxy = gameObject.GetComponent<VRMBlendShapeProxy>();
			Animator animator = gameObject.GetComponent<Animator>();

			if (animator == null || blendShapeProxy == null) {
				return false;
			}

			if (vrmModel != null) {
				Destroy(vrmModel);
			}

			this.vrmModel = gameObject;
			this.blendShapeProxy = blendShapeProxy;
			this.animator = animator;
			return true;
		}

		public GameObject GetVRMModel() {
			return vrmModel;
		}

		// Called when a bone is selected or deselected
		public void OnBoneUpdate(int index, bool set) {
			if (set) {
				// Our program should now track the bone
				// Make sure all parts are cleared
				foreach (HumanBodyBones bone in BoneSettings.GetBones(index)) {
					Transform trans = animator.GetBoneTransform(bone);
					if (trans != null) {
						trans.localRotation = Quaternion.identity;
					}
				}
			}
		}

		protected override void OnStartRun() {
			graphRunner.OnPoseDetectionOutput += OnPoseDetectionOutput;
			graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
			graphRunner.OnPoseRoiOutput += OnPoseRoiOutput;
			graphRunner.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
			graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
			graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
			graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;

			canvas.SetupAnnotations();
		}

		
		public void ResetVRMAnimator() {
			animator.Rebind();
			foreach (BlendShapePreset preset in Enum.GetValues(typeof(BlendShapePreset))) {
				blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), 0);
			}
		}
		
		private Vector3 ConvertPoint(LandmarkList list, int idx) {
			Landmark mark = list.Landmark[idx];
			return new Vector3(-mark.X, mark.Y, mark.Z);
		}

		private Vector3 ConvertPoint(NormalizedLandmarkList list, int idx) {
			NormalizedLandmark mark = list.Landmark[idx];
			return new Vector3(-mark.X * 2, mark.Y, mark.Z);
		}

		protected override void SetupScreen(ImageSource imageSource) {
			canvas.SetupScreen(imageSource);
		}

		protected override void RenderCurrentFrame(TextureFrame textureFrame) {
			canvas.ReadSync(textureFrame);
		}

		private void OnPoseDetectionOutput(object stream, OutputEventArgs<Detection> eventArgs) {
			canvas.OnPoseDetectionOutput(eventArgs);
		}

		private void OnPoseLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			canvas.OnPoseLandmarksOutput(eventArgs);
		}

		private void OnPoseRoiOutput(object stream, OutputEventArgs<NormalizedRect> eventArgs) {
			canvas.OnPoseRoiOutput(eventArgs);
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

				{
					// Mouth
					// left : 324
					// right: 78
					// top  : 13
					
					Vector3 a = ConvertPoint(eventArgs.value, 324);
					Vector3 b = ConvertPoint(eventArgs.value, 78);
					Vector3 c = ConvertPoint(eventArgs.value, 13);
					Vector3 m = (a + b) / 2.0f;

					float width = Vector3.Distance(a, b);
					float height = Vector3.Distance(c, m);
					float area = MovementUtils.GetTriangleArea(a, b, c);
					float perc = height / width; //2 * (area / (width * height));

					//perc = Mathf.Clamp01(perc);
					// Debug.Log("w: " + width + ", h: " + height + ", awh: " + area + ", ah: " + (area / width) + ", aw: " + (area / height) + ", a: " + perc);

					mouthOpen = perc * 2 - 0.1f;
				}

				{
					lEyeOpen = FacePoints.CalculateEyeAspectRatio(
						Array.ConvertAll(FacePoints.LeftEyeEAR, i => ConvertPoint(eventArgs.value, i))
					);

					rEyeOpen = FacePoints.CalculateEyeAspectRatio(
						Array.ConvertAll(FacePoints.RightEyeEAR, i => ConvertPoint(eventArgs.value, i))
					);

					lEyeIris = FacePoints.CalculateIrisPosition(
						Array.ConvertAll(FacePoints.LeftEyeIrisPoint, i => ConvertPoint(eventArgs.value, i))
					);

					rEyeIris = FacePoints.CalculateIrisPosition(
						Array.ConvertAll(FacePoints.RightEyeIrisPoint, i => ConvertPoint(eventArgs.value, i))
					);

					// Debug.Log("l: " + lEyeOpen + ", r: " + rEyeOpen);
				}

				//Quaternion rot = Quaternion.FromToRotation(Vector3.up, faceUpDir);
				Quaternion rot = Quaternion.LookRotation(-forwardDir, faceUpDir);
				neckRotation = rot;
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

			m_leftWrist = handGroup.Wrist;

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

			m_rightWrist = handGroup.Wrist;
			
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

		private void OnPoseWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs) {
			canvas.OnPoseWorldLandmarksOutput(eventArgs);
			if (eventArgs.value == null) {
				return;
			}

			Quaternion chestRotation = Quaternion.identity;
			Quaternion hipsRotation = Quaternion.identity;
			Quaternion rUpperArm = Quaternion.identity;
			Quaternion rLowerArm = Quaternion.identity;
			Quaternion lUpperArm = Quaternion.identity;
			Quaternion lLowerArm = Quaternion.identity;

			float handExtraPercentage = 0.2f;
			try {
				Vector3 rShoulder = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_SHOULDER);
				Vector3 lShoulder = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_SHOULDER);

				float bodyRotation = 1.0f;
				{
					Vector3 vRigA = Vector3.left;
					Vector3 vRigB = rShoulder - lShoulder;
					Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
					chestRotation = rot;
					hipsRotation = rot;
					/*hipsPosition = new Vector3(
						-(rShoulder.x + lShoulder.x) * 0.5f * 2,
						-(rShoulder.z + lShoulder.z) * 0.5f,
						(rShoulder.y + lShoulder.y) * 0.5f + 1.0f
					);*/
					bodyRotation = Mathf.Abs(Mathf.Cos(rot.eulerAngles.y * 1.6f));
				}
				float hep = handExtraPercentage * bodyRotation;

				{
					Vector3 rElbow = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_ELBOW);
					Vector3 rHand = ConvertPoint(eventArgs.value, MediaPipe.Pose.LEFT_WRIST);
					// If we have hand data

					Vector3 vRigA = Vector3.left;
					Vector3 vRigB = rElbow - rShoulder;
					Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
					rUpperArm = rot;

					Vector3 vRigC = rHand - rElbow;
					rot = Quaternion.FromToRotation(vRigA, vRigC);
					rLowerArm = rot;
				}

				{
					Vector3 lElbow = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_ELBOW);
					Vector3 lHand = ConvertPoint(eventArgs.value, MediaPipe.Pose.RIGHT_WRIST);
					// If we have hand data

					Vector3 vRigA = Vector3.right;
					Vector3 vRigB = lElbow - lShoulder;
					Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
					lUpperArm = rot;

					Vector3 vRigC = lHand - lElbow;
					rot = Quaternion.FromToRotation(vRigA, vRigC);
					lLowerArm = rot;
				}
			} catch {
				// Catch all exceptions
			}

			Pose.Chest.Set(chestRotation, TimeNow);
			Pose.Hips.Set(hipsRotation, TimeNow);
			Pose.RightUpperArm.Set(rUpperArm, TimeNow);
			Pose.RightLowerArm.Set(rLowerArm, TimeNow);
			Pose.LeftUpperArm.Set(lUpperArm, TimeNow);
			Pose.LeftLowerArm.Set(lLowerArm, TimeNow);
		}

		// This is protected to allow being called from child classes
		protected void FixedUpdate() {
			if (!vrmModel.activeInHierarchy) {
				return;
			}

			TestInterpolationStatic = TestInterpolation;
			TestInterpolationValue = InterpolationValue;
			float time = TimeNow;

			// Apply the model transform
			vrmModel.transform.position = guiScript.GetModelTransform();

			// All transformations are inverted from left to right because the VMR
			// models do not allow for mirroring.
			if (BoneSettings.Get(BoneSettings.CHEST)) {
				Pose.Chest.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.Chest), time);
			}

			if (BoneSettings.Get(BoneSettings.NECK)) {
				Pose.Neck.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.Neck), time);
			}

			if (BoneSettings.Get(BoneSettings.LEFT_SHOULDER)) {
				Pose.LeftUpperArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), time);
			}

			if (BoneSettings.Get(BoneSettings.LEFT_ELBOW)) {
				Pose.LeftLowerArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), time);
			}

			if (BoneSettings.Get(BoneSettings.RIGHT_SHOULDER)) {
				Pose.RightUpperArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightUpperArm), time);
			}

			if (BoneSettings.Get(BoneSettings.RIGHT_ELBOW)) {
				Pose.RightLowerArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), time);
			}

			if (BoneSettings.Get(BoneSettings.RIGHT_WRIST)) {
				LeftHand.Wrist.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightHand), time);
			}

			if (BoneSettings.Get(BoneSettings.RIGHT_FINGERS)) {
				LeftHand.IndexPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightIndexProximal), time);
				LeftHand.IndexDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightIndexIntermediate), time);
				LeftHand.IndexTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightIndexDistal), time);
				LeftHand.MiddlePip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal), time);
				LeftHand.MiddleDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate), time);
				LeftHand.MiddleTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal), time);
				LeftHand.RingPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.  RightRingProximal), time);
				LeftHand.RingDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.  RightRingIntermediate), time);
				LeftHand.RingTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.  RightRingDistal), time);
				LeftHand.PinkyPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightLittleProximal), time);
				LeftHand.PinkyDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightLittleIntermediate), time);
				LeftHand.PinkyTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightLittleDistal), time);
				LeftHand.ThumbPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightThumbProximal), time);
				LeftHand.ThumbDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightThumbIntermediate), time);
				LeftHand.ThumbTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightThumbDistal), time);
			}
			
			if (BoneSettings.Get(BoneSettings.LEFT_WRIST)) {
				RightHand.Wrist.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftHand), time);
			}
			
			if (BoneSettings.Get(BoneSettings.LEFT_FINGERS)) {
				RightHand.IndexPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftIndexProximal), time);
				RightHand.IndexDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftIndexIntermediate), time);
				RightHand.IndexTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftIndexDistal), time);
				RightHand.MiddlePip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal), time);
				RightHand.MiddleDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate), time);
				RightHand.MiddleTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal), time);
				RightHand.RingPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.  LeftRingProximal), time);
				RightHand.RingDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.  LeftRingIntermediate), time);
				RightHand.RingTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.  LeftRingDistal), time);
				RightHand.PinkyPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftLittleProximal), time);
				RightHand.PinkyDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftLittleIntermediate), time);
				RightHand.PinkyTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftLittleDistal), time);
				RightHand.ThumbPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftThumbProximal), time);
				RightHand.ThumbDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftThumbIntermediate), time);
				RightHand.ThumbTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftThumbDistal), time);
			}

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
				//animator.GetBoneTransform(HumanBodyBones.Neck).transform.rotation = Quaternion.identity;
				animator.GetBoneTransform(HumanBodyBones.LeftEye).transform.localRotation = Quaternion.Euler(
					(lEyeIris.Average().y - 0.14f) * -30,
					lEyeIris.Average().x * -30,
					0
				);
				animator.GetBoneTransform(HumanBodyBones.RightEye).transform.localRotation = Quaternion.Euler(
					(rEyeIris.Average().y - 0.14f) * -30,
					rEyeIris.Average().x * -30,
					0
				);
			} else {
				blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.O), 0);
				blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_L), 0);
				blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_R), 0);
			}

			/*
			// TODO: Clear values only when they were enabled
			for (int i = 0; i < BoneSettings.Count; i++) {
				if (!BoneSettings.Get(i)) {
					foreach (HumanBodyBones bone in BoneSettings.GetBones(i)) {
						Transform trans = animator.GetBoneTransform(bone);
						if (trans != null) {
							trans.localRotation = Quaternion.identity;
						}
					}
				}
			}
			*/
		}
	}
}
