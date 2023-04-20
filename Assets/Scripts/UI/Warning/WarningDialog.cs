using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace HardCoded.VRigUnity {
	/// <summary>
	/// Main script of the UI classes
	/// </summary>
	public class WarningDialog : MonoBehaviour {
		private static WarningDialog instance;
		public static WarningDialog Instance {
			get {
				if (instance == null) {
					instance = FindObjectOfType<WarningDialog>(true);
				}

				return instance;
			}
		}

		[SerializeField] private TMP_Text dialogTitleField;
		[SerializeField] private Lang dialogTitle;
		[SerializeField] private Button continueButton;
		[SerializeField] private Button cancelButton;

		void Start() {
			Localization.OnLocalizationChangeEvent += UpdateLanguage;
		}

		public void Open(Lang warningMessage, System.Action onContinue) {
			gameObject.SetActive(true);
			dialogTitle = warningMessage;
			UpdateLanguage();

			continueButton.onClick.RemoveAllListeners();
			cancelButton.onClick.RemoveAllListeners();
			continueButton.onClick.AddListener(delegate {
				gameObject.SetActive(false);
				onContinue?.Invoke();
			});
			cancelButton.onClick.AddListener(delegate {
				gameObject.SetActive(false);
			});
		}

		private void UpdateLanguage() {
			if (dialogTitle != null) {
				dialogTitleField.text = dialogTitle.Get();
			}
		}
	}
}
