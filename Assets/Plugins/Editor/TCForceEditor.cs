using System.Collections.Generic;
using TC;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (TCForce))]
[CanEditMultipleObjects]
public class TCForceEditor : Editor
{
	private TCEdtiorBase m_edit;
	private TCForce m_forceTarget;
	private List<TCForce> m_forces;
	private TCForce m_primeForce;

	private OpenClose o;

	private TCNoiseForceGenerator m_forceGenerator;

	private void OnEnable()
	{
		m_edit = CreateInstance<TCEdtiorBase>();
		m_edit.Init(this);
		m_forceTarget = target as TCForce;

		if (m_forceTarget != null) m_forces = new List<TCForce>(m_forceTarget.GetComponents<TCForce>());
		m_primeForce = m_forces[0];
		m_forces.RemoveAt(0);

		o = m_edit.GetOpenClose();
		m_forceGenerator = new TCNoiseForceGenerator(m_forceTarget);
	}

	// Update is called once per frame

	public override void OnInspectorGUI()
	{
		int shape = m_edit.GetProperty("shape").enumValueIndex;
		m_edit.Update();


		EditorGUIUtility.LookLikeInspector();


		if (m_forceTarget != m_primeForce)
		{
			m_edit.GetProperty("primary").boolValue = false;
			m_edit.GetProperty("globalShape").boolValue = false;
		}
		else
		{
			m_edit.GetProperty("primary").boolValue = true;
		}


		if (o.ToggleArea("TC Force", new Color(1.0f, 0.8f, 0.8f)))
		{
			m_edit.PropField("power");

			if (shape != (int) ForceShape.Box && shape != (int) ForceShape.Constant)
			{
				m_edit.PropField("_attenuation");
				m_edit.PropField("attenuationType");
			}

			if (m_forceTarget == m_primeForce)
				m_edit.PropField("_inheritVelocity");
		}
		o.ToggleAreaEnd("TC Force");

		if (!m_primeForce.IsGlobalShape || m_forceTarget == m_primeForce)
		{
			if (o.ToggleArea("Force Shape", new Color(0.8f, 0.8f, 1.0f)))
			{
				if (m_forceTarget == m_primeForce && m_forces.Count > 0)
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField(m_edit.GetProperty("shape"), new GUIContent("shape"), GUILayout.MinWidth(180.0f));
					GUILayout.Label("Global", GUILayout.Width(40.0f));
					GUI.enabled = !EditorApplication.isPlaying;
					EditorGUILayout.PropertyField(m_edit.GetProperty("globalShape"), new GUIContent(""), GUILayout.Width(25.0f));
					GUI.enabled = true;
					GUILayout.Space(5.0f);
					GUILayout.EndHorizontal();
				}
				else
					m_edit.PropField("shape");

				if (shape != (int) ForceShape.Box && shape != (int) ForceShape.Constant)
					m_edit.PropField("radius");

				if (shape == (int) ForceShape.Capsule)
					m_edit.PropField("height");
				else if (shape == (int) ForceShape.Box)
					m_edit.PropField("boxSize");
				else if (shape == (int) ForceShape.Disc)
				{
					m_edit.PropField("discHeight");
					m_edit.PropField("discRounding");
					m_edit.PropField("discType");
				}
			}
			o.ToggleAreaEnd("Force Shape");
		}


		int type = m_edit.GetProperty("forceType").enumValueIndex;
		bool generate = false;

		if (o.ToggleArea("Force Type", new Color(0.8f, 1.0f, 0.8f)))
		{
			GUI.changed = false;
			m_edit.PropField("forceType");
			if (GUI.changed)
				SceneView.RepaintAll();

			switch (type)
			{
				case (int) ForceType.Vortex:
					m_edit.PropField("vortexAxis");
					m_edit.PropField("inwardForce");
					break;

				case (int) ForceType.Vector:
					m_edit.PropField("forceDirection");
					m_edit.PropField("forceDirectionSpace");
					break;

				case (int) ForceType.Turbulence:
					m_edit.PropField("resolution");
					m_edit.PropField("turbulenceFrequency");
					m_edit.PropField("turbulencePower");
					m_edit.PropField("noiseType");
					m_edit.PropField("frequency");
					if (m_edit.GetProperty("noiseType").enumValueIndex != (int) NoiseType.Voronoi)
					{
						m_edit.PropField("lacunarity");
						m_edit.PropField("octaveCount");
					}
					m_edit.PropField("seed");
					if (GUI.changed)
					{
						m_forceGenerator.needsRebake = true;
						m_forceGenerator.needsPreview = true;
					}
					
					break;
			}

			if (type == (int) ForceType.Turbulence || type == (int)ForceType.TurbulenceTexture)
			{
				m_edit.PropField("noiseExtents");
				m_edit.PropField("smoothness");

				GUI.changed = false;
				m_forceGenerator.previewMode =
					(TCNoiseForceGenerator.PreviewMode) EditorGUILayout.EnumPopup("Preview mode", m_forceGenerator.previewMode);
				if (GUI.changed)
					SceneView.RepaintAll();



				m_edit.PropField("forceTexture");
			}


			if (type == (int) ForceType.Turbulence)
			{

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				string btn = "Update Noise";

				if (m_forceTarget.forceTexture == null)
					btn = "Generate Noise";

				if (GUILayout.Button(btn, GUILayout.Width(150.0f)))
					generate = true;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
		}
		o.ToggleAreaEnd("Force Type");

		m_edit.Apply();

		if (generate)
			m_forceGenerator.GenerateTexture();
	}


	public void OnSceneGUI()
	{
		var f = target as TCForce;

		if (m_primeForce.IsGlobalShape && f != m_primeForce || f == null || f.radius == null)
			return;


		switch (f.shape)
		{
			case ForceShape.Sphere:
				f.radius.Max = m_edit.RadiusHandle(f.transform, f.radius.Max);

				if (!f.radius.IsConstant)
					f.radius.Min = m_edit.RadiusHandle(f.transform, f.radius.Min);

				break;
			case ForceShape.Box:
				f.boxSize = m_edit.CubeHandle(f.transform, f.boxSize);
				break;


			case ForceShape.Capsule:
				float r = f.radius.IsConstant ? f.radius.Value : f.radius.Max;
				Vector2 c = m_edit.CapsuleHandle(f.transform, r, f.height);

				if (f.radius.IsConstant)
					f.radius.Value = c.x;
				else
					f.radius.Max = c.x;
				f.height = c.y;
				break;

			case ForceShape.Disc:
				float rmin = f.radius.IsConstant ? 0.0f : (float) f.radius.Min;
				float rmax = f.radius.Max;
				float round;
				m_edit.DiscHandle(f.transform, rmin, rmax, f.discHeight, f.discRounding, (int) f.discType, out rmin, out rmax,
				                out round);
				f.discRounding = round;
				f.radius.Min = rmin;
				f.radius.Max = rmax;
				break;

			case ForceShape.Hemisphere:

				f.radius.Max = m_edit.HemisphereHandle(f.transform, f.radius.Max);

				break;
		}

		if (f.forceType == ForceType.Turbulence || f.forceType == ForceType.TurbulenceTexture)
			m_forceGenerator.DrawTurbulencePreview();
	}
}