using System;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class SettingsTypes {
		public abstract class Field<T> {
			protected readonly string m_key;
			protected readonly T m_def;
			protected bool m_init;
			protected Field(string key, T def) {
				m_key = key;
				m_def = def;
			}
			public abstract void Reset();
			public abstract T Get();
			public abstract void Set(T value);
		}

		public class String : Field<string> {
			private string m_value;
			public String(string key, string def) : base(key, def) {}
			
			public override void Reset() {
				Set(m_def);
			}

			public override string Get() {
				if (!m_init) {
					m_init = true;
					m_value = PlayerPrefs.GetString(m_key, m_def);
				}

				return m_value;
			}

			public override void Set(string value) {
				m_value = value;
				PlayerPrefs.SetString(m_key, value);
			}
		}

		public class Bool : Field<bool> {
			private bool m_value;
			public Bool(string key, bool def) : base(key, def) {}
			
			public override void Reset() {
				Set(m_def);
			}

			public override bool Get() {
				if (!m_init) {
					m_init = true;
					m_value = PlayerPrefs.GetInt(m_key, m_def ? 1 : 0) != 0;
				}

				return m_value;
			}

			public override void Set(bool value) {
				m_value = value;
				PlayerPrefs.SetInt(m_key, value ? 1 : 0);
			}
		}

		public class Int : Field<int> {
			private int m_value;
			public Int(string key, int def) : base(key, def) {}
			
			public override void Reset() {
				Set(m_def);
			}

			public override int Get() {
				if (!m_init) {
					m_init = true;
					m_value = PlayerPrefs.GetInt(m_key, m_def);
				}

				return m_value;
			}

			public override void Set(int value) {
				m_value = value;
				PlayerPrefs.SetInt(m_key, value);
			}
		}
	}
}
