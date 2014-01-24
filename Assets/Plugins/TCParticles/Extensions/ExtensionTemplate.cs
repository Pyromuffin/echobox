using UnityEngine;
using System.Collections;

public class ExtensionTemplate : MonoBehaviour
{
	public TCParticleSystem System;
	public ComputeShader Extension;

	[Range(0.0f, 10.0f)]
	public float AccelSpeed = 1.0f;

	// Update is called once per frame
	void Update ()
	{
		//Bind own custom variables to compute shader
		Extension.SetFloat("AccelSpeed", AccelSpeed);
		//First bind the buffers
		System.Manager.BindParticleBufferToExtension(Extension, "MyExtensionKernel");
		//Then dispatch
		System.Manager.DispatchExtensionKernel(Extension, "MyExtensionKernel");

		//Note that it's always good to do a bind call _first_. This way you can easily extend it to act on multiple systems
	}
}
