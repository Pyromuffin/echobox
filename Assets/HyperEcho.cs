using UnityEngine;
using System.Collections;

public class HyperEcho : MonoBehaviour {
	public RenderTexture current, previous;
    public RenderTexture media;
    public ComputeShader echoCompute;
    public float timeStep, distanceStep, speedOfSound, damping, frequency, amplitude;
    private bool phase = false;
	public int size = 256;

	// Use this for initialization
	void Start () {

        setupTexture3D(ref current, "Current");
        setupTexture3D(ref previous, "Previous");
        setupTexture3D(ref media, "Media");
        //media = new ComputeBuffer(256 * 256 * 256, 8);
        Voxelize.media = media;

        //echoCompute.SetBuffer(0, "Media", media);

        
	}

    void setupTexture3D(ref RenderTexture tex, string name)
    {
        //check if those buffers exist
        DestroyImmediate(tex);

        //make those buffers
        tex = new RenderTexture(this.size, this.size, 0, RenderTextureFormat.RFloat);
        tex.enableRandomWrite = true;
        tex.isVolume = true;
        tex.volumeDepth = size;
        tex.Create();
        tex.SetGlobalShaderProperty(name);
    }

	void OnDisable(){

        DestroyImmediate(media);
        DestroyImmediate(current);
        DestroyImmediate(previous);
        Graphics.ClearRandomWriteTargets();
	}

	// Update is called once per frame
	void Update () {
        echoCompute.SetFloat("timeStep", timeStep);
        echoCompute.SetFloat("distanceStep", distanceStep);
        echoCompute.SetFloat("speedOfSound", speedOfSound);
        echoCompute.SetFloat("damping", damping);
        echoCompute.SetFloat("chaos", Mathf.Sin(Time.timeSinceLevelLoad * frequency) * amplitude );


        echoCompute.SetTexture(0, "Media", media);
        echoCompute.SetTexture(0, "Current", phase ? current : previous);
        echoCompute.SetTexture(0, "Previous", phase ? previous : current);
       

        phase = !phase;

        //echoCompute.Dispatch(0, 32, 32, 32);


	}

	void OnPreCull(){

		



	}
}	