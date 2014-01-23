using UnityEngine;
using System.Collections;

public class HyperEcho : MonoBehaviour {
    public RenderTexture current, previous, media, next;
    public ComputeShader echoCompute;
    public Shader depthShader;
    public float timeFactor, distanceStep, speedOfSound, damping, frequency, amplitude;
    private int phase = 0;
	public int size = 256;

	// Use this for initialization
	void Start () {

        setupTexture3D(ref current, "Current");
        setupTexture3D(ref previous, "Previous");
        setupTexture3D(ref media, "Media");
        setupTexture3D(ref next, "Next");
        var camroids = GetComponentsInChildren<Camera>();
        camroids[0].depthTextureMode = DepthTextureMode.Depth;
        camroids[1].depthTextureMode = DepthTextureMode.Depth;

        //depth = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RFloat);
       // depth.SetGlobalShaderProperty("_Depth");

        Voxelize.media = media;

        
        
	}

    void setupTexture3D(ref RenderTexture tex, string name)
    {
        //check if those buffers exist
        DestroyImmediate(tex);

        //make those buffers
        tex = new RenderTexture(this.size, this.size, 0, RenderTextureFormat.ARGBFloat);
        tex.enableRandomWrite = true;
        tex.isVolume = true;
        tex.volumeDepth = size;
        tex.Create();
        //tex.SetGlobalShaderProperty(name);
    }

	void OnDisable(){

        DestroyImmediate(media);
        DestroyImmediate(current);
        DestroyImmediate(previous);
        DestroyImmediate(next);
       
	}
    float accumulator = 0;
    Color color;
	// Update is called once per frame
	void FixedUpdate () {
        echoCompute.SetFloat("timeStep", Time.fixedDeltaTime/ timeFactor);
        echoCompute.SetFloat("distanceStep", distanceStep);
        echoCompute.SetFloat("speedOfSound", speedOfSound);
        echoCompute.SetFloat("damping", damping);
        if (accumulator > 1)
        {
            color = new Color(Random.value, Random.value, Random.value);
            accumulator = 0;
        }
        accumulator += Time.fixedDeltaTime * frequency;

        echoCompute.SetVector("chaos", new Vector4( color.r, color.g, color.b, Mathf.Sin(Time.timeSinceLevelLoad * frequency * amplitude) ) );


        echoCompute.SetTexture(0, "Media", media);
        if (phase == 0)
        {
            echoCompute.SetTexture(0, "Next", next);
            echoCompute.SetTexture(0, "Current", current );
            echoCompute.SetTexture(0, "Previous", previous );
            
            Shader.SetGlobalTexture("Current", next );
            phase++;
        }
        else if (phase == 1)
        {
            echoCompute.SetTexture(0, "Next", previous);
            echoCompute.SetTexture(0, "Current", next);
            echoCompute.SetTexture(0, "Previous", current);
            
            Shader.SetGlobalTexture("Current", previous);
            phase++;
        }

        else if (phase == 2)
        {
            echoCompute.SetTexture(0, "Next", current);
            echoCompute.SetTexture(0, "Current", previous);
            echoCompute.SetTexture(0, "Previous", next);
            
            Shader.SetGlobalTexture("Current", current);
            phase = 0;
        }
        
        

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