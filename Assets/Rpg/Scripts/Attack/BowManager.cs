using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RTSEngine { 
public class BowManager : NetworkBehaviour {
    public GameObject arrow,rightHand,arrowPrefab;
    public Animator animator;
    public float arrowSpeed = 10f;
    private GameObject newArrow;
    float waitTime = 1f;
    public GameObject crossHair;
    public GameObject fireArrowPreFab;
    public GameObject normalArrowPrefab;
    public bool arrowFlag = false;
        
    float prevDistance = 30f;
        Vector3 theEnd;
	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
	}
	[Command]
    public void CmdinstaniateArrow()
        {

            RpcinstaniateArrow();
        }
    [ClientRpc]
    public void RpcinstaniateArrow()
        {
            if (newArrow == null) 
            {
                if (!arrowFlag)
                    arrowPrefab = normalArrowPrefab;
                else
                    arrowPrefab = fireArrowPreFab;
                newArrow = Instantiate(arrowPrefab);
                newArrow.transform.parent = rightHand.transform;
                newArrow.transform.localPosition = new Vector3(0.395f, -0.097f, 0.11f);
                newArrow.transform.localRotation = Quaternion.Euler(172.263f, -92.14099f, -180.745f);
                arrowMovement movementArrow = newArrow.GetComponent<arrowMovement>();
                movementArrow.Player = gameObject;

                waitTime = 1f;
            }
            else if(animator.GetCurrentAnimatorStateInfo(0).IsName("aim")&&newArrow!=null)
            {
                newArrow.transform.parent = arrow.transform.parent;
                newArrow.transform.localPosition = arrow.transform.localPosition;
                newArrow.transform.localRotation = arrow.transform.localRotation;
            }
        }
	// Update is called once per frame
	void Update () {

            print(GameManager.PlayerFactionID + "\t" + GetComponent<stats>().FactionId);

            if (GetComponent<stats>().FactionId == GameManager.PlayerFactionID)
            {
                print("a");
                return;

            }
            if (!arrowFlag)
                arrowPrefab = normalArrowPrefab;
            else
                arrowPrefab = fireArrowPreFab;
            if (Input.GetMouseButton(1))
            {

                crossHair.SetActive(true);

                CmdinstaniateArrow();


                if (animator.GetCurrentAnimatorStateInfo(0).IsName("aim"))
                {
                    waitTime -= Time.deltaTime;

                 
                        Ray ray = new Ray(arrow.transform.position, arrow.transform.forward);
                        RaycastHit hit;
                        Debug.DrawRay(ray.origin, ray.direction, Color.blue, 10f);

                        if (Physics.Raycast(ray.origin, ray.direction, out hit,100f))
                        {
                            if (hit.distance > 0.1)
                            {
                                prevDistance = hit.distance;
                                crossHair.transform.position = hit.point;
                            }
                            crossHair.transform.LookAt(Camera.main.transform);
                        }
                        else
                        {
                            crossHair.transform.position = theEnd;
                            crossHair.transform.LookAt(Camera.main.transform);
                        }
                    theEnd = ray.origin + ray.direction * 20f;
                        if (Input.GetKeyDown(KeyCode.Space))
                        {

                            CmdFire();
                        }
                    }

                

            }
            else if (!animator.GetBool("isAiming"))
            {
                if (newArrow != null)
                {

                    CmdDestroy();
                   
                }
            }
            else
                crossHair.SetActive(false);
    }
        [Command]
        public void CmdDestroy()
        {
            RpcDestroy();
        }
        [ClientRpc]
        public void RpcDestroy()
        {
            if (newArrow != null)
            {
                Destroy(newArrow);
                    }
        }
        [Command]
        public void CmdFire()
        {
            RpcFire( );
        }
        [ClientRpc]
        public void RpcFire()
        {
            if ( newArrow != null)
            {
                newArrow.transform.parent = null;
                newArrow.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

                newArrow.GetComponent<Rigidbody>().velocity = (arrow.transform.forward * arrowSpeed);
                newArrow.GetComponent<Rigidbody>().useGravity = true;
                newArrow.GetComponent<BoxCollider>().enabled = true;
                newArrow = null;
                animator.Play("drawArrow");
            }
        }
    IEnumerator waitForOtherArrow(int seconds)
    {
        yield return new WaitForSeconds(float.Parse(seconds + ""));
       
    }
}
}