using UnityEngine;
using System.Collections;

public class score : MonoBehaviour {
	public GUIText friend;
	public GameObject ball;
	public GameObject explosion;
    public HyperEcho hyperEcho;
	
	public int scoreValue = 0;
	
	// Use this for initialization
	void Start () {
	
	}

	
	
	IEnumerator makeBall(){
		yield return new WaitForSeconds(5);
		hyperEcho.cube = GameObject.Instantiate(ball, new Vector3(128,131,128), Quaternion.identity) as GameObject;
		
	}
	
	void OnCollisionEnter(Collision other){
		
		if (other.gameObject.CompareTag("ball")){
			scoreValue += 1;
			friend.text = "Score: " + scoreValue.ToString();
			Destroy(GameObject.Instantiate(explosion,other.gameObject.transform.position,Quaternion.identity),4);
			
			gameObject.audio.Play();
			Destroy(other.gameObject);
			StartCoroutine(makeBall());
			
			
		}
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
