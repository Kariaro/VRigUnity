using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniGLTF;
using UnityEngine;
using VRM;

namespace HardCoded.VRigUnity {
	public class GUIScript : MonoBehaviour {
		[SerializeField] GUISettingsMenu settingsMenu;
        [SerializeField] TestHolisticTrackingSolution holisticSolution;
        [SerializeField] Vector3 modelTransform = Vector3.zero;

		public void LoadVrmModel(string path) {
			Debug.Log("Load VRM Model: '" + path + "'");

            var data = new GlbFileParser(path).Parse();
            var vrm = new VRMData(data);
            using (var context = new VRMImporterContext(vrm)) {
                var loaded = context.Load();
                loaded.EnableUpdateWhenOffscreen();
                loaded.ShowMeshes();

                holisticSolution.SetVrmModel(loaded.gameObject);
            }
		}

        public Vector3 GetModelTransform() {
            return modelTransform;
		}

        public void SetModelTransform(float x, float y, float z) {
            modelTransform.x = x;
            modelTransform.y = y;
            modelTransform.z = z;
		}
	}
}
