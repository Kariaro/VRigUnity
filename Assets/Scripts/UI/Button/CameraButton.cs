using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

namespace HardCoded.VRigUnity {
	public class CameraButton : MonoBehaviour {
		private TMP_Text text;
		private Button toggleButton;
		private Image buttonImage;
		private bool isCameraShowing;

		[SerializeField] Color toggleOnColor  = new(0.08009967f, 0.6792453f, 0.3454931f); // 0x14AD58
		[SerializeField] Color toggleOffColor = new(0.6981132f, 0, 0.03523935f); // 0xB30009

		void Start() {
			text = GetComponentInChildren<TMP_Text>();
			buttonImage = GetComponent<Image>();
			toggleButton = GetComponent<Button>();

			InitializeContents();
		}

		private void InitializeContents() {
			buttonImage.color = toggleOnColor;
			isCameraShowing = false;

			toggleButton.onClick.RemoveAllListeners();
			toggleButton.onClick.AddListener(delegate {
				SetCamera(!isCameraShowing);
			});
		}

		private void SetCamera(bool enable) {
			isCameraShowing = enable;
			buttonImage.color = enable ? toggleOffColor : toggleOnColor;
			text.text = enable ? "Stop Camera" : "Start Camera";

			if (enable) {
				var imageSource = SolutionUtils.GetImageSource();

				var sourceId = imageSource.sourceCandidateNames.ToList().FindIndex(source => source == Settings.CameraName);
				if (sourceId >= 0 && sourceId < imageSource.sourceCandidateNames.Length) {
					imageSource.SelectSource(sourceId);
				}

				var resolutionId = imageSource.availableResolutions.ToList().FindIndex(option => option.ToString() == Settings.CameraResolution);
				if (resolutionId >= 0 && resolutionId < imageSource.availableResolutions.Length) {
					imageSource.SelectResolution(resolutionId);
				}

				imageSource.isHorizontallyFlipped = Settings.CameraFlipped;

				// TODO: If the camera failed give an error message
				SolutionUtils.GetSolution().Play();
			} else {
				SolutionUtils.GetSolution().ResetVRMAnimator();
				SolutionUtils.GetSolution().Stop();
			}
		}
	}
}
