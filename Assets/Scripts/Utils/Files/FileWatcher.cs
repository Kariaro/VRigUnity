using System;
using System.IO;

namespace HardCoded.VRigUnity {
	public class FileWatcher {
		public DateTime Date { get; private set; }	
		public string Path { get; private set; }

		/// <summary>
		/// Returns if the file is updated or not. Always returns true the first time this is called
		/// </summary>
		public bool IsUpdated {
			get {
				DateTime current = File.GetLastWriteTime(Path);
				if (current != Date) {
					Date = current;
					return true;
				}

				return false;
			}
		}

		public FileWatcher(string pathname) {
			Path = pathname;
		}
	}
}
