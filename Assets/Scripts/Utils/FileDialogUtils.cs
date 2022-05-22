using SFB;

namespace HardCoded.VRigUnity {
	public class FileDialogUtils {
		// TODO: Save the last used path
		public static string[] OpenFilePanel(string id, string title, string directory, ExtensionFilter[] extensions, bool multiSelect) {
			// TODO: Remember the last path used with a key value
			return StandaloneFileBrowser.OpenFilePanel(title, directory, extensions, multiSelect);
		}
	}
}
