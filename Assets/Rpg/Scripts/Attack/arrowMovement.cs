using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RTSEngine { 
public class arrowMovement : MonoBehaviour {
    Vector3 velocity = new Vector3(0,0,0);
    private GameObject player;
    // Use this for initialization
    private float distToGround;

    public Vector3 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }
    public GameObject Player
    {
        get { return player; }
        set { player = value; }
    }
	void Start () {
        distToGround = GetComponent<BoxCollider>().bounds.extents.y;

    }

    // Update is called once per frame
    void Update () {
        if (player == null)
            return;
        if (transform.parent == null && !IsGrounded())
        {
            transform.rotation = Quaternion.LookRotation(GetComponent<Rigidbody>().velocity, Vector3.up);
        }
    }
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 1f);
    }
    private void OnTriggerEnter(Collider other)
    {
            bool allowedObjects = false;
            if(other.gameObject.GetComponent<stats>())
            {
                if (other.gameObject.GetComponent<stats>() == player.gameObject.GetComponent<stats>())
                    return;
            }

            if (other.gameObject.GetComponent<Unit>() || other.gameObject.GetComponent<stats>() || other.gameObject.GetComponent<Building>()) { 
                allowedObjects = true;
              
            }

            if ( allowedObjects && transform.parent==null)
            {
                if (other.gameObject.GetComponent<Unit>())
                {

                    player.GetComponent<AttackRpg>().onAttackUnit(other.gameObject);
                    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                    transform.parent = other.gameObject.transform;

                    Destroy(gameObject, 5f);
                }
                else if (other.gameObject.GetComponent<Building>())
                {
                    player.GetComponent<AttackRpg>().onAttackBuilding(other.gameObject);
                    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                    transform.parent = other.gameObject.transform;

                    Destroy(transform.gameObject, 5f);
                }
                else if (other.gameObject.GetComponent<stats>() == player.gameObject.GetComponent<stats>())
                    return;
                else if (other.gameObject.GetComponent<stats>() != player.gameObject.GetComponent<stats>())
                {
                    print("entered to here");
                    player.GetComponent<AttackRpg>().onAttackUnit(other.gameObject);

                    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                    transform.parent = other.gameObject.transform;

                    Destroy(transform.gameObject, 5f);
                }
               
            }
        }
}
}