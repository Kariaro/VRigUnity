using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class GUILoggerWindow : GUIWindow {
		private static GUILoggerWindow _loggerWindow;
		public static GUILoggerWindow Window {
			get {
				if (_loggerWindow == null) {
					_loggerWindow = FindObjectOfType<GUILoggerWindow>();
				}

				return _loggerWindow;
			}
		}

		public GUIScript guiScript;

		[Header("Template")]
		public GameObject emptyLog;
		public Transform contentTransform;
		public LoggerButton loggerButton;

		[Header("Settings")]
		public int maxLogs = 128;

		[HideInInspector]
		public List<string> logs = new();

		private string FilterString(TMP_Text tmp, string message) {
			string result = "";
			for (int i = 0; i < message.Length; i++) {
				char c = message[i];
				if (tmp.font.HasCharacter(c)) {
					result += c;
				}
			}

			// Fix escaping issues
			result = result.Replace("\\", "\\\\");

			return result;
		}

		private bool evenMessage;
		public Color evenColor = new(0.5660378f, 0.2269491f, 0.2269491f);
		public Color oddColor;

		public void OpenLogsFolder() {
			// TODO: Implement
		}

		public void AddMessage(Logger.LogLevel level, string tag, object obj) {
			// Every other one should have a different background

			string color = level switch {
				Logger.LogLevel.Fatal or Logger.LogLevel.Error => "red",
				Logger.LogLevel.Warn => "yellow",
				_ => "white"
			};

			switch (level) {
				case Logger.LogLevel.Fatal or Logger.LogLevel.Error:
					loggerButton.HasErrors = true;
					break;
			}

			// TODO: Errors should have a more red background

			string value;
			if (tag != null) {
				value = $"[{DateTime.Now:H:mm:ss}] <color={color}>[{level}]</color> [{tag}]: {obj}";
			} else {
				value = $"[{DateTime.Now:H:mm:ss}] <color={color}>[{level}]</color>: {obj}";
			}

			GameObject empty = Instantiate(emptyLog);
			empty.transform.localScale = Vector3.one;
			empty.transform.SetParent(contentTransform, false);
			empty.SetActive(true);

			if (contentTransform.childCount > maxLogs) {
				// First child is the template
				Destroy(contentTransform.GetChild(1).gameObject);
			}

			TMP_Text text = empty.GetComponentInChildren<TMP_Text>();
			text.text = FilterString(text, value);

			Image image = empty.GetComponent<Image>();
			image.color = evenMessage ? evenColor : oddColor;
			evenMessage = !evenMessage;

			TMP_TextInfo info = text.GetTextInfo(text.text);
			LayoutElement layout = empty.GetComponent<LayoutElement>();
			layout.minHeight = Math.Max(1, info.lineCount) * text.fontSize + 8;
		}
	}
}
