using UnityEngine;

namespace HardCoded.VRigUnity {
	/// <summary>
	/// This class makes it easier to create perfect time based interpolation
	/// by storing the landmarks and annotating if they have been used
	/// </summary>
	public class LandmarkCallback : MonoBehaviour, IHolisticCallback {
		public const int DIRTY_FACE = 1,
						 DIRTY_LEFT_HAND = 2,
						 DIRTY_RIGHT_HAND = 4,
						 DIRTY_POSE = 8,
						 DIRTY_POSE_WORLD = 16;
		private int dirtyFlags;
		private HolisticLandmarks faceLandmarks      = HolisticLandmarks.NotPresent;
		private HolisticLandmarks leftHandLandmarks  = HolisticLandmarks.NotPresent;
		private HolisticLandmarks rightHandLandmarks = HolisticLandmarks.NotPresent;
		private HolisticLandmarks poseLandmarks      = HolisticLandmarks.NotPresent;
		private HolisticLandmarks poseWorldLandmarks = HolisticLandmarks.NotPresent;

		public delegate void UpdateEventHandler(HolisticLandmarks face,
			HolisticLandmarks leftHand,
			HolisticLandmarks rightHand,
			HolisticLandmarks pose,
			HolisticLandmarks poseWorld,
			int dirtyFlags);

		public event UpdateEventHandler OnUpdateEvent;

		public void ClearData() {
			dirtyFlags = 0;
			OnUpdateEvent = null;
			faceLandmarks      = HolisticLandmarks.NotPresent;
			leftHandLandmarks  = HolisticLandmarks.NotPresent;
			rightHandLandmarks = HolisticLandmarks.NotPresent;
			poseLandmarks      = HolisticLandmarks.NotPresent;
			poseWorldLandmarks = HolisticLandmarks.NotPresent;
		}

		public void OnFaceLandmarks(HolisticLandmarks landmarks) {
			faceLandmarks = landmarks;
			dirtyFlags |= DIRTY_FACE;
		}

		public void OnLeftHandLandmarks(HolisticLandmarks landmarks) {
			leftHandLandmarks = landmarks;
			dirtyFlags |= DIRTY_LEFT_HAND;
		}

		public void OnRightHandLandmarks(HolisticLandmarks landmarks) {
			rightHandLandmarks = landmarks;
			dirtyFlags |= DIRTY_RIGHT_HAND;
		}

		public void OnPoseLandmarks(HolisticLandmarks landmarks) {
			poseLandmarks = landmarks;
			dirtyFlags |= DIRTY_POSE;
		}

		public void OnPoseWorldLandmarks(HolisticLandmarks landmarks) {
			poseWorldLandmarks = landmarks;
			dirtyFlags |= DIRTY_POSE_WORLD;
		}

		public void FixedUpdate() {
			// Technically this could cause new values to be marked as
			// non dirty but this would not affect how the model moves
			OnUpdateEvent?.Invoke(
				faceLandmarks,
				leftHandLandmarks,
				rightHandLandmarks,
				poseLandmarks,
				poseWorldLandmarks,
				dirtyFlags);
			dirtyFlags = 0;
		}
	}
}
