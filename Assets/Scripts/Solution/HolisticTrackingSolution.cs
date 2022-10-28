using Mediapipe;
using Mediapipe.Unity;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using VRM;

namespace HardCoded.VRigUnity {
	public class HolisticTrackingSolution : HolisticSolutionBase {
		[Header("Rig")]
		[SerializeField] protected GameObject defaultVrmPrefab;
		[SerializeField] protected GameObject vrmModel;
		[SerializeField] protected VRMBlendShapeProxy blendShapeProxy;
		[SerializeField] protected Animator animator;
		[SerializeField] protected GUIScript guiScript;

		[Header("Debug")]
		public SphereContainer sphereContainer;
		
		private Vector3[] vectors = new Vector3[800];
		private int vectorsSize = 0;

		private RotStruct chestRotation = RotStruct.identity;
		private RotStruct hipsRotation = RotStruct.identity;
		private RotStruct hipsPosition = RotStruct.identity;
		
		private RotStruct neckRotation = RotStruct.identity;

		private RotStruct rUpperArm = RotStruct.identity;
		private RotStruct rLowerArm = RotStruct.identity;
		private RotStruct lUpperArm = RotStruct.identity;
		private RotStruct lLowerArm = RotStruct.identity;

		private RotStruct rHand = RotStruct.identity;
		private RotStruct rIndexPip = RotStruct.identity;
		private RotStruct rIndexDip = RotStruct.identity;
		private RotStruct rIndexTip = RotStruct.identity;
		private RotStruct rMiddlePip = RotStruct.identity;
		private RotStruct rMiddleDip = RotStruct.identity;
		private RotStruct rMiddleTip = RotStruct.identity;
		private RotStruct rRingPip = RotStruct.identity;
		private RotStruct rRingDip = RotStruct.identity;
		private RotStruct rRingTip = RotStruct.identity;
		private RotStruct rPinkyPip = RotStruct.identity;
		private RotStruct rPinkyDip = RotStruct.identity;
		private RotStruct rPinkyTip = RotStruct.identity;
		private RotStruct rThumbPip = RotStruct.identity;
		private RotStruct rThumbDip = RotStruct.identity;
		private RotStruct rThumbTip = RotStruct.identity;

		private RotStruct lHand = RotStruct.identity;
		private RotStruct lIndexPip = RotStruct.identity;
		private RotStruct lIndexDip = RotStruct.identity;
		private RotStruct lIndexTip = RotStruct.identity;
		private RotStruct lMiddlePip = RotStruct.identity;
		private RotStruct lMiddleDip = RotStruct.identity;
		private RotStruct lMiddleTip = RotStruct.identity;
		private RotStruct lRingPip = RotStruct.identity;
		private RotStruct lRingDip = RotStruct.identity;
		private RotStruct lRingTip = RotStruct.identity;
		private RotStruct lPinkyPip = RotStruct.identity;
		private RotStruct lPinkyDip = RotStruct.identity;
		private RotStruct lPinkyTip = RotStruct.identity;
		private RotStruct lThumbPip = RotStruct.identity;
		private RotStruct lThumbDip = RotStruct.identity;
		private RotStruct lThumbTip = RotStruct.identity;

		private float mouthOpen = 0;

		public FaceData.RollingAverage lEyeOpen = new(FaceConfig.EAR_FRAMES);
		public FaceData.RollingAverage rEyeOpen = new(FaceConfig.EAR_FRAMES);

		public FaceData.RollingAverageVector2 lEyeIris = new(FaceConfig.EAR_FRAMES);
		public FaceData.RollingAverageVector2 rEyeIris = new(FaceConfig.EAR_FRAMES);
		
		private readonly long StartTicks = DateTime.Now.Ticks;
		public bool keep = false;
		private float TimeNow => (float)((DateTime.Now.Ticks - StartTicks) / (double)TimeSpan.TicksPerSecond);

		public void ResetVrmModel() {
			SetVrmModel(Instantiate(defaultVrmPrefab));
		}

		public bool SetVrmModel(GameObject gameObject) {
			VRMBlendShapeProxy blendShapeProxy = gameObject.GetComponent<VRMBlendShapeProxy>();
			Animator animator = gameObject.GetComponent<Animator>();

			if (animator == null || blendShapeProxy == null) {
				return false;
			}

			if (vrmModel != null) {
				GameObject.Destroy(vrmModel);
			}

			this.vrmModel = gameObject;
			this.blendShapeProxy = blendShapeProxy;
			this.animator = animator;
			return true;
		}

