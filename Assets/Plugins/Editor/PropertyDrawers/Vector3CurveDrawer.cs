using TC.Internal;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (Vector3Curve))]
public class Vector3CurveDrawer : PropertyDrawer
{
	private Rect rect;
	private GUIContent[] options = {new GUIContent("constant"), new GUIContent("curve")};


	private Rect GetRect(float width)
	{
		Rect ret = new Rect(rect.x, rect.y, width, 16.0f);
		rect.x += width;
		return ret;
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return 32.0f;
	}

	// Here you can define the GUI for your property drawer. Called by Unity.
	public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, prop);
		rect = position;
		bool constant = prop.FindPropertyRelative("isConstant").boolValue;

		rect.x += 14.0f;

		GUI.Label(GetRect(200.0f), label);

		rect.y += 16.0f;
		rect.x = 40.0f;

		if (!constant)
		{
			GUI.Label(GetRect(12.0f), new GUIContent("X"));
			EditorGUI.PropertyField(GetRect(60.0f), prop.FindPropertyRelative("xCurve"), new GUIContent(""));
			GUI.Label(GetRect(12.0f), new GUIContent("Y"));
			EditorGUI.PropertyField(GetRect(60.0f), prop.FindPropertyRelative("yCurve"), new GUIContent(""));
			GUI.Label(GetRect(12.0f), new GUIContent("Z"));
			EditorGUI.PropertyField(GetRect(60.0f), prop.FindPropertyRelative("zCurve"), new GUIContent(""));
		}
		else
		{
			GUI.Label(GetRect(12.0f), new GUIContent("X"));
			EditorGUI.PropertyField(GetRect(60.0f), prop.FindPropertyRelative("x"), new GUIContent(""));
			GUI.Label(GetRect(12.0f), new GUIContent("Y"));
			EditorGUI.PropertyField(GetRect(60.0f), prop.FindPropertyRelative("y"), new GUIContent(""));
			GUI.Label(GetRect(12.0f), new GUIContent("Z"));
			EditorGUI.PropertyField(GetRect(60.0f), prop.FindPropertyRelative("z"), new GUIContent(""));
		}


		int c = constant ? 0 : 1;
		GUIStyle s = "ShurikenPopUp";
		s.contentOffset = new Vector2(100.0f, 100.0f);
		s.fixedWidth = 8.0f;


		c = EditorGUI.Popup(GetRect(80.0f), c, options, s);
		prop.FindPropertyRelative("isConstant").boolValue = c == 0;

		EditorGUI.EndProperty();
	}
}