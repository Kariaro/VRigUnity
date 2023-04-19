using Mediapipe.Unity;
using System.Collections;
using UnityEngine;

namespace HardCoded.VRigUnity {
	public abstract class ImageSource : MonoBehaviour {
		public virtual Texture CurrentTexture { get; }
		public virtual ResolutionStruct Resolution { get; set; }
		public virtual int TextureWidth { get; }
		public virtual int TextureHeight { get; }
		public virtual bool IsPrepared { get; }
		public virtual bool IsPlaying { get; }
		public virtual bool IsVerticallyFlipped { get; set; }
		public virtual bool IsHorizontallyFlipped { get; set; }
		public virtual RotationAngle Rotation { get; }
		public virtual string SourceName { get; }
		public virtual string[] SourceCandidateNames { get; }

		/// <summary>
		/// Returns a list of valid resolutions
		/// </summary>
		public abstract ResolutionStruct[] GetResolutions();

		/// <summary>
		/// Start this image source
		/// </summary>
		public abstract IEnumerator Play();

		/// <summary>
		/// Stop this image source
		/// </summary>
		public abstract void Stop();

		/// <summary>
		/// Apply the current app settings to this image source
		/// </summary>
		public abstract void UpdateFromSettings();
	}
}
