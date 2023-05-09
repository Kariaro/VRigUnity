using System;
using UnityEngine;

namespace HardCoded.VRigUnity.Visuals {
	public class PointListAnnotation : VisualizationBase {
		private MeshRenderer[] array = new MeshRenderer[0];
		private GameObject container;
		
		protected override void Setup() {
			
		}

		public void Clear() {
			foreach (var item in array) {
				if (item != null) {
					Destroy(item.gameObject);
				}
			}
			array = new MeshRenderer[0];
		}

		private MeshRenderer GetRenderer(int index) {
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

		private MeshRenderer CreateNew() {
			if (container == null) {
				container = new("PointListAnnotation");
				container.transform.SetParent(transform, false);
			}

			GameObject child = new("Point");
			child.transform.SetParent(container.transform, false);
			child.SetActive(false);

			var rend = child.AddComponent<MeshRenderer>();
			child.AddComponent<MeshFilter>().mesh = parent.pointMesh;

			return rend;
		}

		/// <summary>
		/// Add an index to the point list
		/// </summary>
		public void AddIndex(int index, Material material, Color color, float radius) {
			var rend = GetRenderer(index);
			rend.material = material;
			rend.material.color = color;
			rend.transform.localScale = new(radius, radius, radius);
		}

		public void Set(int index, Vector3 point) {
			if (index >= array.Length) {
				return;
			}

			var rend = array[index];
			if (rend != null) {
				rend.transform.localPosition = point;
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
