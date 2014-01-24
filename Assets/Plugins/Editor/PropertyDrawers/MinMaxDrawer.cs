using TC.Internal;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (MinMax))]
public class MinMaxDrawer : PropertyDrawer
{
	private Rect rect;
	private string[] options = {"Constant", "Between"};

	private Rect GetRect(float width)
	{
		Rect ret = new Rect(rect.x, rect.y, width, rect.height);
		rect.x += width;
		return ret;
	}

	// Here you can define the GUI for your property drawer. Called by Unity.
	public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, prop);
		rect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
		rect.x -= 15.0f;
		MinMax.MinMaxMode mode = (MinMax.MinMaxMode) prop.FindPropertyRelative("modeProp").enumValueIndex;

		if (mode == MinMax.MinMaxMode.Constant)
			EditorGUI.PropertyField(GetRect(80.0f), prop.FindPropertyRelative("valueProp"), new GUIContent(""));
		else if (mode == MinMax.MinMaxMode.Between)
		{
			EditorGUI.PropertyField(GetRect(70.0f), prop.FindPropertyRelative("minProp"), new GUIContent(""));
			EditorGUI.PropertyField(GetRect(70.0f), prop.FindPropertyRelative("maxProp"), new GUIContent(""));
		}

		rect.x += 5;

		GUIStyle s = "ShurikenPopUp";
		s.contentOffset = new Vector2(100.0f, 100.0f);
		s.fixedWidth = 8.0f;

		prop.FindPropertyRelative("modeProp").enumValueIndex =
			EditorGUI.Popup(GetRect(100.0f), (int) mode, options, s);
		EditorGUI.EndProperty();
	}
}