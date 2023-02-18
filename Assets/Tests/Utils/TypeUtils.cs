using System;
using System.Linq;
using System.Reflection;

namespace Assets.Tests.Utils {
	public class TypeUtils {
		public static Assembly GetAssemblyCSharp() {
			return AppDomain.CurrentDomain.GetAssemblies()
				.Where(item => item.GetName().Name == "Assembly-CSharp")
				.First();
		}
	}
}
