using UnityEngine;
using System.Collections;


public class Voxelize : MonoBehaviour {

	public Shader voxelizeShader;


	// Use this for initialization
	void Start () {
		camera.aspect = 1;

		Matrix4x4 P = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
		Matrix4x4 V = camera.worldToCameraMatrix;
		//Matrix4x4 M = debugObject.renderer.localToWorldMatrix;
		Matrix4x4 MVP = P * V;
		Shader.SetGlobalMatrix("zMVP", MVP);

		camera.SetReplacementShader (voxelizeShader, "");

	}
	
	// Update is called once per frame
	void Update () {
	

	}
}
