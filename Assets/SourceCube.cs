using UnityEngine;
using System.Collections;

public class SourceCube : MonoBehaviour {
    public float startSpeed;
	// Use this for initialization
	void Start () 
    {
        rigidbody.velocity = Random.onUnitSphere * startSpeed;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
