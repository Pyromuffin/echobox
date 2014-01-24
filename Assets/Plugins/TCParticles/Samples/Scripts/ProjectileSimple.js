#pragma strict


var life : float;
private var startTime : float;

function Start () {
	startTime = Time.time;
}

function Update () {	
	if(Time.time - startTime > life){
		Destroy(gameObject);
	}
}