using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

/* Border script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    public class Border : MonoBehaviour
    {

        [HideInInspector]
        public bool IsActive = false; //is the border active or not?

        [Header("Border Object:")]
        //spawn the border object?
        public bool SpawnBorderObj = true;
        public GameObject BorderObj; //Use an object that is only visible on the terrain to avoid drawing borders outside the terrain.
        public float BorderHeight = 20.0f; //The height of the border object here
        [Range(0.0f, 1.0f)]
        public float BorderColorTransparency = 1.0f; //transparency of the border's object color
        public float Size = 10.0f; //The size of the border around this building:
        public float BorderSizeMultiplier = 2.0f; //To control the relation of the border obj's actual size and the border's map. Using different textures for the border objects will require using 

        //If the border belongs to an NPC player, you can require the NPC to build all the objects in the array below.
        //If the border belongs to the player, then this array will only represent the maximum amounts for each building
        //and if a building is not in this array, then the player is free to build as many as he wishes to build.
        [System.Serializable]
        public class BuildingsInsideBorderVars
        {
            public Building Prefab; //prefab of the building to be placed inside the border
            public string FactionCode; //Leave empty if you want this building to be considered by all factions
            [HideInInspector]
            public int CurrentAmount = 0; //current amount of the building type inside this border
            public int MaxAmount = 1; //maximum allowed amount of this building type inside this border
            [HideInInspector]
            public int ProgressAmount; //amount of this building type that is currently getting placed.

            public int RequiredAmount = 1; //Only for NPC factions, if set to true, the faction will have to place this amount of the building type.

            [HideInInspector]
            public bool AskedForResources = false; //Did we ask to collect the resources needed for this building:
            [HideInInspector]
            public bool MaxAmountReached = false; //Is the building already built? 

        }
        [Header("Border Buildings:")]
        public List<BuildingsInsideBorderVars> BuildingsInsideBorder;
        [HideInInspector]
        public List<Building> BuildingsInRange = new List<Building>();

        [Header("Border Buildings (NPC only):")]
        //For more diversity to the NPC player, the timer that checks for the buildings inside a border can take a value between a min and a max one.
        public Vector2 CheckBuildingsTimeRange = new Vector3(3.0f, 5.0f);
        float CheckBuildingsTimer;

        bool AllBuilt = false; //Are all required buildings inside the border built or not?

        //components:
        Building MainBuilding;

        //The list of resources belonging inside this border:
        [HideInInspector]
        public List<Resource> ResourcesInRange = new List<Resource>();

        GameManager GameMgr;
        ResourceManager ResourceMgr;
        [HideInInspector]
        public FactionManager FactionMgr = null;

        void Awake()
        {
            MainBuilding = gameObject.GetComponent<Building>();
        }

        //called to activate the border
        public void ActivateBorder()
        {
            //make sure to get the game manager and resource manager components
            GameMgr = GameManager.Instance;
            ResourceMgr = GameMgr.ResourceMgr;

            //if the border is not active yet
            if (IsActive == false)
            {
                //if we're allowed to spawn the border object
                if (SpawnBorderObj == true)
                {
                    //create and spawn it
                    BorderObj = (GameObject)Instantiate(BorderObj, new Vector3(transform.position.x, BorderHeight, transform.position.z), Quaternion.identity);
                    BorderObj.transform.localScale = new Vector3(Size * BorderSizeMultiplier, BorderObj.transform.localScale.y, Size * BorderSizeMultiplier);
                    BorderObj.transform.SetParent(transform, true);

                    //Set the border's color to the faction it belongs to:
                    Color FactionColor = GameMgr.Factions[MainBuilding.FactionID].FactionColor;
                    BorderObj.GetComponent<MeshRenderer>().material.color = new Color(FactionColor.r, FactionColor.g, FactionColor.b, BorderColorTransparency);
                    //Set the border's sorting order:
                    BorderObj.GetComponent<MeshRenderer>().sortingOrder = GameMgr.LastBorderSortingOrder;
                    GameMgr.LastBorderSortingOrder--;
                }


                //Add the border to all borders list:
                GameMgr.AllBorders.Add(this);

                CheckBorderResources(); //check the resources around the border

                IsActive = true; //mark the border as active
            }

            //set the faction manager
            FactionMgr = GameMgr.Factions[MainBuilding.FactionID].FactionMgr;

            if (FactionMgr != null)
            {
                //if this is a NPC faction in a local game
                if (FactionMgr.FactionID != GameManager.PlayerFactionID && GameManager.MultiplayerGame == false)
                {
                    //launch the timer to check for buildings that need to be built around this center
                    CheckBuildingsTimer = Random.Range(CheckBuildingsTimeRange.x, CheckBuildingsTimeRange.y);
                }
            }

            //Inform the faction's resource manager that we need to check resource inside the new border:
            if (FactionMgr.ResourceMgr != null)
            {
                FactionMgr.ResourceMgr.CheckResources = true;
            }

            //check the buildings in the border
            CheckBuildingsInBorder();
        }

        //called to check the resources inside the range of the border
        public void CheckBorderResources()
        {
            //We'll check the resources inside this border:
            if (ResourceMgr.AllResources.Count > 0)
            {
                for (int j = 0; j < ResourceMgr.AllResources.Count; j++)
                {
                    if (ResourceMgr.AllResources[j] != null) //if the resource is valid
                    {
                        if (ResourcesInRange.Contains(ResourceMgr.AllResources[j]) == false && ResourceMgr.AllResources[j].FactionID == -1)
                        {
                            //Making sure that it doesn't already exist before adding it.
                            if (Vector3.Distance(ResourceMgr.AllResources[j].transform.position, transform.position) < Size)
                            {
                                ResourcesInRange.Add(ResourceMgr.AllResources[j]);
                                ResourceMgr.AllResources[j].FactionID = MainBuilding.FactionID;
                            }
                        }
                    }
                }
            }
        }

        void Update()
        {
            if (FactionMgr != null)
            {
                //if all buildings that need to be built around the border are still unbuilt and there are actual buildings to place while making sure that the building is placed
                if (AllBuilt == false && BuildingsInsideBorder.Count > 0 && FactionMgr.BuildingCenters.IndexOf(MainBuilding) >= 0)
                {
                    PlaceBorderBuildings();
                }
            }
        }

        //attempt to place required buildings inside the BuildingsInsideBorder list:
        void PlaceBorderBuildings()
        {
            //If we still haven't built everything needed:
            if (CheckBuildingsTimer > 0)
            {
                CheckBuildingsTimer -= Time.deltaTime;
            }
            if (CheckBuildingsTimer < 0)
            {
                CheckBuildingsTimer = Random.Range(CheckBuildingsTimeRange.x, CheckBuildingsTimeRange.y);
                AllBuilt = true; //initially assume all buildings are built

                //Loop through all the buildings inside the array
                for (int i = 0; i < BuildingsInsideBorder.Count; i++)
                {
                    //If the building is not built yet:
                    if (BuildingsInsideBorder[i].MaxAmountReached == false)
                    {
                        //If the currrent amount + the in progress amount (amount of buildings that are being placed) is less than the max allowed amount, only then proceed:
                        if (BuildingsInsideBorder[i].ProgressAmount + BuildingsInsideBorder[i].CurrentAmount < BuildingsInsideBorder[i].RequiredAmount)
                        {
                            AllBuilt = false; //to keep check the next check keep this false
                            //ask the building mgr to build this
                            FactionMgr.BuildingMgr.AttemptToAddBuilding(BuildingsInsideBorder[i].Prefab, true, null);
                        }
                    }
                }
            }
        }

        //returns the ID of a building inside the border
        public int GetBuildingIDInBorder(string Code)
        {
            if (BuildingsInsideBorder.Count > 0)
            {
                //Loop through all the buildings inside the array:
                for (int i = 0; i < BuildingsInsideBorder.Count; i++)
                {
                    //When we find the building in the border's list, return it.
                    if (BuildingsInsideBorder[i].Prefab.Code == Code)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        //add the building to a building's list
        public void RegisterBuildingInBorder(Building Building)
        {
            //add the building to the list:
            BuildingsInRange.Add(Building);
            //First check if the building exists inside the border:
            int i = GetBuildingIDInBorder(Building.Code);
            if (i != -1)
            {

                //If we reach the maximum allowed amount for this item then add it:
                BuildingsInsideBorder[i].CurrentAmount++;
                if (BuildingsInsideBorder[i].CurrentAmount == BuildingsInsideBorder[i].MaxAmount)
                {
                    BuildingsInsideBorder[i].MaxAmountReached = true;
                }
            }
        }

        public void UnegisterBuildingInBorder(Building Building)
        {
            //remove the building from the list:
            BuildingsInRange.Remove(Building);
            //First check if the building exists inside the border:
            int i = GetBuildingIDInBorder(Building.Code);
            if (i != -1)
            {

                //If we reach the maximum allowed amount for this item then add it:
                BuildingsInsideBorder[i].CurrentAmount--;
                if (BuildingsInsideBorder[i].CurrentAmount < BuildingsInsideBorder[i].MaxAmount)
                {
                    BuildingsInsideBorder[i].MaxAmountReached = false;
                }
            }
        }

        //Progress amount for buildings inside the border, only for NPC factions so that building orders won't be asked many times:
        public void RegisterInProgressBuilding(string Code)
        {
            //First check if the building exists inside the border:
            if (GetBuildingIDInBorder(Code) != -1)
            {
                int i = GetBuildingIDInBorder(Code);

                //Update the progress amount:
                BuildingsInsideBorder[i].ProgressAmount++;
            }
        }

        public void UnregisterInProgressBuilding(string Code)
        {
            //First check if the building exists inside the border:
            if (GetBuildingIDInBorder(Code) != -1)
            {
                int i = GetBuildingIDInBorder(Code);

                //Update the progress amount:
                BuildingsInsideBorder[i].ProgressAmount--;
            }
        }

        public bool AllowBuildingInBorder(string Code)
        {
            //This determines if we're still able to construct a building inside the borders:
            //Loop through all the buildings inside the array:
            if (BuildingsInsideBorder.Count > 0)
            {
                for (int i = 0; i < BuildingsInsideBorder.Count; i++)
                {
                    //When we find the building in the border's list, return it.
                    if (BuildingsInsideBorder[i].Prefab.Code == Code)
                    {
                        if (BuildingsInsideBorder[i].MaxAmountReached == false)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            //If the building doesn't belong the buildings allowed in border, then we're free to place it without any limitations:
            return true;
        }

        //check all buildings placed in the range of this border
        public void CheckBuildingsInBorder()
        {
            //if this is a single player game or a multiplayer game and this is the local player
            if (GameManager.MultiplayerGame == false || (GameManager.MultiplayerGame == true && GameManager.PlayerFactionID == MainBuilding.FactionID))
            {
                int FactionTypeID = GameMgr.GetFactionTypeID(GameMgr.Factions[MainBuilding.FactionID].Code); //Get the faction type ID depending on the code presented with this faction.
                                                                                                             //This checks if there are buildings that are faction type specific and remove/keep them based on the faction code:
                                                                                                             //Loop through all the buildings inside the array:
                if (BuildingsInsideBorder.Count > 0)
                {
                    int i = 0;
                    while (i < BuildingsInsideBorder.Count)
                    {
                        //When a building has 
                        if (BuildingsInsideBorder[i].FactionCode != "")
                        {
                            if (FactionTypeID != GameMgr.GetFactionTypeID(BuildingsInsideBorder[i].FactionCode))
                            { //if the faction code is different
                                BuildingsInsideBorder.RemoveAt(i);
                            }
                            else
                            {
                                i++;
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            }
        }
    }
}