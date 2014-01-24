using UnityEngine;
using System.Collections;

public class Emit : MonoBehaviour {
    public Color color;
    public float frequency = 5;
    public float amplitude;

	// Use this for initialization
	void Start () {
        color = new Color(Random.value, Random.value, Random.value); 
	
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        renderer.material.color = new Vector4(color.r, color.g, color.b, Mathf.Sin(Time.timeSinceLevelLoad * frequency)) * amplitude;

	}
}