		public GameObject GetVRMModel() {
			return vrmModel;
		}

		protected override void RenderCurrentFrame(TextureFrame textureFrame) {
			guiScript.DrawImage(textureFrame);
		}

		public float GetTriangleArea(Vector3 A, Vector3 B, Vector3 C) {
			Vector3 AB = new(B.x - A.x, B.y - A.y, B.z - A.z);
			Vector3 AC = new(C.x - B.x, C.y - A.y, C.z - A.z);

			float P1 = (AB.y * AC.z - AB.z * AC.y);
			float P2 = (AB.z * AC.x - AB.x * AC.z);
			float P3 = (AB.x * AC.y - AB.y * AC.x);
			return 0.5f * Mathf.Sqrt(P1 * P1 + P2 * P2 + P3 * P3);
		}
		
		public int TestInterpolation;
		public float InterpolationValue = 0.4f; // 0.4 is really good for the current frame interval. Make this adjustable
		public static int TestInterpolationStatic;
		public static float TestInterpolationValue;

		private struct RotStruct {
			public static RotStruct identity => new(Quaternion.identity, 0);

			private float lastTime;
			private float currTime;
			private Quaternion curr;

			public RotStruct(Quaternion init, float time) {
				currTime = time;
				lastTime = time;
				curr = init;
			}

			public void Set(Quaternion value, float time) {
				lastTime = currTime;
				currTime = time;
				curr = value;
			}

			private Quaternion GetUpdatedRotation(Quaternion current, Quaternion curr, float time) {
				switch (TestInterpolationStatic) {
					default: {
						return Quaternion.Lerp(current, curr, TestInterpolationValue);
					}
					case 1: {
						float timeLength = currTime - lastTime;
						float delta = (time - currTime) / timeLength;
						return Quaternion.Lerp(current, curr, delta);
					}
					case 2: {
						return curr;
					}
				}
			}
			
			public void UpdateRotation(Transform transform, float time) {
				if (time - 1 > currTime) {
					// If the part was lost we slowly put it back to it's original position
					transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, 0.1f);
				} else {
					transform.rotation = GetUpdatedRotation(transform.rotation, curr, time);
				}
			}

			public void UpdateLocalRotation(Transform transform, float time) {
				transform.localRotation = GetUpdatedRotation(transform.localRotation, curr, time);
			}
		}

		protected override void OnStartRun() {
			graphRunner.OnPoseDetectionOutput += OnPoseDetectionOutput;
			graphRunner.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
			graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
			graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
			graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
			graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
			graphRunner.OnPoseRoiOutput += OnPoseRoiOutput;

			var imageSource = SolutionUtils.GetImageSource();
			SetupAnnotationController(_poseDetectionAnnotationController, imageSource);
			SetupAnnotationController(_holisticAnnotationController, imageSource);
			SetupAnnotationController(_poseWorldLandmarksAnnotationController, imageSource);
			SetupAnnotationController(_poseRoiAnnotationController, imageSource);
		}

		private Vector3 ConvertPoint(LandmarkList list, int idx) {
			Landmark mark = list.Landmark[idx];
			return new Vector3(-mark.X, mark.Y, mark.Z);
		}

		private Vector3 ConvertPoint2(NormalizedLandmarkList list, int idx) {
			NormalizedLandmark mark = list.Landmark[idx];
			return new Vector3(-mark.X * 2, mark.Y, mark.Z);
		}

		private Vector3 ConvertPoint(NormalizedLandmarkList list, int idx) {
			NormalizedLandmark mark = list.Landmark[idx];
			return new Vector3(-mark.X * 2, mark.Y, mark.Z);
		}

		public float TestA;
		public float TestB;
		public float TestC;

		private void OnPoseDetectionOutput(object stream, OutputEventArgs<Detection> eventArgs) {
			_poseDetectionAnnotationController.DrawLater(eventArgs.value);
		}
		
