using UnityEngine;

public class PressableButtonAttribute : PropertyAttribute {
	public string name;

	public PressableButtonAttribute(string name) {
		this.name = name;
	}
}
