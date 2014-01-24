using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OpenClose : ScriptableObject
{
	[SerializeField] private List<bool> open;
	[SerializeField] private List<string> names;

	private int DeclOpen(string nameDecl)
	{
		if (!names.Contains(nameDecl))
		{
			open.Add(false);
			names.Add(nameDecl);
		}

		return names.IndexOf(nameDecl);
	}

	private Rect GetRect()
	{
		return GUILayoutUtility.GetRect(Screen.width - 30.0f, 16.0f);
	}

	public bool ToggleArea(string areaName, Color col)
	{
		int i = DeclOpen(areaName);
		Rect r = GetRect();

		Color oldCol = GUI.color;

		if (!open[i])
			col *= 0.8f;

		GUI.color = col;
		open[i] = GUI.Toggle(r, open[i], new GUIContent(areaName), "ShurikenModuleTitle");
		GUI.color = oldCol;


		if (open[i])
			GUILayout.BeginVertical("ShurikenModuleBg", GUILayout.MinHeight(20.0f));

		return open[i];
	}

	public void ToggleAreaEnd(string areaName)
	{
		int i = DeclOpen(areaName);
		if (!open[i]) return;

		GUILayout.Space(5.5f);
		GUILayout.EndVertical();
	}


	private void OnEnable()
	{
		if (open != null && names != null) return;

		open = new List<bool>();
		names = new List<string>();
	}
}