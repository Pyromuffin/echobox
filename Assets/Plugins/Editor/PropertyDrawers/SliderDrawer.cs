using UnityEditor;
using UnityEngine;

// The property drawer class should be placed in an editor script, inside a folder called Editor.

// Tell the RangeDrawer that it is a drawer for properties with the RangeAttribute.
[CustomPropertyDrawer(typeof (SliderAttribute))]
internal class SliderDrawer : PropertyDrawer
{
	// Draw the property inside the given rect
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		// First get the attribute since it contains the range for the slider
		var range = attribute as SliderAttribute;

		if (range == null)
			return;

		// Now draw the property as a Slider or an IntSlider based on whether it's a float or integer.
		switch (property.propertyType)
		{
			case SerializedPropertyType.Float:
				EditorGUI.Slider(position, property, range.Min, range.Max, label);
				break;
			case SerializedPropertyType.Integer:
				EditorGUI.IntSlider(position, property, (int) range.Min, (int) range.Max, label);
				break;
			default:
				EditorGUI.LabelField(position, label.text, "Use Slider with float or int.");
				break;
		}
	}
}