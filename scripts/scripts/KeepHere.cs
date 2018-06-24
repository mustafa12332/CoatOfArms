using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class KeepHere : MonoBehaviour {
    public GameObject worker;
    private Vector3 Scale;
    private float myScale;
    private bool built = false;
    private bool clicked = false;
    public bool fromFunc = false;
    // Use this for initialization
    // Update is called once per frame
    private void Awake()
    {
       
    }
    void Update () {
      

        if (Input.GetMouseButton(0) || fromFunc)
            clicked = true;
     
        if (!built && clicked){
           
            myScale = this.transform.localScale.y;
            this.transform.localScale = Vector3.zero;
            built = true;
        }
  
        if (Scale.y < myScale && built && clicked) {
            Scale.y += (myScale / 10) * Time.deltaTime;
            Scale.z = Scale.y;
            Scale.x = Scale.y;
            this.transform.localScale = Scale;
        }
        else if (Scale.y >= myScale && built){

          this.GetComponent<NavMeshObstacle>().enabled = true;
          this.transform.Find("particles").gameObject.SetActive(false);
          this.GetComponent<BoxCollider>().enabled = true;
            worker.GetComponent<Animator>().SetBool("isAttack", false);
            worker.GetComponent<ClickToMove>().enabled = true;
            worker = null;
            fromFunc = false;
        }

    }

}
