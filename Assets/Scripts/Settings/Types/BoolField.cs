using System;
using UnityEngine;

namespace HardCoded.VRigUnity.SettingsTypes {
	public class SafeBool : Bool {
		public SafeBool(string key, bool def, Action<bool> callback = null) : base(key, def, callback) {}
		public override object RawValue() {
			return "*";
		}
	}

	public class Bool : Field<bool> {
		private bool m_value;
		public Bool(string key, bool def, Action<bool> callback = null) : base(key, def, callback) {}
			
		public override void Reset() {
			Set(m_def);
		}

		public override void Init() {
			m_value = PlayerPrefs.GetInt(m_key, m_def ? 1 : 0) != 0;
		}

		public override bool Get() {
			return m_value;
		}

		public override void Set(bool value) {
			m_value = value;
			PlayerPrefs.SetInt(m_key, value ? 1 : 0);
			m_callback?.Invoke(value);
		}
	}
}
