using System;
using System.Collections.Generic;

namespace HardCoded.VRigUnity.SettingsTypes {
	public interface IField {
		public static readonly List<IField> DefinedSettings = new();
			
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
			IField.DefinedSettings.Add(this);
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
}
