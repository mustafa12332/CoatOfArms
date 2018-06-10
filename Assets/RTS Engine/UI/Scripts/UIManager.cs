using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

/* UI Manager script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
	public class UIManager : MonoBehaviour 
	{
		[Header("Menus:")]
		//Winng and loosing menus:
		public GameObject WinningMenu; //activated when the player wins the game.
		public GameObject LoosingMenu; //activated when the player loses the game.
        public GameObject PauseMenu; //activated when the player is pausing.
        public GameObject FreezeMenu; //activated when waiting for players to sync in a MP game.

        [Header("Tasks:")]
		//The buttons that get activated when a unit/building is selected. 
		public Button BaseTaskButton; 
		[HideInInspector]
		public List<Button> TaskButtons; //all the task buttons
		int ActiveTaskButtons = 0;
		public Transform TaskButtonsParent; //all the task buttons are children of this object.
		public bool UseTaskParentCategories;
		public Transform[] TaskParentCategories; //you can assign building tasks to different transform parents in order to organize them for the player.

        [Header("In Progress Tasks:")]
		//In Progress building task info:
		public Button FirstInProgressTaskButton; 
		public Image FirstInProgressTaskBarEmpty;
		public Image FirstInProgressTaskBarFull;
		public Button BaseInProgressTaskButton; 
		[HideInInspector]
		public List<Button> InProgressTaskButtons; //The buttons that show the in progress tasks of a building when selected.
		public Transform InProgressTaskButtonsParent; //all the in progress building tasks are children of this object.

		[Header("Selection Info:")]
		//Selection Info vars:
		public Transform SingleSelectionParent; //the selection menu
		public Text SelectionName;
		public Text SelectionDescription;
		public Text SelectionHealth;
		public Image SelectionIcon;
		public Image SelectionHealthBarEmpty;
		public Image SelectionHealthBarFull;

		[Header("Multiple Selection:")]
		//Multiple selection icons:
		public Image BaseMultipleSelectionIcon;
		[HideInInspector]
		public List<Image> MultipleSelectionIcons;
		public Transform MultipleSelectionParent; //all the selected icons are children of this object.

		[Header("Building Upgrade:")]
		//building upgrade:
		public Button BuildingUpgradeButton;
		public Image BuildingUpgradeBarEmpty;
		public Image BuildingUpgradeBarFull;
		public Text BuildingUpgradeProgress;

        [Header("Hover Health Bar:")]
        public bool EnableHoverHealthBar = true; //if set to true then whenever the mouse hovers over a unit/building, a health bar will appear on top of them
        public bool PlayerFactionOnly = true; //only show the hover health bar for friendly units/buildings or for all factions
        public Canvas HoverHealthBarCanvas; //the canvas that holds the health bar sprites
        public RectTransform HoverHealthBarEmpty; //when the hover health bar is empty
        public RectTransform HoverHealthBarFull; //when the hover health bar is full
        bool HoverHealthBarActive = false; //is the hover health bar active or not?
        SelectionObj HoverSource; //the health bar will be showing this object's health when it's enabled.

        [Header("Tooltips")]
        public GameObject TooltipPanel;
        public Text TooltipMsg;

        [Header("Messages:")]
		//Population UI:
		public Text PopulationText; //a text that shows the faction's population

		//Message UI:
		public enum MessageTypes {Error, Info};
		public Text PlayerMessage;
		public float PlayerMessageReload = 3.0f; //how long does the player message shows for.
		float PlayerMessageTimer;

		//Peace Time UI:
		public GameObject PeaceTimePanel;
		public Text PeaceTimeText;

		[HideInInspector]
		public SelectionManager SelectionMgr;
		[HideInInspector]
		public BuildingPlacement PlacementMgr;
		[HideInInspector]
		public GameManager GameMgr;
		[HideInInspector]
		public ResourceManager ResourceMgr;
		[HideInInspector]
		public UnitManager UnitMgr;

		[System.Serializable]
		public class FactionSpritesVars
		{
			public string Code;
			public Sprite Sprite;
		}
		[System.Serializable]
		public class FactionUIVars
		{
			public Image Image;
			public FactionSpritesVars[] FactionSprites;
		}
		[Header("Custom Faction UI:")]
		public FactionUIVars[] FactionUI;

		void Awake () 
		{
			//Hide all the task buttons/info UI objects at the start of the game
			HideSelectionInfoPanel ();

			//get the scripts:
			GameMgr = GameManager.Instance;
			SelectionMgr = GameMgr.SelectionMgr;
			PlacementMgr = GameMgr.PlacementMgr;
			ResourceMgr = GameMgr.ResourceMgr;
			UnitMgr = GameMgr.UnitMgr;

			//default settings for tasks button.
			BaseTaskButton.GetComponent<UnitTaskUI> ().ID = 0;
			TaskButtons.Add (BaseTaskButton);
			BaseTaskButton.gameObject.SetActive (false);
			ActiveTaskButtons = 0;

			//hide the in proress tasks menu, multiple selection and single selection menus:
			MultipleSelectionIcons.Add (BaseMultipleSelectionIcon);
			BaseMultipleSelectionIcon.gameObject.SetActive (false);
			MultipleSelectionParent.gameObject.SetActive (false);
			SingleSelectionParent.gameObject.SetActive (false);

			//default settings for in progress tasks:
			FirstInProgressTaskButton.GetComponent<UnitTaskUI> ().ID = 0;
			BaseInProgressTaskButton.GetComponent<UnitTaskUI> ().ID = 1;
			InProgressTaskButtons.Add (FirstInProgressTaskButton);
			InProgressTaskButtons.Add (BaseInProgressTaskButton);

			//Hide the winning and loosing menu at the start of the game:
			WinningMenu.SetActive(false);
			LoosingMenu.SetActive (false);

            //Hide the pause menu as well:
            PauseMenu.SetActive(false);

			//update the population UI
			UpdatePopulationUI ();

            if (EnableHoverHealthBar == true) //if hover health bar is enabled.
            {
                //Hover health bar:
                //Hide the health bar canvas in the beginning:
                if (HoverHealthBarCanvas != null)
                {
                    HoverHealthBarCanvas.gameObject.SetActive(false);
                }
                HoverHealthBarActive = false;
                HoverSource = null;
            }

        }

		void Start ()
		{
			if (FactionUI.Length > 0) {
				for (int i = 0; i < FactionUI.Length; i++) {
					if (FactionUI [i].Image) {
						bool Found = false;
						int j = 0;
						while (j < FactionUI [i].FactionSprites.Length && Found == false) {
							if (FactionUI [i].FactionSprites [j].Code == GameMgr.Factions [GameManager.PlayerFactionID].Code) {
								FactionUI [i].Image.sprite = FactionUI [i].FactionSprites [j].Sprite;
								Found = true;
							}
							j++;
						}
					}
				}
			}
		}

		void Update () 
		{
			//player message timer:
			if (PlayerMessageTimer > 0) {
				PlayerMessageTimer -= Time.deltaTime;
			}
			if (PlayerMessageTimer < 0) {
				//Hide the player message:
				PlayerMessage.text = "";
				PlayerMessage.gameObject.SetActive (false);
			}
		}

		//a method tht updates the population UI count
		public void UpdatePopulationUI ()
		{
			if (PopulationText) {
				PopulationText.text = GameMgr.Factions [GameManager.PlayerFactionID].CurrentPopulation.ToString () + "/" + GameMgr.Factions [GameManager.PlayerFactionID].MaxPopulation.ToString ();
			}
		}

		//Task buttons:

		//a method that hides all the task buttons.
		public void HideTaskButtons ()
		{
			if(TaskButtons.Count > 0)
			{
				//go through all the task buttons
				for(int i = 0; i < TaskButtons.Count; i++)
				{
					//hide the task buttons
					TaskButtons[i].gameObject.SetActive(false);
                    Image TaskImg = TaskButtons[i].gameObject.GetComponent<Image>();
                    Color ImgColor = new Color(TaskImg.color.r, TaskImg.color.g, TaskImg.color.b, 1.0f);
                    TaskImg.color = ImgColor;
                }
			}
			ActiveTaskButtons = 0;
		}

		//set the amount of task buttons:
		public void AddTaskButtons(int Amount)
		{
			if (TaskButtons.Count < ActiveTaskButtons+Amount) {
				int AmountNeeded = (Amount+ActiveTaskButtons) - TaskButtons.Count; //calculate how many task buttons we need:
				int j = TaskButtons.Count;

				//create new task buttons:
				for (int i = 0; i < AmountNeeded; i++) {
					GameObject NewTaskButton = Instantiate (BaseTaskButton.gameObject);
					//set the settings for the task button
					NewTaskButton.GetComponent<UnitTaskUI> ().ID = j + i;
					//set the parent of the task button:
					NewTaskButton.transform.SetParent (TaskButtonsParent);
					NewTaskButton.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
					TaskButtons.Add (NewTaskButton.GetComponent<Button>());
				}
			}

			ActiveTaskButtons += Amount;
        }

		public void SetTaskButtonParent (int i, int CategoryID)
		{
			if (i >= 0 && i < TaskButtons.Count) {
				//when category is -1, it means that there is no categories and there's only one parent object for all task buttons:
				if (CategoryID == -1 || UseTaskParentCategories == false) {
					TaskButtons [i].transform.SetParent (TaskButtonsParent);
				}
				//are we using task button categories?
				else if (UseTaskParentCategories == true) {
					//make sure that the task button exists:
					TaskButtons [i].transform.SetParent (TaskParentCategories [CategoryID]);
				}

				TaskButtons[i].transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			}
		}


		//Multiple selection menu methods:

		//a method that hides all the multiple selection icons.
		public void HideMultipleSelectionIcons ()
		{
			if(MultipleSelectionIcons.Count > 0)
			{
				//go through all the multiple selection icons:
				for(int i = 0; i < MultipleSelectionIcons.Count; i++)
				{
					//hide each one of them.
					MultipleSelectionIcons[i].gameObject.SetActive(false);
				}
			}
		}

		//set the amount of multiple selection icons:
		public void SetMultipleSelectionIcons (int Amount)
		{
			//if we don't have enough
			if (MultipleSelectionIcons.Count < Amount) {
				Amount = Amount - MultipleSelectionIcons.Count; //determine how many we need:

				for (int i = 0; i < Amount; i++) {
					//create new selection icons
					GameObject NewMultipleSelectionIcon = Instantiate (BaseMultipleSelectionIcon.gameObject);
					//set their parent to the multiple selectin menu:
					NewMultipleSelectionIcon.transform.SetParent (MultipleSelectionParent);
					NewMultipleSelectionIcon.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
					MultipleSelectionIcons.Add (NewMultipleSelectionIcon.GetComponent<Image>());
				}
			}
		}

		//In progress task buttons:

		//a method that hides all in progress task buttons:
		public void HideInProgressTaskButtons ()
		{
			if(InProgressTaskButtons.Count > 0)
			{
				//go through all of the existing in progress task buttons:
				for(int i = 0; i < InProgressTaskButtons.Count; i++)
				{
					//hide the inprogress task button:
					InProgressTaskButtons[i].gameObject.SetActive(false);
				}
			}
		}

		//set the amount of in progress task buttons:
		public void SetInProgressTaskButtons(int Amount)
		{
			//if we don't have enough in progress task buttons:
			if (InProgressTaskButtons.Count < Amount) {
				//set the amount of needed in progress task button:
				Amount = Amount - InProgressTaskButtons.Count;
				int j = InProgressTaskButtons.Count;

				for (int i = 0; i < Amount; i++) {
					//create a new in progress task button:
					GameObject NewInProgressTaskButton = Instantiate (BaseInProgressTaskButton.gameObject);
					NewInProgressTaskButton.GetComponent<UnitTaskUI> ().ID = j + i;
					//set the parent of the in progress task button
					NewInProgressTaskButton.transform.SetParent (InProgressTaskButtonsParent);
					NewInProgressTaskButton.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
					InProgressTaskButtons.Add (NewInProgressTaskButton.GetComponent<Button>());

				}
			}
		}

		//a method that hides the selection info panel:
		public void HideSelectionInfoPanel ()
		{
			MultipleSelectionParent.gameObject.SetActive (false);
			SingleSelectionParent.gameObject.SetActive (false);
			HideMultipleSelectionIcons ();
			HideInProgressTaskButtons ();
		}

		//Updating the currently selected building UI to show its info:
		public void UpdateBuildingUI (Building SelectedBuilding)
		{
			//show and hide the UI elements in the selection menu:
			MultipleSelectionParent.gameObject.SetActive (false);
			SingleSelectionParent.gameObject.SetActive (true);

			SelectionName.gameObject.SetActive (true);
			SelectionDescription.gameObject.SetActive (true);
			SelectionIcon.gameObject.SetActive (true);
			SelectionHealth.gameObject.SetActive (true);

			//hide upgrade info:
			BuildingUpgradeBarEmpty.gameObject.SetActive(false);
			BuildingUpgradeBarFull.gameObject.SetActive(false);
			BuildingUpgradeProgress.gameObject.SetActive(false);
			BuildingUpgradeButton.gameObject.SetActive (false);

			if (SelectedBuilding.FreeBuilding == false) {
				SelectionName.text = SelectedBuilding.Name + " (" + GameMgr.Factions [SelectedBuilding.FactionID].Name + " - " + GameMgr.Factions [SelectedBuilding.FactionID].TypeName + ")";
			} else {
				SelectionName.text = SelectedBuilding.Name + " (Free Building)";
			}
			//If the building is being built then show how many builders there are:
			if (SelectedBuilding.WorkerMgr.CurrentWorkers > 0 && SelectedBuilding.FactionID == GameManager.PlayerFactionID) {
				SelectionDescription.text = "Builders: " + SelectedBuilding.WorkerMgr.CurrentWorkers.ToString () + "/" + SelectedBuilding.WorkerMgr.WorkerPositions.Length.ToString() + "\n" + SelectedBuilding.Description;
			} else {
				SelectionDescription.text = SelectedBuilding.Description;
			}
			SelectionIcon.sprite = SelectedBuilding.Icon;

			if (GameManager.PlayerFactionID == SelectedBuilding.FactionID && SelectedBuilding.IsBuilt == true) {
				if (SelectedBuilding.BuildingUpgrading == true) { //if the building is upgrading
					//don't show no tasks:
					HideTaskButtons ();
					HideInProgressTaskButtons ();

					BuildingUpgradeBarEmpty.gameObject.SetActive (true);
					BuildingUpgradeBarFull.gameObject.SetActive (true);
					BuildingUpgradeProgress.gameObject.SetActive (true);
				} else {

					UpdateBuildingTasks (SelectedBuilding);

					//update the in progress tasks:
					UpdateInProgressTasksUI (SelectedBuilding);

					//if we can upgrade the building
					if (SelectedBuilding.UpgradeBuilding != null && SelectedBuilding.DirectUpgrade == true) {
						BuildingUpgradeButton.gameObject.SetActive (true);
					}
				}
			}

			//update the building's health:
			UpdateBuildingHealthUI (SelectedBuilding);
		}

		public void UpdateBuildingUpgrade (Building SelectedBuilding)
		{
			if (SelectedBuilding.IsBuilt == true) { //making sure it's built
				if (GameManager.PlayerFactionID == SelectedBuilding.FactionID) { //making sure we're dealing with the local player

					//Update the progress bar:
					float NewBarLength = BuildingUpgradeBarEmpty.gameObject.GetComponent<RectTransform> ().sizeDelta.x -  (SelectedBuilding.BuildingUpgradeTimer / SelectedBuilding.BuildingUpgradeReload) * BuildingUpgradeBarEmpty.gameObject.GetComponent<RectTransform> ().sizeDelta.x;
					BuildingUpgradeBarFull.gameObject.GetComponent<RectTransform> ().sizeDelta = new Vector2 (NewBarLength, BuildingUpgradeBarFull.gameObject.GetComponent<RectTransform> ().sizeDelta.y);
					BuildingUpgradeBarFull.gameObject.GetComponent<RectTransform> ().localPosition = new Vector3 (BuildingUpgradeBarEmpty.gameObject.GetComponent<RectTransform> ().localPosition.x - (BuildingUpgradeBarEmpty.gameObject.GetComponent<RectTransform> ().sizeDelta.x - BuildingUpgradeBarFull.gameObject.GetComponent<RectTransform> ().sizeDelta.x) / 2.0f, BuildingUpgradeBarEmpty.gameObject.GetComponent<RectTransform> ().localPosition.y, BuildingUpgradeBarEmpty.gameObject.GetComponent<RectTransform> ().localPosition.z);

					//update text:
					int Progress = (int) ((SelectedBuilding.BuildingUpgradeTimer/SelectedBuilding.BuildingUpgradeReload)*100.0f); 
					Progress = 100 - Progress;
					BuildingUpgradeProgress.text = "Upgrading: "+Progress.ToString()+"%";
				}
			}
		}

		public void UpdateBuildingTasks (Building SelectedBuilding)
		{
			if (SelectedBuilding == null) {
				return;
			}
			HideTaskButtons ();
			//only show task if selected building is from player team.
			if (SelectedBuilding.IsBuilt == true) {
				if (GameManager.PlayerFactionID == SelectedBuilding.FactionID) {
					int TasksAmount = SelectedBuilding.TotalTasksAmount;
					int LastTaskID = 0;
					if (SelectedBuilding.gameObject.GetComponent<ResourceGenerator> ()) { //if the building is a resource generator
						TasksAmount += SelectedBuilding.gameObject.GetComponent<ResourceGenerator> ().ReadyToCollect.Count; //add tasks
					}
					if (SelectedBuilding.gameObject.GetComponent<APC> ()) {
						if (SelectedBuilding.gameObject.GetComponent<APC> ().CurrentUnits.Count > 0) {
							TasksAmount += SelectedBuilding.gameObject.GetComponent<APC> ().CurrentUnits.Count + 1;
						}
					}
					AddTaskButtons (TasksAmount); //Update the amont of the task buttons

					//normal tasks:
					int i = 0;
					int TasksCount = 0;
					while (TasksCount < SelectedBuilding.BuildingTasksList.Count) {
						if ((SelectedBuilding.BuildingTasksList [TasksCount].TaskType != Building.BuildingTasks.CreateUnit && SelectedBuilding.BuildingTasksList [TasksCount].Active == false && SelectedBuilding.BuildingTasksList [TasksCount].Reached == false) || SelectedBuilding.BuildingTasksList [TasksCount].TaskType == Building.BuildingTasks.CreateUnit) {
							SetTaskButtonParent (i, SelectedBuilding.BuildingTasksList [TasksCount].TaskPanelCategory);
							TaskButtons [i].gameObject.SetActive (true);
							TaskButtons [i].gameObject.GetComponent<Image> ().sprite = SelectedBuilding.BuildingTasksList [TasksCount].TaskIcon;
							TaskButtons [i].gameObject.GetComponent<UnitTaskUI> ().ID = TasksCount;
							TaskButtons [i].gameObject.GetComponent<UnitTaskUI> ().TaskType = TaskManager.BuildingTaskType(SelectedBuilding.BuildingTasksList [TasksCount].TaskType);
                            //if this is a unit creation task
                            if (SelectedBuilding.BuildingTasksList[TasksCount].TaskType == Building.BuildingTasks.CreateUnit)
                            {
                                //set the color of the task depending on the limit for this unit type:
                                TaskButtons[i].GetComponent<Image>().color = (GameMgr.Factions[GameManager.PlayerFactionID].FactionMgr.HasReachedLimit(SelectedBuilding.BuildingTasksList[TasksCount].UnitPrefab.Code)) ? Color.red : Color.white;
                            }

                            i++;
							LastTaskID++;
                            //if the task produces a unit and has upgrades:
                            if (SelectedBuilding.BuildingTasksList [TasksCount].Upgrades.Length > 0 && SelectedBuilding.BuildingTasksList [TasksCount].Active == false && SelectedBuilding.BuildingTasksList [TasksCount].TaskType == Building.BuildingTasks.CreateUnit) {
								//if the current upgrade level has not reached the max level (there are more possible upgrades):
								if (SelectedBuilding.BuildingTasksList [TasksCount].CurrentUpgradeLevel < SelectedBuilding.BuildingTasksList [TasksCount].Upgrades.Length) {
									SetTaskButtonParent (i, SelectedBuilding.BuildingTasksList [TasksCount].UpgradeTaskPanelCategory);
									TaskButtons [i].gameObject.SetActive (true);
									TaskButtons [i].gameObject.GetComponent<Image> ().sprite = SelectedBuilding.BuildingTasksList [TasksCount].Upgrades [SelectedBuilding.BuildingTasksList [TasksCount].CurrentUpgradeLevel].UpgradeIcon;
									TaskButtons [i].gameObject.GetComponent<UnitTaskUI> ().ID = TasksCount;
									TaskButtons [i].gameObject.GetComponent<UnitTaskUI> ().TaskType = TaskManager.TaskTypes.TaskUpgrade;
									i++;
									LastTaskID++;
								}
							}
						}
						TasksCount++;
					}

					//resource generation tasks
					if (SelectedBuilding.gameObject.GetComponent<ResourceGenerator> ()) {
						if (SelectedBuilding.gameObject.GetComponent<ResourceGenerator> ().ReadyToCollect.Count > 0) {
							for (i = 0; i < SelectedBuilding.gameObject.GetComponent<ResourceGenerator> ().ReadyToCollect.Count; i++) {
								int ResourceID = SelectedBuilding.gameObject.GetComponent<ResourceGenerator> ().ReadyToCollect [i];
								ResourceGenerator Gen = SelectedBuilding.gameObject.GetComponent<ResourceGenerator> ();

								SetTaskButtonParent (LastTaskID, -1);
								TaskButtons [LastTaskID].gameObject.SetActive (true);
								TaskButtons [LastTaskID].gameObject.GetComponent<Image> ().sprite = Gen.Resources [ResourceID].TaskIcon;
								TaskButtons [LastTaskID].gameObject.GetComponent<UnitTaskUI> ().ID = i;
								TaskButtons [LastTaskID].gameObject.GetComponent<UnitTaskUI> ().TaskType = TaskManager.TaskTypes.ResourceGen;
								LastTaskID++;
							}
						}
					}

					if (SelectedBuilding.gameObject.GetComponent<APC> ()) {
						int TaskCount = 0;
						if (SelectedBuilding.gameObject.GetComponent<APC> ().CurrentUnits.Count > 0) {
							TaskCount++;
							if (SelectedBuilding.gameObject.GetComponent<APC> ().EjectAllOnly == false) {
								TaskCount += SelectedBuilding.gameObject.GetComponent<APC> ().CurrentUnits.Count;
							}
						}
						if (SelectedBuilding.gameObject.GetComponent<APC> ().CurrentUnits.Count < SelectedBuilding.gameObject.GetComponent<APC> ().MaxAmount) {
							TaskCount++;
						}

						AddTaskButtons (TaskCount);

						if (SelectedBuilding.gameObject.GetComponent<APC> ().CurrentUnits.Count > 0) {

							//first task is for release all units
							SetTaskButtonParent(0, -1);
							TaskButtons [0].gameObject.SetActive (true);
							TaskButtons [0].gameObject.GetComponent<Image> ().sprite = SelectedBuilding.gameObject.GetComponent<APC> ().EjectAllIcon;
							TaskButtons [0].gameObject.GetComponent<UnitTaskUI> ().TaskType = TaskManager.TaskTypes.APCRelease;
							TaskButtons [0].gameObject.GetComponent<UnitTaskUI> ().ID = 0;
							LastTaskID++;

							for (i = 0; i < SelectedBuilding.gameObject.GetComponent<APC> ().CurrentUnits.Count; i++) {
								SetTaskButtonParent (i + 1, -1);
								TaskButtons [i+1].gameObject.SetActive (true);
								TaskButtons [i+1].gameObject.GetComponent<Image> ().sprite = SelectedBuilding.gameObject.GetComponent<APC> ().CurrentUnits [i].Icon;
								TaskButtons [i+1].gameObject.GetComponent<UnitTaskUI> ().TaskType = TaskManager.TaskTypes.APCRelease;
								TaskButtons [i+1].gameObject.GetComponent<UnitTaskUI> ().ID = i+1;
								LastTaskID++;
							}
						}
						if (SelectedBuilding.gameObject.GetComponent<APC> ().CurrentUnits.Count < SelectedBuilding.gameObject.GetComponent<APC> ().MaxAmount) {
							//first task is for release all units
							SetTaskButtonParent (LastTaskID, SelectedBuilding.gameObject.GetComponent<APC> ().CallUnitsTaskCategory);
							TaskButtons [LastTaskID].gameObject.SetActive (true);
							TaskButtons [LastTaskID].gameObject.GetComponent<Image> ().sprite = SelectedBuilding.gameObject.GetComponent<APC> ().CallUnitsIcon;
							TaskButtons [LastTaskID].gameObject.GetComponent<UnitTaskUI> ().TaskType = TaskManager.TaskTypes.APCCall;
							TaskButtons [LastTaskID].gameObject.GetComponent<UnitTaskUI> ().ID = LastTaskID;
							LastTaskID++;
						}
					}
				}
			}
		}

		//Updates the selected building health bar:
		public void UpdateBuildingHealthUI (Building SelectedBuilding)
		{
			//Update the health text:
			SelectionHealth.text = Mathf.FloorToInt(SelectedBuilding.Health).ToString () + "/" +  Mathf.FloorToInt(SelectedBuilding.MaxHealth).ToString ();
			if (SelectionHealthBarFull.gameObject != null && SelectionHealthBarEmpty.gameObject != null) {
				SelectionHealthBarFull.gameObject.SetActive (true);
				SelectionHealthBarEmpty.gameObject.SetActive (true);

				//Update the health bar:
				float NewBarLength = (SelectedBuilding.Health / SelectedBuilding.MaxHealth) * SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform> ().sizeDelta.x;
				SelectionHealthBarFull.gameObject.GetComponent<RectTransform> ().sizeDelta = new Vector2 (NewBarLength, SelectionHealthBarFull.gameObject.GetComponent<RectTransform> ().sizeDelta.y);
				SelectionHealthBarFull.gameObject.GetComponent<RectTransform> ().localPosition = new Vector3 (SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform> ().localPosition.x - (SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform> ().sizeDelta.x - SelectionHealthBarFull.gameObject.GetComponent<RectTransform> ().sizeDelta.x) / 2.0f, SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform> ().localPosition.y, SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform> ().localPosition.z);
			}
		}

		//a method that updates the building's in progress tasks:
		public void UpdateInProgressTasksUI (Building SelectedBuilding)
		{
			if (SelectedBuilding == null) {
				return;
			}
			//show building's task only if the building belongs to the player's faction
			if (SelectedBuilding.FactionID == GameManager.PlayerFactionID) {
				HideInProgressTaskButtons ();

				if (SelectedBuilding.TasksQueue.Count > 0) { //if we actually have pending tasks:
					SetInProgressTaskButtons (SelectedBuilding.TasksQueue.Count);
					for (int i = 0; i < SelectedBuilding.TasksQueue.Count; i++) {
						//show the pending tasks:
						InProgressTaskButtons [i].gameObject.SetActive (true);

						if  (SelectedBuilding.TasksQueue [i].Upgrade == true) {
							InProgressTaskButtons [i].gameObject.GetComponent<Image> ().sprite = SelectedBuilding.BuildingTasksList [SelectedBuilding.TasksQueue [i].ID].Upgrades[SelectedBuilding.BuildingTasksList [SelectedBuilding.TasksQueue [i].ID].CurrentUpgradeLevel].UpgradeIcon;
						}
						else {
							InProgressTaskButtons [i].gameObject.GetComponent<Image> ().sprite = SelectedBuilding.BuildingTasksList [SelectedBuilding.TasksQueue [i].ID].TaskIcon;
						}
						//the task icon's transparency shows the progress of the task. the more clear it is, the faster it will end.
						float Value = 0.5f;
						if (i == 0) {
							Value += ( ((SelectedBuilding.BuildingTasksList[SelectedBuilding.TasksQueue[0].ID].ReloadTime-SelectedBuilding.TaskQueueTimer)*0.5f) / SelectedBuilding.BuildingTasksList[SelectedBuilding.TasksQueue[0].ID].ReloadTime);
							//Update the task's timer bar:
							float NewBarLength = FirstInProgressTaskBarEmpty.gameObject.GetComponent<RectTransform> ().sizeDelta.x -  (SelectedBuilding.TaskQueueTimer / SelectedBuilding.BuildingTasksList[SelectedBuilding.TasksQueue[0].ID].ReloadTime) * FirstInProgressTaskBarEmpty.gameObject.GetComponent<RectTransform> ().sizeDelta.x;
							FirstInProgressTaskBarFull.gameObject.GetComponent<RectTransform> ().sizeDelta = new Vector2 (NewBarLength, FirstInProgressTaskBarFull.gameObject.GetComponent<RectTransform> ().sizeDelta.y);
							FirstInProgressTaskBarFull.gameObject.GetComponent<RectTransform> ().localPosition = new Vector3 (FirstInProgressTaskBarEmpty.gameObject.GetComponent<RectTransform> ().localPosition.x - (FirstInProgressTaskBarEmpty.gameObject.GetComponent<RectTransform> ().sizeDelta.x - FirstInProgressTaskBarFull.gameObject.GetComponent<RectTransform> ().sizeDelta.x) / 2.0f, FirstInProgressTaskBarEmpty.gameObject.GetComponent<RectTransform> ().localPosition.y, FirstInProgressTaskBarEmpty.gameObject.GetComponent<RectTransform> ().localPosition.z);
						}

						Color ImgColor = new Color (InProgressTaskButtons [i].GetComponent<Image> ().color.a, InProgressTaskButtons [i].GetComponent<Image> ().color.g, InProgressTaskButtons [i].GetComponent<Image> ().color.b, Value);
						InProgressTaskButtons [i].GetComponent<Image> ().color = ImgColor;
					}
				}
			}
		}

		//Launch a task in the building/unit:
		public void LaunchTask (int ID, TaskManager.TaskTypes TaskType, Sprite TaskSprite)
		{
			//Check if we actually a building selected.
			if (SelectionMgr.SelectedBuilding != null) {
				if (SelectionMgr.SelectedBuilding.IsBuilt == true) {
					//Always check that the health is above the minimal limit to launch tasks and that the building was built (to max health) at least once:
					if (SelectionMgr.SelectedBuilding.Health >= SelectionMgr.SelectedBuilding.MinTaskHealth) {
						if (SelectionMgr.SelectedBuilding.MaxTasks > SelectionMgr.SelectedBuilding.TasksQueue.Count) {
							if (TaskType == TaskManager.TaskTypes.TaskUpgrade) { //upgrade task:
								//Make sure that there's still space for a new task in this building:
								if (ResourceMgr.CheckResources (SelectionMgr.SelectedBuilding.BuildingTasksList [ID].Upgrades [SelectionMgr.SelectedBuilding.BuildingTasksList [ID].CurrentUpgradeLevel].UpgradeResources, GameManager.PlayerFactionID, 1.0f) == true) {
									GameMgr.TaskMgr.LaunchTask (SelectionMgr.SelectedBuilding, ID,-1, TaskType);

								} else {
									ShowPlayerMessage ("Not enough resources to launch upgrade task!", MessageTypes.Error);
									AudioManager.PlayAudio (GameMgr.GeneralAudioSource.gameObject, SelectionMgr.SelectedBuilding.DeclinedTaskAudio, false); //Declined task audio.
								}
							} else if (TaskType == TaskManager.TaskTypes.ResourceGen) {
								//resource gen task:
								GameMgr.TaskMgr.LaunchTask(SelectionMgr.SelectedBuilding, ID,-1, TaskType);
							}
							else if (TaskType == TaskManager.TaskTypes.APCRelease) {
								SelectionMgr.SelectedBuilding.gameObject.GetComponent<APC> ().DropOffUnits (ID);
							}
							else if (TaskType == TaskManager.TaskTypes.APCCall) {
								SelectionMgr.SelectedBuilding.gameObject.GetComponent<APC> ().CallForUnits();
							}
							else { //if this is not an upgrade task nor a resource gen one:
								//Check if we have enough resources to launch this task:
								if (ResourceMgr.CheckResources (SelectionMgr.SelectedBuilding.BuildingTasksList[ID].RequiredResources, GameManager.PlayerFactionID, 1) == true) {
									//If it's unit creation task and we don't have enough room for population:
									bool Cancel = false;
									if (SelectionMgr.SelectedBuilding.BuildingTasksList [ID].TaskType == Building.BuildingTasks.CreateUnit) {
										if (GameMgr.Factions [GameManager.PlayerFactionID].CurrentPopulation >= GameMgr.Factions [GameManager.PlayerFactionID].MaxPopulation) {
											//Inform the player that there's no more room for new units.
											Cancel = true;
											ShowPlayerMessage ("Maximum population has been reached!", MessageTypes.Error);
											AudioManager.PlayAudio (GameMgr.GeneralAudioSource.gameObject, SelectionMgr.SelectedBuilding.DeclinedTaskAudio, false); //Declined task audio.
										} 
                                        //if there's population slots but the local faction already hit the limit with this faction
                                        else if (GameMgr.Factions[GameManager.PlayerFactionID].FactionMgr.HasReachedLimit(SelectionMgr.SelectedBuilding.BuildingTasksList[ID].UnitPrefab.Code))
                                        {
                                            //inform the player that he can't create this unit
                                            Cancel = true;
                                            ShowPlayerMessage("This unit has reached its creation limit", MessageTypes.Error);
                                            AudioManager.PlayAudio(GameMgr.GeneralAudioSource.gameObject, SelectionMgr.SelectedBuilding.DeclinedTaskAudio, false); //Declined task audio.
                                        }
									} 

									if (Cancel == false) {
										GameMgr.TaskMgr.LaunchTask (SelectionMgr.SelectedBuilding, ID,-1,TaskType);
                                        //update the tasks panel:
                                        UpdateBuildingTasks(SelectionMgr.SelectedBuilding);
									}
								}
								else {
									ShowPlayerMessage ("Not enough resources to launch task!", MessageTypes.Error);
									AudioManager.PlayAudio (GameMgr.GeneralAudioSource.gameObject, SelectionMgr.SelectedBuilding.DeclinedTaskAudio, false); //Declined task audio.
								} 
							}
						}
						else {
							//Notify the playe that the maximum amount of tasks for this building has been reached
							ShowPlayerMessage ("Maximum building tasks has been reached", MessageTypes.Error);
							AudioManager.PlayAudio (GameMgr.GeneralAudioSource.gameObject, SelectionMgr.SelectedBuilding.DeclinedTaskAudio, false); //Declined task audio.
						}
					}
					else {
						ShowPlayerMessage ("Building is damaged, fix it before using tasks!", MessageTypes.Error);
						AudioManager.PlayAudio (GameMgr.GeneralAudioSource.gameObject, SelectionMgr.SelectedBuilding.DeclinedTaskAudio, false); //Declined task audio.
					}
				} 
			}
			else 
			{
				UnitMgr.AwaitingTaskType = TaskManager.TaskTypes.Null; //initialize the awaiting type task in case we don't select a component task.

                //If a building is not currently selected, then surely a unit or more are selected then:
                //Is it a building task? 
                if (TaskType == TaskManager.TaskTypes.PlaceBuilding)
                {
                    //make sure the building hasn't reached its limits:
                    if(!GameMgr.Factions[GameManager.PlayerFactionID].FactionMgr.HasReachedLimit(PlacementMgr.AllBuildings[ID].Code))
                    {
                        //check if the player can build on this map:
                        PlacementMgr.StartPlacingBuilding(ID);
                    }
                    else
                    {
                        //building limit reached, send message to player:
                        ShowPlayerMessage("Building " + PlacementMgr.AllBuildings[ID].Name + "has reached its placement limit", MessageTypes.Error);
                    }
                    
                }
                else if (TaskType == TaskManager.TaskTypes.APCRelease)
                {
                    SelectionMgr.SelectedUnits[0].APCMgr.DropOffUnits(ID);
                }
                else if (TaskType == TaskManager.TaskTypes.APCCall)
                {
                    SelectionMgr.SelectedUnits[0].APCMgr.CallForUnits();
                }
                else if (TaskType == TaskManager.TaskTypes.ToggleInvisibility)
                {
                    SelectionMgr.SelectedUnits[0].InvisibilityMgr.ToggleInvisibility();
                }
                else if (TaskType == TaskManager.TaskTypes.AttackTypeSelection)
                {
                    //make sure the attack type is not in cooldown mode
                    if(SelectionMgr.SelectedUnits[0].MultipleAttacksMgr.AttackTypes[ID].InCoolDownMode == false)
                    {
                        SelectionMgr.SelectedUnits[0].MultipleAttacksMgr.EnableAttackType(ID);
                    }
                    else
                    {
                        ShowPlayerMessage("Attack type in cooldown mode!", MessageTypes.Error);
                    }
                }
                else if (TaskType == TaskManager.TaskTypes.ToggleWander)
                {
                    SelectionMgr.SelectedUnits[0].Wandering = !SelectionMgr.SelectedUnits[0].Wandering;
                    if(SelectionMgr.SelectedUnits[0].Wandering == true) //if wandering is now enabled
                    {
                        if(SelectionMgr.SelectedUnits[0].FixedWanderCenter == true)
                        {
                            SelectionMgr.SelectedUnits[0].WanderCenter = SelectionMgr.SelectedUnits[0].transform.position;
                        }
                        //make the unit wander:
                        SelectionMgr.SelectedUnits[0].Wander();
                    }
                    //update the tasks UI:
                    UpdateUnitTasks();
                }
                //component tasks:
                else
				{
					UnitMgr.SetAwaitingTaskType (TaskType, TaskSprite);
				}

                HideTooltip();
            }
		}

		//Resource UI:

		//updating the resource UI:
		public void UpdateResourceUI (Resource SelectedResource)
		{
			if (SelectionHealthBarEmpty.gameObject != null && SelectionHealthBarFull.gameObject != null) {
				SelectionHealthBarFull.gameObject.SetActive (false);
				SelectionHealthBarEmpty.gameObject.SetActive (false);
			}

			//show and hide object in the selection menu:
			MultipleSelectionParent.gameObject.SetActive (false);
			SingleSelectionParent.gameObject.SetActive (true);

			SelectionName.gameObject.SetActive (true);
			SelectionDescription.gameObject.SetActive (true);
			SelectionIcon.gameObject.SetActive (true);
			SelectionHealth.gameObject.SetActive (true);

			//hide upgrade info:
			BuildingUpgradeBarEmpty.gameObject.SetActive(false);
			BuildingUpgradeBarFull.gameObject.SetActive(false);
			BuildingUpgradeProgress.gameObject.SetActive(false);
			BuildingUpgradeButton.gameObject.SetActive (false);

			//display the resource's info:
			SelectionName.text = SelectedResource.Name;
			SelectionHealth.text = "";
			SelectionDescription.text = "";

			if (SelectedResource.ShowAmount == true) {
				if (SelectedResource.Infinite == true) {
					SelectionHealth.text = "Infinite Amount";
				} else {
					SelectionHealth.text = "Amount: " + SelectedResource.Amount;
				}
			}
			if (SelectedResource.ShowCollectors )
				SelectionDescription.text = "Collectors: "+SelectedResource.WorkerMgr.CurrentWorkers.ToString()+"/"+SelectedResource.WorkerMgr.WorkerPositions.Length.ToString();


			SelectionIcon.sprite = ResourceMgr.ResourcesInfo[ResourceMgr.GetResourceID(SelectedResource.Name)].Icon;

		}

		//update the unit UI to show its info:
		public void UpdateUnitUI (Unit SelectedUnit)
		{
			//hide upgrade info:
			BuildingUpgradeBarEmpty.gameObject.SetActive(false);
			BuildingUpgradeBarFull.gameObject.SetActive(false);
			BuildingUpgradeProgress.gameObject.SetActive(false);
			BuildingUpgradeButton.gameObject.SetActive (false);

			if (SelectionMgr.SelectedUnits.Count == 1) {
				//if we're selecing only one unit
				MultipleSelectionParent.gameObject.SetActive (false);
				SingleSelectionParent.gameObject.SetActive (true);

				//Showing the unit's info:
				SelectionName.gameObject.SetActive (true);
				SelectionDescription.gameObject.SetActive (true);
				SelectionIcon.gameObject.SetActive (true);
				SelectionHealth.gameObject.SetActive (true);
				if (SelectedUnit.FreeUnit == false) {
					SelectionName.text = SelectedUnit.Name + " (" + GameMgr.Factions [SelectedUnit.FactionID].Name + " - " + GameMgr.Factions [SelectedUnit.FactionID].TypeName + ")";
				} else {
					SelectionName.text = SelectedUnit.Name + " (Free Unit)";
				}
				SelectionDescription.text = SelectedUnit.Description;
				if (SelectedUnit.APCMgr) {
					SelectionDescription.text += "\nCapacity: " + SelectedUnit.APCMgr.CurrentUnits.Count.ToString() + "/" + SelectedUnit.APCMgr.MaxAmount.ToString();
				}
				SelectionIcon.sprite = SelectedUnit.Icon;


			} else if(SelectionMgr.SelectedUnits.Count > 1) { //if more than one unit has been selected.
				//show the multiple selection menu:
				HideMultipleSelectionIcons ();
				SingleSelectionParent.gameObject.SetActive (false);
				MultipleSelectionParent.gameObject.SetActive (true);

				SetMultipleSelectionIcons (SelectionMgr.SelectedUnits.Count);

				for (int i = 0; i < SelectionMgr.SelectedUnits.Count; i++) {
					//only each seleced unit's icon.
					MultipleSelectionIcons [i].gameObject.SetActive (true);
					MultipleSelectionIcons [i].sprite = SelectionMgr.SelectedUnits[i].Icon;
				}
			}

			//only show task if selected unit is from player team.
			if(SelectedUnit.FactionID == GameManager.PlayerFactionID) 
			{
				UpdateUnitTasks ();
			}

			UpdateUnitHealthUI (SelectedUnit);
		}

        public void UpdateRpgUI(stats rpgManager)
        {
            MultipleSelectionParent.gameObject.SetActive(false);
            SingleSelectionParent.gameObject.SetActive(true);
            SelectionName.gameObject.SetActive(true);
            SelectionDescription.gameObject.SetActive(true);
            SelectionIcon.gameObject.SetActive(true);
            SelectionHealth.gameObject.SetActive(true);
            BuildingUpgradeBarEmpty.gameObject.SetActive(false);
            BuildingUpgradeBarFull.gameObject.SetActive(false);
            BuildingUpgradeProgress.gameObject.SetActive(false);
            BuildingUpgradeButton.gameObject.SetActive(false);
            SelectionName.text = rpgManager.Name + " (Free Unit)";
            
            SelectionDescription.text = rpgManager.Description;
          
            SelectionIcon.sprite = rpgManager.Icon;
            UpdateRpgHealthUi(rpgManager);
        }

        private void UpdateRpgHealthUi(stats rpgManager)
        {
            SelectionHealth.text = Mathf.FloorToInt(rpgManager.HealthSystem.CurrentHealth).ToString() + "/" + Mathf.FloorToInt(rpgManager.HealthSystem.MaxHealth).ToString();
            if (SelectionHealthBarFull.gameObject != null && SelectionHealthBarEmpty.gameObject != null)
            {
                SelectionHealthBarFull.gameObject.SetActive(true);
                SelectionHealthBarEmpty.gameObject.SetActive(true);
                float NewBarLength = (rpgManager.HealthSystem.CurrentHealth / rpgManager.HealthSystem.MaxHealth) * SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform>().sizeDelta.x;
                SelectionHealthBarFull.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(NewBarLength, SelectionHealthBarFull.gameObject.GetComponent<RectTransform>().sizeDelta.y);
                SelectionHealthBarFull.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform>().localPosition.x - (SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform>().sizeDelta.x - SelectionHealthBarFull.gameObject.GetComponent<RectTransform>().sizeDelta.x) / 2.0f, SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform>().localPosition.y, SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform>().localPosition.z);
            }
        }
        //Updates the selected units health bar:
        public void UpdateUnitHealthUI (Unit SelectedUnit)
		{
			if (SelectionMgr.SelectedUnits.Count == 1) {

				//Update the health text:
				SelectionHealth.text = Mathf.FloorToInt(SelectedUnit.Health).ToString () + "/" +  Mathf.FloorToInt(SelectedUnit.MaxHealth).ToString ();

				if (SelectionHealthBarFull.gameObject != null && SelectionHealthBarEmpty.gameObject != null) {
					SelectionHealthBarFull.gameObject.SetActive (true);
					SelectionHealthBarEmpty.gameObject.SetActive (true);
					//Update the health bar:
					float NewBarLength = (SelectedUnit.Health / SelectedUnit.MaxHealth) * SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform> ().sizeDelta.x;
					SelectionHealthBarFull.gameObject.GetComponent<RectTransform> ().sizeDelta = new Vector2 (NewBarLength, SelectionHealthBarFull.gameObject.GetComponent<RectTransform> ().sizeDelta.y);
					SelectionHealthBarFull.gameObject.GetComponent<RectTransform> ().localPosition = new Vector3 (SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform> ().localPosition.x - (SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform> ().sizeDelta.x - SelectionHealthBarFull.gameObject.GetComponent<RectTransform> ().sizeDelta.x) / 2.0f, SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform> ().localPosition.y, SelectionHealthBarEmpty.gameObject.GetComponent<RectTransform> ().localPosition.z);
				}
			} else if(SelectionMgr.SelectedUnits.Count > 1) {

				//Loop through the selected units sprites.
				for (int i = 0; i < SelectionMgr.SelectedUnits.Count; i++) {

					if (MultipleSelectionIcons [i].gameObject.GetComponent<UnitTaskUI> ()) {
						if (MultipleSelectionIcons [i].gameObject.GetComponent<UnitTaskUI> ().EmptyBar != null && MultipleSelectionIcons [i].gameObject.GetComponent<UnitTaskUI> ().FullBar != null) {
							Image EmptyBar = MultipleSelectionIcons [i].gameObject.GetComponent<UnitTaskUI> ().EmptyBar;
							Image FullBar = MultipleSelectionIcons [i].gameObject.GetComponent<UnitTaskUI> ().FullBar;

							//Update the health bar:
							float NewBarLength = (SelectionMgr.SelectedUnits[i].Health / SelectionMgr.SelectedUnits[i].MaxHealth) * EmptyBar.gameObject.GetComponent<RectTransform> ().sizeDelta.x;
							FullBar.gameObject.GetComponent<RectTransform> ().sizeDelta = new Vector2 (NewBarLength, FullBar.gameObject.GetComponent<RectTransform> ().sizeDelta.y);
							FullBar.gameObject.GetComponent<RectTransform> ().localPosition = new Vector3 (EmptyBar.gameObject.GetComponent<RectTransform> ().localPosition.x - (EmptyBar.gameObject.GetComponent<RectTransform> ().sizeDelta.x - FullBar.gameObject.GetComponent<RectTransform> ().sizeDelta.x) / 2.0f, EmptyBar.gameObject.GetComponent<RectTransform> ().localPosition.y, EmptyBar.gameObject.GetComponent<RectTransform> ().localPosition.z);
						}
					}
				}
			}
		}

		//A function used to check shared components between selected units.
		public void UpdateUnitTasks ()
		{
			HideTaskButtons ();
            if (SelectionMgr.SelectedUnits.Count > 0)
            {
                //Update the task bar:
                int LastTaskID = 0;

                bool CanMove = SelectionMgr.SelectedUnits[0].CanBeMoved; //can the player be moved? 
                Attack AttackComp = SelectionMgr.SelectedUnits[0].AttackMgr; //can the player attack? 
                Builder BuilderComp = SelectionMgr.SelectedUnits[0].BuilderMgr; //can the player attack? 
                Converter ConverterComp = SelectionMgr.SelectedUnits[0].ConvertMgr; //can the player convert? 
                Healer HealerComp = SelectionMgr.SelectedUnits[0].HealMgr; //can the player heal? 
                GatherResource CollectComp = SelectionMgr.SelectedUnits[0].ResourceMgr;

                //Loop through the rest of the selected units
                if (SelectionMgr.SelectedUnits.Count > 1)
                {
                    int i = 1; //counter
                    while (i < SelectionMgr.SelectedUnits.Count && (BuilderComp != null || CanMove == true || AttackComp != null || ConverterComp != null || HealerComp != null || CollectComp != null))
                    {
                        //Checking the building tasks:
                        if (!SelectionMgr.SelectedUnits[i].BuilderMgr && BuilderComp != null)
                        {
                            //Don't show the builder component on the tasks tab:
                            BuilderComp = null;
                        }
                        if (SelectionMgr.SelectedUnits[i].CanBeMoved == false && CanMove == true)
                        { //if the unit can't be moved then 
                          //don't show no mvt task
                            CanMove = false;
                        }
                        if (!SelectionMgr.SelectedUnits[i].AttackMgr && AttackComp != null)
                        {
                            //Don't show the attack component on the tasks tab:
                            AttackComp = null;
                        }
                        if (!SelectionMgr.SelectedUnits[i].HealMgr && HealerComp != null)
                        {
                            //Don't show the heal component on the tasks tab:
                            HealerComp = null;
                        }
                        if (!SelectionMgr.SelectedUnits[i].ConvertMgr && ConverterComp != null)
                        {
                            //Don't show the convert component on the tasks tab:
                            ConverterComp = null;
                        }
                        if (!SelectionMgr.SelectedUnits[i].ResourceMgr && CollectComp != null)
                        {
                            //Don't show the collect component on the tasks tab:
                            CollectComp = null;
                        }

                        i++;
                    }
                }

                //movement task:
                if (CanMove == true && UnitMgr.MvtTaskIcon != null)
                {
                    AddTaskButtons(1);
                    AddTask(ref LastTaskID, -1, TaskManager.TaskTypes.Mvt, UnitMgr.MvtTaskCategory, UnitMgr.MvtTaskIcon);
                }

                //collect task:
                if (CollectComp != null && UnitMgr.CollectTaskIcon != null)
                {
                    AddTaskButtons(1);
                    AddTask(ref LastTaskID, -1, TaskManager.TaskTypes.Collect, UnitMgr.CollectTaskCategory, UnitMgr.CollectTaskIcon);
                }

                //attack task:
                if (AttackComp != null && UnitMgr.AttackTaskIcon != null)
                {
                    AddTaskButtons(1);
                    AddTask(ref LastTaskID, -1, TaskManager.TaskTypes.Attack, UnitMgr.AttackTaskCategory, UnitMgr.AttackTaskIcon);
                }

                //heal task:
                if (HealerComp != null && UnitMgr.HealTaskIcon != null)
                {
                    AddTaskButtons(1);
                    AddTask(ref LastTaskID, -1, TaskManager.TaskTypes.Heal, UnitMgr.HealTaskCategory, UnitMgr.HealTaskIcon);
                }

                //convert task:
                if (ConverterComp != null && UnitMgr.ConvertTaskIcon != null)
                {
                    AddTaskButtons(1);
                    AddTask(ref LastTaskID, -1, TaskManager.TaskTypes.Convert, UnitMgr.ConvertTaskCategory, UnitMgr.ConvertTaskIcon);
                }

                //builder task:
                if (BuilderComp != null)
                {
                    if (UnitMgr.BuildTaskIcon != null)
                    { //if the builder task icon has been set then add another task for it
                        AddTaskButtons(PlacementMgr.AllBuildings.Count + 1); //Update the amont of the task buttons:
                    }
                    else
                    {
                        AddTaskButtons(PlacementMgr.AllBuildings.Count); //Update the amont of the task buttons:
                    }
                    for (int i = 0; i < PlacementMgr.AllBuildings.Count; i++)
                    {
                        AddTask(ref LastTaskID, i, TaskManager.TaskTypes.PlaceBuilding, PlacementMgr.AllBuildings[i].TaskPanelCategory, PlacementMgr.AllBuildings[i].GetComponent<Building>().Icon);
                        TaskButtons[LastTaskID-1].GetComponent<Image>().color = (GameMgr.Factions[GameManager.PlayerFactionID].FactionMgr.HasReachedLimit(PlacementMgr.AllBuildings[i].Code)) ? Color.red : Color.white;
                    }

                    if (UnitMgr.BuildTaskIcon != null)
                    {
                        //builder component task button:
                        AddTask(ref LastTaskID, -1, TaskManager.TaskTypes.Build, UnitMgr.BuildTaskCategory, UnitMgr.BuildTaskIcon);
                    }

                }
               
                if (SelectionMgr.SelectedUnits.Count == 1) //if only one unit is selected.
                {
                    //check if only one APC is selected:
                    if (SelectionMgr.SelectedUnits[0].APCMgr)
                    {
                        int TaskCount = 0;
                        if (SelectionMgr.SelectedUnits[0].APCMgr.CurrentUnits.Count > 0)
                        {
                            TaskCount++;
                            if (SelectionMgr.SelectedUnits[0].APCMgr.EjectAllOnly == false)
                            {
                                TaskCount += SelectionMgr.SelectedUnits[0].APCMgr.CurrentUnits.Count;
                            }
                        }
                        if (SelectionMgr.SelectedUnits[0].APCMgr.CurrentUnits.Count < SelectionMgr.SelectedUnits[0].APCMgr.MaxAmount)
                        {
                            TaskCount++;
                        }

                        AddTaskButtons(TaskCount);
                        if (SelectionMgr.SelectedUnits[0].APCMgr.CurrentUnits.Count > 0)
                        {

                            //first task is for release all units
                            AddTask(ref LastTaskID, 0, TaskManager.TaskTypes.APCRelease, SelectionMgr.SelectedUnits[0].APCMgr.EjectAllOnlyTaskCategory, SelectionMgr.SelectedUnits[0].APCMgr.EjectAllIcon);

                            if (SelectionMgr.SelectedUnits[0].APCMgr.EjectAllOnly == false)
                            {
                                for (int i = 0; i < SelectionMgr.SelectedUnits[0].APCMgr.CurrentUnits.Count; i++)
                                {
                                    AddTask(ref LastTaskID, i + 1, TaskManager.TaskTypes.APCRelease, SelectionMgr.SelectedUnits[0].APCMgr.EjectOneUnitTaskCategory, SelectionMgr.SelectedUnits[0].APCMgr.CurrentUnits[i].Icon);
                                }
                            }
                        }
                        if (SelectionMgr.SelectedUnits[0].APCMgr.CurrentUnits.Count < SelectionMgr.SelectedUnits[0].APCMgr.MaxAmount)
                        {
                            AddTask(ref LastTaskID, -1, TaskManager.TaskTypes.APCCall, SelectionMgr.SelectedUnits[0].APCMgr.CallUnitsTaskCategory, SelectionMgr.SelectedUnits[0].APCMgr.CallUnitsIcon);
                            //first task is for release all units
                        }
                    }
                    //Invisibility:
                    if (SelectionMgr.SelectedUnits[0].InvisibilityMgr)
                    {
                        AddTaskButtons(1);
                        if (SelectionMgr.SelectedUnits[0].IsInvisible == true)
                        {
                            AddTask(ref LastTaskID, -1, TaskManager.TaskTypes.ToggleInvisibility, SelectionMgr.SelectedUnits[0].InvisibilityMgr.InvisibilityTasksCategory, SelectionMgr.SelectedUnits[0].InvisibilityMgr.GoVisibleSprite);
                        }
                        else
                        {
                            AddTask(ref LastTaskID, -1, TaskManager.TaskTypes.ToggleInvisibility, SelectionMgr.SelectedUnits[0].InvisibilityMgr.InvisibilityTasksCategory, SelectionMgr.SelectedUnits[0].InvisibilityMgr.GoInvisibleSprite);
                        }
                    }
                    //Multiple attacks:
                    if (SelectionMgr.SelectedUnits[0].MultipleAttacksMgr)
                    {
                        //Only if we're allowed to switch the attack:
                        if (SelectionMgr.SelectedUnits[0].MultipleAttacksMgr.CanSwitchAttackTypes == true)
                        {
                            AddTaskButtons(SelectionMgr.SelectedUnits[0].MultipleAttacksMgr.AttackTypes.Length - 1); //(why -1? because is already enabled! so we won't show it:)
                            for (int i = 0; i < SelectionMgr.SelectedUnits[0].MultipleAttacksMgr.AttackTypes.Length; i++)
                            {
                                if (SelectionMgr.SelectedUnits[0].MultipleAttacksMgr.AttackTypes[i].IsActive == false)
                                {
                                    AddTask(ref LastTaskID, i, TaskManager.TaskTypes.AttackTypeSelection, SelectionMgr.SelectedUnits[0].MultipleAttacksMgr.AttackTypesTaskCategory, SelectionMgr.SelectedUnits[0].MultipleAttacksMgr.AttackTypes[i].AttackIcon);
                                    //if the attack is in cooldown mode:
                                    if (SelectionMgr.SelectedUnits[0].MultipleAttacksMgr.AttackTypes[i].InCoolDownMode == true)
                                    {
                                        Image TaskImg = TaskButtons[LastTaskID - 1].gameObject.GetComponent<Image>();
                                        Color ImgColor = new Color(TaskImg.color.r, TaskImg.color.g, TaskImg.color.b, 0.5f);
                                        TaskImg.color = ImgColor;
                                    }
                                }
                            }
                        }
                    }

                    //Wandering:
                    if(SelectionMgr.SelectedUnits[0].CanWander == true) //can the unit wander? 
                    {
                        //add a new task button
                        AddTaskButtons(1);
                        Sprite WanderTaskSprite = (SelectionMgr.SelectedUnits[0].Wandering == true) ? UnitMgr.DisableWanderIcon : UnitMgr.EnableWanderIcon;
                        AddTask(ref LastTaskID, -1, TaskManager.TaskTypes.ToggleWander, UnitMgr.WanderTaskCategory, WanderTaskSprite);
                    }
                }
            }
        }

		public void AddTask (ref int TaskButtonID, int TaskID, TaskManager.TaskTypes TaskType, int Category, Sprite Sprite)
		{
			SetTaskButtonParent (TaskButtonID, Category);
			TaskButtons [TaskButtonID].gameObject.SetActive (true);
			TaskButtons [TaskButtonID].gameObject.GetComponent<Image> ().sprite = Sprite;
			TaskButtons [TaskButtonID].gameObject.GetComponent<UnitTaskUI> ().TaskType = TaskType;
			TaskButtons [TaskButtonID].gameObject.GetComponent<UnitTaskUI> ().TaskSprite = Sprite;
			TaskButtons [TaskButtonID].gameObject.GetComponent<UnitTaskUI> ().ID = TaskID;
			TaskButtonID++;
		}

		//a method that contrls showing messags to the player:
		public void ShowPlayerMessage(string Text, MessageTypes Type)
		{
			PlayerMessage.gameObject.SetActive (true);

			PlayerMessage.text = Text;

			PlayerMessageTimer = PlayerMessageReload;
		}

		//Peace time UI:
		public void UpdatePeaceTimeUI (float PeaceTime)
		{
			if (PeaceTimeText != null) {
				if (PeaceTime > 0) {
					PeaceTimePanel.gameObject.SetActive (true);

					int Seconds = Mathf.RoundToInt (PeaceTime);
					int Minutes = Mathf.FloorToInt (Seconds / 60.0f);

					Seconds = Seconds - Minutes * 60;

					string MinutesText = "";
					string SecondsText = "";

					if (Minutes < 10) {
						MinutesText = "0" + Minutes.ToString ();
					} else {
						MinutesText = Minutes.ToString ();
					}

					if (Seconds < 10) {
						SecondsText = "0" + Seconds.ToString ();
					} else {
						SecondsText = Seconds.ToString ();
					}

					PeaceTimeText.text = MinutesText + ":" + SecondsText;
				} else {
					PeaceTimePanel.gameObject.SetActive (false);
				}
			}
		}

        //Pause menu:
        public void TogglePause ()
        {
            if(GameManager.GameState == GameStates.Paused) //if the game was paused
            {
                //unpause it:
                PauseMenu.SetActive(false); //Hide the pause menu:
                //game is running again:
                GameManager.GameState = GameStates.Running;
            }
            else if(GameManager.GameState == GameStates.Running) //if the game is running
            {
                //pause it:
                PauseMenu.SetActive(true); //Show the pause menu
                GameManager.GameState = GameStates.Paused;
            }
        }

        //Hover health bar:
        public void TriggerHoverHealthBar(bool Enable, SelectionObj Source, float Height)
        {
            if (Enable == true) //enabling the hover health bar
            {
                if (Source != null && HoverHealthBarActive == false) //make sure we have a valid source and the hover health bar is inactive
                {
                    HoverSource = Source; //set the new source
                                          //enable the hover health bar:
                    HoverHealthBarCanvas.gameObject.SetActive(true);
                    //make the canvas a child object of the source obj:
                    HoverHealthBarCanvas.transform.SetParent(Source.MainObj.transform, true);
                    HoverHealthBarActive = true;
                    //set the new health bar canvas position, the height is specified in either the Unit or Building component.
                    HoverHealthBarCanvas.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, Height, 0.0f);
                }
            }
            else //disabling it
            {
                //make sure that the source is disabling it (or there is no source):
                if (HoverSource == null || HoverSource == Source)
                {
                    //disable the hover health bar:
                    HoverHealthBarCanvas.gameObject.SetActive(false);
                    //no transform parent anymore
                    HoverHealthBarCanvas.transform.SetParent(null, true);

                    HoverHealthBarActive = false;
                }
            }
        }

        //check if a selection obj is actually the source of the hover health bar
        public bool IsHoverSource(SelectionObj Obj)
        {
            if (Obj == null)
                return false;
            if (Obj == HoverSource)
                return true;
            return false;
        }

        //the method that updates the health bar of the unit/building that the player has their mouse on.
        public void UpdateHoverHealthBar(float Health, float MaxHealth)
        {
            //only proceed if there's actually a health bar canvas
            if (HoverHealthBarCanvas != null)
            {
                if (Health < 0.0f)
                    Health = 0.0f; //Health can't be below 0

                if (HoverHealthBarFull.gameObject != null && HoverHealthBarEmpty.gameObject != null) //make sure we have both sprites
                {
                    //Update the health bar:
                    float NewBarLength = (Health / MaxHealth) * HoverHealthBarEmpty.sizeDelta.x;
                    HoverHealthBarFull.sizeDelta = new Vector2(NewBarLength, HoverHealthBarFull.sizeDelta.y);
                    HoverHealthBarFull.localPosition = new Vector3(HoverHealthBarEmpty.localPosition.x - (HoverHealthBarEmpty.sizeDelta.x - HoverHealthBarFull.sizeDelta.x) / 2.0f, HoverHealthBarEmpty.localPosition.y, HoverHealthBarEmpty.localPosition.z);
                }
            }
        }

        //Tooltip:

        //a method to enable the tooltip panel:
        public void ShowTooltip(string Msg)
        {
            TooltipPanel.SetActive(true); //activate the panel

            TooltipMsg.text = Msg; //display the message
        }

        //a method to hide the tooltip panel:
        public void HideTooltip()
        {
            TooltipPanel.SetActive(false); //hide the panel

            TooltipMsg.text = ""; //remove the last message
        }

    }
}