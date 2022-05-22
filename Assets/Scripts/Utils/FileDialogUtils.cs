using SFB;
using System.IO;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class FileDialogUtils {
		public static string[] OpenFilePanelRemember(string id, string title, ExtensionFilter[] extensions, bool multiSelect) {
			string directory = PlayerPrefs.GetString(id, "");

			string[] result = StandaloneFileBrowser.OpenFilePanel(title, directory, extensions, multiSelect);
			if (result.Length > 0) {
				directory = Path.GetDirectoryName(result[0]);
				PlayerPrefs.SetString(id, directory);
			}

			return result;
		}
	}
}
