using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



[RequireComponent(typeof(NavMeshAgent))]
public class multiAnime : MonoBehaviour
{


    // Use this for initialization
    Animator animator;
    void Start() { 
        animator = GetComponent<Animator>();
  
    }

    public void setAnime(int rr)
    {
        switch (rr) {

            case 0:  //Idel state
                animator.SetBool("isWalking", false);
                animator.SetBool("isAttack", false);
                break;
            case 1: //WALKING STATE
                animator.SetBool("isWalking", true);
                animator.SetBool("isAttack", false);
                break;
            case 2: // ATTACDK
                animator.SetBool("isWalking", false);
                animator.SetBool("isAttack", true);
                break;
            case 3: // DEAD
                animator.SetBool("isDead", true);
                break;

        }
    }


}
