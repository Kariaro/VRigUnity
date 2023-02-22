using System;

namespace HardCoded.VRigUnity {
	public class Logger {
		public enum LogLevel {
			Fatal,
			Error,
			Warn,
			Info,
			Verbose,
			Debug,
		}

		public static void Log(object obj) {
			LogWithLevel(LogLevel.Info, null, obj);
		}

		public static void Log(string tag, object obj) {
			LogWithLevel(LogLevel.Info, tag, obj);
		}

		public static void Warning(object obj) {
			LogWithLevel(LogLevel.Warn, null, obj);
		}

		public static void Warning(string tag, object obj) {
			LogWithLevel(LogLevel.Warn, tag, obj);
		}

		public static void Info(object obj) {
			LogWithLevel(LogLevel.Warn, null, obj);
		}

		public static void Info(string tag, object obj) {
			LogWithLevel(LogLevel.Warn, tag, obj);
		}

		public static void Error(object obj) {
			LogWithLevel(LogLevel.Error, null, obj);
		}

		public static void Error(string tag, object obj) {
			LogWithLevel(LogLevel.Error, tag, obj);
		}

		public static void Verbose(object obj) {
			LogWithLevel(LogLevel.Verbose, null, obj);
		}

		public static void Verbose(string tag, object obj) {
			LogWithLevel(LogLevel.Verbose, tag, obj);
		}

		public static void Debug(object obj) {
			LogWithLevel(LogLevel.Debug, null, obj);
		}

		public static void Debug(string tag, object obj) {
			LogWithLevel(LogLevel.Debug, tag, obj);
		}

		public static void Exception(Exception e) {
			LogWithLevel(LogLevel.Error, null, e);
		}

		public static void Exception(string tag, Exception e) {
			LogWithLevel(LogLevel.Error, tag, e);
		}

		public static void LogWithLevel(LogLevel level, string tag, object obj) {
			if (obj is Exception) {
				UnityEngine.Debug.LogException(obj as Exception);
				return;
			}

			string msg;
			// TODO: Include timestamp
			if (tag != null) {
				msg = string.Format("[{0}] [{1}]: {2}", level, tag, obj);
			} else {
				msg = string.Format("[{0}]: {1}", level, obj);
			}
			
			if (GUITabLogger.Instance != null) {
				GUITabLogger.Instance.AddMessage(level, tag, obj);
			}

			// TODO: Print this to stdout always
			UnityEngine.Debug.LogWarning(msg);
		}
	}
}
