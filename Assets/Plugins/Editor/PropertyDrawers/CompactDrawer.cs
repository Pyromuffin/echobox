using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (CompactAttribute))]
public class CompactDrawer : PropertyDrawer
{
	// These constants describe the height of the help box and the text field.
	private const int textHeight = 26;

	// Provide easy access to the RegexAttribute for reading information from it.
	private CompactAttribute rangeAttribute
	{
		get { return ((CompactAttribute) attribute); }
	}

	// Here you must define the height of your property drawer. Called by Unity.
	public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
	{
		return base.GetPropertyHeight(prop, label) + textHeight;
	}

	// Here you can define the GUI for your property drawer. Called by Unity.
	public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
	{
		EditorGUI.BeginChangeCheck();
		var val = EditorGUI.Vector3Field(position, label.text, prop.vector3Value);
		if (EditorGUI.EndChangeCheck())
			prop.vector3Value = val;
	}
}