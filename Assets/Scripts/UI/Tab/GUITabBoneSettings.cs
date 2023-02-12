using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace HardCoded.VRigUnity {
	public class GUITabBoneSettings : GUITab {
		[Header("Settings")]
		[SerializeField] private Toggle[] toggles;
		[SerializeField] private TMP_Text hoverText;
		[SerializeField] private RectTransform bonesTransform;
		
		private class ToggleHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
			public GUITabBoneSettings gui;
			public int index;
			
			public void OnPointerEnter(PointerEventData data) {
				gui.OnToggleHover(index);
			}

			public void OnPointerExit(PointerEventData data) {
				gui.OnToggleHover(-1);
			}
		}

		void Start() {
			for (int i = 0; i < toggles.Length; i++) {
				ToggleHover toggleHover = toggles[i].gameObject.AddComponent<ToggleHover>();
				toggleHover.gui = this;
				toggleHover.index = i;
			}
		}

		void OnEnable() {
			for (int i = 0; i < toggles.Length; i++) {
				int index = i;
				toggles[i].isOn = BoneSettings.Get(i);
				toggles[i].onValueChanged.RemoveAllListeners();
				toggles[i].onValueChanged.AddListener(delegate {
					BoneSettings.Set(index, toggles[index].isOn);
					SolutionUtils.GetSolution().Model.OnBoneUpdate(index, toggles[index].isOn);
				});
			}
		}

		private void OnToggleHover(int index) {
			string desc = GetDescription(index);
			if (desc == null) {
				hoverText.text = "";
			} else {
				hoverText.text = ": " + desc;
			}
		}

		void Update() {
			float height = rectTransform.rect.height - 34;
			if (height < bonesTransform.rect.height) {
				float size = height / bonesTransform.rect.height;
				bonesTransform.localScale = new(size, size, 1);
			} else {
				bonesTransform.localScale = Vector3.one;
			}
		}

		private string GetDescription(int index) {
			return index switch {
				BoneSettings.FACE => "Face",
				BoneSettings.NECK => "Neck",
				BoneSettings.LEFT_ARM => "Left Arm",
				BoneSettings.LEFT_WRIST => "Left Wrist",
				BoneSettings.LEFT_FINGERS => "Left Fingers",
				BoneSettings.RIGHT_ARM => "Right Arm",
				BoneSettings.RIGHT_WRIST => "Right Wrist",
				BoneSettings.RIGHT_FINGERS => "Right Fingers",
				BoneSettings.CHEST => "Chest",
				BoneSettings.HIPS => "Hips",
				BoneSettings.LEFT_LEG => "Left Leg",
				BoneSettings.LEFT_ANKLE => "Left Ankle",
				BoneSettings.RIGHT_LEG => "Right Leg",
				BoneSettings.RIGHT_ANKLE => "Right Ankle",
				_ => null
			};
		}
	}
}
