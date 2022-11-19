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
			protected readonly Action<T> m_callback;
			protected readonly string m_key;
			protected readonly T m_def;
			protected bool m_init;
			protected Field(string key, T def, Action<T> callback) {
				m_callback = callback;
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
		
		public class SafeInt : Int {
			public SafeInt(string key, int def, Action<int> callback = null) : base(key, def, callback) {}
			public override object RawValue() {
				return "*";
			}
		}

		public class Int : Field<int> {
			private int m_value;
			public Int(string key, int def, Action<int> callback = null) : base(key, def, callback) {}
			
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
				m_callback?.Invoke(value);
			}
		}
		
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
}
