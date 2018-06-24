using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMotor : MonoBehaviour {

    // Use this for initialization
    private CharacterController controller;
    private float speedV;
	void Start () {
        controller = GetComponent<CharacterController>(); 

	}
	
	// Update is called once per frame
	void Update () {

        Vector3 inputs = Vector3.zero;
        inputs.x = Input.GetAxis("Horizontal");
        
        if (controller.isGrounded)
        {
            speedV = -2;

            if (Input.GetButton("Jump"))
                speedV = 15;
        }
        else
            speedV -= 18.0f * Time.deltaTime;
        //  if(Input.GetButton)
        inputs.y = speedV;

        controller.Move(inputs * Time.deltaTime*50);
        
	}
}
