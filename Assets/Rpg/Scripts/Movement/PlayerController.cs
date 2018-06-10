using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RTSEngine { 
public class PlayerController : MonoBehaviour
{
    public float walkSpeed;
    public float runSpeed;
    public float gravity = -9.81f;

    private float turnSmoothTime = 0.2f;
    private float turnSmoothVelocity;
    private float velocityY;
    private float speedSmoothTime = 0.1f;
    private float speedSmoothVelocity;
    private float currentSpeed;
        GameManager GameMgr;
    Transform CameraT;
    Animator animator;
    CharacterController controller;
    NavMeshAgent agent;
    // Use this for initialization
    void Start()
    {
            GameMgr = GameManager.Instance;
        animator = GetComponentInChildren<Animator>();
       
        controller = GetComponent<CharacterController>();
            agent = GetComponent<NavMeshAgent>();

    }

    // Update is called once per frame
    void Update()
    {
            
            if (GameMgr != null)
            {
                if (GameMgr.Factions[GameMgr.PlayerId].playerType != "rpg")
                    return;

                string teamTag = "team" + (GameMgr.Factions[GameMgr.PlayerId].team + 1) + "Rpg";
                if (teamTag != transform.gameObject.tag)
                    return;
            }
            if(gameObject.GetComponent<stats>().DeadFlag)
            {
                return;
            }
            if(CameraT==null)
            {
                CameraT = Camera.main.transform;
                if (!CameraT.GetComponent<ThirdPersonCamera>())
                    CameraT = null;
            }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("drawArrow"))
        {
            animator.SetBool("isDrawingArrow", true);
        }
        else
        {
            animator.SetBool("isDrawingArrow", false);
        }
        if (Input.GetMouseButton(1))
        {
           
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            Vector2 inputDir = input.normalized;
            float targetRotation = CameraT.eulerAngles.y;
           
          if(CameraT.rotation.eulerAngles.x > 300)
            {
                animator.SetFloat("aimSpine", (CameraT.rotation.eulerAngles.x - 360)*2.25f, speedSmoothTime, Time.deltaTime);
            }
            else { 
                animator.SetFloat("aimSpine", CameraT.rotation.eulerAngles.x*1.06f, speedSmoothTime, Time.deltaTime);
            }
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
            float speed = walkSpeed * inputDir.magnitude;
           
            animator.SetBool("isAiming", true);
            currentSpeed = Mathf.SmoothDamp(currentSpeed, speed, ref speedSmoothVelocity, 0.5f);
           
            velocityY += Time.deltaTime * gravity;
                  Vector3 velocity = Vector3.zero;
            bool clickedW, clickedA, clickedS, clickedD;

            
            clickedW = Input.GetKey(KeyCode.W);
            clickedA = Input.GetKey(KeyCode.A);
            clickedS = Input.GetKey(KeyCode.S);
            clickedD = Input.GetKey(KeyCode.D);
            if (clickedA) 
            {
                velocity += -transform.right * currentSpeed ;
                animator.SetFloat("aimHorizontal", -1, speedSmoothTime, Time.deltaTime);
            }
            if (clickedD)
            {
                velocity += transform.right * currentSpeed ;
                animator.SetFloat("aimHorizontal", 1, speedSmoothTime, Time.deltaTime);
            }
            if (clickedW)
            {
                velocity += transform.forward * currentSpeed ;
                animator.SetFloat("aimVertical", 1, speedSmoothTime, Time.deltaTime);
            }
            if (clickedS)
            {
                velocity += -transform.forward * currentSpeed ;
                animator.SetFloat("aimVertical", -1, speedSmoothTime, Time.deltaTime);
            }
            if((!clickedW && !clickedA && !clickedS && !clickedD) ||(clickedW&&clickedS)||(clickedA&&clickedD))
            {
                animator.SetFloat("aimVertical", 0, speedSmoothTime, Time.deltaTime);
                animator.SetFloat("aimHorizontal", 0, speedSmoothTime, Time.deltaTime);
            }
            
            
        }
        else
        {
            animator.SetBool("isAiming", false);
            animator.SetFloat("aimSpine", 1,speedSmoothTime,Time.deltaTime);
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

           
            Vector3 velocity = transform.forward * currentSpeed;


           
            controller.Move(velocity * Time.deltaTime);
                this.agent.velocity = controller.velocity;
            
            currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;
          

            float animationSpeedPercent = ((running) ? currentSpeed / runSpeed : currentSpeed / walkSpeed * 0.5f);
            animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);

        }
    }
}
}