using TC;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof (TCParticleSystem))]
public class TCParticleSystemEditor : Editor
{
	private TCEdtiorBase edit;
	private TCParticleSystem syst;

	[SerializeField] private OpenClose o;

	private Material lineMat;
	private Vector3[] lines;

	private void BuildLineMesh()
	{
		if (syst.Emitter == null || syst.Emitter.EmitMesh == null)
			return;

		var vertices = syst.Emitter.EmitMesh.vertices;
		var triangles = syst.Emitter.EmitMesh.triangles;

		lines = new Vector3[triangles.Length];

		for (int i = 0; i < triangles.Length; i += 3)
		{
			lines[i + 0] = vertices[triangles[i]];
			lines[i + 1] = vertices[triangles[i + 1]];
			lines[i + 2] = vertices[triangles[i + 2]];
		}
	}

	private void OnEnable()
	{
		hideFlags = HideFlags.HideAndDontSave;
		edit = CreateInstance<TCEdtiorBase>();
		edit.Init(this);

		syst = target as TCParticleSystem;

		o = edit.GetOpenClose();

		lineMat =
			new Material(
				"Shader \"Lines/Colored Blended\" { SubShader { Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Front Fog { Mode Off } } } }")
				{
					hideFlags = HideFlags.HideAndDontSave,
					shader = {hideFlags = HideFlags.HideAndDontSave}
				};

		BuildLineMesh();
	}

	private void OnDisable()
	{
		DestroyImmediate(edit);

		if (lineMat.shader != null)
			DestroyImmediate(lineMat.shader);

		DestroyImmediate(lineMat);
	}

	private void OnDestroy()
	{
		DestroyImmediate(edit);

		if (lineMat != null)
		{
			DestroyImmediate(lineMat.shader);
			DestroyImmediate(lineMat);
		}
	}


	private enum DampingPopup
	{
		Constant,
		Curve
	}

	private void Space()
	{
		GUILayout.Space(10.0f);
	}


	// Update is called once per frame
	public override void OnInspectorGUI()
	{
		EditorGUIUtility.LookLikeInspector();
		edit.Update();

		GUILayout.BeginVertical("ShurikenEffectBg", GUILayout.Height(100.0f));

		if (o.ToggleArea("Particle System", new Color(1.0f, 0.8f, 0.8f)))
		{
			GUI.enabled = !EditorApplication.isPlaying;
			edit.PropField("_manager._duration", "System Life");
			edit.PropField("_manager.looping");
			edit.PropField("_manager.prewarm");
			edit.PropField("_manager.playOnAwake");
			edit.PropField("_manager.delay", "Start Delay");
			edit.PropField("_manager._maxParticles");

			foreach (TCParticleSystem t in targets)
				t.MaxParticles = Mathf.Clamp(t.MaxParticles, 0, 5000000);

			GUI.enabled = true;

			Space();

			edit.PropField("_manager._simulationSpace");
			edit.PropField("_emitter._inheritVelocity");
			edit.PropField("_manager.shurikenFallback");
		}
		o.ToggleAreaEnd("Particle System");

		if (o.ToggleArea("Emission", new Color(0.8f, 1.0f, 0.8f)))
		{
			edit.PropField("_emitter.emit");
			edit.PropField("_emitter._emissionRate");

			foreach (TCParticleSystem t in targets)
				t.Emitter.EmissionRate = Mathf.Clamp(t.Emitter.EmissionRate, 0, 5000000);

			SerializedProperty bursts = edit.GetProperty("_emitter.bursts");
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(12.0f);
			GUILayout.Label("Bursts", GUILayout.Width(80.0f));
			const float width = 51.0f;
			GUILayout.Label("Time", GUILayout.Width(width));
			GUILayout.Label("Particles", GUILayout.Width(width));
			EditorGUILayout.EndHorizontal();

			int del = -1;
			for (int i = 0; i < bursts.arraySize; ++i)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(85.0f);
				EditorGUILayout.PropertyField(edit.CheckBurstProp("time", i), new GUIContent(""), GUILayout.Width(width));
				EditorGUILayout.PropertyField(edit.CheckBurstProp("amount", i), new GUIContent(""), GUILayout.Width(width));
				GUILayout.Space(10.0f);

				if (GUILayout.Button("", "OL Minus", GUILayout.Width(24.0f)))
					del = i;

				GUILayout.EndHorizontal();
			}
			GUILayout.BeginHorizontal();
			GUILayout.Space(103.0f + 2 * width);
			if (GUILayout.Button("", "OL Plus", GUILayout.Width(24.0f)))
				bursts.InsertArrayElementAtIndex(bursts.arraySize);
			GUILayout.EndHorizontal();

			if (del != -1)
				bursts.DeleteArrayElementAtIndex(del);

			Space();


			edit.PropField("_emitter.pes.shape");

			int s = edit.GetProperty("_emitter.pes.shape").enumValueIndex;

			switch (s)
			{
				case (int) EmitShapes.HemiSphere:
				case (int) EmitShapes.Sphere:
					edit.PropField("_emitter.pes.radius");
					break;
				case (int) EmitShapes.Box:
					edit.PropField("_emitter.pes.cubeSize");
					break;
				case (int) EmitShapes.Cone:
					edit.PropField("_emitter.pes.coneRadius");
					edit.PropField("_emitter.pes.coneHeight");
					edit.PropField("_emitter.pes.coneAngle");
					break;
				case (int) EmitShapes.Ring:
					edit.PropField("_emitter.pes.ringRadius");
					edit.PropField("_emitter.pes.ringOuterRadius");
					break;
				case (int) EmitShapes.Line:
					edit.PropField("_emitter.pes.lineLength");
					edit.PropField("_emitter.pes.lineRadius");
					break;
				case (int) EmitShapes.Mesh:
					edit.PropField("_emitter.pes.emitMesh");
					edit.PropField("_emitter.pes.spawnOnMeshSurface");
					break;
			}

			GUILayout.Space(10.0f);


			edit.PropField("_emitter.pes.startDirectionType");

			int type = edit.GetProperty("_emitter.pes.startDirectionType").enumValueIndex;
			if (type == (int) StartDirectionType.Vector)
			{
				edit.PropField("_emitter.pes.startDirectionVector");
				edit.PropField("_emitter.pes.startDirectionRandomAngle");
			}
		}
		o.ToggleAreaEnd("Emission");

