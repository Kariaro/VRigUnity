using UnityEngine;
using System;

namespace HardCoded.VRigUnity {
	public class CustomLocalization : MonoBehaviour {
		private Action action;
		
		public void Init(Action action) {
			this.action = action;
		}

		void Start() {
			Localization.OnLocalizationChangeEvent += UpdateLanguage;
		}

		void OnEnable() {
			UpdateLanguage();
		}

		private void UpdateLanguage() {
			action.Invoke();
		}
	}
}
