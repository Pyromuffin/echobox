using UnityEngine;
using System.Collections;


public class ball : MonoBehaviour {
	
	public float velocityScale = 1;
	private Collider lastCollider;
	public float maxIntensity = 5;
	public float bpm;
	private GameObject song;
	
	// Use this for initialization
	void Start () {
		song = GameObject.FindGameObjectWithTag("song");
		song.audio.Play();
		rigidbody.velocity = Random.onUnitSphere * 4;
		var dudes = GameObject.FindGameObjectsWithTag("Player");
		foreach( GameObject d in dudes)
			Physics.IgnoreCollision(d.collider,gameObject.collider);
		
	}
	
	void OnDestroy(){
		song.audio.Stop();	
		
	}

	float beatTimeAccumulator = 0;
	
	
	
	void doLight(float bpm){
		var lights = gameObject.GetComponentsInChildren<Light>();
		beatTimeAccumulator += Time.deltaTime;
		var beatTime =  bpm/240f;
		
		
		if (beatTimeAccumulator >= beatTime){
				
			foreach(Light l in lights)
				 l.color = new Color(Random.Range(0,1f),Random.Range(0,1f),Random.Range(0,1f));
			
			
		beatTimeAccumulator = 0;
		
		
		}
	
		foreach(Light l in lights)
				l.intensity = Mathf.Lerp (maxIntensity,0,beatTimeAccumulator/beatTime);
		
	
 }
		
		
		
		
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Space))
            rigidbody.velocity = Vector3.zero;



		doLight(bpm);


	}



    void OnCollisionEnter(Collision other)
    {
        var velocity = rigidbody.velocity;
        var direction = -Vector3.Reflect(velocity.normalized, other.contacts[0].normal);
        Debug.DrawRay(other.contacts[0].point, direction, Color.white, 10f);
        Debug.DrawRay(other.contacts[0].point, other.contacts[0].normal, Color.red, 10f);
        Debug.DrawRay(other.contacts[0].point, velocity, Color.green, 10f);


 
		if(other.gameObject.tag == "paddle"){
       //     Debug.Log(other.collider);
            
			Physics.IgnoreCollision(other.gameObject.collider, rigidbody.collider);

            if (other.transform.forward.z < 0 && rigidbody.velocity.z > 0)
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, rigidbody.velocity.y, rigidbody.velocity.z * -1);
            if (other.transform.forward.z > 0 && rigidbody.velocity.z < 0)
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, rigidbody.velocity.y, rigidbody.velocity.z * -1);

            //rigidbody.AddForceAtPosition(direction * other.gameObject.GetComponent<SixenseObjectController>().velocity.magnitude * velocityScale, other.contacts[0].point);


            rigidbody.velocity  += (other.gameObject.GetComponent<SixenseObjectController>().velocity * velocityScale);
            rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, 25);
           
		}
		if(lastCollider != null)
			Physics.IgnoreCollision(lastCollider, gameObject.collider,false);
		lastCollider = other.collider;

		audio.pitch = rigidbody.velocity.magnitude * .05f;
		gameObject.audio.Play();
        

		rigidbody.velocity  *= 1.05f;
		
	}
	
	
}
