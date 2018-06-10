using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSEngine
{
    public class TerrainManager : MonoBehaviour
    {
        public static TerrainManager Instance;

        public LayerMask TerrainLayerMask; //layers used for terrain objects 
        public GameObject FlatTerrain;
        public GameObject AirTerrain; //Terrain object for flying units (it does not have to be a terrain object, it can be a simple plane but make sure to bake it all as walkable in the navmesh).
        //the air terrain helps to seperate the movement of flying units and normal units.

        void Awake ()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        //get the height of the closest terrain tile
        public float SampleHeight (Vector3 Position)
        {
            float Height = 0.0f;
            float Distance = 0.0f;

            RaycastHit[] Hits = Physics.RaycastAll(new Vector3(Position.x, Position.y+10.0f, Position.z), Vector3.down,50.0f, TerrainLayerMask);

            if(Hits.Length > 0)
            {
                Height = Hits[0].point.y;
                Distance = Vector3.Distance(Position, Hits[0].point);

                if (Hits.Length > 1)
                {
                    for (int i = 1; i < Hits.Length; i++)
                    {
                        if(Distance > Vector3.Distance(Hits[i].point, Position))
                        {
                            Height = Hits[i].point.y;
                            Distance = Vector3.Distance(Position, Hits[i].point);
                        }
                    }
                }
            }

            return Height;
        }

        //determine if an object belongs to the terrain tiles:
        public bool IsTerrainTile (GameObject Obj)
        {
            return TerrainLayerMask == (TerrainLayerMask | (1 << Obj.layer));
        }
    }
}