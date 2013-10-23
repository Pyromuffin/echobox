using UnityEngine;
using System.Collections;


public class Voxelize : MonoBehaviour {

	public Shader voxelizeShader;
    public static RenderTexture media;


	// Use this for initialization
	void Start () {
		camera.aspect = 1;

		Matrix4x4 P = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
		Matrix4x4 V = camera.worldToCameraMatrix;
		Matrix4x4 MVP = P * V;
		Shader.SetGlobalMatrix("zMVP", MVP);

		camera.SetReplacementShader (voxelizeShader, "");

	}

    void OnPreRender()
    {
        Graphics.SetRandomWriteTarget(1, media);



    }
    void OnPostRender()
    {
        Graphics.ClearRandomWriteTargets();

    }
}
