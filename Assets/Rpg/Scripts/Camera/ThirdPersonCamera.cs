using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RTSEngine
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public float mouseSensetivity = 10;
        public Transform target;
        public float dstFromTarget = 2;
        public Vector2 pitchMinMax = new Vector2(-40, 85);
        private float offset = 2.1f;
        public int teamNo;
        public float rotationSmoothTime = 0.01f;
        Vector3 rotationSmoothVelocity;
        Vector3 currentRotation;
        public int type = 0;
        float yaw;
        float pitch;



        void Update()
        {

            if (target == null)
            {
               
            }
            if (target != null)
            {
                if (type == 0) { 
                if(target.gameObject.transform.parent.gameObject.GetComponent<stats>())
                if (target.gameObject.transform.parent.gameObject.GetComponent<stats>().DeadFlag)
                    return;
              

                if (Input.GetMouseButton(1))
                {
                    dstFromTarget = 1f;
                }
                else
                {
                    dstFromTarget = 3f;
                }
                }
                else if (type == 1)
                {
                    dstFromTarget = 3f;
                }
                yaw += Input.GetAxis("Mouse X") * mouseSensetivity;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensetivity;
                pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
                currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);

                transform.eulerAngles = currentRotation;
                Vector3 desiredPosition = target.position - transform.forward * dstFromTarget;
                transform.position = Vector3.Lerp(transform.position, desiredPosition, 0.3f);
            }

        }
        public Transform Target
        {
            get { return target; }
            set { target = value; }
        }
    }

}
