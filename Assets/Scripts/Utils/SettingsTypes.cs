using System;
using System.Collections.Generic;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class SettingsTypes {
		public static readonly List<IField> DefinedSettings = new();

		public interface IField {
			string Name();
			void Init();
			object RawValue();
		}

		public abstract class Field<T> : IField {
			protected readonly string m_key;
			protected readonly T m_def;
			protected bool m_init;
			protected Field(string key, T def) {
				m_key = key;
				m_def = def;
				DefinedSettings.Add(this);
			}

			public string Name() {
				return m_key;
			}

			public virtual object RawValue() {
				return Get();
			}

			public T Default() {
				return m_def;
			}

			public abstract void Init();
			public abstract void Reset();
			public abstract T Get();
			public abstract void Set(T value);
		}

		public class Text : Field<string> {
			private string m_value;
			public Text(string key, string def) : base(key, def) {}
			
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
			}
		}

		// This is used when the content should not be printed to the console
		public class SafeText : Text {
			public SafeText(string key, string def) : base(key, def) {}
			public override object RawValue() {
				return new string('*', Get().Length);
			}
		}

		public class Bool : Field<bool> {
			private bool m_value;
			public Bool(string key, bool def) : base(key, def) {}
			
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
			}
		}

		public class Int : Field<int> {
			private int m_value;
			public Int(string key, int def) : base(key, def) {}
			
			public override void Reset() {
				Set(m_def);
			}

			public override void Init() {
				m_value = PlayerPrefs.GetInt(m_key, m_def);
			}

			public override int Get() {
				return m_value;
			}

			public override void Set(int value) {
				m_value = value;
				PlayerPrefs.SetInt(m_key, value);
			}
		}

		public class Float : Field<float> {
			private float m_value;
			public Float(string key, float def) : base(key, def) {}
			
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
			}
		}
	}
}
