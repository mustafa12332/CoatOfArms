using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* NPC Resource script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine 
{
	public class NPCResource : MonoBehaviour {

		public int FactionID = 0; //The faction ID managed by this script.

        [Header("Resource Collection Settings:")]
        public Vector2 ResourceNeedRatioRange = new Vector2 (1.0f, 1.2f);
		[HideInInspector]
		public float ResourceNeedRatio = 1.0f; //Must always be > 1.0. This determines how safe the NPC team would like to use their resources.
		//For example, if the "ResourceNeedRatio" is set to 2.0 and the faction needs 200 woods. Only when 400 (200 x 2) wood is available, the 200 can be used.

		public Vector2 ExploitResourceOnNeedRange = new Vector2 (0.8f, 0.85f);
		[HideInInspector]
		public float ExploitResourceOnNeed = 0.8f; //Determines how the faction decides to collect resources on need. For example, when the faction needs
		//200 wood to build something and they lack that amount: A Random float between 0.0f and 1.0f will be generated everytime the faction attempts to fill the needed amount of a resource
		//and if this float is below the ExploitResourceOnNeed float, then it will start collecting that resource. And vice versa.
		//So to disable exploiting resources on need, you can set this var to 0.0f. If you want to always exploit on need, you can set this var to 1.00f or above.

		[HideInInspector]
		public List<Building> CollectorsCreators = new List<Building> (); //This will store the possible buildings that can produce resource collectors
		//Everytime a new building that can produce resource collectors, it will be added to this list (This happens in the "Awake() event of the "Building.cs" script)

		public Vector2 DefaultResourceExploitRange = new Vector2 (0.8f, 0.85f);
		//Exploiting resource found around each building center:
		[HideInInspector]
		public float DefaultResourceExploit = 0.8f; //MUST BE BETWEEN 0 and 1. Defines the percentage of the resources (inside the faction's border) that can collected without needing at that time.

		//Each time the resource check timer is done, we check if the faction is correctly exploiting available resources.
		public Vector2 ResourceCheckTimeRange = new Vector2(4.5f,8.0f);
		float ResourceCheckTimer;

		[HideInInspector]
		public bool CheckResources = true; //If the last we've checked default resource exploiting and no action was taken, this will be set to false because it will be useless to check again.
		//But as soon as new building center is spawned, it will be set again to true to check the resources around it.

		//Moving collectors from one resource to another.
		public Vector2 MoveCollectorsRange = new Vector2 (0.2f,0.5f); //Must be between 0 and 1.
		//Defines the chance that in case we don't have resources to create resource collectors/or population is satured and all resource collectors are busy then when this is true some of the exisitng resource collectors can go
		float MoveCollectorsChance;

        [Header("Expanding Territory Settings:")]

        //Expanding:
        public bool ExpandWithoutNeed = false; //Can the faction build new centers without the need to actually expand to collect resources.
		public Vector2 ExpandTargetRange = new Vector2(0.18f, 0.23f);
		[HideInInspector]
		public float ExpandTarget = 0.2f; //Must be between 0 and 1. It refers to the percentage of the map that the team attempts to expand to. 1 being the whole map.
		[System.Serializable]
		public class ExpandResourcesVars
		{
			public string Name;
			public int Amount;
		}
		public ExpandResourcesVars[] ExpandResources; //The resources that need to be present to expand (create a new town center).

		//How much time is needed for the faction to decide to expand or not.
		public Vector2 ExpandTimeRange = new Vector2(10.0f, 20.0f);
		float ExpandTimer;

		//A list that holds all the currently exploited resources by this faction:
		public List<List<Resource>> ExploitedResources = new List<List<Resource>>();

        [Header("Task/Building Upgrade Settings:")]

        //The timer at which the NPC Faction decodes whether to upgrade units or not.
        public Vector2 TaskUpgradeTimeRange = new Vector2 (10.0f, 20.0f);
		[HideInInspector]
		public float TaskUpgradeTimer;
		//works like the "ResourceNeedRatioRange" (explained above) but it's a special one for upgrades.
		public Vector2 TaskUpgradeResourceRatioRange = new Vector2 (1f, 2.0f);
		[HideInInspector]
		public float TaskUpgradeResourceRatio;
		[HideInInspector]
		public bool AllUnitsUpgraded = false; //When all the possible upgrades in the placed buildings are made.

		//Building upgrades:
		//The timer at which the NPC Faction decodes whether to upgrade buildings or not.
		public Vector2 BuildingUpgradeTimeRange = new Vector2 (10.0f, 20.0f);
		[HideInInspector]
		public float BuildingUpgradeTimer;
		//works like the "ResourceNeedRatioRange" (explained above) but it's a special one for building upgrades..
		public Vector2 BuildingUpgradeResourceRatioRange = new Vector2 (1f, 2.0f);
		[HideInInspector]
		public float BuildingUpgradeResourceRatio;
		[HideInInspector]
		public bool AllBuildingsUpgraded = false; //When all the possible building upgrades in the placed buildings are made.

		//Drop off buildings:
		//List of resources that are being collected/that the faction wants to collect with the suitable drop off building for each resource
		public class FactionResourcesVars
		{
			public Resource Resource;
			public Building DropOff;
		}
		List<FactionResourcesVars> FactionResources = new List<FactionResourcesVars>();

        [Header("Drop Off Settings:")]
        //The timer at which the NPC faction checks whether all resources that are being collected have suitable drop off buildings or not
        public Vector2 DropOffControlTimeRange = new Vector2(10.0f, 20.0f);
		[HideInInspector]
		public float DropOffControlTimer;
		[HideInInspector]
		public bool DropOffControlDone = false; //when all drop off buildings are correctly placed, then this will be true and the timer will stop
		public bool MustHaveDropOff = true; //when true then the first priority of this faction is to have a warehouse building.

		//Resource generators:
		class ResourceGeneratorVars
		{
			public string Name;
			public Building Prefab;
		}
		List<ResourceGeneratorVars> ResourceGenerators = new List<ResourceGeneratorVars> ();

		//Other scripts:
		ResourceManager ResourceMgr;
		[HideInInspector]
		public FactionManager FactionMgr;
		GameManager GameMgr;

		// Use this for initialization
		void Awake () {

			ResourceNeedRatio = Random.Range(ResourceNeedRatioRange.x,ResourceNeedRatioRange.y);
			ExploitResourceOnNeed = Random.Range(ExploitResourceOnNeedRange.x,ExploitResourceOnNeedRange.y);
			DefaultResourceExploit = Random.Range(DefaultResourceExploitRange.x,DefaultResourceExploitRange.y);
			ExpandTarget = Random.Range(ExpandTargetRange.x,ExpandTargetRange.y);
			TaskUpgradeResourceRatio = Random.Range (TaskUpgradeResourceRatioRange.x, TaskUpgradeResourceRatioRange.y);
			BuildingUpgradeResourceRatio = Random.Range (BuildingUpgradeResourceRatioRange.x, BuildingUpgradeResourceRatioRange.y);
			MoveCollectorsChance = Random.Range (MoveCollectorsRange.x, MoveCollectorsRange.y);

			GameMgr = GameManager.Instance;
			ResourceMgr = GameMgr.ResourceMgr;

			if (ResourceNeedRatio < 1.0f)
				ResourceNeedRatio = 1.0f; //It can't be lower than 1.0f.
			if(ExpandTarget < 0) ExpandTarget = 0.0f; 

			if (TaskUpgradeResourceRatio < 1.0f) {
				TaskUpgradeResourceRatio = 1.0f; //can't be lower than 1
			}

			//If there's a map manager script in the scene, it means that we just came from the map menu, so the faction manager and the settings have been already set by the game manager:
			if (GameMgr.SinglePlayerMgr == null) {
				FactionMgr = GameMgr.Factions [FactionID].FactionMgr;
				FactionMgr.ResourceMgr = this;

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
			}

			ResourceMgr.FactionResourcesInfo [FactionID].NeedRatio = ResourceNeedRatio; //Set the need ratio.

			ExpandTimer = Random.Range (ExpandTimeRange.x, ExpandTimeRange.y); //start the expand timer.

			TaskUpgradeTimer = Random.Range (TaskUpgradeTimeRange.x, TaskUpgradeTimeRange.y); //start the upgrade units timer
			AllUnitsUpgraded = false;

			BuildingUpgradeTimer = Random.Range (BuildingUpgradeTimeRange.x, BuildingUpgradeTimeRange.y); //start the building upgrade timer
			AllBuildingsUpgraded = false;

			DropOffControlTimer = Random.Range (DropOffControlTimeRange.x, DropOffControlTimeRange.y); //start the drop off buildings control
			DropOffControlDone = false;

			//Launch the resource check timer for this faction:
			ResourceCheckTimer = Random.Range(ResourceCheckTimeRange.x,ResourceCheckTimeRange.y);
			CheckResources = true;

			//Go through all the resource types in this game:
			ExploitedResources = new List<List<Resource>>();
			for (int i = 0; i < ResourceMgr.ResourcesInfo.Length; i++) {
				//Define the exploited resources slots.
				List<Resource> NewResourceList = new List<Resource> ();
				ExploitedResources.Add (NewResourceList);
			}

			//get the resource generators buildings and arrange them:
			if (FactionMgr.BuildingMgr) {
				if (FactionMgr.BuildingMgr.AllBuildings.Count > 0) {
					for (int i = 0; i < FactionMgr.BuildingMgr.AllBuildings.Count; i++) {
						if (FactionMgr.BuildingMgr.AllBuildings [i].gameObject.GetComponent<ResourceGenerator> ()) {
							if (FactionMgr.BuildingMgr.AllBuildings [i].gameObject.GetComponent<ResourceGenerator> ().Resources.Length > 0) {
								for (int j = 0; j < FactionMgr.BuildingMgr.AllBuildings [i].gameObject.GetComponent<ResourceGenerator> ().Resources.Length; j++) {
									ResourceGeneratorVars Temp = new ResourceGeneratorVars ();
									Temp.Name = FactionMgr.BuildingMgr.AllBuildings [i].gameObject.GetComponent<ResourceGenerator> ().Resources [j].Name;
									Temp.Prefab = FactionMgr.BuildingMgr.AllBuildings [i];

									ResourceGenerators.Add (Temp);
								}
							}
						}
					}
				}
			}
		}

		// Update is called once per frame
		public void Update () 
		{
			//check which resources to collect timer
			if (CheckResources == true) {
				if (ResourceCheckTimer > 0) {
					ResourceCheckTimer -= Time.deltaTime;
				}
				if (ResourceCheckTimer < 0 && FactionMgr.BuildingMgr.CheckBuildings == false) { //make sure that there are not other buildings that require construction
					CheckResourceCollection ();
					//Launch the resource check timer for this faction:
					ResourceCheckTimer = Random.Range(ResourceCheckTimeRange.x,ResourceCheckTimeRange.y);
				}
			}

			//control drop off buildings timer:
			if (DropOffControlDone == false) {
				if (DropOffControlTimer > 0) {
					DropOffControlTimer -= Time.deltaTime;
				}
				if (DropOffControlTimer < 0 && FactionMgr.BuildingMgr.CheckBuildings == false) { //make sure that there are not other buildings that require construction
					DropOffControl ();
					//Launch the drop off control timer again
					DropOffControlTimer = Random.Range(DropOffControlTimeRange.x,DropOffControlTimeRange.y);
				}
			}

			//Unit upgrades:
			if (AllUnitsUpgraded == false) {
				if (TaskUpgradeTimer > 0) {
					TaskUpgradeTimer -= Time.deltaTime;
				}

				if (TaskUpgradeTimer < 0) {
					AllUnitsUpgraded = true;

					if (FactionMgr.Buildings.Count > 0) {
						//Go through all the faction's buildings:
						for (int i = 0; i < FactionMgr.Buildings.Count; i++) {
							if (FactionMgr.Buildings [i].IsBuilt == true) { //making sure the building is already built.
								//go through the faction's tasks:
								if (FactionMgr.Buildings [i].BuildingTasksList.Count > 0) {
									for (int j = 0; j < FactionMgr.Buildings [i].BuildingTasksList.Count; j++) {
										if (FactionMgr.Buildings [i].BuildingUpgrading == false) { //make sure the building is not upgrading
											if (FactionMgr.Buildings [i].BuildingTasksList [j].TaskType == Building.BuildingTasks.CreateUnit) { //if this task allows to create a unit.
												if (FactionMgr.Buildings [i].BuildingTasksList [j].Upgrades.Length > FactionMgr.Buildings [i].BuildingTasksList [j].CurrentUpgradeLevel) { //if this task has possible upgrades:
													AllUnitsUpgraded = false;

													if (FactionMgr.Buildings [i].BuildingTasksList [j].Active == false) {//if the task is not currently upgrading:
														//Not enough resources:
														if (ResourceMgr.CheckResources (FactionMgr.Buildings [i].BuildingTasksList [j].Upgrades [FactionMgr.Buildings [i].BuildingTasksList [j].CurrentUpgradeLevel].UpgradeResources, FactionID, TaskUpgradeResourceRatio) == true) {
															if (FactionMgr.Buildings [i].MaxTasks > FactionMgr.Buildings [i].TasksQueue.Count) { //make sure the building can still launch tasks

																GameMgr.TaskMgr.LaunchTask (FactionMgr.Buildings [i], j, -1, TaskManager.TaskTypes.TaskUpgrade);
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}

					}

					TaskUpgradeTimer = Random.Range (TaskUpgradeTimeRange.x, TaskUpgradeTimeRange.y); //run the timer again
				}
			}

			//Building upgrades:
			if (AllBuildingsUpgraded == false) {
				if (BuildingUpgradeTimer > 0) {
					BuildingUpgradeTimer -= Time.deltaTime;
				}

				if (BuildingUpgradeTimer < 0) {
					AllBuildingsUpgraded = true;

					if (FactionMgr.Buildings.Count > 0) {
						//Go through all the faction's buildings:
						for (int i = 0; i < FactionMgr.Buildings.Count; i++) {
							//go through the faction's tasks:
							if (FactionMgr.Buildings [i].UpgradeBuilding != null && FactionMgr.Buildings[i].BuildingUpgrading == false) {
								//if the building can be upgraded and is not currently upgrading
								AllBuildingsUpgraded = false;

								FactionMgr.Buildings [i].CheckBuildingUpgrade ();
							}
						}

					}

					BuildingUpgradeTimer = Random.Range (TaskUpgradeTimeRange.x, TaskUpgradeTimeRange.y); //run the timer again
				}
			}

			//Expanding:
			if (ExpandWithoutNeed == true && FactionMgr.BuildingMgr.IsBeingBuilt (FactionMgr.BuildingMgr.BuildingCenter) == false) {
				if (ExpandTimer > 0) {
					ExpandTimer -= Time.deltaTime;
				}

				if (ExpandTimer < 0) {
					if (FactionMgr.BuildingCenters.Count > 0) {
						float CurrentSize = 0f;
						//Go through all the faction's building centers:
						for (int i = 0; i < FactionMgr.BuildingCenters.Count; i++) {
							//Calculate the current faction's total territory size:
							float BorderSize = FactionMgr.BuildingCenters [i].CurrentCenter.Size;
							CurrentSize += BorderSize;
						}

						//If the current faction territory size is smaller than the target we're aiming for then, we'll simply expand:
						if (CurrentSize < GameMgr.MapSize * ExpandTarget) {
							//Expanding is done to get more resources inside the faction's borders:


							//So now we'll check if we have more than the allowed resurces to expand:
							bool HaveResources = true;
							int j = 0;

							while (HaveResources == true && j < ExpandResources.Length) {
								int ID = ResourceMgr.GetResourceID (ExpandResources [j].Name); //Get the resource type ID from its name:
								if(ID >= 0)
								{
									//If we don't have enough from this resource, abort the expanding:
									if (ExpandResources [j].Amount * ResourceNeedRatio > ResourceMgr.FactionResourcesInfo [FactionID].ResourcesTypes [ID].Amount) {
										HaveResources = false;
									}
								}

								j++;
							}

							//If we have all required resources, we will proceed with expanding:
							if (HaveResources == true) {
								//Now we will randomly choose a building center to expand from:
								FactionMgr.BuildingMgr.AttemptToAddBuilding (FactionMgr.BuildingMgr.BuildingCenter, false, null);
							}
						}
					}

					ExpandTimer = Random.Range (ExpandTimeRange.x, ExpandTimeRange.y);
				}
			}
		}

		public void CheckResourceCollection ()
		{
			CheckResources = false;
			//First we have to go through all the building centers:
			for (int i = 0; i < FactionMgr.BuildingCenters.Count; i++) {
				if (FactionMgr.BuildingCenters[i].CurrentCenter.ResourcesInRange.Count > 0 && FactionMgr.BuildingCenters[i].CurrentCenter.IsActive == true) { //If there are resources inside the building center's borders
					//We'll go through all different types of resources:
					for (int n = 0; n < ResourceMgr.ResourcesInfo.Length; n++) {
						string Name = ResourceMgr.ResourcesInfo [n].Name;
						int j = 0;
						int AmountExploited =0;

						//Search and save the resources that have the above name:

						List<Resource> SearchResources = new List<Resource>();
						for (j = 0; j < FactionMgr.BuildingCenters [i].CurrentCenter.ResourcesInRange.Count; j++) {
							if (FactionMgr.BuildingCenters [i].CurrentCenter.ResourcesInRange [j] != null) { //if the resource still exists and it's active:
								if (FactionMgr.BuildingCenters [i].CurrentCenter.ResourcesInRange [j].gameObject.activeInHierarchy == true) {
									if (FactionMgr.BuildingCenters [i].CurrentCenter.ResourcesInRange [j].Name == Name) {
										SearchResources.Add (FactionMgr.BuildingCenters [i].CurrentCenter.ResourcesInRange [j]);
										if (FactionMgr.BuildingCenters [i].CurrentCenter.ResourcesInRange [j].FactionID == FactionID) {
                                            //If the current resource is already being exploited by this faction, we will simply count it out:
											if (FactionMgr.BuildingCenters [i].CurrentCenter.ResourcesInRange [j].WorkerMgr.CurrentWorkers >= FactionMgr.BuildingCenters [i].CurrentCenter.ResourcesInRange [j].WorkerMgr.WorkerPositions.Length) {
												AmountExploited++;
											}
										}
									}
								}
							}
						}
						//Calculate the amount that can be exploited:
						int AmountToExploit = Mathf.CeilToInt (SearchResources.Count * DefaultResourceExploit) - AmountExploited;
						if (AmountToExploit > 0 && SearchResources.Count > 0) {
							//Set the amount of resources to exploit inside the current border:

							j = 0;
							while (AmountToExploit > 0 && j < SearchResources.Count) {
								if (SearchResources[j].FactionID == -1 || (SearchResources[j].FactionID == FactionID && SearchResources[j].WorkerMgr.CurrentWorkers < SearchResources[j].WorkerMgr.WorkerPositions.Length)) {
                                    CheckResources = true; //This means that we still haven't exploited all available resources so we will attempt to do tht again after the timer goes off.
                                    //does the resource has a suitable drop off? if not a drop off control will automatically start.
                                    if (HasDropOff (SearchResources [j])) {
										//If the resource is available to be exploited, then exploit it:
										int AmountNeeded = SearchResources [j].WorkerMgr.WorkerPositions.Length - (SearchResources [j].WorkerMgr.CurrentWorkers + GetInProgressCollectors (SearchResources [j]));
										if (AmountNeeded > 0) {
											//Check if we have room in the population for new units in this faction:
											LookForCollectors (SearchResources [j]);
										}
										AmountToExploit--;
									}
								}

								j++;
							}
						}

					}
				}
			}
		}

		//check if there are enough and suitable drop off buildings
		public void DropOffControl ()
		{
			DropOffControlDone = true;
			//go through all faction resources:
			if (FactionResources.Count > 0) {
				for (int i = 0; i < FactionResources.Count; i++) {
					//if the resource still exists:
					if (FactionResources [i].Resource != null) {
						//if the resource does not have a suitable drop off building
						if (FactionResources [i].DropOff == null) { 
							DropOffControlDone = false; //so we can check again

							//first search if there's already a suitable drop off building spawned:

							//go through all spawned drop off buildings:
							bool Found = false;
							int j = 0;
							if (FactionMgr.DropOffBuildings.Count > 0) { //if there are drop off buildings
								while (Found == false && j < FactionMgr.DropOffBuildings.Count) {
                                    if (FactionMgr.DropOffBuildings[j] == null) //if the building has been destroyed then remove it from the list
                                    {
                                        FactionMgr.DropOffBuildings.RemoveAt(j);
                                    }
                                    else
                                    {
                                        if (FactionMgr.DropOffBuildings[j].CanDrop(FactionResources[i].Resource.Name))
                                        { //if the faction can drop the target resource at this drop off building
                                          //if the target resource has a drop range then the drop off building must be inside that range, if not then proceed.
                                            if (FactionResources[i].Resource.HaveDropOffRange == false || (Vector3.Distance(FactionResources[i].Resource.transform.position, FactionMgr.DropOffBuildings[j].transform.position) < FactionResources[i].Resource.DropOffRange && FactionResources[i].Resource.HaveDropOffRange == true))
                                            {
                                                Found = true;
                                                //set as new choice for drop off building for this resource
                                                FactionResources[i].DropOff = FactionMgr.DropOffBuildings[j];
                                            }
                                        }
                                        j++;
                                    }
                                }
							}

							//if we still haven't a suitable drop off building:
							if (Found == false) {
								//go ahead and ask the NPC Building Placement to create one:
								if (FactionMgr.BuildingMgr.AllBuildings.Count > 0) {
									j = 0;
									while (j < FactionMgr.BuildingMgr.AllBuildings.Count) { //go through all the buildings prefabs:
										if (FactionMgr.BuildingMgr.AllBuildings [j].CanDrop (FactionResources [i].Resource.Name)) { //if this building matches
											//place prefab and don't allow multiple buildings to avoid creating too many unwanted drop off buildings
											FactionMgr.BuildingMgr.AttemptToAddBuilding (FactionMgr.BuildingMgr.AllBuildings [j], false, FactionResources [i].Resource.gameObject);
										}
										j++;
									}
								}
							}
						}
					}
				}
			}

		}

		//check if resource has a drop off building:
		public bool HasDropOff (Resource Resource)
		{
			//first of all check if the resource is in the list:
			bool Found = false;
			int i = 0;
			if (FactionResources.Count > 0) {
				while (Found == false && i < FactionResources.Count) {
					//go through all the resources in the list
					if (FactionResources [i].Resource == Resource) { //if the resource is found
						Found = true;
						//does it have a drop off building? 
						if (FactionResources [i].DropOff == null) { //if no
							//trigger the drop off control:
							DropOffControlDone = false;
							//and return false;
							return false;
						} else { //if yes
							//return true
							return true;
						}
					}

					i++;
				}
			}

			//if the resource is not on the list:
			if (Found == false) { //if the resource is not found
				FactionResourcesVars NewResource = new FactionResourcesVars ();
				NewResource.Resource = Resource;
				FactionResources.Add (NewResource);
			}

			//trigger the drop off control
			DropOffControlDone = false;
			return false;
		}


		//Setting the target amount of a resource type:
		public void LookForResources(string Name, int Amount)
		{
			int ID = ResourceMgr.GetResourceID (Name);
			if (ID >= 0) { //Make sure that the resource name is valid.
				ResourceMgr.FactionResourcesInfo[FactionID].ResourcesTypes[ResourceMgr.GetResourceID(Name)].TargetAmount += Mathf.RoundToInt(Amount*ResourceNeedRatio);

				//If the target amount is above the current amount:
				if (ResourceMgr.FactionResourcesInfo [FactionID].ResourcesTypes [ID].TargetAmount > ResourceMgr.FactionResourcesInfo [FactionID].ResourcesTypes [ID].Amount) {
					//We need to collect some resources boss:
					//First we need to search for a resource of that type to collect from:
					Resource TargetResource = null;

					int i = 0;
					//First we have to go through all the building centers:
					while(i < FactionMgr.BuildingCenters.Count && TargetResource == null) {
						if (FactionMgr.BuildingCenters [i]) {
							if (FactionMgr.BuildingCenters [i].CurrentCenter) {
								if (FactionMgr.BuildingCenters [i].CurrentCenter.IsActive == true) {
									if (FactionMgr.BuildingCenters [i].CurrentCenter.ResourcesInRange.Count > 0) { //If there are resources inside the building center's borders
										int j = 0;
										while (TargetResource == null && j < FactionMgr.BuildingCenters [i].CurrentCenter.ResourcesInRange.Count) {
											//If we find a resource area that matches our search and is not crowded with collectors:
											Resource CurrentResource = FactionMgr.BuildingCenters [i].CurrentCenter.ResourcesInRange [j];
											if (CurrentResource.Name == Name && CurrentResource.WorkerMgr.CurrentWorkers < CurrentResource.WorkerMgr.WorkerPositions.Length && CurrentResource.Amount > 0) {
												if (CurrentResource.FactionID == -1 || CurrentResource.FactionID == FactionID) {
													TargetResource = CurrentResource; //This is the place we're going to collect from what we need.
												}
											}
											j++;
										}
									}
								}
							}
						}
						i++;
					}
					//Debug.Log (TargetResource);


					if (TargetResource == null) {//If no resources are found

						//check if the resource we're looking for 
						if (ResourceGenerators.Count > 0) {
							int n = 0;
							bool Found = false;
							while (n < ResourceGenerators.Count && Found == false) {
								if (ResourceGenerators [n].Name == Name) { //resource generator building found:
									Found = true;
								}
								n++;
							}
						}

						//make sure that no building center is currently being built and that no building center is waiting for resources.
						if (FactionMgr.BuildingMgr.IsBeingBuilt (FactionMgr.BuildingMgr.BuildingCenter) == false && FactionMgr.BuildingMgr.BuildingsWaitingResources.Contains (FactionMgr.BuildingMgr.BuildingCenter) == false) {
							AddCenterToCollectResource (Name);
						}

					} else {
						//We found a resource to collect from, now we need to send collectors:
						//But first we'll check if the faction can exploit resources on need or would it have to just wait for the required amount to be collected
						//Please see above in the "ExploitResourceOnNeed" var for an explanation:

						//if we haven't even exploited that type of resource once. then we'll do it regardless of the exploit on resource need.
						if (Random.Range (0.0f, 1.0f) < ExploitResourceOnNeed || ExploitedResources[ResourceMgr.GetResourceID(TargetResource.Name)].Count == 0) {
							//does the resource has a suitable drop off? if not a drop off control will automatically start.
							if (HasDropOff (TargetResource)) {
								LookForCollectors (TargetResource);
							}
						}
					}
				}
			}
		}

		//Look for a resource outside the faction's borders then build a new town center to get it inside the border:
		public void AddCenterToCollectResource (string Name)
		{
			//make sure we are not currently building a center (that might be getting built to exploit the resource this faction needs).
			if (FactionMgr.BuildingMgr.IsBeingBuilt (FactionMgr.BuildingMgr.BuildingCenter) == false) {

				if (FactionMgr.BuildingMgr.BuildingCenter != null && FactionMgr.BuildingCenters.Count > 0) {
					int ID = ResourceMgr.GetResourceID (Name);
					int CenterID = ResourceMgr.FactionResourcesInfo [FactionID].ResourcesTypes [ID].LastCenterID;
					Resource TargetResource = null;

					float Distance = 0.0f;

					//Search all resources in the map and look for the nearest resource that the factin needs:
					for (int i = 0; i < ResourceMgr.AllResources.Count; i++) {
						if (ResourceMgr.AllResources [i].Name == Name && ResourceMgr.AllResources [i].Amount > 0) {
							if (ResourceMgr.AllResources [i].FactionID == -1) { //Make sure no one else is exploiting this resouce 
								//Make sure to pick the nearest resource we can find:
								if (FactionMgr.BuildingCenters [CenterID]) {
									if (TargetResource == null) {
										TargetResource = ResourceMgr.AllResources [i];
										Distance = Vector3.Distance (TargetResource.transform.position, FactionMgr.BuildingCenters [CenterID].transform.position);
									} else if (Distance > Vector3.Distance (FactionMgr.BuildingCenters [CenterID].transform.position, ResourceMgr.AllResources [i].transform.position)) {
										TargetResource = ResourceMgr.AllResources [i];
										Distance = Vector3.Distance (TargetResource.transform.position, FactionMgr.BuildingCenters [CenterID].transform.position);
									}
								} else {
									Debug.Log ("invalid center");
								}
							}
						}
					}

					//If we found a resource that we can exploit but it's not inside the faction's borders, then we will create a new center building to enlarge the borders and reach the new resource:
					if (TargetResource != null) {
						FactionMgr.BuildingMgr.AttemptToAddBuilding (FactionMgr.BuildingMgr.BuildingCenter, false, TargetResource.gameObject);
					}
				}
			}
		}

		public void LookForCollectors (Resource TargetResource)
		{
			int i = 0;

			int Amount = TargetResource.WorkerMgr.WorkerPositions.Length - TargetResource.WorkerMgr.CurrentWorkers;

			//Search for this faction's idle collectors:
			while (TargetResource.WorkerMgr.CurrentWorkers < TargetResource.WorkerMgr.WorkerPositions.Length && TargetResource.Amount > 0 && i < FactionMgr.Collectors.Count) {
				if (FactionMgr.Collectors [i] != null) {
					if (FactionMgr.Collectors [i].UnitMvt.IsIdle () == true) {
						//Send the collectors to collect some resources:
						FactionMgr.Collectors [i].SetTargetResource (TargetResource);
						Amount--;
					}
				}
				i++;
			}

			Amount -= GetInProgressCollectors (TargetResource);

			if (GameMgr.Factions [FactionID].CurrentPopulation + Amount > GameMgr.Factions [FactionID].MaxPopulation) {
				if (GameMgr.Factions [FactionID].CurrentPopulation < GameMgr.Factions [FactionID].MaxPopulation) {
					Amount = GameMgr.Factions [FactionID].MaxPopulation - GameMgr.Factions [FactionID].CurrentPopulation;
                } else {
					Amount = 0;
				}

                FactionMgr.BuildingMgr.AttemptToAddBuilding(FactionMgr.BuildingMgr.PopulationBuilding, false, null);
			}

			bool CanProduceCollectors = false;
			//If we haven't found any idle collectors:
			if (Amount > 0) {
				//Create some.
				CanProduceCollectors = CreateCollectors(TargetResource, Amount,TargetResource.transform.position);
			}

			//if we can't create collectors because of population saturation or because there's no resources and we can actually move collectors
			if ((Amount == 0 || CanProduceCollectors == false) && Random.Range (0.0f, 1.0f) < MoveCollectorsChance) {
				//then check if we can actually move collectors from another resources to this resource
				//check if the target resource doesn't have any collectors
				if (TargetResource.WorkerMgr.CurrentWorkers == 0) {
					//only then will we move existing collectors.
					MoveCollectors(TargetResource);
				}
			}
		}

		//Get the amount of pending resource collectors being created to be delivered to collect a type of resources after creation:
		public int GetInProgressCollectors (Resource Target)
		{
			int Amount = 0;
			//Search the spawned buildings to find one that produces resource collectors:
			for (int i = 0; i < CollectorsCreators.Count; i++) {
				if (CollectorsCreators [i].ResourceUnits.Count > 0) { //If the current building includes one or more tasks that produce collector units:
					for (int j = 0; j < CollectorsCreators [i].TasksQueue.Count; j++) {

						if (CollectorsCreators [i].TasksQueue [j].TargetResource) {
							//If this building is already producing a builder unit that will go, when spawned, to construct the target building, then we need one less resource collector.
							if (CollectorsCreators [i].TasksQueue [j].TargetResource == Target) {
								Amount++;
							}
						}
					}
				}
			}

			return Amount;

		}

		public bool CreateCollectors(Resource Target, int Amount, Vector3 TargetPos)
		{
			bool CanProduceCollectors = false;

			//First we'll see if the required resource collectors are already in progress (being produced):
			int i = 0;
			int j = 0;

			//If still, not all required resource collectors are not being produced:
			if (Amount > 0 && CollectorsCreators.Count > 0) {
				int ID = -1; //Holds the ID of the nearest building to the target area:
				int TaskID = -1; //Holds the task ID of the building to produce resource collectors.
				j = 0;


				float Distance = 0.0f;

				//Look through the rest of the resource collector creators:
				for (i = 0; i < CollectorsCreators.Count; i++) {
					//If we find a building close to the target area, save its ID.
					if (Vector3.Distance (CollectorsCreators [i].transform.position, TargetPos) < Distance || ID == -1) {
						if (CollectorsCreators[i].BuildingUpgrading == false && CollectorsCreators[i].IsBuilt == true && CollectorsCreators[i].Health >= CollectorsCreators[i].MinTaskHealth && FactionMgr.Buildings [i].MaxTasks > FactionMgr.Buildings [i].TasksQueue.Count) {
							//Check all the tasks that produce resource collectors units:
							j = 0;
							while (j < CollectorsCreators [i].ResourceUnits.Count) {
                                //make sure that the unit hasn't reached its limit:
                                if (FactionMgr.HasReachedLimit(CollectorsCreators[i].BuildingTasksList[CollectorsCreators[i].ResourceUnits[j]].UnitPrefab.Code) == false)
                                {
                                    //first check if the resource unit is non attacking unit and the amount of non attacking units is still conform to the amount specified in the NPC Army component
                                    //or if the resource collector from the building we're looking at has an attack component and continue:
                                    if (CollectorsCreators[i].BuildingTasksList[CollectorsCreators[i].ResourceUnits[j]].UnitPrefab.gameObject.GetComponent<Attack>() || FactionMgr.ArmyMgr.CanCreateNonAttackUnit())
                                    {
                                        if (ResourceMgr.CheckResources(CollectorsCreators[i].BuildingTasksList[CollectorsCreators[i].ResourceUnits[j]].RequiredResources, FactionID, ResourceMgr.FactionResourcesInfo[FactionID].NeedRatio) == true)
                                        {
                                            //Check if we have enough resources to launch this task:
                                            Distance = Vector3.Distance(CollectorsCreators[i].transform.position, TargetPos);
                                            ID = i;
                                            TaskID = CollectorsCreators[i].ResourceUnits[j];
                                        }
                                    }
                                }
								j++;
							}
						} else {
							//...
						}
					}
				}


				//Add tasks to produce resource collector units:
				if (ID >= 0) {
					while  (Amount > 0 && ResourceMgr.CheckResources (CollectorsCreators[ID].BuildingTasksList[TaskID].RequiredResources, FactionID, ResourceMgr.FactionResourcesInfo[FactionID].NeedRatio) == true && CollectorsCreators [ID].TasksQueue.Count < CollectorsCreators [ID].MaxTasks) {
						//Check if we have enough resources to launch this task:
						//Add the new task to the building's task queue
						GameMgr.TaskMgr.LaunchTask (CollectorsCreators [ID], TaskID,-1, TaskManager.TaskTypes.CreateUnit);

						Amount--;
						CanProduceCollectors = true; //if we can even create one, then mark as done
					} 
				} else {
					//Couldn't produce collectors due to lack of resources
				}

			}

			return CanProduceCollectors;
		}

		//Move collectors from one resource to another.
		//Basic stuff: Look for the resource with the most collectors and send half their workers or the amount we needed in case that's smaller than the half to collect from the new resource.
		public void MoveCollectors (Resource TargetResource)
		{
			Resource TakeFromResource = null;
			int Collectors = 0;

			//Loop through all the building centers:
			for (int i = 0; i < FactionMgr.BuildingCenters.Count; i++) {
				Border CurrentBorder = FactionMgr.BuildingCenters [i].CurrentCenter;
				//Loop through the resources in range of the building center:
				for(int j = 0; j < CurrentBorder.ResourcesInRange.Count; j++) {
					if (CurrentBorder.ResourcesInRange [j] != null) { //if the resource still exists
						if(CurrentBorder.ResourcesInRange [j].gameObject.activeInHierarchy == true) //if the resource is active
						{
							//if it's not the same type as the resource we're attempting to collect:
							if (CurrentBorder.ResourcesInRange [j].Name != TargetResource.Name) {
								if (CurrentBorder.ResourcesInRange [j].FactionID == FactionID) { //If this resource is being collected by this faction:
									if (CurrentBorder.ResourcesInRange [j].WorkerMgr.CurrentWorkers > Collectors) { //If this resource has the most collectors
										TakeFromResource = CurrentBorder.ResourcesInRange [j]; //then this is the one we're going to take collectors from.
										Collectors = CurrentBorder.ResourcesInRange [j].WorkerMgr.CurrentWorkers;
									}
								}
							}
						}
					}
				}
			}

			//We will free half of those resource collectors / the amount we need in case that's lower and send them to collect the new resource
			if (TakeFromResource != null) {
				int FreeAmount = Mathf.RoundToInt (TakeFromResource.WorkerMgr.CurrentWorkers / 2); //get half of the collectors
				if (FreeAmount < 1) 
					FreeAmount = 1;
				if (FreeAmount > TargetResource.WorkerMgr.WorkerPositions.Length)
					FreeAmount = TargetResource.WorkerMgr.WorkerPositions.Length;

				int n = 0;

				//Loop through all the collectors of the resource
				while (n < TargetResource.WorkerMgr.WorkerPositions.Length && FreeAmount > 0) {
					if (TakeFromResource.WorkerMgr.WorkerPositions[n].CurrentUnit != null) {
                        Unit CurrentUnit = TakeFromResource.WorkerMgr.WorkerPositions[n].CurrentUnit;
                        //Make the unit stop collecting the resource..
                        CurrentUnit.CancelCollecting ();
						//Send the unit to collect the new resource
						CurrentUnit.ResourceMgr.SetTargetResource (TargetResource);

						FreeAmount--;
					}

					n++;
				}
			}
		}
		//This checks if we have enough resources to launch an upgrade
		public bool CheckTaskUpgradeResources (ResourceManager.Resources[] RequiredResources)
		{
			if(RequiredResources.Length > 0)
			{
				for(int i = 0; i < RequiredResources.Length; i++) //Loop through all the requried resources:
				{
					//Check if the team resources are lower than one of the demanded amounts:
					if(ResourceMgr.GetResourceAmount(FactionID,RequiredResources[i].Name) < RequiredResources[i].Amount*TaskUpgradeResourceRatio)
					{
						return false; //If yes, return false.
					}
				}
				return true; //If not, return true.
			}
			else //This means that no resource are required to build this building.
			{
				return true;
			}
		}

		//this method takes the building's resources from the faction:
		public void TakeTaskUpgradeResources (ResourceManager.Resources[] RequiredResources)
		{
			if(RequiredResources.Length > 0) //If the building requires resources:
			{
				for(int i = 0; i < RequiredResources.Length; i++) //Loop through all the requried resources:
				{
					//Remove the demanded resources amounts:
					ResourceMgr.AddResource(FactionID, RequiredResources[i].Name, -RequiredResources[i].Amount);
				}
			}
		}
	}
}