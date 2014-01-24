using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TCEdtiorBase : ScriptableObject
{
	private Editor e;
	private Dictionary<string, SerializedProperty> properties;


	//base API functions
	public SerializedProperty GetProperty(string propName)
	{
		return CheckProp(propName);
	}

	public void Init(Editor editor)
	{
		e = editor;
		hideFlags = HideFlags.HideAndDontSave;
		properties = new Dictionary<string, SerializedProperty>();
	}

	public void Update()
	{
		e.serializedObject.Update();
	}

	public void Apply()
	{
		e.serializedObject.ApplyModifiedProperties();
	}

	private SerializedProperty CheckProp(string varName)
	{
		if (!properties.ContainsKey(varName))
		{
			SerializedProperty p = e.serializedObject.FindProperty(varName);
			if (p != null)
				properties.Add(varName, p);
			else
			{
				Debug.Log("Property not found! " + varName);
				return null;
			}
		}

		return properties[varName];
	}

	private SerializedProperty CheckPropRelative(string baseName, string varName)
	{
		string propName = baseName + "." + varName;

		if (!properties.ContainsKey(propName))
		{
			SerializedProperty b = CheckProp(baseName);

			if (b == null)
				return null;

			SerializedProperty p = b.FindPropertyRelative(varName);

			if (p != null)
				properties.Add(propName, p);
			else
				Debug.Log("Relative property not found!" + varName);
		}

		return properties[propName];
	}


	public void PropField(string varName)
	{
		SerializedProperty prop = CheckProp(varName);

		if (prop != null)
			EditorGUILayout.PropertyField(prop);
	}


	public void PropField(string varName, string dispName)
	{
		EditorGUILayout.PropertyField(CheckProp(varName), new GUIContent(dispName));
	}

	public SerializedProperty CheckBurstProp(string propName, int i)
	{
		string varName = "_emitter.bursts." + propName + i;

		if (!properties.ContainsKey(varName))
		{
			SerializedProperty p = CheckProp("_emitter.bursts").GetArrayElementAtIndex(i).FindPropertyRelative(propName);

			if (p != null)
				properties.Add(varName, p);
			else
				Debug.Log("Property not found! " + varName);
		}

		return properties[varName];
	}

	public OpenClose GetOpenClose()
	{
		var o = AssetDatabase.LoadMainAssetAtPath("Assets/Plugins/Editor/OpenClose.asset") as OpenClose;

		if (o == null)
		{
			o = CreateInstance<OpenClose>();
			AssetDatabase.CreateAsset(o, "Assets/Plugins/Editor/OpenClose.asset");
		}

		return o;
	}


	//=========================================
	//Handle functions

	private float ValueSlider(Vector3 offset, float val, Vector3 dir, float scale)
	{
		Vector3 pos = offset + val * dir * scale;
		Vector3 newPos = Handles.Slider(pos, dir, HandleUtility.GetHandleSize(Vector3.zero) * 0.045f, Handles.DotCap, 0.0f);
		return Mathf.Abs(val + Vector3.Dot(newPos - pos, dir) / scale);
	}

	private void DrawDiscCap(Vector3 p1, Vector3 p2, float mSign, Vector3 h, float hsign, float rounding, Vector3 discNorm,
	                         Vector3 move, Vector3 up, float rMin)
	{
		Vector3 rup = hsign * h + hsign * rounding * up;
		Vector3 mVec = mSign * rounding * move;

		Handles.DrawLine(p2 + h, p2 - h);
		Handles.DrawWireArc(p2 + hsign * h - mVec, mSign * hsign * -discNorm, p2, 90.0f, rounding);


		if (rounding < rMin)
		{
			Handles.DrawLine(p1 + rup + mVec, p2 + rup - mSign * rounding * move);
			Handles.DrawLine(p1 + h, p1 - h);

			Handles.DrawWireArc(p1 + hsign * h + mVec, mSign * hsign * discNorm, -p1, 90.0f, rounding);
		}
		else
		{
			Handles.DrawLine(rup, p2 + rup - mVec);
		}
	}


	private void DrawDiscWiresVertical(float rMin, float rMax, float height, float rounding, float angle)
	{
		Vector3 up = Vector3.up;
		Vector3 right = Vector3.right;
		Vector3 fw = Vector3.forward;


		float h = height / 2.0f - rounding;
		Vector3 hup = h * up;


		Vector3 fw1 = fw * (rMin - rounding);
		Vector3 fw2 = fw * (rMax + rounding);

		Vector3 bck1 = -fw * (rMin - rounding);
		Vector3 bck2 = -fw * (rMax + rounding);

		Vector3 rt1 = right * (rMin - rounding);
		Vector3 rt2 = right * (rMax + rounding);

		Vector3 lt1 = -right * (rMin - rounding);
		Vector3 lt2 = -right * (rMax + rounding);

		DrawDiscCap(fw1, fw2, 1.0f, hup, 1.0f, rounding, right, fw, up, rMin);
		DrawDiscCap(fw1, fw2, 1.0f, hup, -1.0f, rounding, right, fw, up, rMin);


		DrawDiscCap(rt1, rt2, 1.0f, hup, 1.0f, rounding, -fw, right, up, rMin);
		DrawDiscCap(rt1, rt2, 1.0f, hup, -1.0f, rounding, -fw, right, up, rMin);

		if (angle > 90)
		{
			DrawDiscCap(lt1, lt2, -1.0f, hup, 1.0f, rounding, -fw, right, up, rMin);
			DrawDiscCap(lt1, lt2, -1.0f, hup, -1.0f, rounding, -fw, right, up, rMin);
		}

		if (angle > 180)
		{
			DrawDiscCap(bck1, bck2, -1.0f, hup, 1.0f, rounding, right, fw, up, rMin);
			DrawDiscCap(bck1, bck2, -1.0f, hup, -1.0f, rounding, right, fw, up, rMin);
		}
	}

	private void ArcAngle(float angle, float radius, float height, float rounding, float roundMult)
	{
		Handles.DrawWireArc((height / 2.0f - rounding) * Vector3.up, -Vector3.up, Vector3.right, angle,
		                    radius + roundMult * rounding);
		Handles.DrawWireArc(-(height / 2.0f - rounding) * Vector3.up, -Vector3.up, Vector3.right, angle,
		                    radius + roundMult * rounding);

		Handles.DrawWireArc(height / 2.0f * Vector3.up, -Vector3.up, Vector3.right, angle, radius);
		Handles.DrawWireArc(-height / 2.0f * Vector3.up, -Vector3.up, Vector3.right, angle, radius);
	}

	private void DiscValueSlider(float rMin, float rMax, Vector3 dir, float sc, out float rMinOut, out float rMaxOut)
	{
		if (rMin != 0.0f)
			rMin = ValueSlider(Vector3.zero, rMin, dir, sc);

		rMax = ValueSlider(Vector3.zero, rMax, dir, sc);

		rMinOut = rMin;
		rMaxOut = rMax;
	}


	private void DrawBoxCorner(Vector3 sz, float xflip, float yflip, float zflip, float r)
	{
		sz = sz * 0.5f;
		Handles.DrawWireArc(new Vector3(xflip * (sz.x - r), yflip * (sz.y - r), zflip * (sz.z - r)),
		                    yflip * xflip * Vector3.forward,
		                    xflip * Vector3.right, 90.0f, r);
		Handles.DrawWireArc(new Vector3(xflip * (sz.x - r), yflip * (sz.y - r), zflip * (sz.z - r)),
		                    zflip * yflip * Vector3.right,
		                    yflip * Vector3.up, 90.0f, r);
		Handles.DrawWireArc(new Vector3(xflip * (sz.x - r), yflip * (sz.y - r), zflip * (sz.z - r)),
		                    zflip * xflip * Vector3.up,
		                    zflip * Vector3.forward, 90.0f, r);
	}

	private void DrawBoxLine(Vector3 d1, Vector3 d2, Vector3 d3, float r, float x, float y, float z)
	{
		x *= 0.5f;
		y *= 0.5f;
		z *= 0.5f;
		Vector3 pp = (x - r) * d1 + y * d2;
		Vector3 dd = (z - r) * d3;
		Handles.DrawLine(pp - dd, pp + dd);

		Vector3 pph = x * d1 + (y - r) * d2;
		Handles.DrawLine(pph - dd, pph + dd);
	}


	private float RadiusDisc(Vector3 offset, Vector3 norm, Vector3 right, Vector3 forward, float r, float sc)
	{
		Handles.DrawWireDisc(offset, norm, r);

		float newVal = ValueSlider(offset, r, right, sc);

		if (newVal == r)
			newVal = ValueSlider(offset, r, -right, sc);

		if (newVal == r)
			newVal = ValueSlider(offset, r, forward, sc);

		if (newVal == r)
			newVal = ValueSlider(offset, r, -forward, sc);

		return newVal;
	}


	private void BaseHandle(Transform trans, Vector3 scale)
	{
		Handles.matrix = Matrix4x4.TRS(trans.position, trans.rotation, scale);
	}

	private float ScaleXZ(Transform trans)
	{
		return Mathf.Max(trans.localScale.x, trans.localScale.z);
	}

	private float ScaleTrans(Transform trans)
	{
		return Mathf.Max(trans.localScale.x, trans.localScale.y, trans.localScale.z);
	}


	public float LineHandle(float length, Transform trans)
	{
		BaseHandle(trans, Vector3.one);
		Handles.DrawLine(Vector3.zero, Vector3.forward * length * trans.localScale.z);
		return ValueSlider(Vector3.zero, length, Vector3.forward, trans.localScale.z);
	}


	public Vector3 CubeHandle(Transform trans, Vector3 size)
	{
		BaseHandle(trans, Vector3.one);
		Vector3 orig = size;

		size.x = ValueSlider(Vector3.zero, size.x * 0.5f, Vector3.right, trans.localScale.x) * 2.0f;
		size.y = ValueSlider(Vector3.zero, size.y * 0.5f, Vector3.up, trans.localScale.y) * 2.0f;
		size.z = ValueSlider(Vector3.zero, size.z * 0.5f, Vector3.forward, trans.localScale.z) * 2.0f;

		if (size.x == orig.x)
			size.x = ValueSlider(Vector3.zero, size.x * 0.5f, -Vector3.right, trans.localScale.x) * 2.0f;

		if (size.y == orig.y)
			size.y = ValueSlider(Vector3.zero, size.y * 0.5f, -Vector3.up, trans.localScale.y) * 2.0f;

		if (size.z == orig.z)
			size.z = ValueSlider(Vector3.zero, size.z * 0.5f, -Vector3.forward, trans.localScale.z) * 2.0f;

		return size;
	}


	public float HemisphereHandle(Transform trans, float radius, bool nonUniform = false)
	{
		Vector3 scale;
		float sc = 1.0f;

		if (nonUniform)
			scale = trans.localScale;
		else
		{
			sc = ScaleTrans(trans);
			scale = new Vector3(sc, sc, sc);
		}


		BaseHandle(trans, scale);

		Handles.DrawWireArc(Vector3.zero, Vector3.forward, Vector3.right, 180.0f, radius);
		Handles.DrawWireArc(Vector3.zero, Vector3.right, -Vector3.forward, 180.0f, radius);
		Handles.DrawWireDisc(Vector3.zero, Vector3.up, radius * sc);

		var newVal = ValueSlider(Vector3.zero, radius, Vector3.up, 1.0f);

		if (newVal == radius)
			newVal = ValueSlider(Vector3.zero, radius, Vector3.right, 1.0f);

		if (newVal == radius)
			newVal = ValueSlider(Vector3.zero, radius, -Vector3.right, 1.0f);

		if (newVal == radius)
			newVal = ValueSlider(Vector3.zero, radius, Vector3.forward, 1.0f);

		if (newVal == radius)
			newVal = ValueSlider(Vector3.zero, radius, -Vector3.forward, 1.0f);

		return newVal;
	}


	public void ConeHandle(SerializedProperty angle, SerializedProperty height, SerializedProperty radius, Transform trans,
	                       bool flip = false)
	{
		BaseHandle(trans, trans.localScale);


		float r = radius.floatValue;
		float r2 = Mathf.Tan(angle.floatValue * Mathf.Deg2Rad) * height.floatValue;
		float f = flip ? -1 : 1;
		float ret = RadiusDisc(Vector3.zero, Vector3.forward, Vector3.right, Vector3.up, r, 1.0f);

		Vector3 up = height.floatValue * Vector3.forward;

		Handles.DrawLine(-Vector3.right * r, up - Vector3.right * (f * r2 + r));
		Handles.DrawLine(Vector3.right * r, up + Vector3.right * (f * r2 + r));

		Handles.DrawLine(-Vector3.up * r, up - Vector3.up * (f * r2 + r));
		Handles.DrawLine(Vector3.up * r, up + Vector3.up * (f * r2 + r));


		if (height.floatValue != 0)
			angle.floatValue =
				Mathf.Clamp(
					Mathf.Atan((RadiusDisc(up, Vector3.forward, Vector3.right, Vector3.up, (r2 + r), 1.0f) - r) / height.floatValue) *
					Mathf.Rad2Deg, 0.0f, 90.0f);

		height.floatValue = ValueSlider(Vector3.zero, height.floatValue, Vector3.forward, 1.0f);
		radius.floatValue = ret;
	}

	public float RadiusHandle(Transform trans, float radius, bool nonUniform = false)
	{
		float sc = 1.0f;
		Vector3 nsc = Vector3.one;

		if (nonUniform)
			nsc = trans.localScale;
		else
			sc = ScaleTrans(trans);

		Handles.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, nsc);
		return
			Handles.RadiusHandle(trans.rotation,
			                     Vector3.Scale(trans.position, new Vector3(1.0f / nsc.x, 1.0f / nsc.y, 1.0f / nsc.z)),
			                     radius * sc) / sc;
	}


	public void TorusHandle(SerializedProperty radius, SerializedProperty outerRadius, Transform trans)
	{
		BaseHandle(trans, trans.localScale);
		Vector2 ret;

		float r1 = radius.floatValue;
		float r2 = outerRadius.floatValue;

		ret.x = RadiusDisc(Vector3.zero, Vector3.forward, Vector3.right, Vector3.up, r1, 1.0f);

		Color c = Handles.color;

		Handles.color = new Color(c.r, c.g, c.b, 0.4f);

		Handles.DrawWireDisc(Vector3.zero, Vector3.forward, (r1 - r2));
		Handles.DrawWireDisc(Vector3.zero, Vector3.forward, (r1 + r2));

		Handles.DrawWireDisc(Vector3.zero + 0.5f * r2 * Vector3.forward, Vector3.forward, (r1 - 0.5f * Mathf.Sqrt(2.0f) * r2));
		Handles.DrawWireDisc(Vector3.zero - 0.5f * r2 * Vector3.forward, Vector3.forward, (r1 - 0.5f * Mathf.Sqrt(2.0f) * r2));

		Handles.DrawWireDisc(Vector3.zero + 0.5f * r2 * Vector3.forward, Vector3.forward, (r1 + 0.5f * Mathf.Sqrt(2.0f) * r2));
		Handles.DrawWireDisc(Vector3.zero - 0.5f * r2 * Vector3.forward, Vector3.forward, (r1 + 0.5f * Mathf.Sqrt(2.0f) * r2));

		Handles.DrawWireDisc(Vector3.zero + r2 * Vector3.forward, Vector3.forward, r1);
		Handles.DrawWireDisc(Vector3.zero - r2 * Vector3.forward, Vector3.forward, r1);

		Vector3 cross1 = (Vector3.up + Vector3.right).normalized;
		Vector3 cross2 = (Vector3.up - Vector3.right).normalized;

		Handles.DrawWireDisc(Vector3.zero + cross1 * r1, Vector3.Cross(cross1, Vector3.forward), r2);
		Handles.DrawWireDisc(Vector3.zero - cross1 * r1, Vector3.Cross(cross1, Vector3.forward), r2);

		Handles.DrawWireDisc(Vector3.zero + cross2 * r1, Vector3.Cross(cross2, Vector3.forward), r2);
		Handles.DrawWireDisc(Vector3.zero - cross2 * r1, Vector3.Cross(cross2, Vector3.forward), r2);


		Handles.color = c;

		ret.y = r2;
		ret.y = RadiusDisc(Vector3.up * r1, Vector3.right, Vector3.forward, Vector3.up, ret.y, 1.0f);

		if (ret.y == r2)
			ret.y = RadiusDisc(Vector3.right * r1, Vector3.up, Vector3.right, Vector3.forward, ret.y, 1.0f);

		if (ret.y == r2)
			ret.y = RadiusDisc(-Vector3.right * r1, Vector3.up, Vector3.right, Vector3.forward, ret.y, 1.0f);

		if (ret.y == r2)
			ret.y = RadiusDisc(-Vector3.up * r1, Vector3.right, Vector3.forward, Vector3.up, ret.y, 1.0f);

		ret.y = Mathf.Min(ret.x, ret.y);

		radius.floatValue = ret.x;
		outerRadius.floatValue = ret.y;
	}


	public void DiscHandle(Transform trans, float rMin, float rMax, float height, float rounding, int mode,
	                       out float outRMin, out float outRMax, out float outRounding)
	{
		BaseHandle(trans, Vector3.one);
		float angle = 360.0f / Mathf.Pow(2.0f, mode);

		float xzs = ScaleXZ(trans);
		float ys = trans.localScale.y;

		DiscValueSlider(rMin, rMax, Vector3.right, xzs, out rMin, out rMax);
		DiscValueSlider(rMin, rMax, Vector3.forward, xzs, out rMin, out rMax);

		if (angle > 90.0f)
			DiscValueSlider(rMin, rMax, -Vector3.right, xzs, out rMin, out rMax);

		if (angle > 180.0f)
			DiscValueSlider(rMin, rMax, -Vector3.forward, xzs, out rMin, out rMax);

		if (height != 0.0f)
		{
			ArcAngle(angle, rMax * xzs, height * ys, rounding, 1.0f);


			if (rMin - rounding > 0.0f)
				ArcAngle(angle, rMin * xzs, height * ys, rounding, -1.0f);
		}

		DrawDiscWiresVertical(rMin * xzs, rMax * xzs, height * ys, rounding, angle);

		rMin = Mathf.Max(Mathf.Min(rMin, rMax), 0.0f);
		rMax = Mathf.Max(Mathf.Max(rMin, rMax), 0.0f);

		outRMin = rMin;
		outRMax = rMax;

		outRounding = Mathf.Clamp(rounding, 0.0f, height / 2.0f);
	}


	public Vector3 RoundedCubeHandle(Vector3 sz, float r, Transform trans)
	{
		BaseHandle(trans, Vector3.one);

		Vector3 szOrig = sz;

		sz = Vector3.Scale(sz, trans.localScale);

		DrawBoxCorner(sz, 1.0f, 1.0f, 1.0f, r);
		DrawBoxCorner(sz, -1.0f, 1.0f, 1.0f, r);

		DrawBoxCorner(sz, 1.0f, -1.0f, 1.0f, r);
		DrawBoxCorner(sz, -1.0f, -1.0f, 1.0f, r);

		DrawBoxCorner(sz, 1.0f, -1.0f, -1.0f, r);
		DrawBoxCorner(sz, 1.0f, 1.0f, -1.0f, r);

		DrawBoxCorner(sz, -1.0f, -1.0f, -1.0f, r);
		DrawBoxCorner(sz, -1.0f, 1.0f, -1.0f, r);

		DrawBoxLine(Vector3.right, Vector3.up, Vector3.forward, r, sz.x, sz.y, sz.z);
		DrawBoxLine(-Vector3.right, Vector3.up, Vector3.forward, r, sz.x, sz.y, sz.z);

		DrawBoxLine(Vector3.right, -Vector3.up, Vector3.forward, r, sz.x, sz.y, sz.z);
		DrawBoxLine(-Vector3.right, -Vector3.up, Vector3.forward, r, sz.x, sz.y, sz.z);

		DrawBoxLine(-Vector3.up, -Vector3.forward, Vector3.right, r, sz.y, sz.z, sz.x);
		DrawBoxLine(-Vector3.up, Vector3.forward, Vector3.right, r, sz.y, sz.z, sz.x);

		DrawBoxLine(Vector3.up, -Vector3.forward, Vector3.right, r, sz.y, sz.z, sz.x);
		DrawBoxLine(Vector3.up, Vector3.forward, Vector3.right, r, sz.y, sz.z, sz.x);

		DrawBoxLine(-Vector3.right, -Vector3.forward, Vector3.up, r, sz.x, sz.z, sz.y);
		DrawBoxLine(Vector3.right, -Vector3.forward, Vector3.up, r, sz.x, sz.z, sz.y);

		DrawBoxLine(-Vector3.right, Vector3.forward, Vector3.up, r, sz.x, sz.z, sz.y);
		DrawBoxLine(Vector3.right, Vector3.forward, Vector3.up, r, sz.x, sz.z, sz.y);

		return szOrig;
	}


	public Vector2 CapsuleHandle(Transform transform, float radius, float height)
	{
		BaseHandle(transform, Vector3.one);

		float xzs = ScaleXZ(transform);
		float ys = transform.localScale.y;

		var newVal = ValueSlider(Vector3.zero, radius, Vector3.right, xzs);

		if (newVal == radius)
			newVal = ValueSlider(Vector3.zero, radius, -Vector3.right, xzs);

		if (newVal == radius)
			newVal = ValueSlider(Vector3.zero, radius, Vector3.forward, xzs);

		if (newVal == radius)
			newVal = ValueSlider(Vector3.zero, radius, -Vector3.forward, xzs);


		var newHeight = ValueSlider(Vector3.zero, 0.5f * height, Vector3.up, xzs) * 2.0f;

		if (newHeight == height)
			newHeight = ValueSlider(Vector3.zero, 0.5f * height, -Vector3.up, xzs) * 2.0f;

		float offset = Mathf.Max(0.0f, (0.5f * newHeight - newVal));

		Vector3 ymax = offset * Vector3.up * ys;
		Vector3 ymin = -offset * Vector3.up * ys;

		Handles.DrawWireDisc(ymax, Vector3.up, radius * xzs);
		Handles.DrawWireDisc(ymin, -Vector3.up, radius * xzs);

		Handles.DrawLine(ymin + radius * xzs * Vector3.right, ymax + radius * xzs * Vector3.right);
		Handles.DrawLine(ymin + radius * xzs * Vector3.forward, ymax + radius * xzs * Vector3.forward);
		Handles.DrawLine(ymin + radius * xzs * -Vector3.right, ymax + radius * xzs * -Vector3.right);
		Handles.DrawLine(ymin + radius * xzs * -Vector3.forward, ymax + radius * xzs * -Vector3.forward);


		Handles.DrawWireArc(ymax, Vector3.forward, Vector3.right, 180.0f, radius * xzs);
		Handles.DrawWireArc(ymax, Vector3.right, -Vector3.forward, 180.0f, radius * xzs);

		Handles.DrawWireArc(ymin, Vector3.forward, -Vector3.right, 180.0f, radius * xzs);
		Handles.DrawWireArc(ymin, Vector3.right, Vector3.forward, 180.0f, radius * xzs);

		return new Vector2(newVal, Mathf.Max(newHeight, newVal * 2.0f));
	}
}