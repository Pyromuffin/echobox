using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Require a character controller to be attached to the same game object
[RequireComponent(typeof(CharacterMotor))]
[AddComponentMenu("Character/FPS Input Controller")]




public class FPSInputController : MonoBehaviour
{
	
	private Quaternion startRotation;
	
    private CharacterMotor motor;
	public SixenseHands hand;	
	
	SixenseInput.Controller rightHand, leftHand;
	bool sixense = false;
	
	public void enableSixense(){
		
		
		rightHand = SixenseInput.GetController( SixenseHands.RIGHT );
		leftHand = SixenseInput.GetController( SixenseHands.LEFT );
		sixense = true;
				
	}
	
	
	
    // Use this for initialization
    void Awake()
    {
        motor = GetComponent<CharacterMotor>();
		startRotation = transform.localRotation;
	
	
	}

    // Update is called once per frame
    void Update()
    {
			
		transform.localRotation = startRotation;
        // Get the input vector from kayboard or analog stick
        Vector3 directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		
		if(sixense)
			directionVector = new Vector3(SixenseInput.GetController(hand).JoystickX, 0, SixenseInput.GetController(hand).JoystickY);
		
        if (directionVector != Vector3.zero)
        {
            // Get the length of the directon vector and then normalize it
            // Dividing by the length is cheaper than normalizing when we already have the length anyway
            float directionLength = directionVector.magnitude;
            directionVector = directionVector / directionLength;

            // Make sure the length is no bigger than 1
            directionLength = Mathf.Min(1.0f, directionLength);

            // Make the input vector more sensitive towards the extremes and less sensitive in the middle
            // This makes it easier to control slow speeds when using analog sticks
            directionLength = directionLength * directionLength;

            // Multiply the normalized direction vector by the modified length
            directionVector = directionVector * directionLength;
        }

        // Apply the direction to the CharacterMotor
        motor.inputMoveDirection = transform.rotation * directionVector;
		
    }
}