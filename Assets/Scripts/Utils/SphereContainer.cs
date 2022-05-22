using System;
using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class SphereContainer : MonoBehaviour {
		[SerializeField] GameObject sphereMesh;
		[SerializeField] GameObject cubeMesh;
		[SerializeField] Material defaultMat;
		[SerializeField] Material highlightMat;
		[SerializeField] float scale = 0.005f;

		public List<MeshRenderer> pointList = new();
		public List<GameObject> connectionsList = new();

		public void UpdatePoints(Vector3[] points) {
			UpdatePoints(points, points.Length);
		}

		public void UpdatePoints(Vector3[] points, int pointCount) {
			foreach (GameObject obj in connectionsList) {
				Destroy(obj);
			}
			connectionsList.Clear();

			for (int i = pointList.Count; i < pointCount; i++) {
				GameObject obj = GameObject.Instantiate(sphereMesh, transform);
				obj.SetActive(true);
				pointList.Add(obj.GetComponent<MeshRenderer>());
			}

			for (int i = pointCount; i < pointList.Count; i++) {
				pointList[i].gameObject.SetActive(false);
			}

			for (int i = 0; i < pointCount; i++) {
				MeshRenderer rend = pointList[i];
				rend.transform.localPosition = points[i];
				rend.transform.localScale = new(scale, scale, scale);
				rend.material = defaultMat;
			}
		}

		public void HighlightPoints(int[] points) {
			foreach (int index in points) {
				HighlightPoint(index);
			}
		}

		public void HighlightPoint(int index) {
			if (InRange(index)) {
				pointList[index].material = highlightMat;
			}
		}

		public bool InRange(int index) {
			return index >= 0 && index <= pointList.Count;
		}

		public void Connect(int a, int b, bool highlight = false) {
			if (InRange(a) && InRange(b)) {
				GameObject obj = GameObject.Instantiate(cubeMesh, transform);
				obj.SetActive(true);
				connectionsList.Add(obj);
				
				MeshRenderer rend = obj.GetComponent<MeshRenderer>();
				rend.material = highlight ? highlightMat : defaultMat;

				Vector3 p0 = pointList[a].transform.localPosition;
				Vector3 p1 = pointList[b].transform.localPosition;
				float dist = Vector3.Distance(p0, p1);

				Quaternion rot = Quaternion.FromToRotation(Vector3.up, p1 - p0);
				obj.transform.localPosition = (p0 + p1) / 2.0f;
				obj.transform.rotation = rot;
				obj.transform.localScale = new(scale, dist, scale);
			}
		}
	}
}
