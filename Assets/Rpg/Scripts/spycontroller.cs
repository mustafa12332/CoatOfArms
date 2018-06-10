using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RTSEngine { 
public class spycontroller : NetworkBehaviour {
    CharacterController controller;
        GameManager GameMgr;
        Animator animator;

        Transform CameraT;
        public float walkSpeed = 2;
        public float runSpeed = 3;
        public float gravity = -9.81f;

        private float turnSmoothTime = 0.2f;
        private float turnSmoothVelocity;
        private float velocityY;
        private float speedSmoothTime = 0.1f;
        private float speedSmoothVelocity;
        private float currentSpeed;

        // Use this for initialization
        void Start () {
        controller = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();

            GameMgr = GameManager.Instance;
            CameraT = Camera.main.transform;

        }
     

        // Update is called once per frame
        void Update()
        {
            if (GameMgr != null)
            {
                if (GameMgr.Factions[GameMgr.PlayerId].playerType != "rpg")
                    return;

                string teamTag = "team" + (GameMgr.Factions[GameMgr.PlayerId].team + 1) + "RpgSpy";
                if (teamTag != transform.gameObject.tag)
                    return;
            }
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            Vector2 inputDir = input.normalized;

            if (inputDir != Vector2.zero)
            {
                float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + CameraT.eulerAngles.y;
                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);

            }
            bool running = Input.GetKey(KeyCode.LeftShift);
            float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

            velocityY += Time.deltaTime * gravity;
            Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;



            controller.Move(velocity * Time.deltaTime);

            currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;
            if (controller.isGrounded)
                velocityY = 0;
            float animationSpeedPercent = ((running) ? currentSpeed / runSpeed : currentSpeed / walkSpeed * 0.5f);
            animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);



        }
    }
}