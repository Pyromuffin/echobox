using UnityEngine;
using System.Collections;

public class HyperEcho : MonoBehaviour {
    public RenderTexture current, previous, media, next, RMtex;
    public ComputeShader echoCompute;
    public Shader depthShader;
    public float timeFactor, distanceStep, speedOfSound, damping, frequency, amplitude, hueDamp;
    private int phase = 0;
	public int size = 256;
    private Raymarching raymarching;
    public bool VR = true;
    public float huePower;
    public GameObject cube;

	// Use this for initialization
	void Start () {
        raymarching = FindObjectOfType<Raymarching>();
        setupTexture3D(ref current, "Current");
        setupTexture3D(ref previous, "Previous");
        setupTexture3D(ref media, "Media");
        setupTexture3D(ref next, "Next");
        //setupRMTex3D(ref RMtex, "RayMarching");
        
        var camroids = GetComponentsInChildren<Camera>();
        camroids[0].depthTextureMode = DepthTextureMode.Depth;
        if(VR)
            camroids[1].depthTextureMode = DepthTextureMode.Depth;

        Voxelize.media = media;

        InvokeRepeating("changeColor", 0, 1);
     
	}

    void setupTexture3D(ref RenderTexture tex, string name)
    {
        //check if those buffers exist
        DestroyImmediate(tex);

        //make those buffers
        tex = new RenderTexture(this.size, this.size, 0, RenderTextureFormat.RGFloat);

        tex.enableRandomWrite = true;
        tex.filterMode = FilterMode.Trilinear;
        tex.isVolume = true;
        tex.volumeDepth = size;
        tex.Create();
        //tex.SetGlobalShaderProperty(name);
    }
    void setupRMTex3D(ref RenderTexture tex, string name)
    {
        //check if those buffers exist
        DestroyImmediate(tex);

        //make those buffers
        tex = new RenderTexture(this.size, this.size, 0, RenderTextureFormat.ARGBHalf);

        tex.enableRandomWrite = true;
        tex.isVolume = true;
        tex.volumeDepth = size;
        tex.Create();
        Shader.SetGlobalTexture("RayMarching", tex);
    }

	void OnDisable(){

        DestroyImmediate(media);
        DestroyImmediate(current);
        DestroyImmediate(previous);
        DestroyImmediate(next);
        DestroyImmediate(RMtex);
	}
    void changeColor()
    {
        color = (Vector4)Random.insideUnitSphere;
        color2 = (Vector4)Random.insideUnitSphere;
    }

    public Color color, color2;
	// Update is called once per frame
	void FixedUpdate () {
        echoCompute.SetFloat("timeStep", Time.fixedDeltaTime/ timeFactor);
        echoCompute.SetFloat("distanceStep", distanceStep);
        echoCompute.SetFloat("speedOfSound", speedOfSound);
        echoCompute.SetFloat("damping", damping);
        echoCompute.SetFloat("hueDamping", hueDamp);
        echoCompute.SetVector("chaos", new Vector4(Mathf.Sin(Time.timeSinceLevelLoad * frequency) * amplitude, huePower)  );
        if( cube)
            echoCompute.SetVector("cubePosition", cube.transform.position);

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
        
        
         echoCompute.Dispatch(0, 32, 32, 32);


	}

    void OnPreRender()
    {
        if(!VR)
            raymarching.doCamera(camera);
    }
}	