		if (o.ToggleArea("Particles", new Color(0.0f, 0.8f, 1.0f)))
		{
			edit.PropField("_emitter._energy", "Lifetime");

			GUILayout.Space(10.0f);

			edit.PropField("_emitter._speed", "Start Speed");
			edit.PropField("_emitter._velocityOverLifetime");

			Space();

			edit.PropField("_emitter._size", "Start Size");
			edit.PropField("_emitter._sizeOverLifetime");

			Space();

			edit.PropField("_emitter._rotation", "Start Rotation");
			edit.PropField("_emitter._angularVelocity");

			GUILayout.Space(10.0f);

			GUILayout.BeginHorizontal(GUILayout.Width(Screen.width - 28.0f));
			var mode = edit.GetProperty("_particleRenderer.colourGradientMode");
			var colProp = edit.GetProperty("_particleRenderer._colourOverLifetime");

			switch (mode.enumValueIndex)
			{
				case (int) ParticleColourGradientMode.OverLifetime:
					EditorGUILayout.PropertyField(colProp, new GUIContent("Colour over lifetime"));
					break;
				default:
					EditorGUILayout.PropertyField(colProp, new GUIContent("Colour over speed"));
					break;
			}

			mode.enumValueIndex =
				(int)
				((ParticleColourGradientMode)
				 EditorGUILayout.EnumPopup("", (ParticleColourGradientMode) mode.enumValueIndex, GUILayout.Width(28.0f)));

			GUILayout.EndHorizontal();

			if (mode.enumValueIndex != (int) ParticleColourGradientMode.OverLifetime)
				EditorGUILayout.PropertyField(edit.GetProperty("_particleRenderer.maxSpeed"));
		}
		o.ToggleAreaEnd("Particles");


		if (o.ToggleArea("Forces", new Color(1.0f, 1.0f, 0.8f)))
		{
			edit.PropField("_forcesManager._maxForces");
			edit.PropField("_manager.gravityMultiplier");
			edit.PropField("_emitter._constantForce");
			edit.PropField("_forcesManager._massVariance");

			var fmanager = syst.ForceManager;


			EditorGUILayout.BeginHorizontal();
			edit.PropField("_forcesManager._forceLayers");


			if (GUILayout.Button("", "OL Plus", GUILayout.Width(20.0f)))
				fmanager.BaseForces.Add(null);

			EditorGUILayout.EndHorizontal();

			if (fmanager.BaseForces != null)
			{
				int del = -1;
				for (int i = 0; i < fmanager.BaseForces.Count; ++i)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Space(20.0f);
					fmanager.BaseForces[i] =
						EditorGUILayout.ObjectField("Link to ", fmanager.BaseForces[i], typeof (TCForce), true) as TCForce;

					if (GUILayout.Button("", "OL Minus", GUILayout.Width(20.0f)))
						del = i;

					GUILayout.EndHorizontal();
				}

				if (del != -1)
					fmanager.BaseForces.RemoveAt(del);

				fmanager.MaxForces = Mathf.Max(fmanager.MaxForces, fmanager.BaseForces.Count);
			}

