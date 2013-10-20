using UnityEngine;
using System.Collections;

public class Raymarching : MonoBehaviour
{
	// Fields
	public Vector3 LL;
	public Vector3 LR;
	public Camera mainCamera;
	public static Material rayMat;
	public Vector2 size;
	public float stepSize;
	public Vector3 UL;
	public static RenderTexture volume;
	
	// Methods
	private void Start()
	{
		rayMat = base.gameObject.renderer.material;
		this.mainCamera = Camera.main;
		this.LL = this.mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, this.mainCamera.nearClipPlane));
		this.UL = this.mainCamera.ViewportToWorldPoint(new Vector3(0f, 1f, this.mainCamera.nearClipPlane));
		this.LR = this.mainCamera.ViewportToWorldPoint(new Vector3(1f, 0f, this.mainCamera.nearClipPlane));
		Vector3 vector = this.LL - this.LR;
		Vector3 vector2 = this.LL - this.UL;
		this.size = new Vector2(vector.magnitude, vector2.magnitude);
		rayMat.SetVector("cameraWorldSize", new Vector4(this.size.x, this.size.y));
	}
	
	private void Update()
	{
		Vector3 up = this.mainCamera.gameObject.transform.up;
		Vector3 right = this.mainCamera.gameObject.transform.right;
		rayMat.SetVector("cameraUp", new Vector4(up.x, up.y, up.z));
		rayMat.SetVector("cameraRight", new Vector4(right.x, right.y, right.z));
		rayMat.SetFloat("StepSize", this.stepSize);
	}
}


