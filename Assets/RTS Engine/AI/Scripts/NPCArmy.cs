using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* NPC Army script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine 
{
	public class NPCArmy : MonoBehaviour {

		//Some of the variables are Vector2 variables. The x presents the minimum value and the y presents the maximum value.
		//A random value will be chosen each time the variable is used. If you want to have a fixed value, make sure the x and y are identical.
		public int FactionID; //Faction's ID.

		[System.Serializable]
		public class ArmyPrefabsVars
		{
			public Unit Prefab; //add the prefabs in order of preferance, the faction will always attempt to produce the first one in the list if possible, if not, it checks the next one in the list
			[HideInInspector]
			public List<Building> SpawnedBuildings = new List<Building>(); //a list of the spawned buildings that can produce the desired prefab.
		}

		//The class below holds all the info about the army units:
		[System.Serializable]
		public class ArmyVars 
		{
			[HideInInspector]
			public Unit Prefab; //Make sure it has the attack component.
		    [HideInInspector]
			public Building SourceBuilding = null;
			[HideInInspector]
			public int PrefabID;
			public ArmyPrefabsVars[] Prefabs;
			public int AmountPerHundred = 20; //The sum of this variable for all units in the class must be equal to 100.
			[HideInInspector]
			public int AmountNeeded = 0; //Determined depending on the enemy team and how much resources this team has.
			[HideInInspector]
			public List<Unit> CurrentUnits;
			[HideInInspector]
			public int ProgressAmount = 0; //Holds the amount of the units the team is currently creating
			//Resources:
			[HideInInspector]
			public bool AskedForTaskRscr = false;
		}
        [Header("Army Unit Management:")]
        //The timer that determines when to check if we have the required army power to launch an attack, create army units, create army buildings, etc...
        public Vector2 CheckArmyReload = new Vector2(10.0f, 15.0f);
        [HideInInspector]
        public float CheckArmyTimer;

        public ArmyVars[] ArmyUnits;

		public Vector2 DefaultSearchRange = new Vector2(8.0f,10.0f); //The default attacking range for units when they are not attacking nor defending.

        //Non attack units:
        //max ratio of units who don't have the Attack component in relation to all spawned units
        public Vector2 NonAttackUnitsRatioRange = new Vector2(0.3f, 0.5f);
        float NonAttackUnitsRatio;

        //maximum amount of units who don't have the Attack component.
        public Vector2 MaxNonAttackUnitsRange = new Vector2(12, 15);
        int MaxNonAttackUnits;

        [Header("Offense:")]

        public bool CanAttack = false; //If true, this faction can launch attacks on other factins, if not it will only defend.
        //Setting target army timer:
        public Vector2 SetTargetReload = new Vector2(4.0f, 6.0f); //Time needed in peace to determine the target enemy:
		[HideInInspector]
		public float SetTargetTimer;

		public bool RandomlyPickTarget = true; //If true, the faction will randomly pick a target faction, if not, it will pick the weakest one in terms of army power.
		[HideInInspector]
		public FactionManager TargetFaction; //Self explanatory
		[HideInInspector]
		public bool Attacking = false; //Is the team currently launching an attack.

		public Vector2 ArmyPowerRatioRange = new Vector2(0.6f,0.8f); // "The attacker's team army power" / "The target's team army power" must be > this variable to launch an attack.
		[HideInInspector]
		public float ArmyPowerRatio = 0.7f;

		public Vector2 MinArmyPowerRange = new Vector2(130,200); //The minimum army power that the team must have at all times
		[HideInInspector]
		public float MinArmyPower = 150;
		//The timer used to give orders for attacking units:
		public Vector2 AttackTimeRange = new Vector2(3.0f, 5.0f);
		float AttackTimer;

		//the ratio of custom units that can go with the army while attacking: (ratio range must be between 0 and 1)
		public Vector2 HealersRatioRange = new Vector2(0.6f, 0.8f);
		float HealersRatio;
		public Vector2 ConvertersRatioRange = new Vector2 (0.6f, 0.8f);
		float ConvertersRatio;

		List<Unit> ExtraArmyUnits = new List<Unit>();

        //War state:
        [HideInInspector]
		public List<Unit> Army; //The army (units) that will launch the attack
		[HideInInspector]
		public float AttackingArmyPower = 0.0f; //Current attacking army power
		public float AttackingSupportRange = 10.0f; //If a unit (that belongs to the attacking army) gets attacked, the units inside this range will be called for help.

        [Header("Defense:")]

        public float SurrenderArmyPower = 80f; //If the attacking army reaches this power, the faction stops sending new units to attack and starts rebuilding the army.
		[HideInInspector]
		public Building TargetBuilding; //The target faction building to attack next
		[HideInInspector]
		public bool TargetingBuilders = false;
		[HideInInspector]
		public Vector3 LastTargetBuildingPos;

		public Vector2 DefenseUnitsRatioRange = new Vector2(0.15f,0.3f);
		[HideInInspector]
		public float DefenseUnitsRatio = 0.2f; //The part of the army that will stay to defend when an attack is launched.


		//Defense: 
		[HideInInspector]
		public bool UnderAttack = false;//True when the faction is under attack.

		[HideInInspector]
		public float DefenseRatio = 1.2f; //The ratio of the defending army power in relation to the attacking army.
		[HideInInspector]
		public bool Defending = false; //True when the team is under attack and is defending the attack.

		//The timer that checks the defense state of the faction, whether it should keep defending or not and sets the defense orders for army units.
		public Vector2 DefenseReload = new Vector2(4.0f, 6.0f);
		float DefenseTimer;

		//Other scripts:
		[HideInInspector]
		public FactionManager FactionMgr;
		GameManager GameMgr;
        MovementManager MvtMgr;

		void Awake () {

			//Some values are chosen at the beginning of the game only
			ArmyPowerRatio = Random.Range(ArmyPowerRatioRange.x,ArmyPowerRatioRange.y);
			MinArmyPower = Random.Range (MinArmyPowerRange.x, MinArmyPowerRange.y);
			DefenseUnitsRatio = Random.Range (DefenseUnitsRatioRange.x, DefenseUnitsRatioRange.y);
			HealersRatio = Random.Range (HealersRatioRange.x, HealersRatioRange.y);
			ConvertersRatio = Random.Range (ConvertersRatioRange.x, ConvertersRatioRange.y);
            NonAttackUnitsRatio = Random.Range(NonAttackUnitsRatioRange.x, NonAttackUnitsRatioRange.y);
            MaxNonAttackUnits = Mathf.CeilToInt(Random.Range(MaxNonAttackUnitsRange.x, MaxNonAttackUnitsRange.y));

            Attacking = false;
			CheckArmyTimer = Random.Range(CheckArmyReload.x,CheckArmyReload.y);

			GameMgr = GameManager.Instance;
            MvtMgr = MovementManager.Instance;

            //If there's a map manager script in the scene, it means that we just came from the map menu, so the faction manager and the settings have been already set by the game manager:
            if (GameMgr.SinglePlayerMgr == null) {
				FactionMgr = GameMgr.Factions [FactionID].FactionMgr;
				FactionMgr.ArmyMgr = this;

				if (FactionMgr == null) {
					//If we can't find the AI Faction manager script, then print an error:
					Debug.LogError ("Can't find the AI Faction Manager for Faction ID: " + FactionID.ToString ());
				}
			}
		}

		void Start ()
		{
			//if this belongs to the local player, destroy it
			if (FactionID == GameManager.PlayerFactionID) {
				Destroy (this);
				return;
			} else {
				CheckArmyUnitsPriority (0, ArmyUnits.Length);
			}
		}

		//check army units preferance:
		public void CheckArmyUnitsPriority (int Min, int Max)
		{
			for (int i = Min; i < Max; i++) {
				bool Found = false;
				int j = 0;
				while (Found == false && j < ArmyUnits [i].Prefabs.Length) {
					if (ArmyUnits [i].Prefabs [j].SpawnedBuildings.Count > 0) {
						ArmyUnits [i].PrefabID = j;
						ArmyUnits [i].Prefab = ArmyUnits [i].Prefabs [j].Prefab;
						Found = true;
					}
					j++;
				}


				if (Found == false && ArmyUnits[i].Prefabs.Length > 0) {
					ArmyUnits [i].PrefabID = ArmyUnits[i].Prefabs.Length-1;
					ArmyUnits [i].Prefab = ArmyUnits [i].Prefabs [ArmyUnits[i].Prefabs.Length-1].Prefab;
				}

				ArmyUnits [i].SourceBuilding = null;
			}
		}

		//set the target for the next army unit to build:
		public void ReloadArmyUnitsPriority (Building Building, bool Add)
		{
			//make sure the building has army units:
			if (Building.ArmyUnits.Count > 0) {
				//loop through them:
				for (int i = 0; i < Building.ArmyUnits.Count; i++) {
					int NewPrefabID = 0;

					bool Found = false;
					int j = 0;

					while(Found == false && j < ArmyUnits.Length)
					{
						int x = 0;
						while (Found == false && x < ArmyUnits[j].Prefabs.Length) {
							if (ArmyUnits [j].Prefabs [x].Prefab.Code == Building.BuildingTasksList [Building.ArmyUnits [i]].UnitPrefab.Code) { //if the prefab was found.
								Found = true;

								if (Add == true) {
									//add the building:
									ArmyUnits [j].Prefabs [x].SpawnedBuildings.Add (Building);

									NewPrefabID = x;
								} else {
									ArmyUnits [j].Prefabs [x].SpawnedBuildings.Remove (Building);

									//if there no more buildings to produce this unit:
									if (ArmyUnits [j].Prefabs [x].SpawnedBuildings.Count == 0) {
										//check for priority:
										CheckArmyUnitsPriority(j,j);
									}
								}
							}
							x++;
						}
						j++;
					}

					if (Found == true && Add == true) {
						if (NewPrefabID < ArmyUnits [j-1].PrefabID) {
							ArmyUnits [j-1].PrefabID = NewPrefabID;
							ArmyUnits [j - 1].Prefab = ArmyUnits [j - 1].Prefabs [NewPrefabID].Prefab;
						}
					}

				}
			}
		}

		// Update is called once per frame
		void Update () {
			//Defending:
			if (Defending == true) {
				//defense timer
				if (DefenseTimer > 0) {
					DefenseTimer -= Time.deltaTime;
				}
				if (DefenseTimer < 0) {
					//stop defending when the timer is done.
					StopDefending ();
				}

			}

			//searching the target faction timer:
			if (SetTargetTimer > 0) {
				SetTargetTimer -= Time.deltaTime;
			}
			if (SetTargetTimer < 0) {
				//pick the target faction:
				SetTargetTimer = 0.0f;
				PickTargetFaction ();
			}

			//if we don't have target yet and the target timer is not running and we're no longer in peace time
			if (TargetFaction == null && GameMgr.PeaceTime == 0.0f && SetTargetTimer == 0.0f) {
				//start the timer that will set the target faction at the end.
				SetTargetTimer = Random.Range(SetTargetReload.x,SetTargetReload.y);
			}

			//if we do have a target faction
			if (TargetFaction != null) {
				//if the target faction is the same as our faction or the targe faction has lost.
				if (TargetFaction.FactionID == FactionID || GameMgr.Factions [TargetFaction.FactionID].Lost == true) {
					//then the target faction is invalid.
					TargetFaction = null;
				}
			}

			//taking care of the army, creating army units and buildings:
			if (CheckArmyTimer > 0) {
				CheckArmyTimer -= Time.deltaTime;
			}
			if (CheckArmyTimer <= 0) {
				CheckArmyTimer = Random.Range(CheckArmyReload.x,CheckArmyReload.y);
				//Try to make the army:
				if (ArmyUnits.Length > 0) {
					//Decide how many army units we need to create:

					float ArmyPower = 0.0f;
					if (TargetFaction != null) {
						ArmyPower = TargetFaction.ArmyPower*ArmyPowerRatio;
					}
					if (ArmyPower < MinArmyPower)
						ArmyPower = MinArmyPower;

					//Loop through all different types of army units:
					for (int i = 0; i < ArmyUnits.Length; i++) {
						//Determine how many units we need:
						ArmyUnits [i].AmountNeeded = Mathf.RoundToInt(((ArmyUnits [i].AmountPerHundred*ArmyPower)/100)/ArmyUnits [i].Prefab.gameObject.GetComponent<Attack>().UnitDamage) + 1;

						if (ArmyUnits [i].CurrentUnits.Count < ArmyUnits [i].AmountNeeded) {
							int j = 0;
							//We will check if the units we're searching for are already spawned.
							while (j < FactionMgr.Army.Count && ArmyUnits [i].CurrentUnits.Count < ArmyUnits [i].AmountNeeded) {
								//When the needed unit is found, add it to the list:
								if (FactionMgr.Army [j] != null && ArmyUnits [i].Prefab != null) {
									if (ArmyUnits [i].CurrentUnits.Contains (FactionMgr.Army [j].UnitMgr) == false) {
										//making sure the army unit has the code that we're looking for:
										if (FactionMgr.Army [j].UnitMgr.Code == ArmyUnits [i].Prefab.Code) {
											//add it to tAAhe list.
											ArmyUnits [i].CurrentUnits.Add (FactionMgr.Army [j].UnitMgr);
											FactionMgr.Army [j].ArmyUnitID = i;

											//Make all enemy units attack on range:
											FactionMgr.Army [j].AttackInRange = true;

											if (ArmyUnits [i].ProgressAmount > 0) {
												ArmyUnits [i].ProgressAmount--;
											}
										}
									}
								}
								j++;
							}

							int ID = -1; //This will hold the task ID that produces the required army type, if found.
							//If we still haven't found enough units then we'll simply look for buildings that can produce them.
							if (ArmyUnits [i].CurrentUnits.Count+ArmyUnits [i].ProgressAmount < ArmyUnits [i].AmountNeeded) {
								int n = 0;
								while (n < ArmyUnits [i].Prefabs [ArmyUnits [i].PrefabID].SpawnedBuildings.Count && ArmyUnits [i].CurrentUnits.Count+ArmyUnits [i].ProgressAmount < ArmyUnits [i].AmountNeeded) {
									Building Building = ArmyUnits [i].Prefabs [ArmyUnits [i].PrefabID].SpawnedBuildings [n];
									//Make sure that the current building has enough health and is completely built.
									//Check if the building can produce army units and that it still have avilable space for a new task and that the building is not upgrading.
									if (Building.ArmyUnits.Count > 0 && Building.TasksQueue.Count < Building.MaxTasks && Building.BuildingUpgrading == false) { 
										int Counter = 0;
										while (ID == -1 && Counter < Building.ArmyUnits.Count) {
                                            //if this is the unit we're looking for.
                                            if (Building.FactionID == FactionID && Building.BuildingTasksList[Building.ArmyUnits[Counter]].UnitPrefab.Code == ArmyUnits[i].Prefab.Code)
                                            {
                                                //make sure that the unit hasn't reached its limit:
                                                if (FactionMgr.HasReachedLimit(Building.BuildingTasksList[Building.ArmyUnits[Counter]].UnitPrefab.Code) == false)
                                                {
                                                    ID = Building.ArmyUnits[Counter];
                                                }
                                            }
                                            Counter++;
										}
										if (ID >= 0) {
											if (Building.IsBuilt == true && Building.Health >= Building.MinTaskHealth) {
												//Check if we have enough resources to launch this task:
												while(GameMgr.ResourceMgr.CheckResources (Building.BuildingTasksList[ID].RequiredResources, FactionID, GameMgr.ResourceMgr.FactionResourcesInfo[FactionID].NeedRatio) == true && ArmyUnits [i].CurrentUnits.Count+ArmyUnits [i].ProgressAmount < ArmyUnits [i].AmountNeeded && ArmyUnits [i].ProgressAmount < (ArmyUnits [i].AmountNeeded-ArmyUnits [i].CurrentUnits.Count) && GameMgr.Factions [FactionID].CurrentPopulation < GameMgr.Factions [FactionID].MaxPopulation) {
													//Add the new task to the building's task queue
													GameMgr.TaskMgr.LaunchTask (Building, ID, -1, TaskManager.TaskTypes.CreateUnit);

													//Start waiting for the new unit to be created:
													ArmyUnits [i].ProgressAmount++;
												}

												if (GameMgr.ResourceMgr.CheckResources (Building.BuildingTasksList[ID].RequiredResources, FactionID, GameMgr.ResourceMgr.FactionResourcesInfo[FactionID].NeedRatio) == false) { // No resources are available for this task:
													int RequiredAmount = ArmyUnits [i].AmountNeeded - (ArmyUnits [i].CurrentUnits.Count + ArmyUnits [i].ProgressAmount);
													for(int x = 0; x < Building.BuildingTasksList[ID].RequiredResources.Length; x++)
													{
														//ask the resource manager for resources to launch the unit creation task.
														if (ArmyUnits [i].AskedForTaskRscr == true) {
															RequiredAmount = 0;
														}
														FactionMgr.ResourceMgr.LookForResources (Building.BuildingTasksList [ID].RequiredResources [x].Name, Building.BuildingTasksList [ID].RequiredResources [x].Amount*RequiredAmount);

													}

													//Ask for resources:
													ArmyUnits[i].AskedForTaskRscr = true;
												}
												//If we don't have any more space for new units:
												if (GameMgr.Factions [FactionID].CurrentPopulation >= GameMgr.Factions [FactionID].MaxPopulation) {
													FactionMgr.BuildingMgr.AttemptToAddBuilding (FactionMgr.BuildingMgr.PopulationBuilding, false, null);
												}
											} else {
												//Call for builders as the building hasn't been built yet.
												if(Building.Placed == true) FactionMgr.BuildingMgr.ConstructBuilding(Building);
											}
										}
									} else {
										//...
									}

									n++;
								}
							}
							//No building to create 
							if (ID == -1 && ArmyUnits [i].CurrentUnits.Count+ArmyUnits [i].ProgressAmount < ArmyUnits [i].AmountNeeded) {
								//We need to construct a buildign that can create the required army unit:
								//look for the suitable building to build this unit if we don't already have it saved:
								if (ArmyUnits [i].SourceBuilding == null) {
									ArmyUnits [i].SourceBuilding = FactionMgr.BuildingMgr.GetBuildingByUnit (ArmyUnits [i].Prefab);
								}
								FactionMgr.BuildingMgr.AttemptToAddBuilding (ArmyUnits [i].SourceBuilding,false,null);
							}
						}
					}

					//Make sure it's not the peace time and that the AI player is not under attack:
					if (GameMgr.PeaceTime == 0.0f && Defending == false && Attacking == false && UnderAttack == false) {
						//Attempt to launch an attack:
						if (CanAttack == true && TargetFaction != null) {
							//If the team can actually launch an attack and they have enough army power:
							if (FactionMgr.ArmyPower / TargetFaction.ArmyPower >= ArmyPowerRatio && FactionMgr.ArmyPower >= MinArmyPower) {
								Attacking = true;
								LaunchAttack ();
							}
						}
					}
				}
			}


			if (Attacking == true) { //If the faction is currently in war:
				if (TargetFaction != null) {
					if (AttackTimer > 0) {
						//Handling the attack timer.
						AttackTimer -= Time.deltaTime;

						if (TargetBuilding == null) {
							AttackTimer = -1.0f;
						} else if (TargetingBuilders == false && TargetBuilding.WorkerMgr.CurrentWorkers > 0) {
							SendUnitsToTargetBuilding ();
						}
					}
					if (AttackTimer < 0) {
						//Each time the attack timer is done, the faction gives order to its attacking units:

						//First check if we have reached the surrender army power:
						if (AttackingArmyPower <= SurrenderArmyPower) {
							//Cancel the attack.
							StopAttack();
						}
						else
						{
							if (TargetBuilding == null) { //If the target building is not valid
								//Search for one:
								SetTargetBuilding();
							}

							if (TargetBuilding != null) {
								//If we've found a target building, command the attacking units.
								SendUnitsToTargetBuilding();
							}

							AttackTimer = Random.Range (AttackTimeRange.x, AttackTimeRange.y); //Reload the attack timer.
						}
					}
				} else {
					StopAttack ();
				}
			}
		}

		//a method that launches an attack on the target faction:
		public void LaunchAttack ()
		{
			//clear the last army to start a new one:
			Army.Clear ();
			AttackingArmyPower = 0.0f;

			//Loop through all different types of army units:
			for (int i = 0; i < ArmyUnits.Length; i++) {
				//Calculate how many units of this type are going to attack and how many will stay to defend.
				int NeededAmount = ArmyUnits [i].CurrentUnits.Count-Mathf.RoundToInt(ArmyUnits [i].CurrentUnits.Count*DefenseUnitsRatio);
				int j = 0;
				while(j < ArmyUnits [i].CurrentUnits.Count && NeededAmount > 0) {
					if (ArmyUnits [i].CurrentUnits [j] != null) {
						//Add it to the current army list:
						Army.Add(ArmyUnits [i].CurrentUnits [j]);
						AttackingArmyPower += ArmyUnits [i].CurrentUnits [j].AttackMgr.UnitDamage;
						NeededAmount--;
					}
					j++;
				}
			}

			if (TargetBuilding == null) { //If the target building is not valid
				//Search for one:
				SetTargetBuilding();
			}

			if (TargetBuilding != null) {
				//If we've found a target building, command the attacking units.
				SendUnitsToTargetBuilding();
			}

			AttackTimer = Random.Range (AttackTimeRange.x, AttackTimeRange.y); //Set the attack timer.

			if (TargetFaction != null) { //if the target faction is a NPC (has a army manager)
				if (TargetFaction.ArmyMgr != null) {
					TargetFaction.ArmyMgr.UnderAttack = true; //mark it as under attack
				}
			}

		}

		//Search for the nearest enemy building center to attack, we will search from the capital of this faction:
		public void SetTargetBuilding ()
		{
			//always start the search from the faction capital..
			Vector3 SearchFrom = FactionMgr.BuildingCenters [0].transform.position;
			//unless we've already targeted a building before..
			if (TargetBuilding != null) {
				//in that case, start from the last targeted building:
				SearchFrom = LastTargetBuildingPos;
			}
			TargetBuilding = null;

			//make sure the target faction still has a capital building
			if (GameMgr.Factions[TargetFaction.FactionID].CapitalBuilding != null) {
				//search for the nearest building to attack.
				TargetBuilding = TargetFaction.BuildingCenters [0];
				float Distance = Vector3.Distance (TargetBuilding.transform.position, SearchFrom);
				if (TargetFaction.Buildings.Count > 1) {
					for (int i = 1; i < TargetFaction.Buildings.Count; i++) {
						if (TargetFaction.Buildings [i].Placed == true && TargetFaction.Buildings [i].CanBeAttacked == true) {
							if (Distance > Vector3.Distance (TargetFaction.Buildings [i].transform.position, SearchFrom)) {
								TargetBuilding = TargetFaction.Buildings [i];
								Distance = Vector3.Distance (TargetFaction.Buildings [i].transform.position, SearchFrom);
							}
						}
					}
				}

				if (TargetBuilding != null) {
					LastTargetBuildingPos = TargetBuilding.transform.position;
				}
			} else {
				//If the capital building of the target faction does not exist then 
				TargetFaction = null;
				Attacking = false;

				StopAttack ();

			}

		}

		//Send units to attack a center:
		public void SendUnitsToTargetBuilding ()
		{
			if (TargetBuilding != null) { //if there's a target building
				GameObject Target = TargetBuilding.gameObject;

				//if the target building is currently being fixed by builders
				if (TargetBuilding.WorkerMgr.CurrentWorkers > 0) {
					int n = 0;
					//attack these builders instead.
					while (n < TargetBuilding.WorkerMgr.WorkerPositions.Length) {
                        //first check if there's a worker in this slot:
                        if (TargetBuilding.WorkerMgr.WorkerPositions[n].CurrentUnit != null)
                        {
                            //if the worker is constructing
                            if (TargetBuilding.WorkerMgr.WorkerPositions[n].CurrentUnit.BuilderMgr.IsBuilding == true)
                            {
                                //set him as target
                                Target = TargetBuilding.WorkerMgr.GetWorker().gameObject;
                                TargetingBuilders = true;
                            }
                        }
						n++;
					}

				} else {
					TargetingBuilders = false;
				}

				if (Army.Count > 0) {
					for (int i = 0; i < Army.Count; i++) {

						if (Army [i] != null) {

							if (Army [i].AttackMgr.AttackTarget == null) { //set the attacking support range that will call nearby units for support.
								Army [i].AttackMgr.SearchRange = AttackingSupportRange;
							}
						}
					}
                    MvtMgr.LaunchAttack (Army, Target.gameObject, (TargetingBuilders == true) ? MovementManager.AttackModes.Change : MovementManager.AttackModes.None); //launch the attack.

					//move healers and converters to the target building
					//Healers:
					int HealersAmount = Mathf.RoundToInt(FactionMgr.Healers.Count*HealersRatio);
					int ConvertersAmount = Mathf.RoundToInt(FactionMgr.Converters.Count * ConvertersRatio);
					//if there are healers to send:
					int j = 0;
					while (HealersAmount > 0 && j < FactionMgr.Healers.Count) {
						if (FactionMgr.Healers [j].TargetUnit == null) { //only move the unit if it has no target
                            MvtMgr.Move(FactionMgr.Healers[j].UnitMvt, Target.transform.position, TargetBuilding.Radius, Target, InputTargetMode.Building);
						}
						HealersAmount--;
						ExtraArmyUnits.Add (FactionMgr.Healers [j].UnitMvt);
						j++;
					}
					j = 0;
					//if there are converters to send, do it:
					while (ConvertersAmount > 0 && j < FactionMgr.Converters.Count) {
						if (FactionMgr.Converters [j].TargetUnit == null) {
                            MvtMgr.Move(FactionMgr.Converters[j].UnitMvt, Target.transform.position, TargetBuilding.Radius, Target, InputTargetMode.Building);
						}
						ConvertersAmount--;
						ExtraArmyUnits.Add (FactionMgr.Converters [j].UnitMvt);
						j++;
					}
				}
			}
		}

		//Cancel the attack:
		public void StopAttack ()
		{
			//we're not attacking anymore.
			Attacking = false;
			TargetBuilding = null;

			SendBackArmy (); //send back the army units.

			if (Army.Count > 0) {
				for (int i = 0; i < Army.Count; i++) {
					if (Army [i] != null) {
						//set the attack range to default:
						Army [i].AttackMgr.SearchRange = Random.Range(DefaultSearchRange.x, DefaultSearchRange.y);

					}
				}
			}

			Army.Clear (); //clear the army list.
			AttackingArmyPower = 0.0f;

		}

		//the method that sends back the units when they stopped attacking.
		public void SendBackArmy ()
		{
			List<Unit> AllUnits = new List<Unit> ();
			for (int i = 0; i < ArmyUnits.Length; i++) {
				if (ArmyUnits [i].CurrentUnits.Count > 0) {
					//add all army units in one list:
					AllUnits.AddRange (ArmyUnits [i].CurrentUnits);
				}
			}

			//add the extra units in the same list:
			AllUnits.AddRange(ExtraArmyUnits);
			ExtraArmyUnits.Clear (); //clear the extra units list.

			//If one of the current units in the list is null (the unit is dead), clear it to create another one:
			for (int j = 0; j < AllUnits.Count; j++) {
				if (AllUnits[j] != null) {
					Building GotoBuilding = null;
					if (AllUnits[j].CreatedBy != null) {
						//If the unit's building that created it is still available
						GotoBuilding = AllUnits[j].CreatedBy;
					} else {
						//If not the unit will simply move back to the capital
						GotoBuilding = FactionMgr.BuildingMgr.CapitalBuilding;
					}

					AllUnits[j].CancelAttack (); //cancel attacking
                    MvtMgr.Move(AllUnits[j], GotoBuilding.transform.position, GotoBuilding.Radius, GotoBuilding.gameObject, InputTargetMode.Building);
				} 
				j++;

			}
		}

		//Defending:

		//start defending the faction's buildings:
		public void StartDefending ()
		{
			//defense timer, that stops defending at its end.
			DefenseTimer = Random.Range(DefenseReload.x, DefenseReload.y);

			//we're not attacking, we're just defending.
			Attacking = false;
			TargetBuilding = null;
			Defending = true;
			UnderAttack = true;

		}

		//pick a defense center building:
		public void SetDefenseCenter (Building Center, int SourceFactionID)
		{
            //TO BE CHANGED
			//search for all enemy units inside the center's borders:
			Collider[] ObjsInBorder = Physics.OverlapSphere (Center.transform.position, Center.CurrentCenter.Size);
			float EnemyArmyPower = 0.0f;

			foreach (Collider Obj in ObjsInBorder) {
				Attack AttackUnit = Obj.gameObject.GetComponent<Attack> ();
				if (AttackUnit != null) {
                    //making sure it's a unit and not a building:
                    if (AttackUnit.AttackerType == Attack.AttackerTypes.Unit)
                    {
                        //If the current obj inside the border belongs to the enemy army:
                        if (AttackUnit.UnitMgr.FactionID == SourceFactionID)
                        {
                            //count the potential army power of the attacking faction:
                            EnemyArmyPower += AttackUnit.UnitDamage;
                        }
                    }
				}
			}

			//set the power needed to counter this attck:
			float PowerNeeded = EnemyArmyPower * DefenseRatio;

			int i = 0;
			int j = 0;

			while (PowerNeeded > 0 && i < ArmyUnits.Length) {
				//determine how many units we're sending to defend:
				int MaxAmount = Mathf.RoundToInt(((ArmyUnits [i].AmountPerHundred*PowerNeeded)/100)/ArmyUnits [i].Prefab.gameObject.GetComponent<Attack>().UnitDamage);
				if (MaxAmount > ArmyUnits [i].CurrentUnits.Count) {
					MaxAmount = ArmyUnits [i].CurrentUnits.Count;
				}
				while (PowerNeeded > 0 && j < MaxAmount && j < ArmyUnits [i].CurrentUnits.Count) {
					if (ArmyUnits [i].CurrentUnits[j]) {
						//If the unit doesn't have anywhere to go (or doesn't have a task), move it to the building:
						ArmyUnits [i].CurrentUnits [j].AttackMgr.AttackRangeFromCenter = true;
						ArmyUnits [i].CurrentUnits [j].AttackMgr.AttackRangeCenter = Center;

						PowerNeeded -=  ArmyUnits [i].CurrentUnits [j].AttackMgr.UnitDamage;
					}
					j++;
				}
				i++;
			}


			StartDefending ();
		}

		//a method that stops defending
		public void StopDefending ()
		{
			//stop defending:
			Defending = false;
			UnderAttack = false;

			//loop through all the current units
			for (int i = 0; i < ArmyUnits.Length; i++) {
				if (ArmyUnits [i].CurrentUnits.Count > 0) {
					//If one of the current units in the list is null (the unit is dead), clear it to create another one:
					int j = 0;
					while (j < ArmyUnits [i].CurrentUnits.Count) {
						if (ArmyUnits [i].CurrentUnits [j] != null) {
							//stop defending the city center.
							ArmyUnits [i].CurrentUnits [j].AttackMgr.AttackRangeFromCenter = false;
						}
						j++;
					}
				}
			}

			SendBackArmy (); //send back army units to the buildings that created them.
		}

		//Picking the weakest faction or picking a random target:
		public void PickTargetFaction ()
		{
			//if we're not picking the target faction randomly.
			if (RandomlyPickTarget == false) {
				//Pick the weakest enemy faction available:
				int TargetID = -1;
				for (int i = 0; i < GameMgr.Factions.Count; i++) {
					if (i != FactionID && GameMgr.Factions [i].Lost == false) {
						if (TargetID == -1) {
							TargetID = i;
						} else {
							if (GameMgr.Factions [i].FactionMgr.ArmyPower < GameMgr.Factions [TargetID].FactionMgr.ArmyPower) {
								TargetID = i;
							}
						}
					}
				}

				if (TargetID >= 0) {
					TargetFaction = GameMgr.Factions [TargetID].FactionMgr;
				}
			} else {
				List<FactionManager> AvailableFactions = new List<FactionManager>();
				for (int i = 0; i < GameMgr.Factions.Count; i++) {
					if (i != FactionID && GameMgr.Factions [i].Lost == false) {
						AvailableFactions.Add (GameMgr.Factions [i].FactionMgr);
					}
				}

				//Randomly pick one:
				if (AvailableFactions.Count > 0) {
					TargetFaction = AvailableFactions [Random.Range (0, AvailableFactions.Count)];
				}
			}
		}

        //Handling non attacking units (civilians):
        public bool CanCreateNonAttackUnit ()
        {
            //if the already created non attack units amount exceeds the maximum allowed amount
            if (FactionMgr.NonArmy.Count >= MaxNonAttackUnits)
                return false; //can't create more.

            //if the max amount of non attack units hasn't been reached, we check if the current amount matches the defined ratio
            if (FactionMgr.NonArmy.Count >= NonAttackUnitsRatio * GameMgr.Factions[FactionID].MaxPopulation)
                return false; //can't create more.

            //if we reached this point then it's allowed to create non attacking units:
            return true;
        }
	}
}