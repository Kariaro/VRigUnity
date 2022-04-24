using HardCoded.VRigUnity;
using Mediapipe;
using Mediapipe.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRM;

public class RigScript : MonoBehaviour {
	[SerializeField] VRMBlendShapeProxy blendShapeProxy;
	[SerializeField] Animator animator;
	[SerializeField] TestHolisticTrackingGraph trackingGraph;
	

	// Start is called before the first frame update
	void Start() {
		trackingGraph.OnPoseDetectionOutput += OnPoseDetectionOutput;
		trackingGraph.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
		trackingGraph.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
		trackingGraph.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
		trackingGraph.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
		trackingGraph.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
		trackingGraph.OnPoseRoiOutput += OnPoseRoiOutput;
	}

	// Update is called once per frame
	void Update() {
		animator.GetBoneTransform(HumanBodyBones.Head).Rotate(Vector3.up, 0.5f);
	}

	private void OnPoseDetectionOutput(object stream, OutputEventArgs<Detection> eventArgs) {
		
	}

	private void OnFaceLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
		
	}

	private void OnPoseLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
		Debug.Log("Pose Landmarks");
	}

	private void OnLeftHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
		
	}

	private void OnRightHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs) {
		
	}

	private void OnPoseWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs) {
		
	}

	private void OnPoseRoiOutput(object stream, OutputEventArgs<NormalizedRect> eventArgs) {
		
	}
}
