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
				BoneSettings.FACE => Lang.BonesTabFace.Get(),
				BoneSettings.NECK => Lang.BonesTabNeck.Get(),
				BoneSettings.LEFT_ARM => Lang.BonesTabLeftArm.Get(),
				BoneSettings.LEFT_WRIST => Lang.BonesTabLeftWrist.Get(),
				BoneSettings.LEFT_FINGERS => Lang.BonesTabLeftFingers.Get(),
				BoneSettings.RIGHT_ARM => Lang.BonesTabRightArm.Get(),
				BoneSettings.RIGHT_WRIST => Lang.BonesTabRightWrist.Get(),
				BoneSettings.RIGHT_FINGERS => Lang.BonesTabRightFingers.Get(),
				BoneSettings.CHEST => Lang.BonesTabChest.Get(),
				BoneSettings.HIPS => Lang.BonesTabHips.Get(),
				BoneSettings.LEFT_LEG => Lang.BonesTabLeftLeg.Get(),
				BoneSettings.LEFT_ANKLE => Lang.BonesTabLeftAnkle.Get(),
				BoneSettings.RIGHT_LEG => Lang.BonesTabRightLeg.Get(),
				BoneSettings.RIGHT_ANKLE => Lang.BonesTabRightAnkle.Get(),
				_ => null
			};
		}
	}
}
