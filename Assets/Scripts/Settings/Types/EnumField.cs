using System;
using UnityEngine;

namespace HardCoded.VRigUnity.SettingsTypes {
	public class SafeEnumOf<T> : EnumOf<T> where T : Enum {
		public SafeEnumOf(string key, T def, Action<T> callback = null) : base(key, def, callback) {}
		public override object RawValue() {
			return "*";
		}
	}

	public class EnumOf<T> : Field<T> where T : Enum {
		private T m_eval;
		private int m_value;
		public EnumOf(string key, T def, Action<T> callback = null) : base(key, def, callback) {}
			
		public override void Reset() {
			Set(m_def);
		}

		public override void Init() {
			m_value = PlayerPrefs.GetInt(m_key, Convert.ToInt32(m_def));
			m_eval = (T) Enum.ToObject(typeof(T), m_value);
		}

		public override T Get() {
			return m_eval;
		}

		public override void Set(T value) {
			m_value = Convert.ToInt32(value);
			m_eval = value;
			PlayerPrefs.SetInt(m_key, m_value);
			m_callback?.Invoke(value);
		}
	}
}
