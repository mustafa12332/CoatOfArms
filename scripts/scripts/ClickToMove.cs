using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMove : MonoBehaviour
{

    Texture2D tex;
    public Terrain WorldTerrain;
    public bool isWalking;
    public bool isAttack;
    public bool isCharge;
    public int unitState;
    float BoxLeft, BoxTop, BoxWidth, BoxHeight, Boxtop;
    Vector3 clicked;
    public bool Draw;
    Vector3 hitPoint;
    public GUIStyle mouseDragSkin;
    NavMeshAgent m_Agent;
    public Transform target;
    int unitsCount = pointer.selectedObjets.Count;
    Client c;
    int myId;
    bool reached = false;
    RaycastHit m_HitInfo = new RaycastHit();
    pointer p;


    // Use this for initialization
    Animator animator;
    void Start() {
        myId = Client.getMyConId();
        isWalking = false;
        isAttack = false;
        isCharge = false;
        animator = GetComponent<Animator>();
        Vector3 clicked = Vector3.zero;
        m_Agent = GetComponent<NavMeshAgent>();
        c = GameObject.Find("Client").gameObject.GetComponent<Client>();
        Renderer[] rs;

        if (Client.getMyConId() == 1)
            tex = (Texture2D)Resources.Load("White");
        else if (Client.getMyConId() == 2)
            tex = (Texture2D)Resources.Load("Red");
        else if (Client.getMyConId() == 3)
            tex = (Texture2D)Resources.Load("Blue");
        else
            tex = (Texture2D)Resources.Load("Green");


        if (this.gameObject.name == "worker(Clone)" || this.gameObject.name == "spearMan(Clone)")
        {
            print("WORKER");
            rs = this.gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rs) {
                foreach(Material m in r.materials)
                r.material.mainTexture = tex;
            }
        }
        else
        {
            rs = this.gameObject.GetComponentsInChildren<Renderer>();

            Material[] m = rs[0].materials;
            if (gameObject.name == "cavalry(Clone)")
                m[1].mainTexture = tex;
            else// if (gameObject.name == "Soldier(Clone)")
                m[0].mainTexture = tex;
        }
    }


    // Update is called once per frame

 
    void Update()
    {
  
        if (Input.GetMouseButton(1) && (transform.Find("selected").gameObject.active == true))
        {

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out m_HitInfo))
            {

                m_Agent.isStopped = false;
                GameObject frstunit = pointer.selectedObjets[0] as GameObject;
                hitPoint = m_HitInfo.point;
                frstunit.gameObject.GetComponent<NavMeshAgent>().SetDestination(hitPoint);
                int L = 1;
                int k = 1;
                for (int i = 1; i <pointer.selectedObjets.Count; i++) {
                    
                    if (k % 8 == 0) {
                        k = 1;
                        L +=30;
                    }
                    GameObject unitObj = pointer.selectedObjets[i] as GameObject;
                    Vector3 newPos = new Vector3(hitPoint.x + ( k *20), hitPoint.y, hitPoint.z - ( L));
                    unitObj.gameObject.GetComponent<NavMeshAgent>().SetDestination(newPos);

                 
                    k++;
                  
                }
                animator.SetBool("isWalking", true);
                animator.SetBool("isAttack", false);
                c.players[myId].unitState = 1;
            }
        }
 
    }

void LateUpdate()
    {
        NavMeshAgent myAgent = this.gameObject.GetComponent<NavMeshAgent>();
        Vector3 myPos = this.gameObject.transform.position;
  

        if (stopUnits(myPos,10) && animator.GetBool("isWalking")==true){
            animator.SetBool("isWalking", false);
            c.players[myId].unitState = 0;
            m_Agent.SetDestination(this.gameObject.transform.position);
            m_Agent.isStopped = true ;
        }
        if (m_Agent.isStopped == true)
        {
            animator.SetBool("isWalking", false);
            m_Agent.SetDestination(this.gameObject.transform.position);
        }
    }

    public void goTo( GameObject c,Vector3 Epos)
    {
        // m_Agent.isStopped = false;
        NavMeshAgent n = c.gameObject.GetComponent<NavMeshAgent>();
        n.isStopped = false;
        n.SetDestination(Epos);
        animator.SetBool("isWalking", true);
        animator.SetBool("isAttack", false);
        //   m_Agent.SetDestination(Epos);




        // animator.SetBool("isAttack", false);
        //animator.SetBool("isWalking", true);
        //   n.SetDestination(Epos);
    }
    public void stopUnit(GameObject c) {
        NavMeshAgent n = c.GetComponent<NavMeshAgent>();
        n.isStopped = true;
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttack", true) ;
    }
    private void goTo(NavMeshAgent Agent, Vector3 Pos)
    {

        m_Agent.isStopped = false;
        m_Agent.SetDestination(Pos);
        animator.SetBool("isAttack", false);
        animator.SetBool("isWalking", true);

   
    }

 
    public bool floatToBool(float num)
    {
        if (num < 0f)
            return false;
        return true;
    }

    private bool EnemeyinSight(Vector3 myPos, Vector3 u,int attackDis)
    {
      
        if ((myPos.x + attackDis > u.x && myPos.x - attackDis < u.x) &&
                  (myPos.z + attackDis > u.z && myPos.z - attackDis < u.z))
                     return true;
        return false;
    }

   public bool stopUnits(Vector3 myPos, int unitsCount)
    {
        if (unitsCount == 0)
            unitsCount = 185;

        float y = ((myPos.x * myPos.x - m_Agent.destination.x * m_Agent.destination.x) + (myPos.z * myPos.z - m_Agent.destination.z * m_Agent.destination.z));
        if (y < 0)
            y = y * -1;
        float t = Mathf.Sqrt(y);
        if (t <= unitsCount) //was 30
            return true;
        return false;
    }


}
