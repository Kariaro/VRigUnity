using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class LineHolder : MonoBehaviour {
		[SerializeField] private List<Line> lines = new();
		[SerializeField] private float scale = 1;
		[SerializeField] private Mesh defaultMesh;
		
		struct Line {
			public GameObject line;
			public GameObject g1;
			public GameObject g2;

			public Line(GameObject line, GameObject g1, GameObject g2) {
			   this.line = line;
			   this.g1 = g1;
			   this.g2 = g2;
			}
		}
		
		public void Clear() {
			foreach (Line line in lines) {
				Destroy(line.line);
			}

			lines.Clear();
		}

		private void UpdateLine(Line line) {
			GameObject obj = line.line;
			Vector3 p0 = line.g1.transform.position;
			Vector3 p1 = line.g2.transform.position;

			float dist = Vector3.Distance(p0, p1);
			Quaternion rot = Quaternion.FromToRotation(Vector3.up, p1 - p0);
			obj.transform.position = (p0 + p1) / 2.0f;
			obj.transform.rotation = rot;

			// Remove divide by zero
			Vector3 ls = transform.lossyScale;
			if (ls.x == 0) ls.x = 1;
			if (ls.y == 0) ls.y = 1;
			if (ls.z == 0) ls.z = 1;

			obj.transform.localScale = new(scale / ls.x, dist / ls.y, scale / ls.z);
		}

		public void AddConnection(GameObject g1, GameObject g2) {
			GameObject obj = new();
			obj.transform.parent = transform;
			obj.AddComponent<MeshFilter>().mesh = defaultMesh;
			obj.AddComponent<MeshRenderer>();

			Line line = new(obj, g1, g2);
			UpdateLine(line);
			lines.Add(line);
		}

		void Update() {
			foreach (Line line in lines) {
				UpdateLine(line);
			}
		}
	}
}