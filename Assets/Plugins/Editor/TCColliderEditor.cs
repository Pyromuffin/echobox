using TC;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (TCCollider))]
[CanEditMultipleObjects]
public class TCColliderEditor : Editor
{
	[SerializeField] [Range(0.0f, 1.0f)] private float bounciness;

	public float Bounciness
	{
		get { return bounciness; }
		set { bounciness = value; }
	}

	[SerializeField] [Range(0.0f, 1.0f)] private float inheritVelocity;

	public float InheritVelocity
	{
		get { return inheritVelocity; }
		set { inheritVelocity = value; }
	}

	[Range(0.0f, 1.0f)] [SerializeField] private float particleLifeLoss;

	public float ParticleLifeLoss
	{
		get { return particleLifeLoss; }
		set { particleLifeLoss = value; }
	}


	private TCEdtiorBase edit;

	private void OnEnable()
	{
		edit = CreateInstance<TCEdtiorBase>();
		edit.Init(this);
	}

	// Update is called once per frame
	public override void OnInspectorGUI()
	{
		EditorGUIUtility.LookLikeInspector();
		edit.Update();

		var shape = (ColliderShape) edit.GetProperty("shape").enumValueIndex;

		edit.PropField("shape");

		switch (shape)
		{
			case ColliderShape.Disc:
				edit.PropField("radius");
				edit.PropField("rounding");
				edit.PropField("discHeight");
				edit.PropField("discType");
				break;

			case ColliderShape.Hemisphere:
				edit.PropField("radius");
				break;

			case ColliderShape.RoundedBox:
				edit.PropField("boxSize");
				edit.PropField("rounding");
				break;
		}


		GUILayout.Space(10.0f);


		if (shape != ColliderShape.Terrain)
		{
			edit.PropField("inverse");
			edit.PropField("_inheritVelocity");
			edit.PropField("_particleLifeLoss");
		}
		edit.PropField("_bounciness");
		edit.PropField("_stickiness");

		edit.Apply();
	}


	public void OnSceneGUI()
	{
		var c = target as TCCollider;

		if (c == null) return;

		switch (c.shape)
		{
			case ColliderShape.Disc:
				if (c.radius == null)
					return;

				float rmin = c.radius.IsConstant ? 0.0f : (float) c.radius.Min;
				float rmax = c.radius.Max;

				float round;
				edit.DiscHandle(c.transform, rmin, rmax, c.discHeight, c.rounding, (int) c.discType, out rmin, out rmax, out round);

				c.rounding = round;

				c.radius.Min = rmin;
				c.radius.Max = rmax;

				break;

			case ColliderShape.Hemisphere:
				c.radius.Max = edit.HemisphereHandle(c.transform, c.radius.Value);
				break;

			case ColliderShape.RoundedBox:
				Vector3 sz = c.boxSize;

				float r = c.rounding;
				c.rounding = Mathf.Clamp(c.rounding, 0.0f, Mathf.Min(c.boxSize.x, c.boxSize.y, c.boxSize.z) * 0.5f);
				c.boxSize = edit.RoundedCubeHandle(sz, r, c.transform);

				break;
		}
	}
}