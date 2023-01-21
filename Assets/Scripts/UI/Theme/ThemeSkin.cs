using UnityEngine;

namespace HardCoded.VRigUnity.Theme {
	public abstract class ThemeSkin : MonoBehaviour {
		public virtual void OnEnable() {
			// TODO: Find gui theme and notify about doing an update
		}

		/// <summary>
		/// Update this component from a selected theme
		/// </summary>
		/// <param name="theme"></param>
		public abstract void Apply(GUITheme theme);
	}
}
