using UnityEngine;
using System.Collections;

public class Echo : MonoBehaviour
{
	// Fields
	public float amplitude;
	public Color bored;
	private RenderTexture boxTexture;
	public Camera copyCam;
	private RenderTexture current;
	public float damping;
	public float distanceStep;
	public bool doVoxels = true;
	public ComputeShader echoShader;
	public float frequency;
	public gridSize grid;
	public ComputeBuffer hearing;
	public float[] huge;
	public GameObject leftEarGo;
	public float maxF;
	public ComputeBuffer media1;
	public float minF;
	private RenderTexture next;
	public float notSpeed;
	public LayerMask onlyRaybox;
	private int phase;
	public static GameObject player;
	private RenderTexture previous;
	public static GameObject raybox;
	public GameObject rayMarchingBox;
	public GameObject rightEarGo;
	private float simFactor;
	public float simulationWorldSize;
	private int size;
	public float speedOfSound;
	public float transparency;
	public GameObject VoxelOrigin;
	private Vector3 voxelPosition;
	public Camera xCam;
	public Shader xShader;
	public Camera yCam;
	public Shader yShader;
	public Camera zCam;
	public Shader zShader;
	
	// Methods
	private void Awake()
	{
		player = base.gameObject;
	}
	
	public void dispatch(int kernel)
	{
		switch (this.grid)
		{
		case gridSize.cube64:
			this.echoShader.Dispatch(kernel, 8, 8, 8);
			break;
			
		case gridSize.cube128:
			this.echoShader.Dispatch(kernel, 0x10, 0x10, 0x10);
			break;
			
		case gridSize.cube256:
			this.echoShader.Dispatch(kernel, 0x20, 0x20, 0x20);
			break;
		}
	}

	
	public void makeBuffers()
	{
		Object.DestroyImmediate(this.current);
		Object.DestroyImmediate(this.next);
		Object.DestroyImmediate(this.previous);
		if (this.media1 != null)
		{
			this.media1.Release();
			this.media1 = null;
		}
		if (this.hearing != null)
		{
			this.hearing.Release();
			this.hearing = null;
		}
		this.current = new RenderTexture(this.size, this.size, this.size, RenderTextureFormat.RFloat);
		this.next = new RenderTexture(this.size, this.size, this.size, RenderTextureFormat.RFloat);
		this.previous = new RenderTexture(this.size, this.size, this.size, RenderTextureFormat.RFloat);
		this.media1 = new ComputeBuffer((this.size * this.size) * this.size, 8);
		this.hearing = new ComputeBuffer(2, 4);
		Graphics.SetRandomWriteTarget(1, this.media1);
		this.echoShader.SetBuffer(0, "Media", this.media1);
		this.echoShader.SetBuffer(0, "Hearing", this.hearing);
		this.echoShader.SetFloat("size", (float) this.size);
		this.current.enableRandomWrite = true;
		this.next.enableRandomWrite = true;
		this.previous.enableRandomWrite = true;
		this.current.isVolume = true;
		this.next.isVolume = true;
		this.previous.isVolume = true;
		this.current.volumeDepth = this.size;
		this.next.volumeDepth = this.size;
		this.previous.volumeDepth = this.size;
		this.current.Create();
		this.next.Create();
		this.previous.Create();
	}
	
	public void makeVoxelOrigin()
	{
		raybox = Object.Instantiate(this.rayMarchingBox, this.voxelPosition, Quaternion.identity) as GameObject;
		raybox.transform.localScale = new Vector3(this.simulationWorldSize, this.simulationWorldSize, this.simulationWorldSize);
		this.xCam.transform.position = this.voxelPosition;
		this.xCam.nearClipPlane = -this.simulationWorldSize / 2f;
		this.xCam.farClipPlane = this.simulationWorldSize / 2f;
		this.xCam.orthographicSize = this.simulationWorldSize / 2f;
		this.xCam.aspect = 1f;
		this.yCam.transform.position = this.voxelPosition;
		this.yCam.nearClipPlane = -this.simulationWorldSize / 2f;
		this.yCam.farClipPlane = this.simulationWorldSize / 2f;
		this.yCam.orthographicSize = this.simulationWorldSize / 2f;
		this.yCam.aspect = 1f;
		this.zCam.transform.position = this.voxelPosition;
		this.zCam.nearClipPlane = -this.simulationWorldSize / 2f;
		this.zCam.farClipPlane = this.simulationWorldSize / 2f;
		this.zCam.orthographicSize = this.simulationWorldSize / 2f;
		this.zCam.aspect = 1f;
	}
	
	public void OnDisable()
	{
		this.current.Release();
		this.next.Release();
		this.previous.Release();
		if (this.media1 != null)
		{
			this.media1.Release();
			this.media1 = null;
		}
		if (this.hearing != null)
		{
			this.hearing.Release();
			this.hearing = null;
		}
		Graphics.ClearRandomWriteTargets();
	}
	
