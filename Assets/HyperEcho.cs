using UnityEngine;
using System.Collections;

public class HyperEcho : MonoBehaviour {
	public RenderTexture media;
	public int size = 512;

	// Use this for initialization
	void Start () {
		//check if those buffers exist
		DestroyImmediate (media);


		//make those buffers
		media = new RenderTexture(this.size, this.size, 0 ,RenderTextureFormat.RFloat);
		media.enableRandomWrite = true;
		media.isVolume = true;
		media.volumeDepth = size;
		media.Create ();

		Shader.SetGlobalTexture (0, media);

	
	}

	void OnDisable(){

		DestroyImmediate (media);
	}

	// Update is called once per frame
	void Update () {
	
	}

	void OnPreCull(){

		Vector3 vector = camera.ViewportToWorldPoint(new Vector3(0f, 0f, camera.nearClipPlane));
		Raymarching.rayMat.SetVector("screenCorner", new Vector4(vector.x, vector.y, vector.z));



	}
}	