using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class HolisticLandmarks {
		private readonly static HolisticLandmarks _empty = new(null, null);
		public static HolisticLandmarks NotPresent => _empty;

		/// <summary>
		/// Indexer for landmarks in this list
		/// </summary>
		public Vector4 this[int i] => m_list[i];

		/// <summary>
		/// The amount of landmarks in this list
		/// </summary>
		public int Count => m_list.Count;

		/// <summary>
		/// If this landmark has data
		/// </summary>
		public bool IsPresent => m_list != null;

		private readonly List<Vector4> m_list;
		private readonly List<Vector4> m_raw;

		public HolisticLandmarks(List<Vector4> list, List<Vector4> raw) {
			m_list = list;
			m_raw = raw;
		}

		public Vector4 GetRaw(int index) {
			return m_raw[index];
		}
	}
}
