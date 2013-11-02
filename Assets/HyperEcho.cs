using UnityEngine;
using System.Collections;

public class HyperEcho : MonoBehaviour {
    public RenderTexture current, previous, media, depth;
    public ComputeShader echoCompute;
    public Shader depthShader;
    public float timeStep, distanceStep, speedOfSound, damping, frequency, amplitude;
    private bool phase = false;
	public int size = 256;

	// Use this for initialization
	void Start () {

        setupTexture3D(ref current, "Current");
        setupTexture3D(ref previous, "Previous");
        setupTexture3D(ref media, "Media");

        camera.depthTextureMode = DepthTextureMode.Depth;

        //depth = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RFloat);
       // depth.SetGlobalShaderProperty("_Depth");

        Voxelize.media = media;

        
        
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
       // tex.SetGlobalShaderProperty(name);
    }

	void OnDisable(){

        DestroyImmediate(media);
        DestroyImmediate(current);
        DestroyImmediate(previous);
       
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
        Shader.SetGlobalTexture("Current", phase ? current : previous);
        phase = !phase;

        /*
        camera.targetTexture = depth;
        camera.cullingMask = ~(1 << LayerMask.NameToLayer("raymarching box"));
        camera.RenderWithShader(depthShader, "");
        camera.targetTexture = null;
        camera.cullingMask = 1 << LayerMask.NameToLayer("raymarching box");
        */
         echoCompute.Dispatch(0, 32, 32, 32);


	}


}	