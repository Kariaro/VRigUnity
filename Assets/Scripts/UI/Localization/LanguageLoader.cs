using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace HardCoded.VRigUnity {
	/// <summary>
	/// Used to load languages
	/// </summary>
	public class LanguageLoader {
		public struct Language {
			public string Code { get; set; }
			public string DisplayName { get; set; }

			public bool IsDebug => Code == "debug";
		}

		/// <summary>
		/// Returns a path to the language json file
		/// </summary>
		public static string LanguageJsonFile => Path.Combine(Application.streamingAssetsPath, "lang", "languages.json");

		public static string TemplateFallback {
			get {
				StringBuilder sb = new();
				foreach (var lang in Lang.Elements) {
					sb.Append(lang.id).Append('=').Append(EscapeString(lang.fallback)).Append('\n');
				}
				return sb.ToString();
			}
		}

		private static readonly FileWatcher _languageWatcher = new(LanguageJsonFile);
		private static List<Language> _languages;
		public static List<Language> Languages {
			get {
				if (_languageWatcher.IsUpdated || _languages == null) {
					_languages = TryGetLanguages();
				}

				return _languages;
			}
		}

		private static string EscapeString(string text) {
			StringBuilder sb = new();

			foreach (char c in text) {
				sb.Append(c switch {
					'\n' => "\\n",
					'\r' => "\\r",
					'\t' => "\\t",
					'\\' => "\\\\",
					_ => c
				});
			}

			return sb.ToString();
		}
		
		private static string UnescapeString(string text) {
			StringBuilder sb = new();

			bool lastSlash = false;
			for (int i = 0; i < text.Length; i++) {
				char c = text[i];

				if (lastSlash) {
					lastSlash = false;
					switch (c) {
						case '\\': sb.Append('\\'); break;
						case 'n': sb.Append('\n'); break;
						case 'r': sb.Append('\r'); break;
						case 't': sb.Append('\t'); break;
						default: throw new Exception($"Invalid escape character '{c}'");
					}

					continue;
				}

				if (c == '\\') {
					lastSlash = true;
					continue;
				}

				sb.Append(c);
			}

			if (lastSlash) {
				throw new Exception($"Missing escape at end of line");
			}

			return sb.ToString();
		}

		private static List<Language> TryGetLanguages() {
			try {
				return GetLanguages();
			} catch (Exception e) {
#if UNITY_EDITOR
				if (Application.isPlaying) {
					Logger.Error("languages.json", e.Message);
				} else {
					Debug.LogError("[language.json] " + e.Message);
				}
#else
				Logger.Error("languages.json", e.Message);
#endif
			}

			return new();
		}

		private static List<Language> GetLanguages() {
			List<Language> languages = new();
			var json = JObject.Parse(File.ReadAllText(LanguageJsonFile));

			foreach (var entry in json) {
				if (entry.Value.Type != JTokenType.String) {
					throw new Exception($"languages.json: Invalid display name for language '{entry.Key}'");
				}
				
				string code = entry.Key.ToString();
				string name = entry.Value.ToString();
				if (code == "debug") {
					name = "Debug";
				}

				languages.Add(new() { Code = code, DisplayName = name });
			}

			return languages;
		}

		public static Language FromCode(string code) {
			if (code == "debug") {
				return new() { Code = "debug", DisplayName = "Debug" };
			}
			return new() { Code = code, DisplayName = "unspecified" };
		}

		public static string GetLanguagePath(Language lang) {
			return Path.Combine(Application.streamingAssetsPath, "lang", lang.Code, lang.Code + ".lang");
		}

		/// <summary>
		/// Returns a dictionary containing the loaded language data.
		/// If the language file contains errors this method will throw an exception
		/// </summary>
		public static Dictionary<string, string> LoadLanguage(Language lang) {
			Dictionary<string, string> values = new();

			if (!Languages.Exists(i => i.Code == lang.Code)) {
				throw new Exception($"Language '{lang.Code}' is not defined in language.json");
			}

			if (lang.IsDebug) {
				foreach (var item in Lang.Elements) {
					values.Add(item.id, item.id);
				}

				return values;
			}

			string languageFile = GetLanguagePath(lang);
			if (!File.Exists(languageFile)) {
				throw new Exception($"Language '{lang.Code}' does not exist");
			}

			string[] lines = File.ReadAllLines(languageFile);
			for (int i = 0; i < lines.Length; i++) {
				string line = lines[i];

				// Remove comments
				int commentIdx = line.IndexOf("###");
				if (commentIdx != -1) {
					line = line[..commentIdx];
				}

				// Remove trailing whitespaces
				line = line.TrimEnd();

				// Remove empty lines
				if (line.Length == 0) {
					continue;
				}

				int equalsIndex = line.IndexOf('=');
				if (equalsIndex == -1) {
					throw new Exception($"Language '{lang.Code}' is missing '=' on line '{i + 1}'");
				}

				string id = line[..equalsIndex];
				string translation = line[(equalsIndex + 1)..];
				values.Add(id, UnescapeString(translation));
			}

			return values;
		}
	}
}