			GUILayout.BeginHorizontal(GUILayout.Width(Screen.width - 28.0f));
			SerializedProperty curveProp = edit.GetProperty("_manager.dampingIsCurve");
			EditorGUILayout.PropertyField(
				curveProp.boolValue ? edit.GetProperty("_manager.dampingCurve") : edit.GetProperty("_manager.damping"),
				new GUIContent("Damping"));

			curveProp.boolValue =
				(DampingPopup)
				EditorGUILayout.EnumPopup("", curveProp.boolValue ? DampingPopup.Curve : DampingPopup.Constant,
				                          GUILayout.Width(28.0f)) == DampingPopup.Curve;

			
			GUILayout.EndHorizontal();
			
			edit.PropField("_forcesManager.useBoidsFlocking");

			if (edit.GetProperty("_forcesManager.useBoidsFlocking").boolValue)
			{
				edit.PropField("_forcesManager.boidsPositionStrength");
				edit.PropField("_forcesManager.boidsVelocityStrength");
				edit.PropField("_forcesManager.boidsCenterStrength");
			}
			
		}
		o.ToggleAreaEnd("Forces");


		if (o.ToggleArea("Collision", new Color(1.0f, 0.8f, 1.0f)))
		{
			edit.PropField("_colliderManager._maxColliders");
			edit.PropField("_colliderManager.overrideBounciness");

			if (edit.GetProperty("_colliderManager.overrideBounciness").boolValue)
				edit.PropField("_colliderManager._bounciness");

			edit.PropField("_colliderManager.overrideStickiness");

			if (edit.GetProperty("_colliderManager.overrideStickiness").boolValue)
				edit.PropField("_colliderManager._stickiness");

			edit.PropField("_colliderManager._particleThickness");

			TCParticleColliderManager cmanager = syst.ColliderManager;

			var baseColliders = cmanager.BaseColliders;

			EditorGUILayout.BeginHorizontal();
			edit.PropField("_colliderManager._colliderLayers");

			if (GUILayout.Button("", "OL Plus", GUILayout.Width(20.0f)))
				baseColliders.Add(null);

			EditorGUILayout.EndHorizontal();

			int del = -1;
			for (int i = 0; i < baseColliders.Count; ++i)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(20.0f);
				baseColliders[i] =
					EditorGUILayout.ObjectField("Link to ", baseColliders[i], typeof (TCCollider), true) as TCCollider;

				if (GUILayout.Button("", "OL Minus", GUILayout.Width(20.0f)))
					del = i;

				GUILayout.EndHorizontal();
			}

			if (del != -1)
				baseColliders.RemoveAt(del);

			cmanager.MaxColliders = Mathf.Max(cmanager.MaxColliders, baseColliders.Count);
		}
		o.ToggleAreaEnd("Collision");

		if (o.ToggleArea("Renderer", new Color(0.8f, 1.0f, 1.0f)))
		{
			edit.PropField("_particleRenderer._material");
			edit.PropField("_particleRenderer._renderMode");

			var renderMode = (RenderMode) edit.GetProperty("_particleRenderer._renderMode").enumValueIndex;

			switch (renderMode)
			{
				case RenderMode.Mesh:
					edit.PropField("_particleRenderer._mesh");
					break;
				case RenderMode.TailStretchBillboard:
				case RenderMode.StretchedBillboard:
					edit.PropField("_particleRenderer._lengthScale");
					edit.PropField("_particleRenderer._speedScale");
					if (renderMode == RenderMode.TailStretchBillboard)
					{
						edit.PropField("_particleRenderer.tailUv");
					}
					break;
			}

			if (renderMode != RenderMode.Mesh) {
				edit.PropField("_particleRenderer.isPixelSize");
				edit.PropField("_particleRenderer.spriteSheetAnimation");
			}

			if (edit.GetProperty("_particleRenderer.spriteSheetAnimation").boolValue)
			{
				edit.PropField("_particleRenderer.spriteSheetSpriteWidth");
				edit.PropField("_particleRenderer.spriteSheetSpriteHeight");
			}

			edit.PropField("_particleRenderer.glow");
			edit.PropField("_particleRenderer._useFrustumCulling");

			if (edit.GetProperty("_particleRenderer._useFrustumCulling").boolValue)
			{
				SerializedProperty boundsProp = edit.GetProperty("_particleRenderer._bounds");
				boundsProp.boundsValue = EditorGUILayout.BoundsField(boundsProp.boundsValue);

				edit.PropField("_particleRenderer.culledSimulationMode");

				if (edit.GetProperty("_particleRenderer.culledSimulationMode").enumValueIndex ==
				    (int) CulledSimulationMode.SlowSimulation)
					edit.PropField("_particleRenderer.cullSimulationDelta");
			}
		}
		o.ToggleAreaEnd("Renderer");

		GUILayout.EndVertical();


		if (EditorApplication.isPlaying)
		{
			foreach (TCParticleSystem t in targets)
			{
				t.ParticleRenderer.UpdateColourOverLifetime();
				t.Emitter.UpdateSizeOverLifetime();
			}
		}

		edit.Apply();

		if (GUI.changed)
		{
			EditorUtility.SetDirty(o);


			if (syst.Emitter.Shape == EmitShapes.Mesh)
			{
				BuildLineMesh();
			}
		}
	}

	public void OnSceneGUI()
	{
		TCParticleEmitter e = syst.Emitter;

		if (e == null)
			return;


		var col = new Color(0.6f, 0.9f, 1.0f);

		Handles.color = col;

		switch (e.Shape)
		{
			case EmitShapes.Sphere:


				e.Radius.Max = edit.RadiusHandle(syst.transform, e.Radius.Max, true);
				if (!e.Radius.IsConstant)
				{
					Handles.color = new Color(0.6f, 0.9f, 1.0f, 0.4f);
					if (e.Radius.Min != 0.0f)
						e.Radius.Min = edit.RadiusHandle(syst.transform, e.Radius.Min, true);
					Handles.color = col;
				}


				break;

			case EmitShapes.Box:
				e.CubeSize = edit.CubeHandle(syst.transform, e.CubeSize);
				break;

			case EmitShapes.HemiSphere:


				e.Radius.Value = edit.HemisphereHandle(syst.transform, e.Radius.Max, true);

				if (!e.Radius.IsConstant)
				{
					if (e.Radius.Min != 0.0f)
						e.Radius.Min = edit.HemisphereHandle(syst.transform, e.Radius.Min, true);
				}

				break;

			case EmitShapes.Cone:
				edit.ConeHandle(edit.GetProperty("_emitter.pes.coneAngle"), edit.GetProperty("_emitter.pes.coneHeight"),
				                edit.GetProperty("_emitter.pes.coneRadius"), syst.transform);
				break;

			case EmitShapes.Ring:
				edit.TorusHandle(edit.GetProperty("_emitter.pes.ringRadius"), edit.GetProperty("_emitter.pes.ringOuterRadius"),
				                 syst.transform);
				break;

			case EmitShapes.Line:
				SerializedProperty length = edit.GetProperty("_emitter.pes.lineLength");

				length.floatValue = edit.LineHandle(length.floatValue, syst.transform);


				break;

			case EmitShapes.Mesh:
				if (e.EmitMesh == null || lines == null || lines.Length == 0)
					break;

				lineMat.SetPass(0);

				GL.PushMatrix();
				GL.MultMatrix(Matrix4x4.TRS(syst.transform.position, syst.transform.rotation, syst.transform.localScale));
				GL.Begin(GL.LINES);
				GL.Color(new Color(1.0f, 1.0f, 1.0f, 0.4f));

				for (int i = 0; i < lines.Length / 3; i++)
				{
					GL.Vertex(lines[i * 3]);
					GL.Vertex(lines[i * 3 + 1]);

					GL.Vertex(lines[i * 3 + 1]);
					GL.Vertex(lines[i * 3 + 2]);

					GL.Vertex(lines[i * 3 + 2]);
					GL.Vertex(lines[i * 3]);
				}

				GL.End();
				GL.PopMatrix();

				break;
		}

		serializedObject.ApplyModifiedProperties();
	}


	[DrawGizmo(GizmoType.SelectedOrChild)]
	private static void RenderGizmo(TCParticleSystem syst)
	{
		if (syst.ParticleRenderer == null)
			return;

		if (syst.ParticleRenderer.UseFrustumCulling)
		{
			Gizmos.DrawWireCube(syst.transform.position + syst.ParticleRenderer.Bounds.center,
			                    syst.ParticleRenderer.Bounds.extents);
		}
	}
}