		private void OnPoseLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			_holisticAnnotationController.DrawPoseLandmarkListLater(eventArgs.value);
		}

		private void OnPoseRoiOutput(object stream, OutputEventArgs<NormalizedRect> eventArgs) {
			_poseRoiAnnotationController.DrawLater(eventArgs.value);
		}

		private void OnFaceLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			_holisticAnnotationController.DrawFaceLandmarkListLater(eventArgs.value);
			
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
					float area = GetTriangleArea(a, b, c);
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

				/*
				if (!keep) {
					for (int i = 0; i < eventArgs.value.Landmark.Count; i++) {
						Vector3 vec = ConvertPoint(eventArgs.value, i);
						vec = -vec;
						vectors[i] = vec;
					}
					vectorsSize = eventArgs.value.Landmark.Count;
				}
				*/
			}

			this.neckRotation.Set(neckRotation, TimeNow);
			this.mouthOpen = mouthOpen;
			this.lEyeOpen.Add(lEyeOpen);
			this.rEyeOpen.Add(rEyeOpen);
			this.lEyeIris.Add(lEyeIris);
			this.rEyeIris.Add(rEyeIris);
		}

		public Vector3 fixThumbTest = Vector3.zero;
		public bool fixHand = false;
		private void OnLeftHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			_holisticAnnotationController.DrawLeftHandLandmarkListLater(eventArgs.value);
			
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
			_holisticAnnotationController.DrawRightHandLandmarkListLater(eventArgs.value);

			if (eventArgs.value == null) {
				return;
			}
			
			Groups.HandPoints handPoints = new();

			int count = eventArgs.value.Landmark.Count;
			for (int i = 0; i < count; i++) {
				handPoints.Data[i] = ConvertPoint(eventArgs.value, i);
			}

			this.handPoints = handPoints;
			OnRightHandLandmarks(handPoints);
		}

		[SerializeField] private HandGroup handGroup;
		private Groups.HandPoints handPoints = new();

		private void OnLeftHandLandmarks(Groups.HandPoints points) {
			Groups.HandRotation handGroup = new();
			Quaternion preHand = Quaternion.identity;

			{
				Vector3 handUpDir;
				Vector3 handForwardDir;
				{
					Vector3 palm = points.Wrist;
					Vector3 indexFinger = points.IndexFingerMCP;
					Vector3 middleFinger = points.MiddleFingerMCP;
					Vector3 ringFinger = points.RingFingerMCP;
					Vector3 pinkyFinger = points.PinkyMCP;

					// Figure out their position on the eye socket plane
					handUpDir = new Vector3(
						(middleFinger.x - palm.x),
						(middleFinger.y - palm.y),
						(middleFinger.z - palm.z)
					);

					Plane plane = new(palm, indexFinger, pinkyFinger);
					handForwardDir = plane.normal;
					
					Quaternion rotTest = Quaternion.Inverse(Quaternion.LookRotation(handForwardDir, handUpDir));
					HandPoints.ComputeFinger2(rotTest, -1, 3,
						indexFinger,
						points.IndexFingerPIP,
						points.IndexFingerDIP,
						points.IndexFingerTIP,
						vectors,
						out handGroup.IndexFingerPIP, out handGroup.IndexFingerDIP, out handGroup.IndexFingerTIP);
					HandPoints.ComputeFinger2(rotTest, -1, 2,
						middleFinger,
						points.MiddleFingerPIP,
						points.MiddleFingerDIP,
						points.MiddleFingerTIP,
						vectors,
						out handGroup.MiddleFingerPIP, out handGroup.MiddleFingerDIP, out handGroup.MiddleFingerTIP);
					HandPoints.ComputeFinger2(rotTest, -1, 1,
						ringFinger,
						points.RingFingerPIP,
						points.RingFingerDIP,
						points.RingFingerTIP,
						vectors,
						out handGroup.RingFingerPIP, out handGroup.RingFingerDIP, out handGroup.RingFingerTIP);
					HandPoints.ComputeFinger2(rotTest, -1, 0,
						pinkyFinger,
						points.PinkyPIP,
						points.PinkyDIP,
						points.PinkyTIP,
						vectors,
						out handGroup.PinkyPIP, out handGroup.PinkyDIP, out handGroup.PinkyTIP);
					HandPoints.ComputeThumb(rotTest, -1, 4,
						points.Wrist,
						points.ThumbMCP,
						points.ThumbIP,
						points.ThumbTIP,
						vectors,
						out handGroup.ThumbMCP, out handGroup.ThumbIP, out handGroup.ThumbTIP);
				}

				Quaternion test = Quaternion.Euler(0, 90, 90);
				preHand = Quaternion.LookRotation(handForwardDir, -handUpDir);
				Quaternion rot = preHand * test;
				handGroup.Wrist = rot;
			}

			float time = TimeNow;
			lHand.Set(handGroup.Wrist, time);
			lIndexPip.Set(handGroup.IndexFingerPIP, time);
			lIndexDip.Set(handGroup.IndexFingerDIP, time);
			lIndexTip.Set(handGroup.IndexFingerTIP, time);
			lMiddlePip.Set(handGroup.MiddleFingerPIP, time);
			lMiddleDip.Set(handGroup.MiddleFingerDIP, time);
			lMiddleTip.Set(handGroup.MiddleFingerTIP, time);
			lRingPip.Set(handGroup.RingFingerPIP, time);
			lRingDip.Set(handGroup.RingFingerDIP, time);
			lRingTip.Set(handGroup.RingFingerTIP, time);
			lPinkyPip.Set(handGroup.PinkyPIP, time);
			lPinkyDip.Set(handGroup.PinkyDIP, time);
			lPinkyTip.Set(handGroup.PinkyTIP, time);
			lThumbPip.Set(handGroup.ThumbMCP, time);
			lThumbDip.Set(handGroup.ThumbIP, time);
			lThumbTip.Set(handGroup.ThumbTIP, time);

			// TODO: Show unrotated hand so that it's fixed in space
			// TODO: Show fingers and calculate correct rotation
			{
				Quaternion rev = Quaternion.Inverse(preHand);
				Vector3 wrist = points.Wrist;
				for (int i = 0; i < points.Data.Length; i++) {
					Vector3 vec = points.Data[i] - wrist;
					vec = -vec;
					if (fixHand) {
						vec = rev * vec;
					}
					vec.x *= handPositionScale.x;
					vec.y *= handPositionScale.y;
					vec.z *= handPositionScale.z;
					vec += handPositionOffset;
					vectors[i] = vec;
				}
				vectorsSize = points.Data.Length + 20;
			}
		}

		public Vector3 handPositionOffset = Vector3.zero;
		public Vector3 handPositionScale = Vector3.one;

		private void OnRightHandLandmarks(Groups.HandPoints points) {
			Groups.HandRotation handGroup = new();

			Vector3 handUpDir;
			Vector3 handForwardDir;
			{
				Vector3 palm = points.Wrist;
				Vector3 indexFinger = points.IndexFingerMCP;
				Vector3 middleFinger = points.MiddleFingerMCP;
				Vector3 ringFinger = points.RingFingerMCP;
				Vector3 pinkyFinger = points.PinkyMCP;

				// Figure out their position on the eye socket plane
				handUpDir = new Vector3(
					(middleFinger.x - palm.x),
					(middleFinger.y - palm.y),
					(middleFinger.z - palm.z)
				);

				Plane plane = new(palm, indexFinger, pinkyFinger);
				handForwardDir = plane.normal;
					
				Quaternion rotTest = Quaternion.Inverse(Quaternion.LookRotation(handForwardDir, handUpDir));
				HandPoints.ComputeFinger2(rotTest, 1, 3,
					indexFinger,
					points.IndexFingerPIP,
					points.IndexFingerDIP,
					points.IndexFingerTIP,
					null,
					out handGroup.IndexFingerPIP, out handGroup.IndexFingerDIP, out handGroup.IndexFingerTIP);
				HandPoints.ComputeFinger2(rotTest, 1, 2,
					middleFinger,
					points.MiddleFingerPIP,
					points.MiddleFingerDIP,
					points.MiddleFingerTIP,
					null,
					out handGroup.MiddleFingerPIP, out handGroup.MiddleFingerDIP, out handGroup.MiddleFingerTIP);
				HandPoints.ComputeFinger2(rotTest, 1, 1,
					ringFinger,
					points.RingFingerPIP,
					points.RingFingerDIP,
					points.RingFingerTIP,
					null,
					out handGroup.RingFingerPIP, out handGroup.RingFingerDIP, out handGroup.RingFingerTIP);
				HandPoints.ComputeFinger2(rotTest, 1, 0,
					pinkyFinger,
					points.PinkyPIP,
					points.PinkyDIP,
					points.PinkyTIP,
					null,
					out handGroup.PinkyPIP, out handGroup.PinkyDIP, out handGroup.PinkyTIP);
				HandPoints.ComputeThumb(rotTest, 1, 4,
					points.Wrist,
					points.ThumbMCP,
					points.ThumbIP,
					points.ThumbTIP,
					null,
					out handGroup.ThumbMCP, out handGroup.ThumbIP, out handGroup.ThumbTIP);
			}

			Quaternion test = Quaternion.Euler(0, 90, -90);
			Quaternion rot = Quaternion.LookRotation(handForwardDir, -handUpDir);
			handGroup.Wrist = rot * test;
			
			float time = TimeNow;
			rHand.Set(handGroup.Wrist, time);
			rIndexPip.Set(handGroup.IndexFingerPIP, time);
			rIndexDip.Set(handGroup.IndexFingerDIP, time);
			rIndexTip.Set(handGroup.IndexFingerTIP, time);
			rMiddlePip.Set(handGroup.MiddleFingerPIP, time);
			rMiddleDip.Set(handGroup.MiddleFingerDIP, time);
			rMiddleTip.Set(handGroup.MiddleFingerTIP, time);
			rRingPip.Set(handGroup.RingFingerPIP, time);
			rRingDip.Set(handGroup.RingFingerDIP, time);
			rRingTip.Set(handGroup.RingFingerTIP, time);
			rPinkyPip.Set(handGroup.PinkyPIP, time);
			rPinkyDip.Set(handGroup.PinkyDIP, time);
			rPinkyTip.Set(handGroup.PinkyTIP, time);
			rThumbPip.Set(handGroup.ThumbMCP, time);
			rThumbDip.Set(handGroup.ThumbIP, time);
			rThumbTip.Set(handGroup.ThumbTIP, time);
		}

		private void OnPoseWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs) {
			_poseWorldLandmarksAnnotationController.DrawLater(eventArgs.value);
			
			if (eventArgs.value == null) {
				return;
			}

			Quaternion chestRotation = Quaternion.identity;
			Quaternion hipsRotation = Quaternion.identity;
			Vector3 hipsPosition = Vector3.zero;
			Quaternion rUpperArm = Quaternion.identity;
			Quaternion rLowerArm = Quaternion.identity;
			Quaternion lUpperArm = Quaternion.identity;
			Quaternion lLowerArm = Quaternion.identity;

			float handExtraPercentage = 0.2f;
			try {
				Vector3 rShoulder = ConvertPoint(eventArgs.value, 11);
				Vector3 lShoulder = ConvertPoint(eventArgs.value, 12);

				float bodyRotation = 1.0f;
				{
					Vector3 vRigA = Vector3.left;
					Vector3 vRigB = rShoulder - lShoulder;
					Quaternion rot = Quaternion.FromToRotation(vRigA, vRigB);
					chestRotation = rot;
					hipsRotation = rot;
					hipsPosition = new Vector3(
						-(rShoulder.x + lShoulder.x) * 0.5f * 2,
						-(rShoulder.z + lShoulder.z) * 0.5f,
						(rShoulder.y + lShoulder.y) * 0.5f + 1.0f
					);
					bodyRotation = Mathf.Abs(Mathf.Cos(rot.eulerAngles.y * 1.6f));
				}
				float hep = handExtraPercentage * bodyRotation;

				{
					Vector3 rElbow = ConvertPoint(eventArgs.value, 13);
					Vector3 rHand = ConvertPoint(eventArgs.value, 15);
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
					Vector3 lElbow = ConvertPoint(eventArgs.value, 14);
					Vector3 lHand = ConvertPoint(eventArgs.value, 16);
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

			// TODO: Do all of these one update late
			// So that everything can be smoothly synched between each timestep of the AI :D
			this.chestRotation.Set(chestRotation, TimeNow);
			//this.hipsPosition.Set(hipsPosition, TimeNow);
			this.hipsRotation.Set(hipsRotation, TimeNow);
			this.rUpperArm.Set(rUpperArm, TimeNow);
			this.rLowerArm.Set(rLowerArm, TimeNow);
			this.lUpperArm.Set(lUpperArm, TimeNow);
			this.lLowerArm.Set(lLowerArm, TimeNow);
		}

		void FixedUpdate() {
			if (handGroup != null && handPoints != null) {
				handGroup.Apply(handPoints, animator.GetBoneTransform(HumanBodyBones.LeftHand).transform.position, 0.5f);
			}

			if (!vrmModel.activeInHierarchy) {
				return;
			}

			TestInterpolationStatic = TestInterpolation;
			TestInterpolationValue = InterpolationValue;
			float time = TimeNow;
			//animator.GetBoneTransform(HumanBodyBones.Head).Rotate(Vector3.up, 0.5f);

			// Apply the model transform
			vrmModel.transform.position = guiScript.GetModelTransform();

			// All transformations are inverted from left to right because the VMR
			// models do not allow for mirroring.
			chestRotation.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.Chest), time);
			rUpperArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightUpperArm), time);
			rLowerArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), time);
			lUpperArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), time);
			lLowerArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), time);
			neckRotation.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.Neck), time);

			lHand.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.          RightHand), time);
			lIndexPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightIndexProximal), time);
			lIndexDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightIndexIntermediate), time);
			lIndexTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightIndexDistal), time);
			lMiddlePip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal), time);
			lMiddleDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate), time);
			lMiddleTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal), time);
			lRingPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.  RightRingProximal), time);
			lRingDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.  RightRingIntermediate), time);
			lRingTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.  RightRingDistal), time);
			lPinkyPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightLittleProximal), time);
			lPinkyDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightLittleIntermediate), time);
			lPinkyTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightLittleDistal), time);
			lThumbPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightThumbProximal), time);
			lThumbDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightThumbIntermediate), time);
			lThumbTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. RightThumbDistal), time);
			
			rHand.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.          LeftHand), time);
			rIndexPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftIndexProximal), time);
			rIndexDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftIndexIntermediate), time);
			rIndexTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftIndexDistal), time);
			rMiddlePip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal), time);
			rMiddleDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate), time);
			rMiddleTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal), time);
			rRingPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.  LeftRingProximal), time);
			rRingDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.  LeftRingIntermediate), time);
			rRingTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones.  LeftRingDistal), time);
			rPinkyPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftLittleProximal), time);
			rPinkyDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftLittleIntermediate), time);
			rPinkyTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftLittleDistal), time);
			rThumbPip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftThumbProximal), time);
			rThumbDip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftThumbIntermediate), time);
			rThumbTip.UpdateLocalRotation(animator.GetBoneTransform(HumanBodyBones. LeftThumbDistal), time);

			blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.O), mouthOpen);

			float rEyeTest = blendShapeProxy.GetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_L));
			float lEyeTest = blendShapeProxy.GetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_R));
			float rEyeValue = (rEyeOpen.Max() < FaceConfig.EAR_TRESHHOLD) ? 1 : 0;
			float lEyeValue = (lEyeOpen.Max() < FaceConfig.EAR_TRESHHOLD) ? 1 : 0;
			rEyeValue = (rEyeValue + rEyeTest * 2) / 3.0f;
			lEyeValue = (lEyeValue + lEyeTest * 2) / 3.0f;

			blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_L), rEyeValue);
			blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_R), lEyeValue);

			// TODO: Update this code to make it more correct
			//animator.GetBoneTransform(HumanBodyBones.Neck).transform.rotation = Quaternion.identity;
			animator.GetBoneTransform(HumanBodyBones.LeftEye).transform.localRotation = Quaternion.Euler(
				(rEyeIris.Average().y - 0.14f) * -30,
				rEyeIris.Average().x * -30,
				0
			);
			animator.GetBoneTransform(HumanBodyBones.RightEye).transform.localRotation = Quaternion.Euler(
				(lEyeIris.Average().y - 0.14f) * -30,
				lEyeIris.Average().x * -30,
				0
			);

			// Debug.Log(rEyeIris.Average());

			sphereContainer.UpdatePoints(vectors, vectorsSize);
			for (int i = 0; i < vectorsSize; i++) sphereContainer.HighlightPoint(i);
			sphereContainer.Connect(0, 1);
			sphereContainer.Connect(0, 5);
			sphereContainer.Connect(0, 17);
			sphereContainer.Connect(5, 9);
			sphereContainer.Connect(9, 13);
			sphereContainer.Connect(13, 17);
			for (int i = 0; i < 3; i++) {
				sphereContainer.Connect(1 + i, 2 + i);
				sphereContainer.Connect(5 + i, 6 + i);
				sphereContainer.Connect(9 + i, 10 + i);
				sphereContainer.Connect(13 + i, 14 + i);
				sphereContainer.Connect(17 + i, 18 + i);

				
				sphereContainer.Connect(21 + i, 22 + i);
				sphereContainer.Connect(25 + i, 26 + i);
				sphereContainer.Connect(29 + i, 30 + i);
				sphereContainer.Connect(33 + i, 34 + i);
				sphereContainer.Connect(37 + i, 38 + i);
			}
			

			// sphereContainer.HighlightPoints(FacePoints.LeftEye);
			// sphereContainer.HighlightPoints(FacePoints.RightEye);
		}
	}
}
