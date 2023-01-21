using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity.Theme {
	public class TabTheme : ThemeSkin {
		[SerializeField] private Image outline;
		[SerializeField] private Image color;

		public override void Apply(GUITheme theme) {
			color.color = theme.colorB;
			outline.color = theme.colorA;
		}
	}
}
