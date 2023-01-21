using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace HardCoded.VRigUnity {
	public class GUITabLogger : GUITab {
		private static GUITabLogger _loggerWindow;
		public static GUITabLogger Window {
			get {
				if (_loggerWindow == null) {
					_loggerWindow = FindObjectOfType<GUITabLogger>(true);
					
					// Initialize the logger loop
					if (_loggerWindow != null) {
						// Initialize thread
						_loggerWindow.unityThread = Thread.CurrentThread;

						LoggerLoop loggerLoop = _loggerWindow.transform.parent.gameObject.AddComponent<LoggerLoop>();
						loggerLoop.window = _loggerWindow;
					}
				}

				return _loggerWindow;
			}
		}

		// We need to add something that keeps updating the messages
		public class LoggerLoop : MonoBehaviour {
			public GUITabLogger window;

			void Update() {
				window.UpdateMesages();
			}
		}

		[Header("Template")]
		public RectTransform emptyLog;
		public Transform contentTransform;
		// public LoggerButton loggerButton;

		[Header("Settings")]
		public int maxLogs = 128;
		
		// Style
		public Color errorColor = new(0.3962264f, 0.1925062f, 0.1967066f);
		public Color evenColor = new(0.5660378f, 0.2269491f, 0.2269491f);
		public Color oddColor;
		private bool evenMessage;
		public int TotalLogs { get; protected set; }

		// Size correction
		private Vector2 previousSize;
		private RectTransform rectTransform;
		[SerializeField] private Scrollbar scrollbar;

		// Thread safe
		protected Thread unityThread;
		private readonly ConcurrentQueue<Message> messageQueue = new();

		void Start() {
			rectTransform = GetComponent<RectTransform>();
		}

		public void OpenLogsFolder() {
			static string CombinePaths(params string[] paths) {
				if (paths == null) {
					throw new ArgumentNullException("paths");
				}
				return paths.Aggregate(Path.Combine);
			}

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
			var path = CombinePaths(Environment.GetEnvironmentVariable("AppData"), "..", "LocalLow", Application.companyName, Application.productName);
			System.Diagnostics.Process.Start("explorer.exe", path);
#elif UNITY_STANDALONE_LINUX
			var path = CombinePaths("~/.config/unity3d", Application.companyName, Application.productName);
			System.Diagnostics.Process.Start("open", path);
#elif UNITY_STANDALONE_OSX
			var path = CombinePaths("~~/Library/Logs", Application.companyName, Application.productName);
			System.Diagnostics.Process.Start("open", path);
#else
#  warning Opening logs path is not implemented
#endif
		}

		void UpdateMesages() {
			// Technically this could put messages in the wrong order
			while (messageQueue.TryDequeue(out Message message)) {
				InternalAddMessage(message);
			}
		}
		
		public void AddMessage(Logger.LogLevel level, string tag, object obj) {
			Message message = new() { level = level, message = FormatMessage(level, tag, obj) };

			if (Thread.CurrentThread != unityThread) {
				// We can only add messages in the unity thread
				messageQueue.Enqueue(message);
			} else {
				InternalAddMessage(message);
			}
		}

		private void InternalAddMessage(Message msg) {
			// Increment the log count
			TotalLogs++;

			GameObject template = Instantiate(emptyLog.gameObject);
			template.transform.localScale = Vector3.one;
			template.transform.SetParent(contentTransform, false);
			template.SetActive(true);

			if (contentTransform.childCount > maxLogs) {
				// First child is the template
				Destroy(contentTransform.GetChild(1).gameObject);
			}

			TMP_Text text = template.GetComponentInChildren<TMP_Text>();
			text.text = FilterString(text, msg.message);

			var imageColor = evenMessage ? evenColor : oddColor;
			switch (msg.level) {
				case Logger.LogLevel.Fatal or Logger.LogLevel.Error: {
					// loggerButton.HasErrors = true;
					imageColor = errorColor;
					break;
				}
			}

			Image image = template.GetComponent<Image>();
			image.color = imageColor;
			evenMessage = !evenMessage;

			UpdateTextBox(template, text);
		}

		private void UpdateTextBox(GameObject template, TMP_Text text) {
			TMP_Text emptyText = emptyLog.GetComponentInChildren<TMP_Text>();
			emptyLog.localScale = Vector3.zero;
			emptyLog.ForceUpdateRectTransforms();

			TMP_TextInfo info = emptyText.GetTextInfo(text.text);
			LayoutElement layout = template.GetComponent<LayoutElement>();
			layout.minHeight = Math.Max(1, info.lineCount) * text.fontSize + 8;
		}

		void Update() {
			if (rectTransform == null) {
				return;
			}

			Vector2 currentSize = rectTransform.rect.size;
			if (previousSize != currentSize) {
				previousSize = currentSize;

				foreach (Transform item in contentTransform) {
					UpdateTextBox(item.gameObject, item.GetComponentInChildren<TMP_Text>());
				}
			}
		}

		/// <summary>
		/// Format a message
		/// </summary>
		private string FormatMessage(Logger.LogLevel level, string tag, object obj) {
			string color = level switch {
				Logger.LogLevel.Fatal or Logger.LogLevel.Error => "red",
				Logger.LogLevel.Warn => "yellow",
				_ => "white"
			};

			string value;
			if (tag != null) {
				value = $"[{DateTime.Now:H:mm:ss}] <color={color}>[{level}]</color> [{tag}]: {obj}";
			} else {
				value = $"[{DateTime.Now:H:mm:ss}] <color={color}>[{level}]</color>: {obj}";
			}

			return value;
		}

		// Filter a string to make it displayable in the text field
		private string FilterString(TMP_Text tmp, string message) {
			string result = "";
			for (int i = 0; i < message.Length; i++) {
				char c = message[i];
				if (tmp.font.HasCharacter(c)) {
					result += c;
				}
			}

			// Fix escaping issues
			return result.Replace("\\", "\\\\");
		}

		struct Message {
			public Logger.LogLevel level;
			public string message;
		}
	}
}