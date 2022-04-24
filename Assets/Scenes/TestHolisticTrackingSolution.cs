using Mediapipe;
using Mediapipe.Unity;
using System;
using System.Collections;
using UnityEngine;
using VRM;

namespace HardCoded.VRigUnity {
	public class TestHolisticTrackingSolution : HolisticSolutionBase {
		[Header("Rig")]
		[SerializeField] protected VRMBlendShapeProxy blendShapeProxy;
		[SerializeField] protected Animator animator;

		[Header("Debug")]
		public GameObject sphereMesh;
		public Transform sphereParent;


		private Vector3[] vectors = new Vector3[200];
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
		private RotStruct lHand = RotStruct.identity;
		
		private readonly long StartTicks = DateTime.Now.Ticks;
		private float TimeNow => (float)((DateTime.Now.Ticks - StartTicks) / 1000000000.0);

		protected override void OnStartRun() {
			graphRunner.OnPoseDetectionOutput += OnPoseDetectionOutput;
			graphRunner.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
			graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
			graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
			graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
			graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
			graphRunner.OnPoseRoiOutput += OnPoseRoiOutput;

			var imageSource = ImageSourceProvider.ImageSource;
			SetupAnnotationController(_poseDetectionAnnotationController, imageSource);
			SetupAnnotationController(_holisticAnnotationController, imageSource);
			SetupAnnotationController(_poseWorldLandmarksAnnotationController, imageSource);
			SetupAnnotationController(_poseRoiAnnotationController, imageSource);
		}

		private Vector3 ConvertPoint(LandmarkList list, int idx) {
			Landmark mark = list.Landmark[idx];
			return new Vector3(-mark.X, mark.Y, mark.Z);
		}

		private Vector3 ConvertPoint(NormalizedLandmarkList list, int idx) {
			NormalizedLandmark mark = list.Landmark[idx];
			return new Vector3(mark.X, mark.Y, mark.Z);
		}

		private void OnPoseDetectionOutput(object stream, OutputEventArgs<Detection> eventArgs) {
			_poseDetectionAnnotationController.DrawLater(eventArgs.value);

		}

		private void OnFaceLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			_holisticAnnotationController.DrawFaceLandmarkListLater(eventArgs.value);

			Quaternion neckRotation = Quaternion.identity;
			
			try {
				Vector3 faceUpDir;
				{
					Vector3 botHead = ConvertPoint(eventArgs.value, 152);
					Vector3 topHead = ConvertPoint(eventArgs.value, 10);

					// Figure out their position on the eye socket plane
					faceUpDir = new Vector3(
						-(topHead.x - botHead.x),
						-(topHead.y - botHead.y),
						-(topHead.z - botHead.z)
					);
				}

				Quaternion rot = Quaternion.FromToRotation(Vector3.up, faceUpDir);
				neckRotation = rot;
			} catch {

			}

			this.neckRotation.Set(neckRotation, TimeNow);
		}

		private void OnPoseLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			_holisticAnnotationController.DrawPoseLandmarkListLater(eventArgs.value);

		}

		private void OnLeftHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			_holisticAnnotationController.DrawLeftHandLandmarkListLater(eventArgs.value);

			Quaternion lHand = Quaternion.identity;

			try {
				Vector3 handUpDir;
				{
					Vector3 palm = ConvertPoint(eventArgs.value, 0);
					Vector3 middleFinger = ConvertPoint(eventArgs.value, 5);

					// Figure out their position on the eye socket plane
					handUpDir = new Vector3(
						(middleFinger.x - palm.x),
						(middleFinger.y - palm.y),
						(middleFinger.z - palm.z)
					);
				}

				Quaternion rot = Quaternion.FromToRotation(Vector3.left, handUpDir);
				lHand = rot;
			} catch {

			}

			this.lHand.Set(lHand, TimeNow);
		}

		private void OnRightHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
			_holisticAnnotationController.DrawRightHandLandmarkListLater(eventArgs.value);

			Quaternion rHand = Quaternion.identity;

			try {
				Vector3 handUpDir;
				{
					Vector3 palm = ConvertPoint(eventArgs.value, 0);
					Vector3 middleFinger = ConvertPoint(eventArgs.value, 5);

					// Figure out their position on the eye socket plane
					handUpDir = new Vector3(
						(middleFinger.x - palm.x),
						(middleFinger.y - palm.y),
						(middleFinger.z - palm.z)
					);
				}

				Quaternion rot = Quaternion.FromToRotation(Vector3.right, handUpDir);
				rHand = rot;
			} catch {

			}

			this.rHand.Set(rHand, TimeNow);
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

			public void UpdateRotation(Transform transform, float time) {
				switch (TestInterpolationStatic) {
					default: {
						transform.rotation = Quaternion.Lerp(transform.rotation, curr, TestInterpolationValue);
						break;
					}
					case 1: {
						float timeLength = currTime - lastTime;
						float delta = (time - currTime) / timeLength;
						transform.rotation = Quaternion.Lerp(transform.rotation, curr, delta);
						break;
					}
					case 2: {
						transform.rotation = curr;
						break;
					}
				}
			}
		}


		private void OnPoseWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs) {
			_poseWorldLandmarksAnnotationController.DrawLater(eventArgs.value);
			
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

				for (int i = 0; i < eventArgs.value.Landmark.Count; i++) {
					vectors[i] = ConvertPoint(eventArgs.value, i);
				}

				vectorsSize = eventArgs.value.Landmark.Count;
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

		private void OnPoseRoiOutput(object stream, OutputEventArgs<NormalizedRect> eventArgs) {
			_poseRoiAnnotationController.DrawLater(eventArgs.value);

		}

		void FixedUpdate() {
			TestInterpolationStatic = TestInterpolation;
			TestInterpolationValue = InterpolationValue;
			float time = TimeNow;
			//animator.GetBoneTransform(HumanBodyBones.Head).Rotate(Vector3.up, 0.5f);

			chestRotation.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.Chest), time);
			rUpperArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightUpperArm), time);
			rLowerArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), time);
			lUpperArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), time);
			lLowerArm.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), time);
			neckRotation.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.Neck), time);
			lHand.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.RightHand), time);
			rHand.UpdateRotation(animator.GetBoneTransform(HumanBodyBones.LeftHand), time);

			if (sphereParent.childCount < vectorsSize) {
				for (int i = sphereParent.childCount; i < vectorsSize; i++) {
					GameObject obj = GameObject.Instantiate(sphereMesh, sphereParent);
					obj.SetActive(true);
				}
			} else {
				for (int i = 0; i < vectorsSize; i++) {
					sphereParent.GetChild(i).localPosition = vectors[i];
				}
			}
		}
	}
}
