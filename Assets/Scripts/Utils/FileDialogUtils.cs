using SFB;
using System.IO;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public class FileDialogUtils {
		public static string[] OpenFilePanel(string title, string file, ExtensionFilter[] extensions, bool multiSelect) {
			string directory = "";
			if (File.Exists(file)) {
				directory = Path.GetDirectoryName(file);
			}

			return StandaloneFileBrowser.OpenFilePanel(title, directory, extensions, multiSelect);
		}
	}
}
