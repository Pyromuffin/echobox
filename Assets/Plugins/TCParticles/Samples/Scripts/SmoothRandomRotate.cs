using UnityEngine;
using System.Collections;

public class SmoothRandomRotate : MonoBehaviour {
    public float strength = 1.0f;
    float seed;
	Vector3 rotateSpeed;
	float friction = 0.99f;
	Rigidbody rigid;

	// Use this for initialization
	void Start () {
        seed = Random.Range(0.0f, 100.0f);
		rigid = rigidbody;
	}

    float SmoothRand(float y)
    {
        return Mathf.PerlinNoise(Time.time + seed, y) * 2.0f - 1.0f;
    }

	// Update is called once per frame
	void Update () {
        //Adjust speed with three random noise tracks
		rotateSpeed += new Vector3(SmoothRand(0.0f),
									 SmoothRand(1.0f),
									 SmoothRand(2.0f)) * strength * Time.deltaTime * 20.0f;
		rotateSpeed *= friction;


		if (rigid == null)
			transform.Rotate(rotateSpeed);
	}

	void FixedUpdate()
	{
		if (rigid != null)
			rigid.MoveRotation(Quaternion.Euler(rotateSpeed));
	}
}
