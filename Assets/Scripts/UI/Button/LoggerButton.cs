using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	public class LoggerButton : BaseButton {
		private CanvasGroup canvasGroup;
		
		[SerializeField]
		private TMP_Text text;
		private Button button;
		private bool hasErrors;
		
		public bool HasErrors {
			private get => hasErrors;
			set => hasErrors = value;
		}

		private Color errColor = new(0.6132076f, 0.08966714f, 0.08966714f, 1); // 0x9C1717
		private Color defColor = new(0, 0, 0, 1);

		void Start() {
			canvasGroup = GetComponent<CanvasGroup>();
			button = GetComponent<Button>();

			button.onClick.AddListener(delegate {
				HasErrors = false;
				GUILoggerWindow.Window.ShowWindow();
			});
		}

		void Update() {
			canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, hasErrors ? 1 : 0, 0.2f);
			
			ColorBlock block = button.colors;
			var nextColor = Color.Lerp(block.normalColor, hasErrors ? errColor : defColor, 0.2f);
			block.normalColor = nextColor;
			block.pressedColor = nextColor;
			block.highlightedColor = new(nextColor.r, nextColor.g, nextColor.b, 0.8f);
			button.colors = block;
		}
	}
}
