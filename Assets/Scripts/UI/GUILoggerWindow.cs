using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace HardCoded.VRigUnity {
	public class GUILoggerWindow : GUIWindow {
		private static GUILoggerWindow _loggerWindow;
		public static GUILoggerWindow Window {
			get {
				if (_loggerWindow == null) {
					_loggerWindow = FindObjectOfType<GUILoggerWindow>(true);
					
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
			public GUILoggerWindow window;

			void Update() {
				window.UpdateMesages();
			}
		}

		[Header("Template")]
		public GameObject emptyLog;
		public Transform contentTransform;
		public LoggerButton loggerButton;

		[Header("Settings")]
		public int maxLogs = 128;
		
		// Style
		public Color evenColor = new(0.5660378f, 0.2269491f, 0.2269491f);
		public Color oddColor;
		private bool evenMessage;
		public int TotalLogs { get; protected set; }

		// Thread safe
		protected Thread unityThread;
		private readonly ConcurrentQueue<Message> messageQueue = new();

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
			// Thread safe queue
			// Technically this could have the wrong date
			while (messageQueue.TryDequeue(out Message message)) {
				InternalAddMessage(message);
			}
		}
		
		public void AddMessage(Logger.LogLevel level, string tag, object obj) {
			// Format the message
			Message message = new() { level = level, message = FormatMessage(level, tag, obj) };

			if (Thread.CurrentThread != unityThread) {
				// We can only messages in the unity thread
				messageQueue.Enqueue(message);
			} else {
				InternalAddMessage(message);
			}
		}

		private void InternalAddMessage(Message msg) {
			// Increment the log count
			TotalLogs++;

			GameObject empty = Instantiate(emptyLog);
			empty.transform.localScale = Vector3.one;
			empty.transform.SetParent(contentTransform, false);
			empty.SetActive(true);

			if (contentTransform.childCount > maxLogs) {
				// First child is the template
				Destroy(contentTransform.GetChild(1).gameObject);
			}

			TMP_Text text = empty.GetComponentInChildren<TMP_Text>();
			text.text = FilterString(text, msg.message);

			var imageColor = evenMessage ? evenColor : oddColor;
			switch (msg.level) {
				case Logger.LogLevel.Fatal or Logger.LogLevel.Error: {
					loggerButton.HasErrors = true;
					imageColor = new((imageColor.r + 1f) / 2.0f, imageColor.g, imageColor.b);
					break;
				}
			}

			Image image = empty.GetComponent<Image>();
			image.color = imageColor;
			evenMessage = !evenMessage;

			TMP_TextInfo info = text.GetTextInfo(text.text);
			LayoutElement layout = empty.GetComponent<LayoutElement>();
			layout.minHeight = Math.Max(1, info.lineCount) * text.fontSize + 8;
		}
		
		// Format a message
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
