using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class FlagScript : MonoBehaviour {
		// List of images
		public Image[] images;

		public enum Flag {
			None,
			Mlm,
			Lesbian,
			Lgbt,
			Trans,
			Ace,
			Nb,
			Pan
		};

		// In the same order as flag
		public Sprite[] flags;

		private Color hidden = new(1, 1, 1, 0);
		private Color visible = new(1, 1, 1, 0.25f);
		private Flag last = Flag.None;

		void Update() {
			if (Settings.Flag != last) {
				last = Settings.Flag;

				bool none = last == Flag.None;
				Sprite sprite = flags[(int) last];

				foreach (Image image in images) {
					image.color = none ? hidden : visible;
					image.sprite = sprite;
				}
			}
		}
	}
}
