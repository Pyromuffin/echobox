using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (TCOffscreenRenderer))]
public class TCOffscreenRendererEditor : Editor
{
	private TCEdtiorBase edit;

	private void OnEnable()
	{
		hideFlags = HideFlags.HideAndDontSave;
		edit = CreateInstance<TCEdtiorBase>();
		edit.Init(this);
	}

	private void OnDisable()
	{
		DestroyImmediate(edit);
	}

	private void OnDestroy()
	{
		DestroyImmediate(edit);
	}

	// Update is called once per frame
	public override void OnInspectorGUI()
	{
		EditorGUIUtility.LookLikeInspector();
		edit.Update();


		edit.PropField("offscreenLayer");
		edit.PropField("downsampleFactor");
		edit.PropField("compositeMode");

		if (!serializedObject.isEditingMultipleObjects)
		{
			if (edit.GetProperty("compositeMode").enumValueIndex == (int) TCOffscreenRenderer.CompositeMode.Gradient)
			{
				edit.PropField("compositeGradient");
				edit.PropField("tint");
				edit.PropField("gradientScale");
			}
		}
		if (EditorApplication.isPlaying)
		{
			foreach (TCOffscreenRenderer t in targets)
			{
				t.UpdateCompositeGradient();
			}
		}
		EditorGUILayout.HelpBox("This feature is still experimental! For more info, refer to the manual", MessageType.Warning);

		edit.Apply();
	}
}