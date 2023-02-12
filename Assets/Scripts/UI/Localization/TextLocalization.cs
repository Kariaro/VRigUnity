using UnityEngine;
using TMPro;

namespace HardCoded.VRigUnity {
	public class TextLocalization : MonoBehaviour {
		[SerializeField] private string id;
		[SerializeField] private TMP_Text text;
		private Lang data;

		public void Init(Lang data, TMP_Text text) {
			id = data.id;
			this.text = text;
			this.data = data;
		}

		void Start() {
			Localization.OnLocalizationChangeEvent += UpdateLanguage;
		}

		void OnEnable() {
			UpdateLanguage();
		}

		private void UpdateLanguage() {
			if (data == null) {
				data = Lang.FromId(id);
			}
			text.text = data.Get();
		}
	}
}
