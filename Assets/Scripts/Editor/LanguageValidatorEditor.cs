using HardCoded.VRigUnity;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LanguageValidator), true, isFallback = true)]
public class LanguageValidatorEditor : Editor {
	public static string TemplateFallback {
		get {
			StringBuilder sb = new();
			foreach (var lang in Lang.Elements) {
				sb.Append(lang.id).Append('=').Append(LanguageLoader.EscapeString(lang.fallback)).Append('\n');
			}
			return sb.ToString();
		}
	}

	private bool once;

	private void OnEnable() {
		once = false;
	}

	public override void OnInspectorGUI() {
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		
		GUILayout.BeginVertical();
		GUILayout.Label("Editor Commands", EditorStyles.boldLabel);
		if (GUILayout.Button("Force Update", GUILayout.Width(200))) {
			Validate();
		}

		if (GUILayout.Button("Copy Fallback", GUILayout.Width(200))) {
			TextEditor textEditor = new();
			textEditor.text = TemplateFallback;
			textEditor.SelectAll();
			textEditor.Copy();
		}
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		if (!once) {
			once = true;
			Validate();
		}
	}

	public void Validate() {
		foreach (var lang in LanguageLoader.Languages) {
			if (!LanguageLoader.HasLanguage(lang)) {
				Debug.LogError($"A language '{lang.Code}' was specified but was not found!");
			}
		}

		// Check that the english language and fallback are synced
		var path = LanguageLoader.GetLanguagePath(LanguageLoader.FromCode("en_US"));
		string a = string.Join('\n', File.ReadAllLines(path)).Trim();
		string b = TemplateFallback.Trim();
		if (a != b) {
			Debug.LogError($"'en_US' is outdated and not the same as the fallback language");
		}
	}
}
