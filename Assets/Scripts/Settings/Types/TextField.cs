using System;
using UnityEngine;

namespace HardCoded.VRigUnity.SettingsTypes {
	public class SafeText : Text {
		public SafeText(string key, string def, Action<string> callback = null) : base(key, def, callback) {}
		public override object RawValue() {
			return new string('*', Get().Length);
		}
	}

	public class Text : Field<string> {
		private string m_value;
		public Text(string key, string def, Action<string> callback = null) : base(key, def, callback) {}
			
		public override void Reset() {
			Set(m_def);
		}

		public override void Init() {
			m_value = PlayerPrefs.GetString(m_key, m_def);
		}

		public override string Get() {
			return m_value;
		}

		public override void Set(string value) {
			m_value = value;
			PlayerPrefs.SetString(m_key, value);
			m_callback?.Invoke(value);
		}
	}
}
