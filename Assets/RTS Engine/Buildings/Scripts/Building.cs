using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.AI;
//FoW Only
//using FoW; 

/* Building script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
	public class Building : MonoBehaviour {

		public string Name; //Name of the building that will be displayed in the UI.
		public string Code; //The building's unique code that identifies it.
		public string Category; //The category that this building belongs to.
		public string Description; //A short description of the building that will be displayed when it's selected.
		public Sprite Icon; //The building's icon.
		public int TaskPanelCategory; //when using task panel categories, this is the category ID where the task button of this building will appear when selecting builder units.

		public bool FreeBuilding = false; //if true, then no faction will be controlling this building.
		public bool CanBeAttacked = true; //can the building be actually attacked from units? 

        [HideInInspector]
        public WorkerManager WorkerMgr; //the worker manager that handles this building constructors

		public float MinCenterDistance = 10.0f; //The minimum distance between this building and the nearest building center.
        public bool ForceMinDistanceOnPlayer = false; //force the above minimum center distance for the local player as well? 

		//When the building reaches its maximum health, the building will get its initial state.
		[HideInInspector]
		public bool IsBuilt = false; //Has the building been built after it has been placed on the map?

		public float Radius = 5.0f; //the building's radius will be used to determine when units stop when attacking the building
		public int FactionID = 0; //Building's team ID.
		public bool PlacedByDefault = false; //Is the building placed by default on the map.
		[HideInInspector]
		public bool Placed = false; //Has the building been placed on the map?
		[HideInInspector]
		public bool NewPos = false; //Did the player move the building while placing it? We need to know this so that we can minimize the times certain functions, that check if the 
		//new position of the building is correct or not, are called.
		[HideInInspector]
		public bool CanPlace = true; //Can the player place the building at its current position?
		[HideInInspector]
		public int CollisionAmount = 0; //The amount of colliders whose this building is in collision with.

		//Building only near resources:
		public bool PlaceNearResource = false; //if true then this building will only be placable in range of a resource of the type below.
		public string ResourceName = ""; //the resource type that this building will be placed in range
		public float ResourceRange = 5.0f; //the maximum distance between the building and the resource

		//Building population:
		public int AddPopulation = 0; //Increase the maximum population for this faction.

		//Resource drop off:
		public bool ResourceDropOff = false; //make resoure collectors able to drop off their resources at this building.
        public Transform DropOffPos;
        public bool AcceptAllResources = true; //when true then the resource collectors can drop off all resources in this building, if not then the resource must be assigned in the array below
		//a list of the resources that this building accepts to be dropped off in case it doesn't accept all resources
		public string[] AcceptedResources;


		//Building health:
		public float MaxHealth = 100.0f; //maximum health.
        public float HoverHealthBarY = 2.0f; //this is the height of the hover health bar (in case the feature is enabled).
        public float MinTaskHealth = 70.0f; //minimum health required in order for a building to launch a task.
		[HideInInspector]
		public float Health; //the building's current health.

		//Building state: You can show and hide parts of the building depending on its health:
		[System.Serializable]
		public class BuildingStateVars
		{
			//Below, you need to enter the interval at which the state will be activated:
			public float MinHealth = 0.0f;
			public float MaxHealth = 50.0f;
			//Make sure that the intervals do not interfere.

			public GameObject[] PartsToShow; //Parts of the building to show (activate).
			public GameObject[] PartsToHide; //Parts of the building to hide (desactivate).
		}
		public BuildingStateVars[] BuildingStates;
		public GameObject BuildingStatesParent; //Extra objects intented to be used only for building states (not shown when the building has max health) should be children of this object.
		[HideInInspector]
		public int BuildingStateID = -1; //Saves the current building state ID.
		public ResourceManager.Resources[] DestroyAward; //these resources will be awarded to the faction that destroyed this unit


		[HideInInspector]
		public Border CurrentCenter = null; //The current building center that this building belongs to (is inside its borders)

		[HideInInspector]
		public bool FactionCapital = false; //If true, then the building is the capital of this faction (meaning that destroying it means that the faction loses).

		public SelectionObj PlayerSelection; //Must be an object that only include this script, a trigger collider and a kinematic rigidbody.
		//the collider represents the boundaries of the object (building or resource) that can be selected by the player.

		public GameObject ConstructionObj; //Must be a child object of the building. If it's assigned, it will be shown when the building is built for the first time.
		[System.Serializable]
		public class ConstructionStateVars
		{
			//Below, you need to enter the interval at which the state will be activated:
			public float MinHealth = 0.0f;
			public float MaxHealth = 50.0f;
			//Make sure that the intervals do not interfere.

			public GameObject ConstructionObj;
		}
		public ConstructionStateVars[] ConstructionStates; //if the construction obj is set to null, then this will be used and will show different construction objects depending on the building's health.
		int ConstructionState = -1;

		//Damage effect:
		public EffectObj DamageEffect; //Created when a damage is received in the contact point between the attack object and this one:

		public GameObject BuildingPlane; //The plane where the selection texture appears.

        public GameObject BuildingModel; //drag and drop the building's model here.

		//If the building allows to create unit, they will spawned in this position.
		public Transform SpawnPosition;
        Vector3 SpawnPos; //the actual spawn position
		//The position that the new unit goes to from the spawn position.
		public Transform GotoPosition;
		public Transform Rallypoint;

		//Building destruction effects:
		public AudioClip DestructionAudio; //audio played when the player is destroyed.
		public GameObject DestructionObj = null; //the object to create as destruction effect.
		bool Destroyed = false;

        [System.Serializable]
        public class FactionColorsVars
        {
            public int MaterialID = 0;
            public Renderer Renderer;
        }
		public FactionColorsVars[] FactionColors; //Mesh renderers 

		public ResourceManager.Resources[] BuildingResources;

		//this the timer during which the building texture flahes when a unit is sent to construct or attack this building
		[HideInInspector]
		public float FlashTime = 0.0f;

		public int MaxTasks = 4; //The amount of maximum tasks that the building can handle at the same time.

		//Building tasks:
		public enum BuildingTasks {CreateUnit, Destroy, Research}; //Task types
		//task defining variables:
		[System.Serializable]
		public class BuildingTasksVars
		{
			public bool FactionSpecific = false;
			public string FactionCode = "Faction001";

			public string Description; //description shown in the task panel when hovering over the task button.
			public BuildingTasks TaskType = BuildingTasks.CreateUnit & BuildingTasks.Research & BuildingTasks.Destroy; //the type of the task
			public int TaskPanelCategory = 0; //if you are using different categories in the task panel then assign this for each task.
			public int UpgradeTaskPanelCategory = 0; //same as above, but for upgrade tasks of this task.
			public Sprite TaskIcon; //the icon shown in the tasks panel.
			public ResourceManager.Resources[] RequiredResources; //Resources required to complete this task.

			//Creating units vars:
			public Unit UnitPrefab; //if this task allows to create a unit, the unit prefab should be placed here.

			//Upgrade Unit Abilites:
			public float AddSpeed = 1.0f;
			public float AddUnitDamage = 1.0f;
			public float AddBuildingDamage = 1.0f;
			public float AddAttackReload = -0.2f;
			public float AddSearchRange = 3.0f;
			public float AddMaxHealth = 50.0f;

			public Unit[] UnitList;

			//Timers:
			public float ReloadTime = 3.0f; //how long does the task last?

			//task upgrades (only if it is a task that produces units).
			public TaskUpgradesVars[] Upgrades;
			[HideInInspector]
			public int CurrentUpgradeLevel = 0;
			[HideInInspector]
			public bool Active = false;
			[HideInInspector]
			public bool Reached = false;

			public AudioClip TaskCompletedAudio; //Audio clip played when the task is completed.
		}
		public List<BuildingTasksVars> BuildingTasksList;

		//Task upgrades vars:
		[System.Serializable]
		public class TaskUpgradesVars
		{
			public Unit TargetUnit; //Target unit to upgrade to.
			public ResourceManager.Resources[] UpgradeResources; //Resources required to to upgrade the task.
			public Sprite UpgradeIcon; //The icon that will appear in the task panel to launch this upgrade.
			public Sprite NewTaskIcon; //The icon that will replace the old task's icon.
			public string UpgradeDescription; //a short description of the upgrade.3
			public string NewTaskDescription; //the new description of the task.
			public ResourceManager.Resources[] NewTaskResources; //Resources required to complete the task after the upgrade (leave empty to make no changes).
			public float UpgradeReload = 5.0f; //how long will the upgrade last to take effect.
			public float NewReloadTime = 3.0f; //how long will the task take after the upgrade.
		}

		public int TotalTasksAmount = 0;

		//a list of the pending tasks:
		[System.Serializable]
		public class PendingTasksInfo
		{
			public Building TargetBuilding = null; //if the unit will be sent to construct a building right after creation, then this is the building.
			public Resource TargetResource = null; //if the unit will be sent to collect a resource right after creation, then this is that resource.
			public NPCArmy TargetArmy = null; //if the unit is an attack unity and will be part of a NPC faction army, this is the component that manages that army.
			public NPCUnitSpawner TargetSpawner = null; //if the unit is required by the NPC spawner:

			public int UnitSpawnerID = -1; //the ID in the NPC Unit spawner for this task (if it is a unit creation task).
			public int ID = -1; //the task ID.
			public bool Upgrade = false;
		}
		[HideInInspector]
		public List<PendingTasksInfo> TasksQueue = new List<PendingTasksInfo>();
		[HideInInspector]
		public float TaskQueueTimer = 0.0f; //this is the task's timer. when it's ended, the task is finished.
        public int team;
		//Building Upgrade:
		public bool DirectUpgrade = true; //allow the player to directly upgrade this building? 
		public Building UpgradeBuilding = null; //the building to upgrade to
		public ResourceManager.Resources[] BuildingUpgradeResources; //resources required to launch the upgrade.
		public Building[] UpgradeRequiredBuildings; //buildings that must be spawned in order to launch the upgrade.
		public float BuildingUpgradeReload = 8.0f; //duration of the upgrade
		public GameObject UpgradeBuildingEffect; //effect spawned when the upgrade is launched.
		[HideInInspector]
		public float BuildingUpgradeTimer;
		[HideInInspector]
		public bool BuildingUpgrading = false;
		public bool UpgradeAllBuildings = false;

		//Resource collection bonus:
		//A building can effect the resources existing inside the same border by increasing the amount of collection per second:
		[System.Serializable]
		public class BonusResourcesVars
		{
			public string Name; //Resource's name
			public float AddCollectAmountPerSecond = 0.22f; //self-explantory
		}
		public BonusResourcesVars[] BonusResources;

		//The variables below determine the types of the tasks this building has:
		[HideInInspector]
		public List<int> UnitCreationTasks = new List<int>(); //if the building has a task that produces any type of units then it will be added in this list.
		[HideInInspector]
		public List<int> ArmyUnits = new List<int>(); //if the building has a task that produce units with the attack comp, the task ID will be added to this list.
		[HideInInspector]
		public List<int> BuilderUnits = new List<int>(); //if the building has a task that produce units with the builder comp, the task ID will be added to this list.
		[HideInInspector]
		public List<int> ResourceUnits = new List<int>(); //if the building has a task that produce units with the resource gather comp, the task ID will be added to this list.

		//NPC Army vars:
		//When the NPC manager launches an order to create army units, the buildings that receive this order will announce how many of the units are in progress.
		[HideInInspector]
		public int PendingUnitsToCreate = 0;
		[HideInInspector]
		public int PendingUnitsArmyID = 0; 

		//Audio:
		public AudioClip SelectionAudio; //Audio played when the building is selected.
		public AudioClip LaunchTaskAudio; //Audio played when a new building task is launched.
		public AudioClip DeclinedTaskAudio; //When the task is declined due to lack of resources, the fact that the maximum in progress task has been reached or the min task health is not present. 
		public AudioClip UpgradeBuildingLaunched; //When the building upgrade starts.
		public AudioClip UpgradeBuildingComplete; //When the building has been upgraded.

        //building components:
        [HideInInspector]
		public Portal PortalMgr;
        [HideInInspector]
        public Attack AttackMgr;
        [HideInInspector]
        public Border BorderMgr;
        [HideInInspector]
        public APC APCMgr;

        Collider Coll;
        Renderer PlaneRenderer;

        //Scripts:
        [HideInInspector]
		public SelectionManager SelectionMgr;
		[HideInInspector]
		public ResourceManager ResourceMgr;
		[HideInInspector]
		public FactionManager FactionMgr;
		[HideInInspector]
		public GameManager GameMgr;
		[HideInInspector]
		public UIManager UIMgr;
        MovementManager MvtMgr;
		[HideInInspector]
		public CameraMovement CamMov;
        [HideInInspector]
        AttackWarningManager AttackWarningMgr;
        [HideInInspector]
        MinimapIconManager MinimapIconMgr;
        TerrainManager TerrainMgr;

        //components:
        [HideInInspector]
		public NavMeshObstacle NavObs;

		void Awake ()
		{
			NavObs = GetComponent <NavMeshObstacle> (); //get the navigation obstacle component.
			PortalMgr = GetComponent<Portal>(); //get the portal component
            AttackMgr = GetComponent<Attack>(); //attack component
            WorkerMgr = GetComponent<WorkerManager>(); //worker manager component
            Coll = GetComponent<Collider>(); //building's main collider.
            BorderMgr = GetComponent<Border>(); //border comp in the building
            APCMgr = GetComponent<APC>(); //APC comp
            PlaneRenderer = BuildingPlane.GetComponent<Renderer>(); //get the building's plane renderer here

            ConstructionState = -1; //initialize the construction state.

			//searching for all the comps that this script needs:
			GameMgr = GameManager.Instance;
			SelectionMgr = GameMgr.SelectionMgr;
			ResourceMgr = GameMgr.ResourceMgr;
			UIMgr = GameMgr.UIMgr;
			CamMov = GameMgr.CamMov;
            AttackWarningMgr = AttackWarningManager.Instance;
            MinimapIconMgr = MinimapIconManager.Instance;
            TerrainMgr = GameMgr.TerrainMgr;
            MvtMgr = GameMgr.MvtMgr;

        }

		void Start () 
		{
			if (LayerMask.NameToLayer ("SelectionPlane") > 0) { //if there's a layer for the selection plane
				BuildingPlane.layer = LayerMask.NameToLayer ("SelectionPlane"); //assign this layer because we don't want the main camera showing it
			}

			if (FreeBuilding == false) { //if this is not a free building (belongs to a faction)
                OnFactionBuildingInit();
			} else { //if this is a free building
				FactionID = -1; //set its faction id to -1 to avoid it getting mixed with other faction
				PlacedByDefault = true; //as its a free building, there's nobody to construct then place by default.
			}
            
			//if the building is placed by default on map (does not need builders):
			if (PlacedByDefault == true) {
                OnDefaultBuildingInit(); //init placed by default building
			} else {//If the building requires construction:
                OnBuildingInit(); //init normal building
            }
            team = GameMgr.Factions[FactionID].team;
            //default settings for placing the building.
            CollisionAmount = 0;
			CanPlace = true;
			if(BuildingPlane == null) //if the building plane is not available.
			{
				Debug.LogError("You must attach a plane object at the bottom of the building and set it to 'BuildingPlane' in the inspector.");
			}

			if(PlacedByDefault == false) //if the building is not placed by default.
			{
                PlaneRenderer.material.color = Color.green; //start by setting the selection texture color to green which implies that it's allowed to place building at its position.
			}
			else
			{
				BuildingPlane.SetActive(false); //hide the building plane in case the building is placed by default.
			}

			//Building boundaries:
			if(Coll == null) //if the building collider is not present.
			{
				Debug.LogError("The building parent object must have a collider to represent the building's boundaries.");
			}
			else
			{
				Coll.isTrigger = true; //the building's main collider must always have "isTrigger" is true.
			}

			Rallypoint = GotoPosition;
			//Hide the goto position:
			if (GotoPosition != null) {
				GotoPosition.gameObject.SetActive (false);
			}

			//Set the selection object if we're using a different collider for player selection:
			if (PlayerSelection != null) {
				//set the player selection object for this building/resource:
				PlayerSelection.MainObj = this.gameObject;
				//Disable the player selection collider object if the building has not been placed yet:
				if (Placed == false) {
					PlayerSelection.gameObject.SetActive (false);
				}
			} else {
				Debug.LogError ("Player selection collider is missing!");
			}
		}

		void SetResourceBonus (bool Add)
		{
			if (BonusResources.Length == 0 || CurrentCenter == null)
				return;

			//Since the resource bonus is given to resources inside the same border, we will first check if the building is a building center or not.
			//If it's a building center, then the resources around it will receive the bonus. If not, then we'll give the bonus to the building's center resources.
			//So first of all, we will pick the position where we will start looking for resources from depending on the above:
			Vector3 CenterPos = Vector3.zero;
			float Size = CurrentCenter.Size;
			if (BorderMgr) {
				CenterPos = transform.position;
			} else {
				CenterPos = CurrentCenter.transform.position;
			}

			//Search for the resources around the center position:
			Collider[] SearchResources = Physics.OverlapSphere (CenterPos, Size);
			if (SearchResources.Length > 0) {
				//Loop through all searched resources:
				for (int i = 0; i < SearchResources.Length; i++) {
					for (int j = 0; j < BonusResources.Length; j++) {
						Resource TempResource = SearchResources [i].gameObject.GetComponent<Resource> ();
						if (TempResource) {
							//Add the bonus amount if resource matches:
							if (TempResource.Name == BonusResources [j].Name) {
								if (Add == true) {
									TempResource.CollectAmountPerSecond += BonusResources [j].AddCollectAmountPerSecond;
								} else {
									TempResource.CollectAmountPerSecond -= BonusResources [j].AddCollectAmountPerSecond;

								}
							}
						}
					}
				}
			}
		}

        //method called when the player places the building
        public void PlaceBuilding ()
        {
            //enable the nav mesh obstacle comp
            if (NavObs)
            {
                NavObs.enabled = true;
            }

            //Disable the selection collider so that it won't get auto selected as soon as it's spawned
            PlayerSelection.gameObject.GetComponent<Collider>().enabled = false;

            //if the building includes a border comp, then enable it as well
            if (BorderMgr)
                BorderMgr.enabled = true;
            if(GameManager.MultiplayerGame==false)
            FactionMgr.AddBuildingToList(this); //add the building to the faction manager list

            //Activate the player selection collider:
            PlayerSelection.gameObject.SetActive(true);

            //Set the building's health to 0 so that builders can start adding health to it:
            Health = 0.0f;

            BuildingPlane.SetActive(false); //hide the building's plane

            //Building is now placed:
            Placed = true;
            //custom event:
            if (GameMgr.Events) GameMgr.Events.OnBuildingPlaced(this);
            ToggleConstructionObj(true); //Show the construction object when the building is placed.

            //if this is the local player
            if (GameManager.PlayerFactionID == FactionID)
            {
                //If god mode is enabled and this is the local player
                if (GodMode.Enabled == true)
                {
                    AddHealth(MaxHealth, null);
                }
                //If God Mode is not enabled, make builders move to the building to construct it.
                else if (SelectionMgr.SelectedUnits.Count > 0)
                {
                    int i = 0; //counter
                    bool MaxBuildersReached = false; //true when the maximum amount of builders for the hit building has been reached.
                    while (i < SelectionMgr.SelectedUnits.Count && MaxBuildersReached == false)
                    { //loop through the selected as long as the max builders amount has not been reached.
                        if (SelectionMgr.SelectedUnits[i].BuilderMgr)
                        { //check if this unit has a builder comp (can actually build).
                          //make sure that the maximum amount of builders has not been reached:
                            if (WorkerMgr.CurrentWorkers < WorkerMgr.WorkerPositions.Length)
                            {
                                //Make the units fix/build the building:
                                SelectionMgr.SelectedUnits[i].BuilderMgr.SetTargetBuilding(this);
                                Debug.Log("entered");

                            }
                            else
                            {
                                MaxBuildersReached = true;
                                //if the max builders amount has been reached.
                                //Show this message: 
                                GameMgr.UIMgr.ShowPlayerMessage("Max building amount for building has been reached!", UIManager.MessageTypes.Error);
                            }
                        }

                        i++;
                    }
                    SelectionMgr.isBuilding = false;

                }
            }
        }

        //method called when placing a building to check if it's current position is valid or not:
        void IsValidBuildingPos ()
        {
            //FoW Only:
            //uncomment this only if you are using the Fog Of War asset and replace MIN_FOG_STRENGTH with the value you need.
            /*if (!FogOfWar.GetFogOfWarTeam(FactionID).IsInFog(transform.position, MIN_FOG_STRENGTH))
            {
                PlaneRenderer.material.color = Color.red; //Show the player that the building can't be placed here.
                CanPlace = false; //The player can't place the building at this position.
                return;
            }*/

            //first check if the building is in range of 
            if (!IsBuildingInRange() || !IsBuildingOnMap() || !IsBuildingNearResource())
            {
                PlaneRenderer.material.color = Color.red; //Show the player that the building can't be placed here.
                CanPlace = false; //The player can't place the building at this position.
            }
            else if (CollisionAmount == 0)
            {
                PlaneRenderer.material.color = Color.green; //Show the player that the building can be placed here.
                CanPlace = true; //The player can place the building at this position.
            }
            NewPos = false;
        }

        //method called when the building is in the process of upgrading
        void UpdateBuildingUpgrade ()
        {
            //if the building is selected:
            if (BuildingUpgradeTimer > 0)
            { //if the timer is still going
                BuildingUpgradeTimer -= Time.deltaTime;
                if (SelectionMgr.SelectedBuilding == this)
                { //if this building is selected
                    UIMgr.UpdateBuildingUpgrade(this); //keep updating the UI
                }
            }
            if (BuildingUpgradeTimer <= 0)
            { //if the upgrade timer comes to an end
                if (BuildingUpgradeTimer == -1.0f)
                {
                    LaunchBuildingUpgrade(false);
                }
                else
                {
                    LaunchBuildingUpgrade(true);
                }
            }
        }

		void Update () 
		{
			//For the player faction only, because we check if the object is in range or not for other factions inside the NPC building manager: 
			if (FactionID == GameManager.PlayerFactionID) {
				if (Placed == false) { //If the building isn't placed yet, we'll check if its inside the chosen range from the nearest building center:
					if (NewPos == true) { //If the building has been moved from its last position.
                        IsValidBuildingPos();
					}
				}
			}

			//Selection flash timer:
			if (FlashTime > 0) {
				FlashTime -= Time.deltaTime;
			}
			if (FlashTime < 0) {
				//if the flash timer is over:
				FlashTime = 0.0f;
				CancelInvoke ("SelectionFlash");
				BuildingPlane.gameObject.SetActive (false); //hide the building plane.
			}

            if(IsBuilt == true)
            {
                if (BuildingUpgrading == true)
                { //if we are upgrading the building
                    UpdateBuildingUpgrade();
                }
                else if (TasksQueue.Count > 0) //if there are pending tasks
                {
                    //keep updating them:
                    UpdateBuildingTasks();
                }
			}

		}

        //method called when the building has pending tasks:
        void UpdateBuildingTasks()
        {
            //if the task timer is still going and we are not using the god mode
            if (TaskQueueTimer > 0 && GodMode.Enabled == false)
            {
                TaskQueueTimer -= Time.deltaTime;

                //Update the building's in progress task UI:
                if (SelectionMgr.SelectedBuilding == this)
                {
                    UIMgr.UpdateInProgressTasksUI(this);
                }
            }
            //till it stops:
            else
            {   
                //play the task complete audio
                if (BuildingTasksList[TasksQueue[0].ID].TaskCompletedAudio != null)
                {
                    AudioManager.PlayAudio(GameMgr.GeneralAudioSource.gameObject, BuildingTasksList[TasksQueue[0].ID].TaskCompletedAudio, false); //Play the audio clip on
                }

                //custom events
                GameMgr.Events.OnTaskCompleted(this, BuildingTasksList[TasksQueue[0].ID]);

                if (TasksQueue[0].Upgrade == true) //if this is an upgrade task
                {
                    //complete it
                    OnUpgradeTaskCompleted();
                }
                else //if this is a normal task
                {
                    OnTaskCompleted();
                }

                if (TasksQueue.Count > 0)
                    TasksQueue.RemoveAt(0);// Remove this task after handling it out.

                //if this building is selected.
                if (SelectionMgr.SelectedBuilding == this)
                {
                    //update the selection panel UI to show that this task is no longer in progress.
                    UIMgr.UpdateInProgressTasksUI(this);
                    UIMgr.UpdateBuildingTasks(this);
                }
                if (TasksQueue.Count > 0)
                {
                    //if there are more tasks in the queue
                    TaskQueueTimer = BuildingTasksList[TasksQueue[0].ID].ReloadTime; //set the reload for the next task and start over.
                }
            }
        }

        //a method called when an upgrade task is complete:
        void OnUpgradeTaskCompleted ()
        {
            //making sure that the current upgrade level is not the maximal one
            if (BuildingTasksList[TasksQueue[0].ID].CurrentUpgradeLevel < BuildingTasksList[TasksQueue[0].ID].Upgrades.Length)
            {
                //update the upgrade on all similar buildings
                CheckTaskUpgrades(TasksQueue[0].ID, false, false);
                //if this is an upgrade task:
                UpgradeTask(TasksQueue[0].ID, BuildingTasksList[TasksQueue[0].ID].CurrentUpgradeLevel);
                //if it's a NPC faction and this upgraded task is a create unit task
                if (FactionID != GameManager.PlayerFactionID && GameManager.MultiplayerGame == false && BuildingTasksList[TasksQueue[0].ID].TaskType == BuildingTasks.CreateUnit)
                {
                    //inform the NPC unit spawner:
                    FactionMgr.UnitSpawner.ReloadBuildingLists(this, false);
                    FactionMgr.UnitSpawner.ReloadBuildingLists(this, true);
                    //if the task belongs to army tasks:
                    if (ArmyUnits.Contains(TasksQueue[0].ID))
                    {
                        //update the army unit in the army manager
                        FactionMgr.ArmyMgr.ReloadArmyUnitsPriority(this, false);
                        FactionMgr.ArmyMgr.ReloadArmyUnitsPriority(this, true);
                    }
                }
            }
        }

        //a method called when a normal task (not an upgrade one) is complete:
        void OnTaskCompleted ()
        {
            if (BuildingTasksList[TasksQueue[0].ID].TaskType == BuildingTasks.CreateUnit)
            { //If the first task in the queue is about creating units.
                float Height = BuildingTasksList[TasksQueue[0].ID].UnitPrefab.UnitHeight;
                if (BuildingTasksList[TasksQueue[0].ID].UnitPrefab.FlyingUnit == true && TerrainMgr.AirTerrain != null)
                { //for flying units, set the initial height higher than the air terrain.
                    Height = TerrainMgr.AirTerrain.transform.position.y * 2;
                }

                if (GameManager.MultiplayerGame == false)
                {
                    bool Cancel = false;
                    //if this is a NPC faction
                    if (GameManager.PlayerFactionID != FactionID)
                    {
                        //If the new unit is supposed to go contrusct a building or go collect resources:
                        //Check if there are places available to construct or collect the resource, if not, we'll cancel creating the unit
                        if (TasksQueue[0].TargetBuilding != null)
                        {
                            if (TasksQueue[0].TargetBuilding.WorkerMgr.CurrentWorkers == TasksQueue[0].TargetBuilding.WorkerMgr.WorkerPositions.Length)
                            {
                                CancelInProgressTask(0);
                                Cancel = true;
                            }
                        }
                        else if (TasksQueue[0].TargetResource != null)
                        {
                            if (TasksQueue[0].TargetResource.WorkerMgr.CurrentWorkers == TasksQueue[0].TargetResource.WorkerMgr.WorkerPositions.Length)
                            {
                                CancelInProgressTask(0);
                                Cancel = true;
                            }
                        }
                    }

                    if (Cancel == false)
                    {
                        // create the new unit object.
                        BuildingTasksList[TasksQueue[0].ID].UnitPrefab.gameObject.GetComponent<NavMeshAgent>().enabled = false; //disable this component before spawning the unit as it might place the unit in an unwanted position when spawned
                        Unit NewUnit = Instantiate(BuildingTasksList[TasksQueue[0].ID].UnitPrefab.gameObject, new Vector3(SpawnPos.x, SpawnPos.y+ Height, SpawnPosition.position.z), BuildingTasksList[TasksQueue[0].ID].UnitPrefab.transform.rotation).GetComponent<Unit>();

                        NewUnit.NPCUnitSpawnerID = TasksQueue[0].UnitSpawnerID; //set the NPC unit spawner id for this unit
                                                                                //set the unit faction ID.
                        NewUnit.FactionID = FactionID;
                        NewUnit.CreatedBy = this;

                        NewUnit.gameObject.GetComponent<NavMeshAgent>().enabled = true; //enable the nav mesh agent component for the newly created unit

                        //rallypoint for NPC players:
                        //if the new unit must construct a building, send the unit to build.
                        if (TasksQueue[0].TargetBuilding != null && NewUnit.GetComponent<Builder>())
                        {
                            NewUnit.GetComponent<Builder>().SetTargetBuilding(TasksQueue[0].TargetBuilding);
                        }
                        //if the new unit is entitled to collect a resource, send the unit to collect.
                        else if (TasksQueue[0].TargetResource != null && NewUnit.GetComponent<GatherResource>())
                        {
                            NewUnit.GetComponent<GatherResource>().SetTargetResource(TasksQueue[0].TargetResource);
                        }

                        //if the new unit belongs to NPC army.
                        if (TasksQueue[0].TargetArmy != null)
                        {
                            if (TasksQueue[0].TargetArmy.ArmyUnits[TasksQueue[0].ID].ProgressAmount > 0)
                            {
                                //add the unit to the NPC army list.
                                TasksQueue[0].TargetArmy.ArmyUnits[TasksQueue[0].ID].CurrentUnits.Add(NewUnit);

                                NewUnit.GetComponent<Attack>().ArmyUnitID = TasksQueue[0].ID;
                                //Make all enemy units attack on range:
                                NewUnit.GetComponent<Attack>().AttackInRange = true;

                                TasksQueue[0].TargetArmy.ArmyUnits[TasksQueue[0].ID].ProgressAmount--;
                            }
                        }
                    }

                }
                else
                {
                    //if it's a MP game, then ask the server to spawn the unit.
                    //if it's a MP game, then ask the server to spawn the unit.
                    //send input action to the input manager
                    InputVars NewInputAction = new InputVars();
                    //mode:
                    NewInputAction.SourceMode = (byte)InputSourceMode.Create;

                    NewInputAction.Source = BuildingTasksList[TasksQueue[0].ID].UnitPrefab.gameObject;
                    NewInputAction.Target = gameObject;

                    NewInputAction.InitialPos = new Vector3(SpawnPos.x, SpawnPos.y + Height, SpawnPosition.position.z);

                    InputManager.SendInput(NewInputAction);
                }

            }
            else if (BuildingTasksList[TasksQueue[0].ID].TaskType == BuildingTasks.Research)
            { //if the tasks upgrades certain units' abilities:
                if (GameManager.MultiplayerGame == false)
                { //if this an offline game:
                    LaunchResearchTaskLocal(TasksQueue[0].ID); //launch the task directly
                }
                else
                {
                    LaunchResearchTask(TasksQueue[0].ID);
                }

            }
            else if (BuildingTasksList[TasksQueue[0].ID].TaskType == BuildingTasks.Destroy)
            { //if this task has a goal to self destroy the building.
                SelectionMgr.DeselectBuilding();
                //Destroy building:
                DestroyBuilding(false);
            }
        }

        //a method to send a unit to the building's rally point
        public void SendUnitToRallyPoint (Unit RefUnit)
		{
			if (Rallypoint != null) {
                Building BuildingRallyPoint = Rallypoint.gameObject.GetComponent<Building>();
                Resource ResourceRallyPoint = Rallypoint.gameObject.GetComponent<Resource>();
                if (BuildingRallyPoint && RefUnit.BuilderMgr) {
					if (BuildingRallyPoint.BuilderUnits.Count < BuildingRallyPoint.WorkerMgr.WorkerPositions.Length) {
                        RefUnit.BuilderMgr.SetTargetBuilding (BuildingRallyPoint);
					}
				}
				else if (ResourceRallyPoint && RefUnit.ResourceMgr) {
					if (ResourceRallyPoint.WorkerMgr.CurrentWorkers < ResourceRallyPoint.WorkerMgr.WorkerPositions.Length) {
                        RefUnit.ResourceMgr.SetTargetResource (ResourceRallyPoint);
					}
				}
				else
				{
                    //move the unit to the goto position
                    MvtMgr.Move(RefUnit, Rallypoint.position, 0.0f, null, InputTargetMode.None);
				}
			}
		}

		//Setting the task types will help factions pick the task that they need:
		void SetTaskTypes ()
		{
			//initialize the task lists:
			UnitCreationTasks = new List<int>();
			ArmyUnits = new List<int>();
			BuilderUnits = new List<int>();
			ResourceUnits = new List<int>();

			if (BuildingTasksList.Count > 0) { //if the building actually has tasks:
				int i = 0;
				while (i < BuildingTasksList.Count) {
					bool TaskRemoved = false;
					//if the faction is controlled by the player in a single player or a multiplayer game:
					if (FactionID == GameManager.PlayerFactionID) {
						if (BuildingTasksList [i].FactionSpecific == true) { //if this task is faction type specific:
							if (BuildingTasksList [i].FactionCode != GameMgr.Factions [FactionID].Code) { //if the faction code does not match
								//remove this task and make it unavailable for the player:
								BuildingTasksList.RemoveAt(i);
								TaskRemoved = true;
							}
						}
					}
					if (TaskRemoved == false) {
						//loop through all the building's task
						if (BuildingTasksList [i].TaskType == BuildingTasks.CreateUnit && BuildingTasksList [i].UnitPrefab != null) {
							UnitCreationTasks.Add (i);
							//for the task that create units, add the task to a list depending on the unit's abilities:
							if (BuildingTasksList [i].UnitPrefab.gameObject.GetComponent<Attack> ()) {
								ArmyUnits.Add (i);
							}
							if (BuildingTasksList [i].UnitPrefab.gameObject.GetComponent<GatherResource> ()) {
								ResourceUnits.Add (i);
							}
							if (BuildingTasksList [i].UnitPrefab.gameObject.GetComponent<Builder> ()) {
								BuilderUnits.Add (i);
							}
						}
						i++;
					}
				}
			}

			//if the faction is NPC, alert the NPC Army manager that a new building has been added, possibility of creating of army units/units that the NPC spawner need:
			if (FactionID != GameManager.PlayerFactionID && PlacedByDefault == true && GameManager.MultiplayerGame == false) {
				//inform the NPC unit spawner:
				FactionMgr.UnitSpawner.ReloadBuildingLists (this, true);
				//inform the npc army:
				FactionMgr.ArmyMgr.ReloadArmyUnitsPriority (this, true);
			}
		}

		//Flashing building selection (when the player sends units to contruct a building, its texture flashes for some time):
		public void SelectionFlash ()
		{
			BuildingPlane.gameObject.SetActive (!BuildingPlane.activeInHierarchy);
		}

		//Cancel a task in progress:
		public void CancelInProgressTask (int ID)
		{
			//make sure the task ID is valid:
			if (TasksQueue.Count > ID && ID >= 0) {
				if (TasksQueue [ID].Upgrade == false) {
					//If it's a task that produces units, then make sure we empty a slot in the population count:
					if (BuildingTasksList [TasksQueue [ID].ID].TaskType == Building.BuildingTasks.CreateUnit) {
						UIMgr.GameMgr.Factions [FactionID].CurrentPopulation--; //update the population slots
						if (GameManager.PlayerFactionID == FactionID) {
							UIMgr.UpdatePopulationUI ();
						}
                        //update the limits list:
                        FactionMgr.DecrementLimitsList(BuildingTasksList[TasksQueue[ID].ID].UnitPrefab.Code);
					}
					ResourceMgr.GiveBackResources (BuildingTasksList [TasksQueue [ID].ID].RequiredResources, FactionID); //Give back the task resources.
				} else {
					ResourceMgr.GiveBackResources (BuildingTasksList [TasksQueue [ID].ID].Upgrades [BuildingTasksList [TasksQueue [ID].ID].CurrentUpgradeLevel].UpgradeResources, FactionID);
				}

				if (TasksQueue [ID].Upgrade == true || BuildingTasksList [TasksQueue [ID].ID].TaskType == Building.BuildingTasks.Research) {
					CheckTaskUpgrades (TasksQueue [ID].ID, false, true);
				}

				//custom events
				GameMgr.Events.OnTaskCanceled (this, BuildingTasksList [TasksQueue [ID].ID]);

				BuildingTasksList [TasksQueue [ID].ID].Active = false;
				TasksQueue.RemoveAt (ID);// Remove this task:

				if (ID == 0 && TasksQueue.Count > 0) {
					//If it's the first task in the queue, reload the timer for the next task:
					TaskQueueTimer = BuildingTasksList [TasksQueue [0].ID].ReloadTime;
				}

				UIMgr.UpdateBuildingTasks (UIMgr.SelectionMgr.SelectedBuilding);
				UIMgr.UpdateInProgressTasksUI (UIMgr.SelectionMgr.SelectedBuilding);

			}
		}

		//Placing the building:

		//Detecting collision with other objects when placing the building.
		void OnTriggerEnter (Collider other)
		{
			if (Placed == false) { //if the building is still not placed.
				if (other != Coll) {
					CollisionAmount += 1; //Counting how many colliders have entered in collision with the building.
				}
				//If the building isn't placed yet and it's in collision with another object
				PlaneRenderer.material.color = Color.red; //Show the player that the building can't be placed here.
				CanPlace = false; //The player can't place the building at this position.
			}
		}

		//If the building is no longer in collision with an object
		void OnTriggerExit (Collider other)
		{
			if (Placed == false) { //if the building has not been placed yet.
				if (other != Coll) {
					CollisionAmount -= 1; //Counting how many colliders have entered in collision with the building.
				}
				if (CollisionAmount <= 0) { //If the building isn't placed yet and it's in collision with another object
					CollisionAmount = 0;
                    PlaneRenderer.material.color = Color.green; //Show the player that the building can be placed here.
					CanPlace = true; //The player can place the building at this position.
				}
			}
		}

		//Building Selection:

		void OnMouseDown ()
		{
			if (PlayerSelection == null) { //If we're not another collider for player selection, then we'll use the same collider for placement.
				if (!EventSystem.current.IsPointerOverGameObject ()) { //Make sure that the mouse is not over any UI element
					if (Placed == true && BuildingPlacement.IsBuilding == false) {
						//Only select the building when it's already placed and when we are not attempting to place any building on the map:
						SelectionMgr.SelectBuilding (this);
					}
				}
			}
		}

		//Toggle construction object:
		public void ToggleConstructionObj (bool Toggle)
		{
			UpdateConstructionState ();

			//If we have an object to show as the building when it's under construction:
			if (ConstructionObj != null) {
                //hide or show the building's model:
                if (BuildingModel)
                    BuildingModel.SetActive(!Toggle);

				ConstructionObj.SetActive (Toggle); //hide or show the construction object.
			}
		}

        //called when the building is under construction and its health changed so the construction state must be updated
		public void UpdateConstructionState ()
		{
            //making sure the building is under construction and that there are actually construction states.
			if (IsBuilt == false && ConstructionStates.Length > 0) {

                //if there's a default construction object then hide it
				if (ConstructionObj != null) {
					ConstructionObj.SetActive (false);
				}

				bool Found = false;
				int i = 0;
                //go through all the construction states
				while (i < ConstructionStates.Length && Found == false) {
                    //when a state that includes the current building's health in its interval is found
					if (Health >= ConstructionStates [i].MinHealth && Health <= ConstructionStates[i].MaxHealth) {
                        //pick it
						ConstructionObj = ConstructionStates[i].ConstructionObj;
						ConstructionState = i;

						Found = true;
					}
					i++;
				}
			} 
		}

        //method called to init a faction building
        void OnFactionBuildingInit()
        {
            //get the faction manager:
            FactionMgr = GameMgr.Factions[FactionID].FactionMgr;

            //if it's not a MP game:
            if (GameManager.MultiplayerGame == true)
            {
                //If this building represents the faction's capital and the game manager still does not have the capital building set:
                if (FactionCapital == true && GameMgr.Factions[FactionID].CapitalBuilding == null)
                {
                    //if this building belongs to the local player
                    if (FactionID == GameManager.PlayerFactionID)
                    {

                        //make the camera look at this building at the beginning of the game.
                        CamMov.LookAt(transform.position);
                        CamMov.SetMiniMapCursorPos(transform.position); //set the minimap cursor to the correct position.
                    }
                }

                if (Placed == true || PlacedByDefault == true)
                { //if the building is already placed in the map (means it was spawned by the server).
                  //add the building to the list:
                    FactionMgr.AddBuildingToList(this);
                }
            }

            if (FactionCapital == true)
            { //if this is a faction capital
              //Make it so in the game manager:
                GameMgr.Factions[FactionID].CapitalBuilding = this;
                if (GameManager.MultiplayerGame == false && FactionID != GameManager.PlayerFactionID)
                {
                    GameMgr.Factions[FactionID].FactionMgr.BuildingMgr.CapitalBuilding = this;
                }
            }

            SetTaskTypes(); //Call a method that will arrange the building tasks by their types.

            //if the game is not multiplayer:
            if (GameManager.MultiplayerGame == false)
            {
                //if this building does not belong to the local player.
                if (FactionID != GameManager.PlayerFactionID)
                {

                    //If the building produces resource collectors, then we'll add it to the list below:
                    if (ResourceUnits.Count > 0)
                    {
                        FactionMgr.ResourceMgr.CollectorsCreators.Add(this);
                    }

                    //If the building produces builders, then we'll add it to the list below:
                    if (BuilderUnits.Count > 0)
                    {
                        FactionMgr.BuildingMgr.BuilderCreators.Add(this);
                    }
                }
            }


            //Set the faction color objects:
            SetBuildingColors();

            if (BuildingTasksList.Count > 0)
            {
                for (int i = 0; i < BuildingTasksList.Count; i++)
                {
                    TotalTasksAmount++;
                    if (BuildingTasksList[i].Upgrades.Length > 0)
                    {
                        TotalTasksAmount++;
                    }
                }
            }
        }

        //method called to set a faction building's colors:
        void SetBuildingColors ()
        {
            //If there's actually objects to color in this prefab:
            if (FactionColors.Length > 0)
            {
                //Loop through the faction color objects (the array is actually a MeshRenderer array because we want to allow only objects that include mesh renderers in this prefab):
                for (int i = 0; i < FactionColors.Length; i++)
                {
                    //Always checking if the object/material is not invalid:
                    if (FactionColors[i].Renderer != null)
                    {
                        //Color the object to the faction's color:
                        FactionColors[i].Renderer.materials[FactionColors[i].MaterialID].color = GameMgr.Factions[FactionID].FactionColor;
                    }
                }
            }
        }

        //method called to init a building that is not placed by default:
        void OnBuildingInit()
        {
            Health = 0; //health is set to 0.

            //if it's a multiplayer game:
            if (GameManager.MultiplayerGame == true)
            {
                if (Placed == true)
                { //if the building is already placed on the map:
                    BuildingPlane.SetActive(false); //hide the building's selection texture.
                    ToggleConstructionObj(true); //Show the construction object when the building is placed.

                    //And this building belongs to the local player:
                    if (GameManager.PlayerFactionID == FactionID)
                    {
                        //Then this is the player who placed this building on the map in the first player, while in single player, builders are moved directly after placing the building
                        //In multiplayer, the server handled placing the building so now we searched for the building's owner and we will send builders from his faction to construct it.
                        //Things to do for clients!
                        if (IsBuildingInRange() == true && CurrentCenter != null)
                        { //By checking if the building is inside the faction's border (which it should), we are simply searching for its current center
                            CurrentCenter.RegisterBuildingInBorder(this); //when the center is found then register the new building
                        }

                        //Make builders move to the building:
                        if (SelectionMgr.SelectedUnits.Count > 0)
                        {
                            //if the builders amount is over the maximum allowed builders
                            int Builders = SelectionMgr.SelectedUnits.Count;
                            if (Builders > WorkerMgr.WorkerPositions.Length - WorkerMgr.CurrentWorkers)
                            {
                                //then only send the maximum allowed amount
                                Builders = WorkerMgr.WorkerPositions.Length - WorkerMgr.CurrentWorkers;
                            }
                            for (int i = 0; i < Builders; i++)
                            {
                                //loop through all the builders and send them to construct this building.
                                if (SelectionMgr.SelectedUnits[i].BuilderMgr)
                                    SelectionMgr.SelectedUnits[i].BuilderMgr.SetTargetBuilding(this);
                            }
                        }

                    }
                }
            }
        }

        //method called to init a placed by default building:
        void OnDefaultBuildingInit()
        {
            //set the layer to IgnoreRaycast as we don't want any raycast to recongize this:
            gameObject.layer = 2;

            Placed = true; //If the building is placed by default is set true, then no need to place the building.
            IsBuilt = true; //ofc
            Health = MaxHealth; //set the health to maximum.

            //custom event:
            if (GameMgr.Events) GameMgr.Events.OnBuildingPlaced(this);
            if (GameMgr.Events) GameMgr.Events.OnBuildingBuilt(this);

            if (FreeBuilding == false)
            { //if this building is not a free building
                OnFactionBuildingBuilt(); //configure the faction building
            }
        }

        //method called to complete the construction of the building
        void CompleteConstruction ()
        {
            IsBuilt = true; //mark as built.

            //set the layer to IgnoreRaycast as we don't want any raycast to recongize this:
            gameObject.layer = 2;

            //custom event:
            if (GameMgr.Events) GameMgr.Events.OnBuildingBuilt(this);

            //If we have an object to show as the building when it's under construction:
            //hide it and shown the actual building:
            ToggleConstructionObj(false);

            //Check if the building is currently selected:
            if (SelectionMgr.SelectedBuilding == this)
            {
                //Update the selection UI:
                SelectionMgr.SelectBuilding(this);
            }

            if (FreeBuilding == false)
            {
                OnFactionBuildingBuilt();
            }
        }

        //method to configure a faction building:
        void OnFactionBuildingBuilt ()
        {
            //if the building includes the border component:
            if (BorderMgr)
            {
                //if the border is not active yet.
                if (BorderMgr.IsActive == false)
                {
                    //add the building to the building centers list (the list that includes all buildings wtih a border comp):
                    FactionMgr.BuildingCenters.Add(this);

                    //activate the border
                    BorderMgr.ActivateBorder();
                    CurrentCenter = BorderMgr; //make the building its own center.

                    //If the building belongs to a NPC player then we'll check for resources:
                    if (FactionID != GameManager.PlayerFactionID && GameManager.MultiplayerGame == false)
                    {
                        FactionMgr.ResourceMgr.CheckResources = true;
                    }
                }
            }

            //set the spawn position:
            if(SpawnPosition != null)
            {
                SpawnPos = new Vector3(SpawnPosition.position.x, TerrainMgr.SampleHeight(SpawnPosition.position));
            }

            //update the tasks to the current upgrade level:
            SyncTaskUpgradeLevel();

            //If the building belongs to a NPC player then we'll check for resources:
            if (FactionID != GameManager.PlayerFactionID && GameManager.MultiplayerGame == false)
            {
                FactionMgr.ResourceMgr.AllUnitsUpgraded = false;

                //if the faction is NPC, alert the NPC Army manager that a new building has been added, possibility of creating of army units/units that the NPC spawner need:
                if (FactionID != GameManager.PlayerFactionID)
                {
                    //inform the NPC army:
                    FactionMgr.ArmyMgr.ReloadArmyUnitsPriority(this, true);
                    //inform the NPC unit spawner:
                    FactionMgr.UnitSpawner.ReloadBuildingLists(this, true);
                }
            }

            if (ResourceDropOff == true)
            {
                //If this building allows resources to be dropped off at it, then add it to the list:
                FactionMgr.DropOffBuildings.Add(this);
                FactionMgr.CheckCollectorsDropOffBuilding();
            }

            //update the faction population slots.
            GameMgr.Factions[FactionID].MaxPopulation += AddPopulation;
            UIMgr.UpdatePopulationUI();

            SetResourceBonus(true); //apply the resource bonus

            //If the building has a goto position:
            if (GotoPosition != null)
            {
                //Check if the building is currently selected:
                if (SelectionMgr.SelectedBuilding == this)
                {
                    //Show the goto pos
                    GotoPosition.gameObject.SetActive(true);
                }
                else
                {
                    //hide the goto pos
                    GotoPosition.gameObject.SetActive(false);
                }
            }
        }

		//Building health:

		public void AddHealthLocal (float Value, GameObject Source) //changes the health for the local player.
		{
			if (Placed == false) { //if the building is not placed, we have nothing to do.
				return;
			}

			Health += Value; //add the requested value to the building's health
			if (Health >= MaxHealth) { //if the building has reached the max health:
				Health = MaxHealth;

                //go through all worker positions and look for all workers
                for (int i = 0; i < WorkerMgr.WorkerPositions.Length; i++)
                {
                    if (WorkerMgr.WorkerPositions[i].CurrentUnit != null) //if this is a avalid worker
                    {
                        WorkerMgr.WorkerPositions[i].CurrentUnit.CancelBuilding();
                    }
                }

                if (IsBuilt == false) { //if the building was being constructed for the time:

                    CompleteConstruction();
				}
			}

            //if the mouse is over this building
            if (UIMgr.IsHoverSource(PlayerSelection))
            {
                //update hover health bar:
                UIMgr.UpdateHoverHealthBar(Health, MaxHealth);
            }

            //Updating the building's health UI when it's selected:
            if (SelectionMgr.SelectedBuilding == this)
            {
                //only update the UI health when the actual building health has changed.
                UIMgr.UpdateBuildingHealthUI(this);
            }

            //If the building receives damage:
            if (Value < 0.0f) {
				if (FreeBuilding == false) { //if it's not a free building
					//If the building belongs to an AI player:
					if (FactionID != GameManager.PlayerFactionID) {
						if (GameManager.MultiplayerGame == false) { //single player game.
							//Call workers:
							FactionMgr.BuildingMgr.CheckBuildings = true;
							//If the source of the damage is known:
							if (Source != null) {
								int SourceFactionID = -1;
								if (Source.GetComponent<Unit> ()) {
									//If the source of the attack is a unit.
									SourceFactionID = Source.GetComponent<Unit> ().FactionID;
								}

								if (CurrentCenter != null && SourceFactionID >= 0) {

									FactionMgr.ArmyMgr.UnderAttack = true;
									//Reload the army gathering timer so that the target faction could prepare itself for the attack:
									FactionMgr.ArmyMgr.CheckArmyTimer = -1.0f;
									FactionMgr.ArmyMgr.SetDefenseCenter (CurrentCenter.gameObject.GetComponent<Building> (), SourceFactionID);
								}
							}

						}
					}
                    else
                    { //local player:
                      //show warning in mini map to let player know that he is getting attacked.
                        AttackWarningMgr.AddAttackWarning(this.gameObject);
                    }
                }
			}

			//if the building's health is null and the building has not been destroyed.
			if (Health <= 0 ) {
				Health = 0.0f;

				if (Destroyed == false) { 

					//award the destroy award to the source:
					if(DestroyAward.Length > 0)
					{
						if (Source != null) {
							//get the source faction ID:
							int SourceFactionID = -1;

							if (Source.gameObject.GetComponent<Unit> ()) {
								SourceFactionID = Source.gameObject.GetComponent<Unit> ().FactionID;
							} else if (Source.gameObject.GetComponent<Building> ()) {
								SourceFactionID = Source.gameObject.GetComponent<Building> ().FactionID;
							}

							//if the source is not the same faction ID:
							if (SourceFactionID != FactionID) {
								for (int i = 0; i < DestroyAward.Length; i++) {
									//award destroy resources to source:
									GameMgr.ResourceMgr.AddResource (SourceFactionID, DestroyAward [i].Name, DestroyAward [i].Amount);
								}
							}
						}
					}

					//destroy the building
					DestroyBuilding (false);
					Destroyed = true;
				}

			} 
			else 
			{
				//Check the building's state: 
				//only if it has been built:
				if (IsBuilt == true) {
					CheckBuildingState ();
				} else {
					if (ConstructionStates.Length > 0) { //if the building uses construction states:
						if (ConstructionState < ConstructionStates.Length) { //check if we have already changed the state:
							if (ConstructionState >= 0) {
								if (Health > ConstructionStates [ConstructionState].MaxHealth) {
									ToggleConstructionObj (true);
								}
							} else {
								ToggleConstructionObj (true);
							}
						}

					}
				}
				if (GameManager.MultiplayerGame == false) { //if it's a single player game and this is not a free building
					if (Health < MinTaskHealth && FactionID != GameManager.PlayerFactionID && FreeBuilding == false) { 
						FactionMgr.BuildingMgr.CheckBuildings = true; //Informing the building manager that we need fix this building.
					}
				}
			}
		}

        //add health to the building.
        public void AddHealth(float Value, GameObject Source)
        {
            //AddHealthLocal (Value, Source);

            if (GameManager.MultiplayerGame == false)
            {
                AddHealthLocal(Value, Source);
            }
            else
            {
                if (GameMgr.IsLocalPlayer(gameObject))
                {
                    //Sync the building's health with everyone in the network

                    //send input action to the input manager
                    InputVars NewInputAction = new InputVars();
                    //mode:
                    NewInputAction.SourceMode = (byte)InputSourceMode.Building;
                    NewInputAction.TargetMode = (byte)InputTargetMode.Self;

                    NewInputAction.Source = gameObject;
                    NewInputAction.Target = Source;
                    NewInputAction.InitialPos = transform.position;

                    NewInputAction.Value = (int)Value;

                    InputManager.SendInput(NewInputAction);
                }
            }
        }

		//a method that handles the building destruction.
		public void DestroyBuilding(bool Upgrade)
		{
			//if it's a single player game, destroy the building locally, simple.
			if (GameManager.MultiplayerGame == false) {
				DestroyBuildingLocal (Upgrade);
			} else {
                //if it's a MP game:
                if (GameMgr.IsLocalPlayer(gameObject))
                {
                    //ask the server to destroy the building
                    InputVars NewInputAction = new InputVars();
                    //mode:
                    NewInputAction.SourceMode = (byte)InputSourceMode.Destroy;

                    NewInputAction.Source = gameObject;
                    NewInputAction.Value = (Upgrade == true) ? 1 : 0; //when upgrade == true, then set to 1. if not set to 0

                    InputManager.SendInput(NewInputAction);
                }
            }
		}

		public void DestroyBuildingLocal (bool Upgrade)
		{
			int i = 0;
            //Destroy building:

            //if this is the current source of the hover health bar:
            if (UIMgr.IsHoverSource(PlayerSelection))
            {
                UIMgr.TriggerHoverHealthBar(false, PlayerSelection, 0.0f);
            }


            //If this building is selected then deselect it:
            if (SelectionMgr.SelectedBuilding == this) {
				SelectionMgr.DeselectBuilding ();
			}

			if (FreeBuilding == false) {
                if (APCMgr)
                {
                    //if the unit is an APC vehicle:
                    while (APCMgr.CurrentUnits.Count > 0)
                    { //go through all units
                      //release all units:
                        if (APCMgr.ReleaseOnDestroy == true)
                        { //release on destroy:
                            APCMgr.RemoveUnit(APCMgr.CurrentUnits[0]);
                        }
                        else
                        {
                            //destroy contained units:
                            APCMgr.CurrentUnits[0].DestroyUnit();
                            APCMgr.CurrentUnits.RemoveAt(0);
                        }
                    }
                }

                //If the building is considered as a center (defines borders)

                if (BorderMgr) {

					if (GameManager.MultiplayerGame == false || (GameManager.MultiplayerGame == true && FactionID == GameManager.PlayerFactionID)) {
						//Free all the resources inside this border so other:
						if (BorderMgr.ResourcesInRange.Count > 0) {
							for (i = 0; i < BorderMgr.ResourcesInRange.Count; i++) {
                                BorderMgr.ResourcesInRange [i].FactionID = -1;
							}
						}
                        BorderMgr.ResourcesInRange.Clear ();

						//Go through all the borders' centers to refresh the resources inside this border (as one of the freed resources above could now belong to another center):
						for (i = 0; i < GameMgr.AllBorders.Count; i++) {
							//Loop through all the borders while respecting their priority order:
							GameMgr.AllBorders [i].CheckBorderResources ();
						}

						//Remove the border from the all borders list if it has been already activated.
						if (GameMgr.AllBorders.Contains (BorderMgr)) {
							GameMgr.AllBorders.Remove (BorderMgr);
						}

						//Remove the building from the building centers list in the faction manager:
						if (FactionMgr.BuildingCenters.Contains (this)) {
							FactionMgr.BuildingCenters.Remove (this);
						}

						//Remove the building from the resource drop off building lists if it's there:
						if (FactionMgr.DropOffBuildings.Contains (this)) {
							FactionMgr.DropOffBuildings.Remove (this);
						}
					}

					//Destroy the border object.
					if (BorderMgr.IsActive == true && BorderMgr.SpawnBorderObj == true) {
						Destroy (BorderMgr.BorderObj);
					}
				} else {
					if (GameManager.MultiplayerGame == false || (GameManager.MultiplayerGame == true && FactionID == GameManager.PlayerFactionID)) {
						//If the building is not a center then we'll check if it occupies a place in the defined buildings for its center:
						if (CurrentCenter != null) {
							CurrentCenter.UnegisterBuildingInBorder (this);
						}
					}
				}

				if (GameManager.MultiplayerGame == false) {
					//If the building is controlled by NPC:
					if (FactionID != GameManager.PlayerFactionID) {
						//If there were pending units to be created for the NPC army:
						if (PendingUnitsToCreate > 0) {
							if (PendingUnitsArmyID >= 0 && FactionMgr.ArmyMgr.ArmyUnits.Length > PendingUnitsArmyID) {
								//Remove from the pending units amount so that the manager won't wait for nothing!
								FactionMgr.ArmyMgr.ArmyUnits [PendingUnitsArmyID].ProgressAmount -= PendingUnitsToCreate;
								if (FactionMgr.ArmyMgr.ArmyUnits [PendingUnitsArmyID].ProgressAmount < 0) {
									FactionMgr.ArmyMgr.ArmyUnits [PendingUnitsArmyID].ProgressAmount = 0;
									//just in case smth weird happens, we reset the progress amount because we can't have it below zero.
								}
							}
						}

                        //alert the NPC Army manager that a building has been destroyed , possibility of creating of army units may be decreasd:
                        FactionMgr.ArmyMgr.ReloadArmyUnitsPriority(this, false);
                        //inform the NPC unit spawner:
                        FactionMgr.UnitSpawner.ReloadBuildingLists(this, false);

                        //If the building belongs to one of the lists below, remove it from them:
                        if (FactionMgr.BuildingMgr.BuilderCreators.Contains (this)) {
							FactionMgr.BuildingMgr.BuilderCreators.Remove (this);
						}
						if (FactionMgr.ResourceMgr.CollectorsCreators.Contains (this)) {
							FactionMgr.ResourceMgr.CollectorsCreators.Remove (this);
						}

                        //if the building is a resource drop off building:
                        if(ResourceDropOff == true)
                        {
                            //then let the resource manager know that the faction needs to check whether we need to build another one or not:
                            FactionMgr.ResourceMgr.DropOffControlDone = false;
                        }
					}
				}
				if (GameManager.MultiplayerGame == false || GameMgr.IsLocalPlayer(gameObject)) {
					GameMgr.Factions [FactionID].MaxPopulation -= AddPopulation; //remove population added by this building when destroyed

					//Remove the building from the spawned buildings list in the faction manager:
					if (FactionMgr.Buildings.Contains (this)) {
						FactionMgr.RemoveBuilding (this);
					}

					//If there are pending tasks, stop them and give the faction back the resources of these tasks:
					if (TasksQueue.Count > 0) {
						int j = 0;
						while (j < TasksQueue.Count) {
							CancelInProgressTask (TasksQueue [j].ID);
						}
					}

					//Clear all the pending tasks.
					TasksQueue.Clear ();

					//Remove bonuses from nearby resources:
					SetResourceBonus (false);

					//Check if it's the capital building:
					if (FactionCapital == true && Upgrade == false) {

						if (GameManager.MultiplayerGame == false) { 

                            //Mark the faction as defeated:
                            GameMgr.OnFactionDefeated(FactionID);
                        }
                        else
                        {
                            //ask the server to announce that the faction has been defeated.
                            //send input action to the input manager
                            InputVars NewInputAction = new InputVars();
                            //mode:
                            NewInputAction.SourceMode = (byte)InputSourceMode.Destroy;
                            NewInputAction.TargetMode = (byte)InputTargetMode.Faction;

                            NewInputAction.Value = FactionID;

                            InputManager.SendInput(NewInputAction);
                        }
                    }
				}
				//Spawn the destruction effect obj if it exists:
				if (DestructionObj != null && Upgrade == false) {
					GameObject DestructionObjClone = (GameObject)Instantiate (DestructionObj.gameObject, transform.position, DestructionObj.transform.rotation);

					//Building destruction sound effect:
					if (DestructionAudio != null) {
						//Add an audio source for the desctruction object if it does not have one already:
						if (DestructionObjClone.GetComponent<AudioSource> () == null) {
							DestructionObjClone.AddComponent<AudioSource> ();
						}
						//Play the destruction:
						AudioManager.PlayAudio (DestructionObjClone, DestructionAudio, false);
					}
				}

			}

            //remove the minimap icon
            MinimapIconMgr.RemoveMinimapIcon(PlayerSelection);

            if (Upgrade == false) {
				//custom event:
				if (GameMgr.Events)
					GameMgr.Events.OnBuildingDestroyed (this);
			}

            //Destroy the building's object:
            Destroy(gameObject);

        }

		//This checks if the building is inside the faction's borders:
		public bool IsBuildingInRange ()
		{
			if (GameMgr.BuildingMgr.BuildingsInsideBorders == false && FactionID == GameManager.PlayerFactionID) {
				return true;
			}

			float Distance = 0.0f;
			bool InRange = false;
			float AddSize = 0.0f;
			int i = 0;

			//if the building we're checking is a center and this is either a NPC faction or a local player faction while the min distance is forced
			if (Code == GameMgr.Factions [FactionID].CapitalBuilding.Code && (ForceMinDistanceOnPlayer == true || FactionID != GameManager.PlayerFactionID)) {
				if (FactionMgr.BuildingCenters.Count > 0) {
					//We need to make sure that the building center is not near any other existing building center:
					i = 0;
					while (i < FactionMgr.BuildingCenters.Count) {
						if(FactionMgr.BuildingCenters [i].BorderMgr.IsActive == true)
						{
							//If the building center gets too close to another spawned building center:
							Distance = Vector3.Distance (FactionMgr.BuildingCenters [i].transform.position, this.transform.position);
							if (Distance < MinCenterDistance) {
								return(false);
							}
						}


						i++;
					}

				}
			} 

			//First we check if the building is still inside its last noted building center's borders:
			if (CurrentCenter != null) {

				if (Vector3.Distance (CurrentCenter.transform.position, this.transform.position) <= CurrentCenter.Size + AddSize) {
                    //check if the building complies again with the chosen minimum center distance or if this is the local player and that distance is not forced on the player
                    if (Vector3.Distance(CurrentCenter.transform.position, this.transform.position) > MinCenterDistance || (FactionID == GameManager.PlayerFactionID && ForceMinDistanceOnPlayer == false))
                    {
                        InRange = true;
                    }
					//If yes, our work is done here, if not we will check if it can be inside another building center or not.
				} else {
					InRange = false;
					CurrentCenter = null;
				}
			}

			if (CurrentCenter == null) {
				if (FactionMgr.BuildingCenters.Count > 0) {
					//We have to start by checking the first city center, if the building is not inside its border, then we will move to the next ones:
					Distance = Vector3.Distance (FactionMgr.BuildingCenters [0].transform.position, this.transform.position);
					if (Distance <= FactionMgr.BuildingCenters [0].BorderMgr.Size + AddSize && FactionMgr.BuildingCenters [0].BorderMgr.IsActive == true) {
						//If we are allowed to place this building inside this border:
						if (FactionMgr.BuildingCenters [0].BorderMgr.AllowBuildingInBorder (Code) == true) {
							//If the current building is inside the center
							InRange = true;
							CurrentCenter = FactionMgr.BuildingCenters [0].BorderMgr;
						}
					}
					if (FactionMgr.BuildingCenters.Count > 1) {
						i = 1;
						while (InRange == false && i < FactionMgr.BuildingCenters.Count) {
							Distance = Vector3.Distance (FactionMgr.BuildingCenters [i].transform.position, this.transform.position);
							if (Distance <= FactionMgr.BuildingCenters [i].BorderMgr.Size + AddSize && FactionMgr.BuildingCenters [i].BorderMgr.IsActive == true) {
								//If we are allowed to place this building inside this border:
								if (FactionMgr.BuildingCenters [i].BorderMgr.AllowBuildingInBorder (Code) == true) {
									//If the current building is inside the center
									InRange = true;
									CurrentCenter = FactionMgr.BuildingCenters [i].BorderMgr;
								}
							}

							i++;
						}
					}
				}
			}


			if (CurrentCenter != null) {
				//Sometimes borders collide with each other but the priority of the border is made by order of creation of the border.
				//That's why we need to check for other factions' borders and make sure the building isn't inside one of them:

				i = 0;
				//So loop through all borders:
				while (i < GameMgr.AllBorders.Count && InRange == true) {
					//Make sure the border is active:
					if (GameMgr.AllBorders [i].IsActive == true) {
						//Make sure the border doesn't belong to this faction:
						if (GameMgr.AllBorders [i].FactionMgr.FactionID != FactionID) {

							//Calculate the distance between this building and the building center the holds the border:
							Distance = Vector3.Distance (GameMgr.AllBorders [i].transform.position, this.transform.position);
							//Check if the building is inside the border:
							if (Distance <= GameMgr.AllBorders [i].Size) {
								//See if the border has a priority over the one that the building belongs to:
								if (GameMgr.AllBorders [i].BorderObj.gameObject.GetComponent<MeshRenderer> ().sortingOrder > CurrentCenter.BorderObj.gameObject.GetComponent<MeshRenderer> ().sortingOrder) {
									InRange = false; //Cancel placing the building here.
								}
							}
						}
					}
					i++;
				}

			}

			if (CurrentCenter == null)
				InRange = false;

			return InRange;
		}

        //check if the building is still on the map
		public bool IsBuildingOnMap ()
		{
			bool OnMap = true; //Are all four corners and center of the building on the map?

			Ray RayCheck = new Ray ();
			RaycastHit[] Hits;

			//Get the main box collider of the building:
			BoxCollider Coll = gameObject.GetComponent<BoxCollider>();

			//Start by checking if the middle point of the building's collider is over the map:

			//Set the ray check source point which is the center of the collider in the world:
			RayCheck.origin = new Vector3(transform.position.x+Coll.center.x, transform.position.y+0.5f, transform.position.z+Coll.center.z);

			//The direction of the ray is always down because we want check if there's terrain right under the building's object:
			RayCheck.direction = Vector3.down;

			int PointID = 1;
			while (OnMap == true && PointID <= 5) {
				Hits = Physics.RaycastAll (RayCheck, 1.5f);
				bool HitTerrain = false;
				if (Hits.Length > 0) {
					for (int i = 0; i < Hits.Length; i++) {
						int TerrainLayer = LayerMask.NameToLayer ("Terrain");
						if (Hits [i].transform.gameObject.layer == TerrainLayer) {
							HitTerrain = true;
						}
					}
				}

				if (HitTerrain == false) {
					OnMap = false;
					return OnMap;
				}

				PointID++;

				//If we reached this stage, then while checking the last, we successfully detected that there a terrain under it, so we'll move to the next point:
				switch (PointID) {

				case 2:
					RayCheck.origin = new Vector3(transform.position.x+Coll.center.x+Coll.size.x/2, transform.position.y+0.5f, transform.position.z+Coll.center.z+Coll.size.z/2);
					break;
				case 3:
					RayCheck.origin = new Vector3(transform.position.x+Coll.center.x+Coll.size.x/2, transform.position.y+0.5f, transform.position.z+Coll.center.z-Coll.size.z/2);
					break;
				case 4:
					RayCheck.origin = new Vector3(transform.position.x+Coll.center.x-Coll.size.x/2, transform.position.y+0.5f, transform.position.z+Coll.center.z-Coll.size.z/2);
					break;
				case 5:
					RayCheck.origin = new Vector3(transform.position.x+Coll.center.x-Coll.size.x/2, transform.position.y+0.5f, transform.position.z+Coll.center.z+Coll.size.z/2);
					break;
				}
			}
			return OnMap;
		}

		//This method checks if the building is near a specific resource that it's supposed to built in range of
		public bool IsBuildingNearResource ()
		{
			if (PlaceNearResource == false) { //if we can place the building free from being in range of a resource
				return true;
			} else {
				//search for resources in range:
				foreach (Resource Resource in ResourceMgr.AllResources) {
					//make sure this is the resource that the building needs:
					if (Resource.Name == ResourceName) {
						if (Vector3.Distance (Resource.transform.position, this.transform.position) < ResourceRange) { //resource is in range
							return true; //found it
						}
					}
				}
			}
			//return false in case we reach this stage (did not find a resource)
			return false;
		}

		//This method allows to show/hide parts of the building depending on the 
		public void CheckBuildingState ()
		{
			//Only set the building state when the health is not maximal:
			if (Health < MaxHealth) {
				//If there are actually building states:
				if (BuildingStates.Length > 0) {
					//Check if we're not in the same state building...
					if (BuildingStateID >= 0 && BuildingStateID < BuildingStates.Length) {
						//...by checking if the building's health is not in the last state interval:
						if (Health < BuildingStates [BuildingStateID].MinHealth || Health > BuildingStates [BuildingStateID].MaxHealth) {
							//Look for a new building state:
							UpdateBuildingState ();
						}
					} else {
						//No valid building state ID was found then look for a valid one:
						UpdateBuildingState ();
					}
				}
			} else {
				//The building has maximum health so update its state:
				UpdateBuildingState ();
			}
		}

        //update the building's state
		public void UpdateBuildingState ()
		{
			int i = 0, j = 0;
			if (BuildingStateID >= 0 && BuildingStateID < BuildingStates.Length) {
				//First hide the parts that were shown in the last state:
				if (BuildingStates[BuildingStateID].PartsToShow.Length > 0) {
					for (i = 0; i < BuildingStates[BuildingStateID].PartsToShow.Length; i++) {
						BuildingStates[BuildingStateID].PartsToShow [i].SetActive (false);
					}
				}
				//and show the parts that were hidden in the last state:
				if (BuildingStates[BuildingStateID].PartsToHide.Length > 0) {
					for (i = 0; i < BuildingStates[BuildingStateID].PartsToHide.Length; i++) {
						BuildingStates[BuildingStateID].PartsToHide [i].SetActive (true);
					}
				}
			}

			//Then move to a new state only if the maximum health has not been reached:
			if (Health < MaxHealth) {
				while (i < BuildingStates.Length) {
					//Check if the current building health is in the interval of this building state:
					if (Health > BuildingStates [i].MinHealth && Health < BuildingStates [i].MaxHealth) {
						//Update the building state to this one:
						//Hide some parts:
						if (BuildingStates [i].PartsToHide.Length > 0) {
							for (j = 0; j < BuildingStates [i].PartsToHide.Length; j++) {
								BuildingStates [i].PartsToHide [j].SetActive (false);
							}
						}

						//and show some others:
						if (BuildingStates [i].PartsToShow.Length > 0) {
							for (j = 0; j < BuildingStates [i].PartsToShow.Length; j++) {
								BuildingStates [i].PartsToShow [j].SetActive (true);
							}
						}

						BuildingStateID = i;

						return;


					}
					i++;
				}
			} else {
				//Reset the building state ID when the maximum health has been reached:
				BuildingStateID = -1;
			}
		}

		//a method that checks if all the buildings (from the same code) are be in the same task upgrade level:
		public void CheckTaskUpgrades (int TaskID, bool Pending, bool Canceled)
		{
			if (FactionMgr.Buildings.Count > 0) {
				//loop through all the faction's buildings:
				for (int i = 0; i < FactionMgr.Buildings.Count; i++) {
					//find buildings with similar codes:
					if (FactionMgr.Buildings [i].Code == Code && FactionMgr.Buildings[i] != this) {
						//apply the same tasks' upgrade state:
						if (Canceled == false) {
							if (Pending == true) {
								FactionMgr.Buildings [i].BuildingTasksList [TaskID].Active = true;
							} else {
								if (FactionMgr.Buildings [i].BuildingTasksList [TaskID].TaskType == BuildingTasks.Research) {
									FactionMgr.Buildings [i].BuildingTasksList [TaskID].Reached = true;
								} else {
									FactionMgr.Buildings [i].UpgradeTask (TaskID, BuildingTasksList [TaskID].CurrentUpgradeLevel);
								}
							}
						} else {
							FactionMgr.Buildings [i].BuildingTasksList [TaskID].Active = false;
						}
					}
				}
			}
		}

		//upgrade a task:
		public void UpgradeTask (int TaskID, int TargetLevel)
		{
			//set the upgraded task settings:
			BuildingTasksList [TaskID].UnitPrefab = BuildingTasksList [TaskID].Upgrades [TargetLevel].TargetUnit;
			BuildingTasksList [TaskID].TaskIcon = BuildingTasksList [TaskID].Upgrades [TargetLevel].NewTaskIcon;
			BuildingTasksList [TaskID].Description = BuildingTasksList [TaskID].Upgrades [TargetLevel].NewTaskDescription;
			BuildingTasksList [TaskID].ReloadTime = BuildingTasksList [TaskID].Upgrades [TargetLevel].NewReloadTime;

			if (BuildingTasksList [TaskID].Upgrades [TargetLevel].NewTaskResources.Length > 0) {
				BuildingTasksList [TaskID].RequiredResources = BuildingTasksList [TaskID].Upgrades [TargetLevel].NewTaskResources;

			}

			//move to the next upgrade level:
			BuildingTasksList [TaskID].CurrentUpgradeLevel = TargetLevel+1;
			BuildingTasksList [TaskID].Active = false;

			if (BuildingTasksList [TaskID].Upgrades.Length == TargetLevel) { //if this is the last upgrade
				TotalTasksAmount--; //then decrease the amount of the total tasks amount
			}
		}


		//sync the building's upgrade level when it's spawned:
		public void SyncTaskUpgradeLevel ()
		{
			//if the building has tasks:
			if (BuildingTasksList.Count > 0) {
				if (FactionMgr.Buildings.Count > 0) {
					//loop through all the faction's buildings:
					for (int i = 0; i < FactionMgr.Buildings.Count; i++) {
						//find buildings with similar codes:
						if (FactionMgr.Buildings [i].Code == Code && FactionMgr.Buildings [i] != this) {
							for (int j = 0; j < BuildingTasksList.Count; j++) {
								if (BuildingTasksList [j].TaskType == BuildingTasks.CreateUnit) { //if the task produces units.
									if (BuildingTasksList [j].Upgrades.Length > 0 && BuildingTasksList [j].CurrentUpgradeLevel < FactionMgr.Buildings [i].BuildingTasksList [j].CurrentUpgradeLevel) {
										UpgradeTask (j, FactionMgr.Buildings [i].BuildingTasksList [j].CurrentUpgradeLevel - 1); //upgrade the task to sync it.
									}
								} else if (BuildingTasksList [j].TaskType == BuildingTasks.Research) {
									if (FactionMgr.Buildings [i].BuildingTasksList [j].Reached == true) {
										BuildingTasksList [j].Active = true;
										BuildingTasksList [j].Reached = true;
									}
								}
							}

							return;
						}
					}
				}
			}
		}

		//Launch building upgrade:
		public void CheckBuildingUpgrade ()
		{
			if (UpgradeBuilding == null) {
				return;
			}

			if (FactionMgr == null) {
				FactionMgr = GameMgr.Factions [FactionID].FactionMgr;
			}

			float Ratio = 1.0f;
			if (FactionID != GameManager.PlayerFactionID) {
				Ratio = GameMgr.Factions [FactionID].FactionMgr.ResourceMgr.BuildingUpgradeResourceRatio;
			}
			if (ResourceMgr.CheckResources (BuildingUpgradeResources, FactionID, Ratio)) { //if the faction has the required resources to upgrade the building.
				//check if the required buildings are spawned:
				if (FactionMgr.AreBuildingsSpawned (UpgradeRequiredBuildings)) {
					if (TasksQueue.Count == 0) { //if there are no pending quests.
						BuildingUpgrading = true; //launch the upgrade timer:
						BuildingUpgradeTimer = BuildingUpgradeReload;

						ResourceMgr.TakeResources (BuildingUpgradeResources, FactionID); //take resources.
						//custom event:
						if (GameMgr.Events)
							GameMgr.Events.OnBuildingStartUpgrade (this, true);

						//show the UI (if it's the local player):
						if (FactionID == GameManager.PlayerFactionID) {
							if (SelectionMgr.SelectedBuilding == this) {
								UIMgr.UpdateBuildingUI (this);
                                UIMgr.HideTooltip();
							}
						}

					} else {
						if (FactionID == GameManager.PlayerFactionID) {
							UIMgr.ShowPlayerMessage ("The building must have no pending tasks to upgrade!", UIManager.MessageTypes.Error);
						}
					}
				} else {
					if (FactionID == GameManager.PlayerFactionID) {
						UIMgr.ShowPlayerMessage ("Not all required buildings for upgrade are built!", UIManager.MessageTypes.Error);
					}
				}
			} else {
				if (FactionID == GameManager.PlayerFactionID) {
					UIMgr.ShowPlayerMessage ("You don't have enough resources to upgrade the building!", UIManager.MessageTypes.Error);
				}
			}

		}

        //launching a building upgrade is done here
		public void LaunchBuildingUpgrade (bool Direct)
		{
			if (UpgradeBuildingEffect != null) { //the upgrade effect:
				Instantiate(UpgradeBuildingEffect, this.transform.position, UpgradeBuildingEffect.transform.rotation);
			}
			BuildingUpgrading = false; //we are not upgrading the building anymore.
			//save the upgrade prefab:
			Building UpradePrefab = UpgradeBuilding;

			//if this triggers a full buildings upgrade and this has been called directly (directly means that this has been called by the player himself by clicking on the UI button);
			if (UpgradeAllBuildings == true && Direct == true) {
				//convert all buildings inside the buildings list in the placement manager if it's the local player:
				if (FactionID == GameManager.PlayerFactionID) {
					for (int j = 0; j < GameMgr.BuildingMgr.AllBuildings.Count; j++) {
						if (GameMgr.BuildingMgr.AllBuildings [j].UpgradeBuilding != null) {
							//set the new building in the placement manager:
							GameMgr.BuildingMgr.ReplaceBuilding (GameMgr.BuildingMgr.AllBuildings [j].Code, GameMgr.BuildingMgr.AllBuildings [j].UpgradeBuilding);
						}
					}
				} else {
					for (int j = 0; j < FactionMgr.BuildingMgr.AllBuildings.Count; j++) {
						if (FactionMgr.BuildingMgr.AllBuildings [j].UpgradeBuilding != null) {
							//set the new building in the placement manager:
							FactionMgr.BuildingMgr.ReplaceBuilding (FactionMgr.BuildingMgr.AllBuildings [j].Code, FactionMgr.BuildingMgr.AllBuildings [j].UpgradeBuilding);
						}
					}
				}

				//save a list of the faction's buildings (because it will change in the process below):
				List<Building> OldBuildings = new List<Building> ();
				OldBuildings.AddRange (FactionMgr.Buildings);
				//go through all the faction's buildings
				int i = 0;
				while (i < OldBuildings.Count) {
					if (OldBuildings [i] != null && OldBuildings [i] != this) { //make sure there's a building and that it's not this one
						if (OldBuildings [i].UpgradeBuilding != null) {
							if (OldBuildings [i].IsBuilt == true) { //if the buildng is built
								OldBuildings [i].LaunchBuildingUpgrade (false);
								OldBuildings.RemoveAt (i);
							} else {
								OldBuildings [i].BuildingUpgrading = true; //launch the upgrade timer:
								OldBuildings [i].BuildingUpgradeTimer = -1.0f; //-1 means that the next upgrade is ordered by another building
								i++;
							}
						} else {
							i++;
						}
					} else {
						i++;
					}
				}

			}

			//Upgrade building:
			if (GameManager.MultiplayerGame == false) {  //if it's a single player game
				UpgradeBuilding = Instantiate (UpgradeBuilding.gameObject, this.transform.position, UpgradeBuilding.transform.rotation).GetComponent<Building> (); //spawn the new building directly
				UpgradeBuilding.PlacedByDefault = true; //so that it won't have to be built.

				if (SelectionMgr.SelectedBuilding == this) { //if the previous building was selected
					SelectionMgr.SelectBuilding (UpgradeBuilding); //select the new one.
				}
				//make sure to pass the configurations of the last building:
				UpgradeBuilding.FactionCapital = FactionCapital; //if it is a faction capital
				UpgradeBuilding.FactionID = FactionID; //pass the faction ID
				FactionMgr.AddBuildingToList(UpgradeBuilding); //add the building to the faction manager's list
			} else {
                //if it's a MP game, then ask the server to spawn it.
                //send input action to the input manager
                InputVars NewInputAction = new InputVars();
                //mode:
                NewInputAction.SourceMode = (byte)InputSourceMode.Create;

                NewInputAction.Source = UpgradeBuilding.gameObject;
                NewInputAction.Value = 3;

                NewInputAction.InitialPos = this.transform.position;

                InputManager.SendInput(NewInputAction);
            }

			//if the building to upgrade is a current center (it has a border comp):
			if (BorderMgr) {
				UpgradeBuilding.gameObject.GetComponent<Border> ().ResourcesInRange = CurrentCenter.ResourcesInRange; //pass the resources inside the border to the new upgraded building
				if (CurrentCenter.BuildingsInRange.Count > 0) { //pass the buildings list inside the border to the upgraded one.
					for (int i = 0; i < CurrentCenter.BuildingsInRange.Count; i++) {
						CurrentCenter.BuildingsInRange [i].CurrentCenter = UpgradeBuilding.GetComponent<Border> (); //change the centers of the buildings inside the border's range
						UpgradeBuilding.GetComponent<Border> ().RegisterBuildingInBorder (CurrentCenter.BuildingsInRange [i]); //register buildings inside the new upgraded border
					}
				}
			}

			//if this faction does not upgrade all building and the upgrade is direct
			if(Direct == true && UpgradeAllBuildings == false) {
				//if this is the local player's faction
				if (FactionID == GameManager.PlayerFactionID) {
					//set the new building in the placement manager:
					GameMgr.BuildingMgr.ReplaceBuilding (Code, UpradePrefab);
				} else {
					FactionMgr.BuildingMgr.ReplaceBuilding (Code, UpradePrefab);
				}
			}

			//custom event:
			if(GameMgr.Events) GameMgr.Events.OnBuildingCompleteUpgrade(this, Direct);

			DestroyBuilding (true);

		}

        //a method to launch a research task on this building:
        public void LaunchResearchTask(int ID)
        {
            if (GameManager.MultiplayerGame == true)
            { //if this is a MP game and it's the local player:
                if (GameMgr.IsLocalPlayer(gameObject))
                { //just checking if this is a local player:
                  //send input action to the input manager
                    InputVars NewInputAction = new InputVars();
                    //mode:
                    NewInputAction.SourceMode = (byte)InputSourceMode.CustomCommand;
                    NewInputAction.TargetMode = (byte)InputCustomMode.BuildingResearch;

                    NewInputAction.Source = gameObject;
                    NewInputAction.Value = ID;

                    InputManager.SendInput(NewInputAction);
                }
            }
            else
            {
                //offline game? update the attack type directly:
                LaunchResearchTaskLocal(ID);
            }
        }

        public void LaunchResearchTaskLocal (int ID)
		{
			if (BuildingTasksList [ID].UnitList.Length > 0) { //if there are actually units to upgrade:
				for (int n = 0; n < BuildingTasksList [ID].UnitList.Length; n++) {
					//do the upgrades:
					//register this upgrade in the unit manager for this current unit:
					UnitManager.UpgradeListVars NewUpgradeList = new UnitManager.UpgradeListVars();
					//set the upgrade values:
					NewUpgradeList.Speed = BuildingTasksList [ID].AddSpeed;
					NewUpgradeList.UnitDamage = BuildingTasksList [ID].AddUnitDamage;
					NewUpgradeList.BuildingDamage = BuildingTasksList [ID].AddBuildingDamage;
					NewUpgradeList.SearchRange = BuildingTasksList [ID].AddSearchRange;
					NewUpgradeList.AttackReload = BuildingTasksList [ID].AddAttackReload;
					NewUpgradeList.MaxHealth = BuildingTasksList [ID].AddMaxHealth;
					//add the upgrade to the list:
					GameMgr.UnitMgr.FactionUnitUpgrades[FactionID].UpgradeList.Add (NewUpgradeList);
					//now add the unit to units to be upgraded list:
					GameMgr.UnitMgr.FactionUnitUpgrades[FactionID].UnitsToUpgrade.Add(BuildingTasksList [ID].UnitList [n].Code);

					if (FactionMgr.Units.Count > 0) {
						for (int x = 0; x < FactionMgr.Units.Count; x++) { //go through the present units in the scene
							if (FactionMgr.Units [x].Code == BuildingTasksList [ID].UnitList [n].Code) { //
								FactionMgr.Units [x].Speed += BuildingTasksList [ID].AddSpeed;
								FactionMgr.Units [x].MaxHealth += BuildingTasksList [ID].AddMaxHealth;
								FactionMgr.Units [x].Health = FactionMgr.Units [x].MaxHealth;

								FactionMgr.Units [x].NavAgent.speed += BuildingTasksList [ID].AddSpeed;

								if (FactionMgr.Units [x].AttackMgr) {
                                    FactionMgr.Units[x].AttackMgr.UnitDamage += BuildingTasksList [ID].AddUnitDamage;
                                    FactionMgr.Units[x].AttackMgr.BuildingDamage += BuildingTasksList [ID].AddBuildingDamage;
                                    FactionMgr.Units[x].AttackMgr.SearchRange += BuildingTasksList [ID].AddSearchRange;
                                    FactionMgr.Units[x].AttackMgr.AttackReload += BuildingTasksList [ID].AddAttackReload;
								}

								if (SelectionMgr.SelectedUnits.Contains (FactionMgr.Units [x])) { //if this unit is selected
									UIMgr.UpdateUnitUI(FactionMgr.Units [x]);
								}
							}
						}
					}
				}
			}

			if (GameManager.PlayerFactionID == FactionID) { //only if this is the local player:
				//Remove the task:
				BuildingTasksList [ID].Reached = true;
				TotalTasksAmount--;
				//Sync the upgrade:
				CheckTaskUpgrades (ID, false, false);
			}
		}

		//Check if a resource can be dropped off here:
		public bool CanDrop (string Name)
		{
			if (ResourceDropOff == false) //it's not a drop off building, return false
				return false;
			if (AcceptAllResources == true) //it's a drop off building and it accepts all resources, return true
				return true;
			if (AcceptedResources.Length > 0) { //it does accept some resources, look for the target resource
				for (int i = 0; i < AcceptedResources.Length; i++) {
					if (AcceptedResources [i] == Name) //resource found then return true
						return true;
				}
			}
			return false; //if target resource is not on the list then return false
			
		}

        //Draw the building's radius in blue
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}