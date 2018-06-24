using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {
    public Terrain WorldTerrain;
    public LayerMask TerrianLayer;
    public static float terrianL, terrianLen, terrianR, terrianT, terrianB, terrianW, terrianH;

	// Use this for initialization
	void Awake() {

        terrianL = WorldTerrain.transform.position.x;
        terrianB = WorldTerrain.transform.position.z;
     
   

        terrianLen = WorldTerrain.terrainData.size.z;
        terrianW = WorldTerrain.terrainData.size.x;
        terrianH = WorldTerrain.terrainData.size.y;
        terrianT = terrianB + terrianW;
        terrianR = terrianL + terrianLen;

      
    }
	
	// Update is called once per frame
	void Update () {
     
           
    }

    // public void InstanstiateRandomPos(string Resource, int amount, float addedHeight) {
    public void InstanstiateRandomPos()
    {
        int i = 0;
        float randomPosX, randomPosY, randomPosZ;
        Vector3 randomPos;

   //    do
   //     {
            i++;
        for (int k = 0; k < 5; k++)
        {
         randomPos = Vector3.zero;
            randomPosX = Random.Range(terrianL, terrianR);
            randomPosZ = Random.Range(terrianB, terrianT);


            Debug.Log(randomPosX + "   " + randomPosZ);
        }
      /*      if (Physics.Raycast(new Vector3(randomPosX, 9999f, randomPosZ), Vector3.down, out hit, Mathf.Infinity, TerrianLayer))
            {
                terrianH = hit.point.y;


            }
            randomPosY = terrianH + addedHeight;

            randomPos = new Vector3(randomPosX, randomPosY, randomPosZ);
            Instantiate(Resources.Load(Resource, typeof(GameObject)), randomPos, Quaternion.identity);
        } while (i < amount);*/
    }

}
