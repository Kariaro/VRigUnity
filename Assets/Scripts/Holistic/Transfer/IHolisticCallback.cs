using Mediapipe;
using Mediapipe.Unity;
using System;
using UnityEngine;
using VRM;

namespace HardCoded.VRigUnity {
	public interface IHolisticCallback {
		void OnFaceLandmarks(HolisticLandmarks landmarks);
		void OnLeftHandLandmarks(HolisticLandmarks landmarks);
		void OnRightHandLandmarks(HolisticLandmarks landmarks);
		void OnPoseLandmarks(HolisticLandmarks landmarks);
		void OnPoseWorldLandmarks(HolisticLandmarks landmarks);
	}
}
