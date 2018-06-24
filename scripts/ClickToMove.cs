using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMove : MonoBehaviour
{
    NavMeshAgent m_Agent;
    public Transform target;
    RaycastHit m_HitInfo = new RaycastHit();
    // Use this for initialization
    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
   
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(1))
        {

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            print(ray);
            if (Physics.Raycast(ray.origin, ray.direction, out m_HitInfo))
                print(m_HitInfo.point);
            m_Agent.destination = m_HitInfo.point;
           // m_Agent.transform.RotateAround(m_Agent.transform.position, new Vector3(0, 1, 0), 90);
            m_Agent.updateRotation = false;

 Quaternion newRotation = Quaternion.LookRotation(m_HitInfo.point - transform.position);
            //newRotation.x -= 90f;
           //  Quaternion newRotation.z -= 90f;
           m_Agent.transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime*100);
           // m_Agent.updateRotation = false;
    

            // Vector3 relativePos = ((m_Agent.destination) - transform.position);
            //relativePos.y -=90;

            /*  Vector3 

               Quaternion rotation = Quaternion.LookRotation(relativePos);
              m_Agent.transform.localRotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 100);*/
            //m_Agent.transform.rotation = rotation;
            /* Vector3 vector;
             vector = Quaternion.AngleAxis(-45, Vector3.up) * vector;
            // That's a general case. For rotation around a world axis, it's faster/ easier:
             vector = Quaternion.Euler(0, -45, 0) * vector;*/
            /*  Vector3 vec = new Vector3();
            vec=  m_Agent.destination - Vector3.up;
              m_Agent.transform.LookAt(vec);*/




            /*this.transform.Rotate(new Vector3(Random.value, Random.value, Random.value));
            this.transform.RotateAround(Vector3.zero, Vector3.right, 1.0f);*/

        m_Agent.transform.RotateAround(m_Agent.transform.position, new Vector3(0, 1, 0), 90);

        }


    }
}
