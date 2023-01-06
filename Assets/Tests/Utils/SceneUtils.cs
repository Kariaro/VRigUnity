using UnityEngine.SceneManagement;

namespace Assets.Tests.Utils {
	public class SceneUtils {
		// Make sure the scene we are using is the production scene
		public static void Load() {
			Scene active = SceneManager.GetActiveScene();
			if (active.name != "Workspace") {
				SceneManager.LoadScene("Workspace", LoadSceneMode.Single);
			}
		}
	}
}
