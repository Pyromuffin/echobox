using System;
using LibNoise;
using TC;
using UnityEditor;
using UnityEngine;

[Serializable]
public class TCNoiseForceGenerator
{

	private Vector3[,,] gradient;

	private Mesh arrowMesh;
	private Material arrowMat;

	private TCForce target;

	private int Res
	{
		get { return (int) target.resolution; }
	}


	private IModule genx;
	private IModule geny;
	private IModule genz;

	//keep libnoise namepsace prefix, as it's names are incredibly generic
	private IModule GetGen(int seed)
	{
		IModule bas = null;

		float frequency = target.frequency * 0.01f;


		int octaveCount = Mathf.Max(target.octaveCount, 1);
		float lacunarity = target.lacunarity;

		switch (target.noiseType)
		{
			case NoiseType.Billow:
				var b = new Billow
					        {Frequency = frequency, OctaveCount = octaveCount, Lacunarity = lacunarity, Seed = seed};
				bas = b;
				break;

			case NoiseType.Pink:
				var p = new Perlin
					        {Frequency = frequency, OctaveCount = octaveCount, Lacunarity = lacunarity, Seed = seed};
				bas = p;
				break;

			case NoiseType.Ridged:
				var r = new RidgedMultifractal
					        {
						        Frequency = frequency,
						        OctaveCount = target.octaveCount,
						        Seed = seed,
						        Lacunarity = lacunarity,
					        };
				bas = r;
				break;

			case NoiseType.Voronoi:
				var v = new Voronoi {Frequency = target.frequency * 0.05f, Seed = seed};
				bas = v;
				break;

			default:
				Debug.Log(
					"Holy shit, you must have messed up baaaaad. Wait, I'm selling source code versions. Quick act proffesional! 0x55E032 Exception: Enumerator in <NoiseType> exception, default switch, exception. Will this exception be caught exception? ...Exception");
				break;
		}

		var t = new Turbulence(bas)
			        {Frequency = target.turbulenceFrequency, Power = target.turbulencePower, Seed = seed + 789};

		return t;
	}

	private void GenerateNoiseGenerator()
	{
		genx = GetGen(target.seed);
		geny = GetGen(target.seed + 123);
		genz = GetGen(target.seed + 1234);
	}

	private Vector3 GetPos(int x, int y, int z)
	{
		return new Vector3((x / (float) Res - 0.5f),
		                   (y / (float) Res - 0.5f),
		                   (z / (float) Res - 0.5f));
	}

	private Vector3 GetValue(int x, int y, int z)
	{
		float xx = Mathf.Clamp((float) genx.GetValue(x, y, z), -1.0f, 1.0f);
		float yy = Mathf.Clamp((float) geny.GetValue(x, y, z), -1.0f, 1.0f);
		float zz = Mathf.Clamp((float) genz.GetValue(x, y, z), -1.0f, 1.0f);

		return new Vector3(xx, yy, zz);
	}



	public TCNoiseForceGenerator(TCForce target)
	{
		this.target = target;
	}

	private bool NeedsNewTexture()
	{
		return target.forceTexture == null || Res != target.forceTexture.width || Res != target.forceTexture.height ||
		       Res != target.forceTexture.depth;
	}


	public void GenerateTexture()
	{
		GenerateNoiseGenerator();
		gradient = new Vector3[Res,Res,Res];

		for (int z = 0; z < Res; ++z)
		{
			for (int y = 0; y < Res; ++y)
			{
				for (int x = 0; x < Res; ++x)
				{
					gradient[x, y, z] = GetValue(x, y, z);
				}
			}
			EditorUtility.DisplayProgressBar("Generating noise", "Generating noise...",
			                                 (z * Res * Res) / (float) (Res * Res * Res));
		}

		EditorUtility.ClearProgressBar();

		target.GenerateTexture(gradient, NeedsNewTexture());

		if (!EditorUtility.IsPersistent(target.forceTexture))
		{
			AssetDatabase.CreateAsset(target.forceTexture, AssetDatabase.GenerateUniqueAssetPath("Assets/TC Noise Force.asset"));
			Selection.activeObject = target.forceTexture;
		}
		needsRebake = false;
	}


	public bool needsRebake = true;
	public bool needsPreview = true;

	private Vector4[,] previewGradient;
	private float[,] previewDeriv;

	public enum PreviewMode
	{
		Stability,
		MagnitudeX,
		MagnitudeY,
		MagnitueZ
	}

	public PreviewMode previewMode;


