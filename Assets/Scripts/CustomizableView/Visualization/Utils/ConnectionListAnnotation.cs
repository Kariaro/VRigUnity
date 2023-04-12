using System;
using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity.Visuals {
	public class ConnectionListAnnotation : VisualizationBase {
		private LineRenderer[] array = new LineRenderer[0];
		private GameObject container;

		public struct Line {
			public int index;
			public int start;
			public int end;
		}

		public static List<Line> CreateConnections(List<(int, int)> connections, ref int offset) {
			List<Line> lines = new();
			for (int i = 0; i < connections.Count; i++) {
				(int start, int end) = connections[i];
				lines.Add(new Line { index = offset++, start = start, end = end });
			}
			return lines;
		}

		protected override void Setup() {
			
		}

		public void Clear() {
			foreach (var item in array) {
				if (item != null) {
					Destroy(item.gameObject);
				}
			}
			array = new LineRenderer[0];
		}

		private LineRenderer GetRenderer(int index) {
			if (index >= array.Length) {
				Array.Resize(ref array, index + 1);
			}

			var rend = array[index];
			if (rend == null) {
				rend = CreateNew();
				array[index] = rend;
			}

			return rend;
		}

		private LineRenderer CreateNew() {
			if (container == null) {
				container = new("ConnectionListAnnotation");
				container.transform.SetParent(transform, false);
			}

			GameObject child = new("Line");
			child.transform.SetParent(container.transform, false);
			child.SetActive(false);

			var rend = child.AddComponent<LineRenderer>();
			rend.positionCount = 2;
			rend.sortingOrder = 1;
			rend.useWorldSpace = false;

			return rend;
		}

		/// <summary>
		/// Add an index to the connection list
		/// </summary>
		public void AddIndex(int index, Material material, Color color, float width) {
			var rend = GetRenderer(index);
			rend.material = material;
			rend.startColor = color;
			rend.endColor = color;
			rend.startWidth = width;
			rend.endWidth = width;
		}

		public void Set(int index, Vector3 start, Vector3 end) {
			if (index >= array.Length) {
				return;
			}

			var rend = array[index];
			if (rend != null) {
				rend.SetPosition(0, start);
				rend.SetPosition(1, end);
				rend.gameObject.SetActive(true);
			}
		}

		public void Hide(int index) {
			if (index >= array.Length) {
				return;
			}
			
			var rend = array[index];
			if (rend != null) {
				rend.gameObject.SetActive(false);
			}
		}

		public void HideAll() {
			for (int i = 0; i < array.Length; i++) {
				var item = array[i];
				if (item != null) {
					item.gameObject.SetActive(false);
				}
			}
		}
	}
}
