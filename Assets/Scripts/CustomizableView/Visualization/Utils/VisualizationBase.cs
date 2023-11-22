using UnityEngine;

namespace HardCoded.VRigUnity.Visuals {
	public abstract class VisualizationBase : MonoBehaviour {
		private Visualization _parent;
		protected Visualization parent {
			get {
				if (_parent == null) {
					_parent = gameObject.GetComponentInParent<Visualization>(true);
				}
				return _parent;
			}
		}

		private bool m_prepared;

		/// <summary>
		/// Returns if this class is prepared to receive data
		/// </summary>
		protected bool IsPrepared => m_prepared;

		protected void Start() {
			Setup();
			m_prepared = true;
		}

		protected abstract void Setup();
	}
}
