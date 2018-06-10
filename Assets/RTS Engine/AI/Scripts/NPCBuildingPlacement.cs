using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* NPC Building Placement script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine 
{
	public class NPCBuildingPlacement : MonoBehaviour {

		public int FactionID; //the faction that this manager belongs to.

		//The class below holds the info that we need while placing a new building on the level:
		[System.Serializable]
		public class PendingBuildingsVars
		{
			public Building BuildingToAdd; //the building object to place
			public int CenterID; //the center ID that the new building will belong to
			public Vector3 SpawnPos; //the initial spawn position that the building spawned at.

			public Vector3 LastRoundPos; //the last main position that the to-be-placed building took.
		}
		[HideInInspector]
		public List<PendingBuildingsVars> PendingBuildings = new List<PendingBuildingsVars>();
		//a list of the buildings that need to be placed but the faction does not have resources to place them yet.
		[HideInInspector]
		public List<Building> BuildingsWaitingResources = new List<Building> ();
		//the initial map height at which buildings will be spawned.
		public float MapHeight = 0.05f;

		public List<Building> AllBuildings; //All buildings prefabs that the faction can construct MUST be in this list.

		[HideInInspector]
		public Building CapitalBuilding; //The capital building of this faction (when destroyed, factions looses).

		public Building BuildingCenter; //This is the prefab for the building center, the building that has a border component.

		public Building PopulationBuilding; //The building that will be created each time we need to increase the population.
        //Because the population building might get requested by multiple NPC managers and this might lead to overflow...
        //...if the faction is requesting collectors and doesn't have the population or the resources to build a population building
        //we need to stop asking for resources and simply wait for them to be available, that's the use of the below attribute:
        bool AskForPopulationResources = true;

		public Building DropOffBuilding; //The building that will be created when units need a place to drop resources at.

		[HideInInspector]
		public List<Building> BuilderCreators = new List<Building> (); //This will store the possible buildings that can builder type units.

		//Each time the buildings check timer is done, we check if all buildings are constructed and if they need builders or not:
		public Vector2 BuildingsCheckTimeRange = new Vector2(4.7f,8f);
		float BuildingsCheckTimer;
		[HideInInspector]
		public bool CheckBuildings = true; //If the last we've checked default buildings states and all of them are 100% built, this will be set to false because it will be useless to check again.
		//But as soon as new building is spawned, it will be set again to true to guarantee it gets built. This will also be trigged if a building is badly damaged to send workers to fix it.

		//Delay time when placing a new building (useful only for diversity and making everything seem more real):
		public float MinDelayTime = 1.0f;
		public float MaxDelayTime = 5.0f;
		float Delay;

		//CHANGEME:
		public float MoveValue = 5.0f;
		public float Timer;

		//Scripts:
		[HideInInspector]
		public FactionManager FactionMgr;
		ResourceManager ResourceMgr;
		GameManager GameMgr;

		void Awake () {

			GameMgr = GameManager.Instance; //get the game manager comp
			ResourceMgr = GameMgr.ResourceMgr; //get the resource manager comp

            AskForPopulationResources = true; //per default we can ask for these resources

            //If there's a map manager script in the scene, it means that we just came from the map menu, so the faction manager and the settings have been already set by the game manager:
            if (GameMgr.SinglePlayerMgr == null) {
				FactionMgr = GameMgr.Factions [FactionID].FactionMgr;
				FactionMgr.BuildingMgr = this;

				if (FactionMgr == null) {
					//If we can't find the AI Faction manager script, then print an error:
					Debug.LogError ("Can't find the AI Faction Manager for Faction ID: " + FactionID.ToString ());
				}	
			}

			//start the buildings check timer (at the end of the timer, the script checks for unbuilt buildings and those who need fixing and sends builders to fix them.
			BuildingsCheckTimer = Random.Range (BuildingsCheckTimeRange.x, BuildingsCheckTimeRange.y);
		}

		void Start ()
		{
			//if this belongs to the local player then destroy the comp as it won't be needed.
			if (FactionID == GameManager.PlayerFactionID) {
				Destroy (this);
				return;
			}

			//Get the capital's building from the faction list in the game manager:
			CapitalBuilding = GameMgr.Factions [FactionID].CapitalBuilding;
		}

		// Update is called once per frame
		void Update () {

			//If check buildings is set to false, it means that all factions' buildings are all built and not damaged, if not then it means that the last time
			//the faction checked its building, we don't know whether it has buildings requiring constructon or not.
			if (CheckBuildings == true) {
				//building check timer:
				if (BuildingsCheckTimer > 0) {
					BuildingsCheckTimer -= Time.deltaTime;
				}
				if (BuildingsCheckTimer < 0) {
					CheckAllBuildings ();
					//Launch the building check timer again:
					BuildingsCheckTimer = Random.Range (BuildingsCheckTimeRange.x, BuildingsCheckTimeRange.y);

				}
			}

			//if we have pending buildings that needs to be placed:
			if (PendingBuildings.Count > 0) {

				//If the building we're trying to place is inactive, then activate it.
				if (PendingBuildings [0].BuildingToAdd.gameObject.activeInHierarchy == false) {
					PendingBuildings [0].BuildingToAdd.gameObject.SetActive (true); 

					//launch the constructing delay (just to make things more real for the NPC):
					Delay = Random.Range (MinDelayTime, MaxDelayTime); 
				} 

				//Delay timer: allows for more dynamic behaviour and a more realistic way for NPC factions to place their buildings as they wait for some time before placing the building.
				if (Delay > 0) {
					Delay -= Time.deltaTime;
				}

				//Making sure that the building to place belongs to a valid buildng center (as it can be destroyed during placing the building)
				if (PendingBuildings [0].CenterID < FactionMgr.BuildingCenters.Count) {
					if (FactionMgr.BuildingCenters [PendingBuildings [0].CenterID] != null) {

						if (Vector3.Distance (PendingBuildings [0].BuildingToAdd.transform.position, PendingBuildings[0].SpawnPos/*FactionMgr.BuildingCenters [PendingBuildings [0].CenterID].transform.position*/) > FactionMgr.BuildingCenters [PendingBuildings [0].CenterID].CurrentCenter.Size) {
							//If the distance between the building to add and its town center is higher than the border size, then abort placing this building
							StopPlacingBuilding ();
						} else {
							//methods like IsBuildingOnMap (which check if the building is over the map and not outside) and IsBuildingInRange (which checks if the building is inside the building center territory
							//help check whether the building is in a valid postion
							if (PendingBuildings [0].BuildingToAdd.IsBuildingOnMap () && PendingBuildings [0].BuildingToAdd.CanPlace == true && PendingBuildings [0].BuildingToAdd.IsBuildingInRange () == true && Delay <= 0) { //If the building can be placed at its current position
								//place the building in case it is in a valid position.
								PlaceBuilding ();
							} else {
								//if we haven't found yet a good position to place the building, then we start a timer that, each time it ends, will
								if (Timer > 0) {
									Timer -= Time.deltaTime;
								}
								if (Vector3.Distance (PendingBuildings [0].BuildingToAdd.transform.position, PendingBuildings [0].LastRoundPos) < 1.0f && Timer < 0) {
									//change the distance between the building and its center a little bit
									Timer = 2.0f;
									PendingBuildings [0].BuildingToAdd.transform.position = new Vector3 (PendingBuildings [0].BuildingToAdd.transform.position.x + MoveValue * Time.deltaTime, PendingBuildings [0].BuildingToAdd.transform.position.y, PendingBuildings [0].BuildingToAdd.transform.position.z);
									PendingBuildings [0].LastRoundPos = PendingBuildings [0].BuildingToAdd.transform.position;
								}
								//we also keep rotating the building around its center till we find a suitable place.
								Quaternion Rot = PendingBuildings [0].BuildingToAdd.transform.rotation;
								PendingBuildings [0].BuildingToAdd.transform.RotateAround (PendingBuildings [0].SpawnPos, Vector3.up, 50 * Time.deltaTime);
								PendingBuildings [0].BuildingToAdd.transform.rotation = Rot;
							}
						}
					} else {
						StopPlacingBuilding (); //stop placing the building because of invalid building center that this one is supposed to be tied to.
					}
				}
				else {
					StopPlacingBuilding (); //stop placing the building because of invalid building center that this one is supposed to be tied to.
				}
			}
		}

		//a method that places the building:
		void PlaceBuilding ()
		{
            PendingBuildings[0].BuildingToAdd.BuildingModel.SetActive(false);

            //enable the nav mesh obstacle comp
            if (PendingBuildings[0].BuildingToAdd.NavObs) {
				PendingBuildings[0].BuildingToAdd.NavObs.enabled = false;
			}

			//Set the building's health to 0 so that builders can start adding health to it:
			PendingBuildings[0].BuildingToAdd.Health = 0.0f;

			//Building is now placed:
			PendingBuildings[0].BuildingToAdd.Placed = true;

			PendingBuildings[0].BuildingToAdd.ToggleConstructionObj (true); //Show the construction object when the building is placed.

			PendingBuildings[0].BuildingToAdd.BuildingPlane.SetActive (false); //hide the building's selection texture

			//Activate the player selection collider:
			PendingBuildings[0].BuildingToAdd.PlayerSelection.gameObject.SetActive(true);

			//Send builders to construct this building:
			ConstructBuilding (PendingBuildings[0].BuildingToAdd);
			CheckBuildings = true; //We'll be checking for buildings to guarantee their construction:

			//Activate the nav mesh obstacle component:
			if (PendingBuildings[0].BuildingToAdd.NavObs) {
				PendingBuildings[0].BuildingToAdd.NavObs.enabled = true;
			}

			//Register the building in its border:
			PendingBuildings[0].BuildingToAdd.CurrentCenter.RegisterBuildingInBorder (PendingBuildings[0].BuildingToAdd);

			//custom event:
			if(GameMgr.Events) GameMgr.Events.OnBuildingPlaced(PendingBuildings[0].BuildingToAdd);

			//Move the next pending building up by removing this one from the lists:
			PendingBuildings.RemoveAt(0);
		}

		//a method that cancels placing a building:
		void StopPlacingBuilding()
		{
			//Give back the building's resources to the faction
			ResourceMgr.GiveBackResources (PendingBuildings[0].BuildingToAdd.BuildingResources, FactionID);

			//Remove the building from the spawned buildings list:
			FactionMgr.Buildings.Remove(PendingBuildings[0].BuildingToAdd);

			if (FactionMgr.BuildingCenters.Contains(PendingBuildings[0].BuildingToAdd)) {
				FactionMgr.BuildingCenters.Remove(PendingBuildings[0].BuildingToAdd);
			}

			//If the building can produce resource collectors, remove it from that list:
			if (FactionMgr.ResourceMgr.CollectorsCreators.Contains (PendingBuildings [0].BuildingToAdd)) {
				FactionMgr.ResourceMgr.CollectorsCreators.Remove(PendingBuildings[0].BuildingToAdd);
			}

			//If the building can produce builders, remove it from that list:
			if (BuilderCreators.Contains (PendingBuildings [0].BuildingToAdd)) {
				BuilderCreators.Remove(PendingBuildings[0].BuildingToAdd);
			}

			//Remove the building from the progress amount:
			if (PendingBuildings [0].BuildingToAdd.CurrentCenter != null) {
				PendingBuildings [0].BuildingToAdd.CurrentCenter.UnregisterInProgressBuilding (PendingBuildings [0].BuildingToAdd.Code);
			}

			//Destroy the building object:
			Destroy(PendingBuildings[0].BuildingToAdd.gameObject);

			//Move the next pending building up by removing this one from the lists:
			PendingBuildings.RemoveAt(0);
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------------

		//Called each time we try to add a building to construct:
		public void AttemptToAddBuilding (Building Building, bool AllowMultiple, GameObject BuildAround)
		{
			/*
		 * Building: The building prefab that we want to add.
		 * Allow Multiple: When true, we need to wait to complete placing the same building before placing another.
		 * Build Around: Sometimes we don't need to build directly around a city center but around a specific position (such as a resource).
		 * 
		 * */

            //if the building's limit has been reached, don't place building:
            if(FactionMgr.HasReachedLimit(Building.Code))
            {
                return; //don't place building
            }

			//check building placement priorities:
			if (FactionMgr.ResourceMgr.MustHaveDropOff == true) { //if the faction must have a drop off building
				if (FactionMgr.DropOffBuildings.Count == 0) { //and there's still no drop off buildings
					//if the building that the faction is attempting to place is not a drop off building
					if (Building.ResourceDropOff == false) {
						return; //don't place it.
					}
				}
			}
			//if the same building is already getting placed and we can't have multiple buildings of the same type at once.
			if (IsBeingBuilt (Building) == true && AllowMultiple == false) {
				return;
			}


			//Check if the faction have enough resources to build this building:
			if (ResourceMgr.CheckResources (Building.BuildingResources, FactionID, ResourceMgr.FactionResourcesInfo[FactionID].NeedRatio) == true) {
				//Search a building center to construct building inside its borders:
				int CenterID = -1;
				if (BuildAround == null) { //if we can build anywhere.
					CenterID = GetAvailableCenterID (Building.Code); //search for an available center to build around
				} else { //else get the nearest center to our build around point
					CenterID = FactionMgr.BuildingCenters.IndexOf (FactionMgr.BuildingMgr.GetNearestBuilding (BuildAround.transform.position, FactionMgr.BuildingMgr.BuildingCenter.Code));
				}

				//If we found a suitable building center:
				if (CenterID != -1) {
					//run this method to add a new building to place:
					AddBuildingToList (Building, CenterID, BuildAround);
				}	
				else { //if we can't find a building center
					AttemptToAddBuilding (BuildingCenter, false, null); //attempt to create a new building center.
				}
			} else { //no available resources
                //if this is a population building:
                if(Building == PopulationBuilding)
                {
                    //if we still can ask for population building resources:
                    if(AskForPopulationResources == true)
                    {
                        AskForPopulationResources = false;
                    }
                    else
                    {
                        return; //if not stop here.
                    }
                }

				//ask for resources:
				AskForBuildingResources(Building);
			}
		}

		public int GetAvailableCenterID (string Code)
		{
			if (Code == BuildingCenter.Code) { //if the building to add is a building center 
				return Random.Range (0, FactionMgr.BuildingCenters.Count); //randomly give a center ID.
			} else {
				int j = 0;

				while (j < FactionMgr.BuildingCenters.Count) { 
					//Look for a building center to build this building around:
					if (FactionMgr.BuildingCenters [j].CurrentCenter.AllowBuildingInBorder (Code) == true) { //make sure it allows this building under its territory
						return j;
					}
					j++;
				}
			}
			return -1;
		}

		public void AskForBuildingResources (Building Building)
		{
			//Not enough resources:
			for(int i = 0; i < Building.BuildingResources.Length; i++)
			{
				int Amount = Building.BuildingResources [i].Amount;
				if (BuildingsWaitingResources.Contains(Building) == true) {
					//If we've already asked for this building's resource, then set the amount to 0, this will let the resource manager know that we're only reminding it
					//that we need these resources and not asking for more.
					Amount = 0;
				}

				if (Building.Code != BuildingCenter.Code && Amount == 0) { //no point in asking again for building centers, once is enough.
					//ask the resource manager to get resources for this building.
					FactionMgr.ResourceMgr.LookForResources (Building.BuildingResources [i].Name, Amount);
				}

			}
			if (BuildingsWaitingResources.Contains(Building) == false) {
				//add the building to the list if it's not already there.
				BuildingsWaitingResources.Add(Building);
			}
		}

		//The method below checks if the faction is currently placing or constructing a new center for the first time:
		public bool IsBeingBuilt (Building CheckBuilding)
		{

			if (CheckBuilding == null)
				return true;

			//First we'll search if there's a similar building inside the new buildings queue:
			if (PendingBuildings.Count > 0) {
				for (int i = 0; i < PendingBuildings.Count; i++) {
					if (PendingBuildings [i].BuildingToAdd.Code == CheckBuilding.Code) {
						return true;
					}
				}
			}
			//If we haven't found the building in the new buildings list, then we'll check the placed buildings and search for a similar building that hasn't been built just yet:
			if (FactionMgr.Buildings.Count > 0) {
				for (int i = 0; i < FactionMgr.Buildings.Count; i++) {
					if (FactionMgr.Buildings[i] != null) {
						if (FactionMgr.Buildings [i].Code == CheckBuilding.Code && FactionMgr.Buildings [i].IsBuilt == false) {
							return true;
						}
					}
				}
			}

			return false;
		}

		//a method that searches for the nearest building to a position
		public Building GetNearestBuilding (Vector3 Position, string Code)
		{
			Building Result = null;
			float Distance = 0.0f;

			if (FactionMgr.Buildings.Count > 0) {
				//Find the nearest building, with a given code, to a certain position.
				for(int i = 0; i < FactionMgr.Buildings.Count; i++)
				{
					if (FactionMgr.Buildings [i] != null) {
						if (FactionMgr.Buildings [i].Placed == true) {
							if (FactionMgr.Buildings [i].Code == Code) {
								if (Result == null) {
									Distance = Vector3.Distance (FactionMgr.Buildings [i].transform.position, Position);
									Result = FactionMgr.Buildings [i];
								} else {
									if (Vector3.Distance (FactionMgr.Buildings [i].transform.position, Position) < Distance) {
										Distance = Vector3.Distance (FactionMgr.Buildings [i].transform.position, Position);
										Result = FactionMgr.Buildings [i];
									}
								}
							}
						}
					}
				}
			}

			return Result;
		}

		//the main method that adds buildings to the list of buildings to place:
		public void AddBuildingToList (Building BuildingToAdd, int CenterID, GameObject BuildAround)
		{
			//if the building is in the buildings waiting for resource list:
			if (BuildingsWaitingResources.Contains(BuildingToAdd) == true) {
				//remove it:
				BuildingsWaitingResources.Remove(BuildingToAdd);
			}
			//by default, the build pos is the center id of the building
			Vector3 SpawnPos = FactionMgr.BuildingCenters [CenterID].transform.position;
			if (BuildAround != null) { //but sometimes we need a custom construction point
				SpawnPos = BuildAround.transform.position;
			}
			SpawnPos.y = GameMgr.TerrainMgr.SampleHeight(SpawnPos)+MapHeight; //make sure that spawn pos is the same as the map height.

			//Spawn the building for the player to place on the map:
			Building BuildingClone = Instantiate(BuildingToAdd, SpawnPos, Quaternion.identity).GetComponent<Building>();
			//set the building's position.
			if (BuildAround == null) {
				BuildingClone.transform.position = new Vector3 (BuildingClone.transform.position.x + BuildingClone.MinCenterDistance, BuildingClone.transform.position.y, BuildingClone.transform.position.z);
			}

			ResourceMgr.TakeResources (BuildingClone.BuildingResources, FactionID); //take the resources that this building has costed.
			BuildingClone.CanPlace = false; //make sure it can not be placed till we define everything for this building.

			//set the building to spawn settings (center, position, etc..)
			PendingBuildingsVars Item = new PendingBuildingsVars ();
			Item.BuildingToAdd = BuildingClone;
			Item.CenterID = CenterID;
			Item.SpawnPos = SpawnPos;
			Item.LastRoundPos = BuildingClone.transform.position;

			PendingBuildings.Add(Item); //add the pending buildings list

			//Add the newly spawned building to the list that keeps track of all available buildings
			FactionMgr.AddBuildingToList(BuildingClone);
			BuildingClone.FactionID = FactionID; //set the building's faction id

			//Hide the new building object for now, when its turn come to be placed on the map, it will be activated again:
			BuildingClone.gameObject.SetActive(false);

			//Add the building to the list:
			FactionMgr.BuildingCenters[CenterID].CurrentCenter.RegisterInProgressBuilding(BuildingToAdd.Code);

			//if thebuilding has a nav mesh obstacle, then hide it while placing the building. It'll be activated later.
			BuildingClone.NavObs = BuildingClone.gameObject.GetComponent <UnityEngine.AI.NavMeshObstacle> ();
			if (BuildingClone.NavObs) {
				BuildingClone.NavObs.enabled = false;
			}

            //Hide the building's model:
            BuildingClone.BuildingModel.SetActive(false);
            BuildingClone.BuildingPlane.SetActive (false); //hide the building's selection texture

			Timer = -1;

		}

		//This searches for builders in order to construct a building:
		public void ConstructBuilding (Building TargetBuilding)
		{
			if (TargetBuilding == null) {
				return;
			}

			int i = 0;
			bool SentExistingBuilders = false; //are sending existing units to do the job?

            //This will store the amount of builders we need:
            int Amount = TargetBuilding.WorkerMgr.WorkerPositions.Length - TargetBuilding.WorkerMgr.CurrentWorkers;

            //Search for this faction's idle builders:
            while (TargetBuilding.WorkerMgr.CurrentWorkers < TargetBuilding.WorkerMgr.WorkerPositions.Length && i < FactionMgr.Builders.Count) {
				if (FactionMgr.Builders [i] != null) {
					//if the builders are idle then:
					if (FactionMgr.Builders [i].UnitMvt.IsIdle() == true) {
						//Send the builders to construct:
						FactionMgr.Builders [i].SetTargetBuilding (TargetBuilding);
						Amount--; //less builder needed.
						SentExistingBuilders = true;
					}
				} 
				i++;
			}

			Amount -= GetInProgressBuilders (TargetBuilding); //using this method, we will remove the amount of the in progress builders that are getting created and directly send to this building.

			//If we don't have enough population slots to create all the required units.
			if (GameMgr.Factions [FactionID].CurrentPopulation + Amount > GameMgr.Factions [FactionID].MaxPopulation) {
				//We will check if we can create some of the required units or not first.
				if (GameMgr.Factions [FactionID].CurrentPopulation < GameMgr.Factions [FactionID].MaxPopulation) {
					Amount = GameMgr.Factions [FactionID].MaxPopulation - GameMgr.Factions [FactionID].CurrentPopulation;
				} else {
					Amount = 0;
				}

				//Then we will ask the faction to create buildings that add population slots:
				AttemptToAddBuilding(PopulationBuilding,false,null);

			}

			bool CanProduceBuilders = false;
			//If we still need builders:
			if (Amount > 0) {
				//Create some.
				CanProduceBuilders = CreateBuilders (TargetBuilding, TargetBuilding.WorkerMgr.WorkerPositions.Length, TargetBuilding.transform.position);
			}

			//if we can't create builders because of population saturation or we don't have enough resources
			if (Amount == 0 || CanProduceBuilders == false) {
				//If we haven't any builders to the target building:
				if (SentExistingBuilders == false) {
					//We will ask some of the resource workers to do the job then send them back later to collect resource:

					SendCollectorsToBuild (TargetBuilding);
				}
			}
		}

		public Building GetBuildingByUnit (Unit NeededUnit)
		{
			int i = 0;
			//Loop through all the available buildings:
			while (i < AllBuildings.Count) {
				int j = 0;
				//If th
				while (j < AllBuildings [i].BuildingTasksList.Count) {
					//only buildings that have space for minimum a new task:
					if (AllBuildings [i].TasksQueue.Count < AllBuildings [i].MaxTasks) {
						if (AllBuildings [i].BuildingTasksList [j].TaskType == Building.BuildingTasks.CreateUnit) {
							//If the current building has a task that allows to produce the needed unit, then return its ID.
							if (AllBuildings [i].BuildingTasksList [j].UnitPrefab.Code == NeededUnit.Code) {
								return AllBuildings [i];
							}

							if (AllBuildings [i].BuildingTasksList [j].Upgrades.Length > 0) {
								int k = 0;
								while (k < AllBuildings [i].BuildingTasksList [j].Upgrades.Length) {
									if (AllBuildings [i].BuildingTasksList [j].Upgrades [k].TargetUnit.Code == NeededUnit.Code) {
										return AllBuildings [i];
									}
									k++;
								}
							}
						}
					}

					j++;
				}

				i++;
			}

			return null;
		}

		//GET THE AMOUNT OF in progress builders (being created) to construct a certain building:
		public int GetInProgressBuilders (Building Target)
		{
			//We'll see if the required builders are already in progress (being produced) and return their amount:
			int Amount = 0;
			//Search the spawned buildings to find one that produces builders:
			for(int i = 0; i < FactionMgr.Buildings.Count; i++) {
				if (FactionMgr.Buildings [i] != null) {
					if (FactionMgr.Buildings [i].BuilderUnits.Count > 0) { //If the current building includes one or more tasks that produce builder units:

						for (int j = 0; j < FactionMgr.Buildings [i].TasksQueue.Count; j++) {
							if (FactionMgr.Buildings [i].TasksQueue [j].TargetBuilding) {
								//If this building is already producing a builder unit that will go, when spawned, to construct the target building, then we need one less builder.
								if (FactionMgr.Buildings [i].TasksQueue [j].TargetBuilding.Code == Target.Code) {
									Amount++;
								}
							}
						}
					}
				}
			}

			return Amount;
		}

		//This method is called when we don't have idle builders and we can't create any:
		public void SendCollectorsToBuild (Building TargetBuilding)
		{
			Resource TargetResource = null;
			float Distance = 0.0f;

			//Loop through all the building centers:
			for (int i = 0; i < FactionMgr.BuildingCenters.Count; i++) {
				Border CurrentBorder = FactionMgr.BuildingCenters [i].CurrentCenter;
				//Loop through the resources in range of the building center:
				for(int j = 0; j < CurrentBorder.ResourcesInRange.Count; j++) {
					if (CurrentBorder.ResourcesInRange [j] != null) {
						if (CurrentBorder.ResourcesInRange [j].gameObject.activeInHierarchy == true) { //if the resource is active
							if (CurrentBorder.ResourcesInRange [j].FactionID == FactionID) { //If this resource is being collected by this faction:
								if (CurrentBorder.ResourcesInRange [j].WorkerMgr.CurrentWorkers > 0) { //If this resource has collectors.
									if (CurrentBorder.ResourcesInRange [j].WorkerMgr.GetWorker().BuilderMgr) { //If at least one of the collectors has a builder component.
											//Pick the nearest resource to the target building and that has collectors 
											if (TargetResource == null) {
												TargetResource = CurrentBorder.ResourcesInRange [j];
												Distance = Vector3.Distance (CurrentBorder.ResourcesInRange [j].transform.position, TargetBuilding.transform.position);
											} else {
												if (Vector3.Distance (CurrentBorder.ResourcesInRange [j].transform.position, TargetBuilding.transform.position) < Distance) {
													TargetResource = CurrentBorder.ResourcesInRange [j];
													Distance = Vector3.Distance (CurrentBorder.ResourcesInRange [j].transform.position, TargetBuilding.transform.position);
												}
											}
										}
								}
							}
						}
					}
				}
			}

			//We will free half of those resource collectors and send them to work there if we found a target resource:
			if (TargetResource != null) {
				int FreeAmount = Mathf.RoundToInt (TargetResource.WorkerMgr.CurrentWorkers / 2);
				if (FreeAmount < 1)
					FreeAmount = 1;

                int n = 0;
				//Loop through all the collectors of the target resource:
				while (n < TargetResource.WorkerMgr.WorkerPositions.Length && FreeAmount > 0) {
					if (TargetResource.WorkerMgr.WorkerPositions[n].CurrentUnit != null) {
						//Make sure the collector can also build stuff:
						if (TargetResource.WorkerMgr.WorkerPositions[n].CurrentUnit.BuilderMgr) {
							Unit CurrentUnit = TargetResource.WorkerMgr.WorkerPositions[n].CurrentUnit;
							//Make the unit stop collecting the resource..
							CurrentUnit.CancelCollecting ();
							//Send the unit to construct the building:
							CurrentUnit.BuilderMgr.SetTargetBuilding (TargetBuilding);

							FreeAmount--;
						}
					}

					n++;
				}
			}
		}

		//this method creates builders:
		public bool CreateBuilders (Building Target, int Amount, Vector3 TargetArea)
		{		
			bool CanProduceBuilders = false;
			int i = 0;
			int j = 0;

			//If not all required builders are not being built:
			if (Amount > 0 && BuilderCreators.Count > 0) {

				int ID = -1; //Holds the ID of the nearest building to the target area:
				int TaskID = -1; //Holds the task ID of the building to produce builders.
				j = 0;
				float Distance = 0.0f;

				//Look through the rest of the builder creators:
				for (i = 0; i < BuilderCreators.Count; i++) {
					if (BuilderCreators[i].BuildingUpgrading == false && BuilderCreators[i].IsBuilt == true && BuilderCreators[i].Health >= BuilderCreators[i].MinTaskHealth && FactionMgr.Buildings[i].TasksQueue.Count < FactionMgr.Buildings[i].MaxTasks) {
						//If we find a building close to the target area, save its ID.
						if (Vector3.Distance (BuilderCreators [i].transform.position, TargetArea) < Distance || ID == -1) {
							//Check all the tasks that produce builder units:
							j = 0;
							while (j < BuilderCreators [i].BuilderUnits.Count) {
                                //make sure that the unit hasn't reached its limit:
                                if (FactionMgr.HasReachedLimit(BuilderCreators[i].BuildingTasksList[BuilderCreators[i].BuilderUnits[j]].UnitPrefab.Code) == false)
                                {
                                    //first check if the resource unit is non attacking unit and the amount of non attacking units is still conform to the amount specified in the NPC Army component
                                    //or if the resource collector from the building we're looking at has an attack component and continue:
                                    if (BuilderCreators[i].BuildingTasksList[BuilderCreators[i].ResourceUnits[j]].UnitPrefab.gameObject.GetComponent<Attack>() || FactionMgr.ArmyMgr.CanCreateNonAttackUnit())
                                    {
                                        if (ResourceMgr.CheckResources(BuilderCreators[i].BuildingTasksList[BuilderCreators[i].BuilderUnits[j]].RequiredResources, FactionID, ResourceMgr.FactionResourcesInfo[FactionID].NeedRatio) == true)
                                        {
                                            //Check if we have enough resources to launch this task:
                                            Distance = Vector3.Distance(BuilderCreators[i].transform.position, TargetArea);
                                            ID = i;
                                            TaskID = BuilderCreators[i].BuilderUnits[j];
                                        }
                                    }
                                }
								j++;
							}
						}
					}
				}

				//Add tasks to produce builder units:
				if (ID >= 0) {
					while  (Amount > 0 && ResourceMgr.CheckResources (BuilderCreators[ID].BuildingTasksList[TaskID].RequiredResources, FactionID, ResourceMgr.FactionResourcesInfo[FactionID].NeedRatio) == true && BuilderCreators [ID].TasksQueue.Count < BuilderCreators [ID].MaxTasks) {
						//Check if we have enough resources to launch this task:
						//Add the new task to the building's task queue.

						GameMgr.TaskMgr.LaunchTask (BuilderCreators [ID], TaskID, -1, TaskManager.TaskTypes.CreateUnit);

						Amount--;
						CanProduceBuilders = true; //as soon as we're able to create even unit then mark this as true.
					} 
				} else {
					//no building to create units is found
				}

			}
			return CanProduceBuilders;
		}

		//Check all spawned buildings and see if they require construction:
		public void CheckAllBuildings ()
		{

			CheckBuildings = false;
			//Loop through all the spawned buildings:
			if (FactionMgr.Buildings.Count > 0) {
				for(int i = 0; i < FactionMgr.Buildings.Count; i++)
				{
					if (FactionMgr.Buildings [i] != null) {
						//If the faction already placed the building on the map:
						if (FactionMgr.Buildings [i].Placed == true) {
							//If the current building hasn't been built for the first time or its current health is lower than the minimum required health to launc a task:
							if (FactionMgr.Buildings [i].IsBuilt == false || FactionMgr.Buildings [i].Health < FactionMgr.Buildings [i].MaxHealth) {
								CheckBuildings = true; //We need to keep checking those buildings that need construction.

								ConstructBuilding (FactionMgr.Buildings [i]); //Send builders to construct this building.
							}
						}
					}
				}
			}

			if (CheckBuildings == false) {
				//In case we sent resource collectors to construct buildings, this will check for resource collection again and then try to send those collectors back:
				FactionMgr.ResourceMgr.CheckResources = true;
			}
		}

		//replace an existing building in the all building list that the faction can spawn with another building (usually after having an age upgrade).
		public void ReplaceBuilding(string Code, Building NewBuilding)
		{
			if (Code == NewBuilding.Code) //if both buildings have the same codes.
				return;

			if (AllBuildings.Count > 0) { //go through all the buildings in the list
				int i = 0;
				bool Found = false;
				while (i < AllBuildings.Count && Found == false) {
					if (AllBuildings [i].gameObject.GetComponent<Building> ().Code == Code) { //when the building is found
						AllBuildings.RemoveAt (i);//remove it
						if (!AllBuildings.Contains (NewBuilding)) { //make sure we don't have the same building already in the list
							AllBuildings.Insert (i,NewBuilding); //place the new building in the same position
						}
						Found = true;
					}
					i++;
				}
			}

			//since this is a NPC building placement manager, check the special buildings as well:
			if (BuildingCenter.Code == Code) {
				BuildingCenter = NewBuilding;
			}
			if (PopulationBuilding.Code == Code) {
				PopulationBuilding = NewBuilding;
			}
			if (DropOffBuilding.Code == Code) {
				DropOffBuilding = NewBuilding;
			}
		}
	}
}