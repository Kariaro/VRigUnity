using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PressableButtonAttribute))]
public class PressableButtonPropertyDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		PressableButtonAttribute toggleButton = attribute as PressableButtonAttribute;

		if (property.propertyType == SerializedPropertyType.Boolean) {
			Vector2 size = GUI.skin.label.CalcSize(new GUIContent(toggleButton.name)) + new Vector2(16, 2);
			size.x = 200;
			property.boolValue = GUI.Toggle(new Rect(position.position, size), property.boolValue, toggleButton.name, GUI.skin.button);
		}
	}
}
