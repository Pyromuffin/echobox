using UnityEngine;
using System.Collections;

public class Emit : MonoBehaviour {
    public Color color;
    public float frequency = 5;
    public float amplitude;
    static float offset = 10000;

	// Use this for initialization
	void Start () {
        color = new Color(Random.value, Random.value, Random.value); 
	
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        renderer.material.SetVector("_Emit", new Vector4(color.r, color.g, color.b, Mathf.Sin(Time.timeSinceLevelLoad * frequency)) * amplitude );

	}
}
