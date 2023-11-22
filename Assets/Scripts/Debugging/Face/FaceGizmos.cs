using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class FaceGizmos : MonoBehaviour {
		[SerializeField] private GameObject sphere;
		[SerializeField] private Color color = Color.white;
		[SerializeField] private Color highlight = Color.yellow;
		[SerializeField] private Color greenlight = Color.green;
		[SerializeField] private float radius = 15;
		[SerializeField] private List<int> greenlightList;
		private int selectedIndex = -1;
		private FacePoint[] points;
		private HolisticDebugSolution solution;

		private class FacePoint : MonoBehaviour {
			private Renderer rend;
			public FaceGizmos gizmos;
			public int index;

			void OnMouseDown() {
				Debug.Log(index);
				gizmos.selectedIndex = index;
			}

			public void UpdateParameters(Color color, float radius) {
				if (rend == null) {
					rend = GetComponent<Renderer>();
				}

				rend.material.color = color;
				transform.localScale = new(radius / 100.0f, radius / 100.0f, radius / 100.0f);
			}
		}

		void Start() {
			solution = SolutionUtils.GetSolution() as HolisticDebugSolution;
			points = new FacePoint[478];

			for (int i = 0; i < points.Length; i++) {
				GameObject obj = Instantiate(sphere);
				obj.transform.SetParent(transform);
				FacePoint point = obj.AddComponent<FacePoint>();
				points[i] = point;
				point.gizmos = this;
				point.index = i;
			}
		}

		void Update() {
			for (int i = 0; i < points.Length; i++) {
				Color col = i == selectedIndex ? highlight : (greenlightList.Contains(i) ? greenlight : color);
				points[i].UpdateParameters(col, radius);
			}

			var facePoints = solution.facePoints;
			if (facePoints != null) {
				int count = facePoints.Count;

				Vector3 average = Vector3.zero;
				for (int i = 0; i < count; i++) {
					average += (Vector3) facePoints[i];
				}
				average /= count;

				Camera mainCamera = Camera.main;
				for (int i = 0; i < points.Length; i++) {
					var point = points[i].gameObject;
					if (i > count) {
						point.SetActive(false);
					} else {
						Vector3 item = facePoints[i];
						point.SetActive(true);
						point.transform.localPosition = average - item;
						point.transform.LookAt(mainCamera.transform.position, mainCamera.transform.up);
					}
				}
			}
		}
	}
}