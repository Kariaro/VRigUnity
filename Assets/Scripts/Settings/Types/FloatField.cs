using System;
using UnityEngine;

namespace HardCoded.VRigUnity.SettingsTypes {
	public class SafeFloat : Float {
		public SafeFloat(string key, float def, Action<float> callback = null) : base(key, def, callback) {}
		public override object RawValue() {
			return "*";
		}
	}

	public class Float : Field<float> {
		private float m_value;
		public Float(string key, float def, Action<float> callback = null) : base(key, def, callback) {}
			
		public override void Reset() {
			Set(m_def);
		}

		public override void Init() {
			m_value = PlayerPrefs.GetFloat(m_key, m_def);
		}

		public override float Get() {
			return m_value;
		}

		public override void Set(float value) {
			m_value = value;
			PlayerPrefs.SetFloat(m_key, value);
			m_callback?.Invoke(value);
		}
	}
}