	private void GenerateNoisePreview()
	{
		GenerateNoiseGenerator();

		previewGradient = new Vector4[Res + 1, Res + 1];


		int y = Res / 2;


		for (int z = 0; z < Res; ++z)
		{
			for (int x = 0; x < Res; ++x)
			{
				Vector3 val = GetValue(x, y, z);
				float mag = val.magnitude;
				val = val.normalized;
				previewGradient[x, z] = new Vector4(val.x, val.y, val.z, mag);

			}
		}


		CalcPreviewDeriv(Res, Res);

		needsPreview = false;
	}

	private Vector3 DecodeVector(Color encoded)
	{
		return new Vector3(encoded.r * 2.0f - 1.0f,
		                   encoded.g * 2.0f - 1.0f,
		                   encoded.b * 2.0f - 1.0f);
	}

	private void GenerateTexturePreview()
	{
		if (target.forceTexture == null)
			return;

		int xres = target.forceTexture.width;
		int yres = target.forceTexture.height;
		int zres = target.forceTexture.depth;

		previewGradient = new Vector4[xres + 1, zres + 1];


		Color[] cols = target.forceTexture.GetPixels(0);

		int y = target.forceTexture.height / 2;


		for (int z = 0; z < zres; ++z)
		{
			for (int x = 0; x < xres; ++x)
			{
				Vector3 val = DecodeVector(cols[x + y * xres + z * xres * yres]);

				float mag = val.magnitude;
				val = val.normalized;
				previewGradient[x, z] = new Vector4(val.x, val.y, val.z, mag);
			}
		}

		CalcPreviewDeriv(xres, zres);
	}

	private void CalcPreviewDeriv(int xres, int yres)
	{
		previewDeriv = new float[xres, yres];

		for (int x = 0; x < xres - 1; ++x)
		{
			for (int z = 0; z < yres - 1; ++z)
			{
				float derivx = (previewGradient[x + 1, z].normalized - previewGradient[x, z].normalized).magnitude;
				float derivz = (previewGradient[x, z + 1].normalized - previewGradient[x, z].normalized).magnitude;

				float d = Mathf.Pow(Mathf.Max(derivx, derivz), 2.0f);
				d *= 18.0f;

				previewDeriv[x, z] = d;
			}
		}

		needsPreview = false;
	}

	private void GeneratePreview()
	{
		if (target.forceType == ForceType.Turbulence)
			GenerateNoisePreview();
		else
			GenerateTexturePreview();
	}

	public void DrawTurbulencePreview()
	{
		if (previewGradient == null || needsPreview)
			GeneratePreview();


		if (arrowMesh == null || arrowMat == null)
		{
			arrowMat = Resources.Load("ArrowMat", typeof (Material)) as Material;
			arrowMesh = Resources.Load("Arrow1", typeof (Mesh)) as Mesh;
		}

		int y = Res / 2;

		Vector3 pos = target.transform.position;
		Vector3 ext = target.noiseExtents;

		Transform trans = target.transform;
		Quaternion parentRot = trans.rotation;
		Vector3 localScale = trans.localScale;

		float sc = Mathf.Min(target.noiseExtents.x / Res, target.noiseExtents.z / Res) * 2.0f + 0.01f;

		for (int x = 0; x < Res; ++x)
		{
			for (int z = 0; z < Res; ++z)
			{
				if (previewGradient == null || previewGradient[x, z].w == 0.0f)
					continue;

				Quaternion rot = parentRot * Quaternion.LookRotation(previewGradient[x, z]) * Quaternion.Euler(90.0f, 0.0f, 0.0f);

				float str = 0.0f;
				switch (previewMode)
				{
					case PreviewMode.MagnitudeX:
						str = Mathf.Abs(previewGradient[x, z].x);
						break;

					case PreviewMode.MagnitudeY:
						str = Mathf.Abs(previewGradient[x, z].y);
						break;

					case PreviewMode.MagnitueZ:
						str = Mathf.Abs(previewGradient[x, z].z);
						break;

					case PreviewMode.Stability:
						str = previewDeriv[x, z];
						break;
				}

				var col = new Color(str * 2.1f, 2.0f - str * 2.0f, 0.0f);

				if (arrowMat == null) continue;

				arrowMat.color = col;
				arrowMat.SetPass(0);

				Graphics.DrawMeshNow(arrowMesh,
				                     Matrix4x4.TRS(
					                     pos + parentRot * Vector3.Scale(GetPos(x, y, z), Vector3.Scale(ext, localScale)),
					                     rot, 0.125f * previewGradient[x, z].w * sc * Vector3.one));
			}
		}

		if (needsRebake) Gizmos.color = Color.white;
	}
}