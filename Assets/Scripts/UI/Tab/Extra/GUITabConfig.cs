using System;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public abstract class GUITabConfig : GUITab {
		private Transform contentTransform;
		private GameObject templateObject;

		protected virtual void Start() {
			templateObject = gameObject.GetComponentInChildren<SettingsFieldTemplate>(true).gameObject;
			contentTransform = templateObject.transform.parent;
			
			// Child classes should not override start
			InitializeSettings();
		}

		protected abstract void InitializeSettings();

		protected void AddDivider(string name) {
			GameObject empty = Instantiate(templateObject);
			empty.transform.localScale = Vector3.one;
			empty.transform.SetParent(contentTransform, false);
			empty.SetActive(true);
			empty.GetComponent<SettingsFieldTemplate>().BuildDivider(name, 24);
		}

		protected SettingsField CreateSetting(string name, Func<SettingsFieldTemplate, SettingsFieldTemplate> builder) {
			GameObject empty = Instantiate(templateObject);
			empty.transform.localScale = Vector3.one;
			empty.transform.SetParent(contentTransform, false);
			empty.SetActive(true);
			return builder.Invoke(empty.GetComponent<SettingsFieldTemplate>()).Build(name);
		}
	}
}
