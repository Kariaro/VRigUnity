using System;
using System.IO;
using UniGLTF;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRM;

namespace HardCoded.VRigUnity {
	public class GUITabSettings : MonoBehaviour {
		[Header("Settings")]
		public RectTransform toggleGroup;
		public RectTransform pageGroup;
		
		[Header("External")]
		[SerializeField] private GUIMain guiMain;

		public GUIMain Main => guiMain;
		private GameObject[] pages;
		private Toggle[] tabs;
		private GameObject[] tabOutlines;
		private int tabIndex;

		void Awake() {
			tabs = toggleGroup.GetComponentsInChildren<Toggle>();
			tabOutlines = new GameObject[tabs.Length];
			for (int i = 0; i < tabs.Length; i++) {
				int index = i;
				tabs[i].onValueChanged.RemoveAllListeners();
				tabs[i].onValueChanged.AddListener(value => {
					if (value && tabIndex != index) {
						TabUpdated(index);
					}
				});
				tabOutlines[i] = tabs[i].transform.GetChild(0).gameObject;
			}

			pages = new GameObject[pageGroup.childCount];
			for (int i = 0; i < pages.Length; i++) {
				pages[i] = pageGroup.GetChild(i).gameObject;
			}

			if (tabs.Length != pages.Length) {
				throw new IndexOutOfRangeException("GUI Tab Settings: Not enough pages for tabs");
			}

			// Default to first tab
			TabUpdated(0);
		}

		/// <summary>
		/// Called when a tab is updated
		/// </summary>
		/// <param name="index">The index of the tab</param>
		void TabUpdated(int index) {
			tabIndex = index;

			for (int i = 0; i < pages.Length; i++) {
				pages[i].SetActive(false);
				tabOutlines[i].SetActive(false);
			}

			pages[index].SetActive(true);
			tabOutlines[index].SetActive(true);
		}

		/*
		public void ResetModel() {
			guiMain.ResetModel();
		}

		public void LoadVrmModel(string path) {
			guiMain.LoadVrmModel(path);
		}

		public void LoadCustomImage(string path) {
			guiMain.LoadCustomImage(path);
		}

		public void SetBackgroundColor(Color color) {
			guiMain.SetBackgroundColor(color);
		}

		public void ResetCamera() {
			guiMain.ResetCamera();
		}

		public void SetShowCamera(bool show) {
			guiMain.SetShowCamera(show);
		}

		public void SetShowBackgroundImage(bool show) {
			guiMain.SetShowBackgroundImage(show);
		}

		public void SetShowBackgroundColor(bool show) {
			guiMain.SetShowBackgroundColor(show);
		}
		*/
	}
}