	//public void OnDrawGizmos()
	//{
	//	Gizmos.DrawWireCube(this.VoxelOrigin.transform.position, new Vector3(this.simulationWorldSize, this.simulationWorldSize, this.simulationWorldSize));
	//}
	
	private void Start()
	{
		this.boxTexture = new RenderTexture(Screen.width, Screen.height, 0x20);
		this.boxTexture.Create();
		this.size = (int) this.grid;
		Shader.SetGlobalFloat("size", (float) this.size);
		Shader.SetGlobalFloat("halfSize", (float) (this.size / 2));
		this.simFactor = ((float) this.size) / this.simulationWorldSize;
		this.huge = new float[2];
		this.makeBuffers();
		this.voxelPosition = this.VoxelOrigin.transform.position;
		this.makeVoxelOrigin();
		Camera.main.depthTextureMode = DepthTextureMode.Depth;
		this.echoShader.SetTexture(1, "Next", this.previous);
		this.dispatch(1);
		this.echoShader.SetTexture(1, "Next", this.current);
		this.dispatch(1);
		this.echoShader.SetTexture(1, "Next", this.next);
		this.dispatch(1);
	}
	
	public Vector3 transformToVoxelSpace(Vector3 position)
	{
		return (((Vector3) ((position - this.voxelPosition) * this.simFactor)) + (Vector3.one * (this.size / 2)));
	}
	
	private void Update()
	{
	
	
		if (Input.GetButtonDown("Reset"))
		{
			this.echoShader.SetTexture(1, "Next", this.previous);
			this.dispatch(1);
			this.echoShader.SetTexture(1, "Next", this.current);
			this.dispatch(1);
			this.echoShader.SetTexture(1, "Next", this.next);
			this.dispatch(1);
		}
		this.echoShader.SetFloat("distanceStep", this.distanceStep);
		this.echoShader.SetFloat("speed", this.speedOfSound);
		this.echoShader.SetFloat("timestep", Time.deltaTime);
		this.echoShader.SetFloat("damping", this.damping);
		//Vector3 vector = this.transformToVoxelSpace(this.leftEarGo.transform.position);
		//Vector3 vector2 = this.transformToVoxelSpace(this.rightEarGo.transform.position);
		//float[] values = new float[] { vector.x, vector.y, vector.z };
		//this.echoShader.SetFloats("leftEar", values);
		//float[] singleArray2 = new float[] { vector2.x, vector2.y, vector2.z };
		//this.echoShader.SetFloats("rightEar", singleArray2);
		Shader.SetGlobalFloat("notSpeed", this.notSpeed);
		Raymarching.rayMat.SetVector("worldOrigin", this.voxelPosition);
		Raymarching.rayMat.SetFloat("halfWorldSize", this.simulationWorldSize / 2f);
		Raymarching.rayMat.SetFloat("transparency", this.transparency);
		if (this.phase == 0)
		{
			this.echoShader.SetTexture(0, "Previous", this.previous);
			this.echoShader.SetTexture(0, "Current", this.current);
			this.echoShader.SetTexture(0, "Next", this.next);
			Raymarching.rayMat.SetTexture("Volume", this.current);
		}
		else if (this.phase == 1)
		{
			this.echoShader.SetTexture(0, "Previous", this.current);
			this.echoShader.SetTexture(0, "Current", this.next);
			this.echoShader.SetTexture(0, "Next", this.previous);
			Raymarching.rayMat.SetTexture("Volume", this.next);
		}
		else if (this.phase == 2)
		{
			this.echoShader.SetTexture(0, "Previous", this.next);
			this.echoShader.SetTexture(0, "Current", this.previous);
			this.echoShader.SetTexture(0, "Next", this.current);
			Raymarching.rayMat.SetTexture("Volume", this.previous);
		}
		this.phase = (this.phase + 1) % 3;
		if (this.doVoxels)
		{
			this.xCam.RenderWithShader(this.xShader, "");
			this.yCam.RenderWithShader(this.yShader, "");
			this.zCam.RenderWithShader(this.zShader, "");
		}
		Raymarching.rayMat.SetColor("bored", this.bored);
		this.dispatch(0);
		this.hearing.GetData(this.huge);
		//AudioMachine.leftPressure = this.huge[0];
		//AudioMachine.rightPressure = this.huge[1];
		Raymarching.rayMat.SetFloat("minF", this.minF);
		Raymarching.rayMat.SetFloat("maxF", this.maxF);
	}
	

	
	public enum gridSize
	{
		cube128 = 0x80,
		cube256 = 0x100,
		cube64 = 0x40
	}
}


