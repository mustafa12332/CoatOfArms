using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CanvasPos : MonoBehaviour{
    public Terrain WorldTerrain;
    private static float terrianB;
    public int count = 0;

    private void Awake()
    {
        terrianB = WorldTerrain.transform.position.z;
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 pos = GameObject.Find("strCam").gameObject.transform.position;

        pos.y = pos.y -= 10; ;
    pos.z += 20;
            pos.x -= 130
            ; ;
        // pos.x -= 100;
        //  pos.z += 50;
     
  
       
       // Vector3 Scale = this.gameObject.transform.localScale;
      // .width;

       // this.gameObject.transform.localScale = Scale;
        //   this.gameObject.transform.localScale.z = Screen.width;


        this.gameObject.transform.position = pos;


    }
}
