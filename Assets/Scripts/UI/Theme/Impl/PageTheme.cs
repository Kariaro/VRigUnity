using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity.Theme {
	public class PageTheme : ThemeSkin {
		[SerializeField] private Image color;
		[SerializeField] private Image[] footers;

		public override void Apply(GUITheme theme) {
			color.color = theme.colorA;
			foreach (var footer in footers) {
				footer.color = theme.colorB;
			}
		}
	}
}
