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

		protected void AddDivider(Lang data) {
			GameObject empty = Instantiate(templateObject);
			empty.transform.localScale = Vector3.one;
			empty.transform.SetParent(contentTransform, false);
			empty.GetComponent<SettingsFieldTemplate>().BuildDivider(data, 24);
			empty.SetActive(true);
		}

		protected SettingsField CreateSetting(Lang data, Func<SettingsFieldTemplate, SettingsFieldTemplate> builder) {
			GameObject empty = Instantiate(templateObject);
			empty.transform.localScale = Vector3.one;
			empty.transform.SetParent(contentTransform, false);
			SettingsField field = builder.Invoke(empty.GetComponent<SettingsFieldTemplate>()).Build(data);
			empty.SetActive(true);
			return field;
		}
	}
}
