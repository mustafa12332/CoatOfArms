using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* NPC Unit Spawner script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine 
{
	public class NPCUnitSpawner : MonoBehaviour {

		public int FactionID = 0;

		[System.Serializable]
		public class UnitsVars
		{
			public Unit Prefab; //unit's prefab
			[HideInInspector]
			public List<Unit> CurrentUnits; //the list of spawned units from this type.
			[HideInInspector]
			public List<Building> SourceBuildings; //list of buildings that can create this type of unit
			[HideInInspector]
			public List<int> TasksIDs; //list of tasks ID of the source buildings that are for this unit.

			public Vector2 PopulationRatio = new Vector2(0.02f,0.04f); //ratio of this unit inside the whole population.
			[HideInInspector]
			public int AmountNeeded; // the amount that the faction wants to create.
			public int MaxAmount = 3; //maximum amount to have.
			[HideInInspector]
			public int ProgressAmount = 0;

			public float StartCreatingAfter = 10.0f; //time in seconds at which the faction will start spawning the unit

			public Vector2 SpawnReloadRange = new Vector2(15.0f,20.0f); //time needed between spawning two consecutive units.
			[HideInInspector]
			public float SpawnTimer;
			[HideInInspector]
			//done creating?
			public bool AllCreated = false;
		}
		public UnitsVars[] Units;
		[HideInInspector]
		//true when all units with the right amount have been created:
		public bool AllCreated = false;

		GameManager GameMgr;
		[HideInInspector]
		public FactionManager FactionMgr;

		void Awake () {
			GameMgr = GameManager.Instance;

			//If there's a map manager script in the scene, it means that we just came from the map menu, so the faction manager and the settings have been already set by the game manager:
			if (GameMgr.SinglePlayerMgr == null) {
				FactionMgr = GameMgr.Factions [FactionID].FactionMgr;
				FactionMgr.UnitSpawner = this;

				if (FactionMgr == null) {
					//If we can't find the AI Faction manager script, then print an error:
					Debug.LogError ("Can't find the AI Faction Manager for Faction ID: " + FactionID.ToString ());
				}
			}
		}

		void Start ()
		{
			//if this belongs to the local player or there are no units to create, destroy it
			if (FactionID == GameManager.PlayerFactionID || Units.Length == 0) {
				Destroy (this);
				return;
			}

			//set the spawn timers:

			if (Units.Length > 0) {
				for (int i = 0; i < Units.Length; i++) { //loop through the units to create
					Units[i].SpawnTimer = Random.Range(Units[i].SpawnReloadRange.x, Units[i].SpawnReloadRange.y);
				}
			}

			//look for already spawnd units:

		}

		void Update () {
			if (Units.Length > 0 && AllCreated == false)
			{
				int ReadyUnits = 0; //amount of unit type that are created with the correct amounts.
				for (int i = 0; i < Units.Length; i++) { //loop through the units to create
					if (Units [i].StartCreatingAfter > 0) {
						Units [i].StartCreatingAfter -= Time.deltaTime; //the timer before starting to create any unit
					} else {
						//spawn timer:
						if (Units [i].SpawnTimer > 0) {
							Units [i].SpawnTimer -= Time.deltaTime;
						} else {
							Units [i].AllCreated = true; //initially we will set this to true then check:
							ReadyUnits++;

							//restart the timer:
							Units[i].SpawnTimer = Random.Range(Units[i].SpawnReloadRange.x, Units[i].SpawnReloadRange.y);

							//calculate the amount to create from this unit:
							Units [i].AmountNeeded = Mathf.RoundToInt (GameMgr.Factions [FactionID].MaxPopulation * Random.Range(Units [i].PopulationRatio.x, Units [i].PopulationRatio.y));
							if (Units [i].AmountNeeded > Units [i].MaxAmount) { //if the current goal is greater than the max allowed amount.
								Units [i].AmountNeeded = Units [i].MaxAmount; //set it to the limit
							}

							if (Units [i].CurrentUnits.Count < Units [i].AmountNeeded) { //still haven't reached our goal?
								Units [i].AllCreated = false;
								ReadyUnits--;
							}


							if (Units [i].AllCreated == false) { //still need to reach our goal:
                                //check if the unit hasn't reached its limit:
                                if (FactionMgr.HasReachedLimit(Units[i].Prefab.Code) == false)
                                {
                                    if ((GameMgr.Factions[FactionID].MaxPopulation - GameMgr.Factions[FactionID].CurrentPopulation) > 0)
                                    { //if we still have space in the population slots:
                                      //search for a building to create this unit:
                                        int AmountLeft = Units[i].AmountNeeded - Units[i].CurrentUnits.Count - Units[i].ProgressAmount; //Amount of units that we still need to create
                                        int j = 0;

                                        while (AmountLeft > 0 && j < Units[i].SourceBuildings.Count)
                                        { //go through the buildings that can create this unit:
                                            while (AmountLeft > 0 && Units[i].SourceBuildings[j].TasksQueue.Count < Units[i].SourceBuildings[j].MaxTasks)
                                            {//if this still has space for one more task and we still haven't finished creating all units

                                                //check if we have enough resources:
                                                if (GameMgr.ResourceMgr.CheckResources(Units[i].SourceBuildings[j].BuildingTasksList[Units[i].TasksIDs[j]].RequiredResources, FactionID, FactionMgr.ResourceMgr.ResourceNeedRatio))
                                                {
                                                    //create a unit
                                                    GameMgr.TaskMgr.LaunchTask(Units[i].SourceBuildings[j], Units[i].TasksIDs[j], i, TaskManager.TaskTypes.CreateUnit);
                                                    AmountLeft--;
                                                    Units[i].ProgressAmount++; //mark the new unit creation as pending
                                                }
                                                else
                                                {
                                                    AmountLeft = 0;
                                                }
                                            }
                                            j++;
                                        }
                                    }
                                    else
                                    {
                                        //not population
                                    }
                                }
                                else
                                {
                                    //limit reached
                                }
							}
						}
					}
				}

				if (ReadyUnits == Units.Length) {
					AllCreated = true; //all units have been created.
				}
			}
		}

		//whenever a building is added or removed, this method will be called to add/remove it to the lists in the spawn units:
		public void ReloadBuildingLists (Building Building, bool Add)
		{
			if (Building.UnitCreationTasks.Count > 0 && Units.Length > 0) { //Make sure that it can actually create units and that there units to spawn
				//go through the units to create:
				for (int i = 0; i < Units.Length; i++) {
					//loop through the unit creation tasks
					for (int j = 0; j < Building.UnitCreationTasks.Count; j++) {
						int ID = Building.UnitCreationTasks [j];
						if (Add == false) { //if this building is getting removed 
							//and we found it in one of the lists:
							if (Units [i].SourceBuildings.Contains (Building)) {
								Units [i].TasksIDs.RemoveAt (Units [i].SourceBuildings.IndexOf (Building)); //remove this from the task IDs list								
								//remove the building from the list:
								Units [i].SourceBuildings.Remove (Building);
							}
						} else { //if we are adding this building
							//do the units match? 
							if (Units [i].Prefab.Code == Building.BuildingTasksList [ID].UnitPrefab.Code) {
								//if this building is just built then the building will be added:
								Units [i].SourceBuildings.Add (Building);
								//add the task ID as well:
								Units[i].TasksIDs.Add(ID);
							}
						}
					}
				}
			}
		}
	}
}