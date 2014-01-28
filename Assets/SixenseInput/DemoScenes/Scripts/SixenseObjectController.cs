using UnityEngine;
using System.Collections;

public class SixenseObjectController : MonoBehaviour {
	
	public GameObject	dude;
	public GameObject dude2;
	public SixenseHands			Hand;
	public Vector3				Sensitivity = new Vector3( 0.01f, 0.01f, 0.01f );
	
	protected bool				m_enabled = false;
	protected Quaternion		m_initialRotation;
	protected Vector3			m_initialPosition;
	protected Vector3			m_baseControllerPosition;
	Vector3 oldPos;
	public Vector3 velocity;	
	
	// Use this for initialization
	protected virtual void Start() 
	{
		m_initialRotation = this.gameObject.transform.localRotation;
		m_initialPosition = this.gameObject.transform.localPosition;
		oldPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if ( Hand == SixenseHands.UNKNOWN )
		{
			return;
		}
		
		SixenseInput.Controller controller = SixenseInput.GetController( Hand );
		if ( controller != null && controller.Enabled )  
		{		
			UpdateObject(controller);
		}	
	
		
		
		//Debug.Log(rigidbody.velocity);
		
		
	
	
	
	}
	
	void FixedUpdate(){
		
		velocity = (transform.position - oldPos)/Time.fixedDeltaTime;
		oldPos = transform.position;	
	}
	
	void OnGUI()
	{
		if ( !m_enabled )
		{
			GUI.Box( new Rect( Screen.width / 2 - 100, Screen.height - 40, 200, 30 ),  "Press Start To Move/Rotate" );
		}
	}
	
	
	protected virtual void UpdateObject(  SixenseInput.Controller controller )
	{
		if ( controller.GetButtonDown( SixenseButtons.START ) )
		{
			// enable position and orientation control
			m_enabled = !m_enabled;
			
			// delta controller position is relative to this point
			m_baseControllerPosition = new Vector3( controller.Position.x * Sensitivity.x,
													controller.Position.y * Sensitivity.y,
													controller.Position.z * Sensitivity.z );
			
			// this is the new start position
			m_initialPosition = this.gameObject.transform.localPosition;
		}
	
		
		if (controller.GetButtonDown(SixenseButtons.ONE)){
			dude.transform.eulerAngles = new Vector3(0,0,0);
			dude2.transform.eulerAngles = new Vector3(0,0,0);	
		}
		
		if ( m_enabled )
		{
			UpdatePosition( controller );
			//UpdateRotation( controller );
		}
	}
	
	
	protected void UpdatePosition( SixenseInput.Controller controller )
	{
		Vector3 controllerPosition = new Vector3( controller.Position.x * Sensitivity.x,
												  controller.Position.y * Sensitivity.y,
												  controller.Position.z * Sensitivity.z );
		
		// distance controller has moved since enabling positional control
		Vector3 vDeltaControllerPos = controllerPosition - m_baseControllerPosition;
		
		// update the localposition of the object
		this.gameObject.transform.localPosition = m_initialPosition + vDeltaControllerPos;
		gameObject.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + controller.Trigger);
		//rigidbody.velocity = controller.Trigger * 5 * transform.forward;
		
	}
	
	
	protected void UpdateRotation( SixenseInput.Controller controller )
	{
		this.gameObject.transform.localRotation = controller.Rotation * m_initialRotation;
	}
}
