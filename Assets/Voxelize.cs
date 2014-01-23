using UnityEngine;
using System.Collections;


public class Voxelize : MonoBehaviour {

	public Shader voxelizeShader;
    public static RenderTexture media;
 

	// Use this for initialization
	void Start () {
		
        RenderTexture sizeTex = new RenderTexture(256, 256, 0);
        camera.targetTexture = sizeTex;
        camera.aspect = 1;

		Matrix4x4 P = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
		Matrix4x4 V = camera.worldToCameraMatrix;
		Matrix4x4 MVP = P * V;
		Shader.SetGlobalMatrix("zMVP", MVP);


       
	}


    void FixedUpdate()
    {
        Graphics.SetRenderTarget(media);
        GL.Clear(true, true, new Color(300,300,300,300));

        Graphics.SetRandomWriteTarget(1, media);
        camera.RenderWithShader(voxelizeShader, "");
        Graphics.ClearRandomWriteTargets();

    }


}
