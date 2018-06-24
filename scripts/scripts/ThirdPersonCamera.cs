using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {
    public float mouseSensetivity = 10;
    public Transform target;
    public float dstFromTarget = 2;
    public Vector2 pitchMinMax = new Vector2(-40, 85);
    private float offset = 2.1f;
    public float rotationSmoothTime = 0.01f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    float yaw;
    float pitch;
	
	

	void Update () {
        yaw += Input.GetAxis("Mouse X") * mouseSensetivity;
        pitch -=Input.GetAxis("Mouse Y") * mouseSensetivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
       
        if (Input.GetMouseButton(1))
        {
            target.transform.localPosition = new Vector3(0.25f, 1.5f, 0f);
            dstFromTarget = 5f;
        }
        else
        {
            dstFromTarget = 15;
            target.transform.localPosition=new Vector3(0, 1.28f, 0);
        }

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);

        transform.eulerAngles = currentRotation;
        Vector3 desiredPosition =  target.position - transform.forward * dstFromTarget;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, 0.3f);
        

	}
}
