using System.Collections;
using UnityEngine;

public class DestroyObj : MonoBehaviour {

 void Destroy()
    {
        Destroy(this.gameObject.transform.parent.gameObject);
    }
}
