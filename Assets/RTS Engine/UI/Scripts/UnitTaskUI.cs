using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/* Unit Task UI script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
	public class UnitTaskUI : MonoBehaviour {

		[HideInInspector]
		public int ID = -1;

		public TaskManager.TaskTypes TaskType;
		[HideInInspector]
		public Sprite TaskSprite; //for component tasks, they their sprites them in order to change the cursor properly if the settings allow

		//Health bars images for the multiple selection icon (both must be children of the selection icon):
		public Image EmptyBar;
		public Image FullBar;

		UIManager UIMgr;
		SelectionManager SelectionMgr;

		// Use this for initialization
		void Start () {
			UIMgr = GameManager.Instance.UIMgr;
			SelectionMgr = GameManager.Instance.SelectionMgr;
		}

		// Update is called once per frame:
		public void LaunchTask ()
		{
			UIMgr.LaunchTask (ID, TaskType, TaskSprite);
		}

        public void ShowTaskInfo()
        {
            //This method is called whenever the mouse hovers over a task button:
            //First activate both the task info menu and text objects to show them for the player:
            string Msg = "";

            //If the player is currently selecting building:
            if (SelectionMgr.SelectedBuilding != null)
            {

                //We'll show the task info for this building.

                if (TaskType != TaskManager.TaskTypes.ResourceGen)
                { //if it's not a resource collection task:
                    ResourceManager.Resources[] RequiredResources = new ResourceManager.Resources[0];
                    //First we will show the task's desription:

                    if (TaskType == TaskManager.TaskTypes.TaskUpgrade)
                    {
                        //if this is an upgrade task, then show the upgrade's description and set its required resources:
                        Msg = SelectionMgr.SelectedBuilding.BuildingTasksList[ID].Upgrades[SelectionMgr.SelectedBuilding.BuildingTasksList[ID].CurrentUpgradeLevel].UpgradeDescription;
                        RequiredResources = SelectionMgr.SelectedBuilding.BuildingTasksList[ID].Upgrades[SelectionMgr.SelectedBuilding.BuildingTasksList[ID].CurrentUpgradeLevel].UpgradeResources;
                    }
                    else if (TaskType == TaskManager.TaskTypes.UpgradeBuilding)
                    {
                        Msg = "Upgrade Building. ";
                        RequiredResources = SelectionMgr.SelectedBuilding.BuildingUpgradeResources;

                        //Then we will go through all the required buildings..
                        if (SelectionMgr.SelectedBuilding.UpgradeRequiredBuildings.Length > 0)
                        {
                            //And show them:
                            Msg += " <b>Required Buildings :</b>";

                            for (int i = 0; i < SelectionMgr.SelectedBuilding.UpgradeRequiredBuildings.Length; i++)
                            {
                                if (i > 0)
                                {
                                    Msg += " -";
                                }
                                Msg += " " + SelectionMgr.SelectedBuilding.UpgradeRequiredBuildings[i].Name;
                            }
                        }
                    }
                    else if (TaskType == TaskManager.TaskTypes.APCRelease)
                    {
                        if (ID == 0)
                        {
                            Msg = "Eject all units inside the APC.";
                        }
                        else
                        {
                            Msg = "Eject unit: " + SelectionMgr.SelectedBuilding.gameObject.GetComponent<APC>().CurrentUnits[ID - 1].Name + " from the APC.";
                        }

                    }
                    else if (TaskType == TaskManager.TaskTypes.APCCall)
                    {
                        Msg = "Call units to get in the APC";
                    }
                    else
                    {
                        RequiredResources = SelectionMgr.SelectedBuilding.BuildingTasksList[ID].RequiredResources;
                        Msg = SelectionMgr.SelectedBuilding.BuildingTasksList[ID].Description;
                    }


                    //Then we will go through all the required resources..
                    if (RequiredResources.Length > 0)
                    {
                        //And show them:
                        Msg += " <b>Required Resources :</b>";

                        for (int i = 0; i < RequiredResources.Length; i++)
                        {
                            if (i > 0)
                            {
                                Msg += " -";
                            }
                            Msg += " " + RequiredResources[i].Name + ": " + RequiredResources[i].Amount.ToString();
                        }
                    }
                }
                else
                {
                    int ResourceID = SelectionMgr.SelectedBuilding.GetComponent<ResourceGenerator>().ReadyToCollect[ID];
                    Msg = "Maximum amount reach! Click to collect " + SelectionMgr.SelectedBuilding.GetComponent<ResourceGenerator>().Resources[ResourceID].Amount.ToString() + " " + SelectionMgr.SelectedBuilding.GetComponent<ResourceGenerator>().Resources[ResourceID].Name;
                }

            }
            else
            {
                //If the player is currently selecting units, we'll display a description of the tasks they can do:

                //If it's a task that allows the player to place a building to construct it:
                if (TaskType == TaskManager.TaskTypes.PlaceBuilding)
                {
                    //Get the building associated with this task ID:
                    Building CurrentBuilding = UIMgr.PlacementMgr.AllBuildings[ID].GetComponent<Building>();
                    Msg += CurrentBuilding.Name + ": " + CurrentBuilding.Description;
                    if (CurrentBuilding.BuildingResources.Length > 0)
                    {
                        //Go through all the required resources to place this building:

                        for (int i = 0; i < CurrentBuilding.BuildingResources.Length; i++)
                        {
                            Msg += "\n" + CurrentBuilding.BuildingResources[i].Name + ": " + CurrentBuilding.BuildingResources[i].Amount.ToString();
                        }
                    }
                }
                else if (TaskType == TaskManager.TaskTypes.APCRelease)
                {
                    if (ID == 0)
                    {
                        Msg = "Eject all units inside the APC.";
                    }
                    else
                    {
                        Msg = "Eject unit: " + GameManager.Instance.SelectionMgr.SelectedUnits[0].gameObject.GetComponent<APC>().CurrentUnits[ID - 1].Name + " from the APC.";
                    }

                }
                else if (TaskType == TaskManager.TaskTypes.APCCall)
                {
                    Msg = "Call units to get in the APC.";
                }
                else if (TaskType == TaskManager.TaskTypes.ToggleInvisibility)
                {
                    Msg = "Toggle Invisibility.";
                }
                else if (TaskType == TaskManager.TaskTypes.Mvt)
                {
                    Msg = "Move unit(s).";
                }
                else if (TaskType == TaskManager.TaskTypes.Build)
                {
                    Msg = "Construct a building.";
                }
                else if (TaskType == TaskManager.TaskTypes.Collect)
                {
                    Msg = "Collect resources.";
                }
                else if (TaskType == TaskManager.TaskTypes.Attack)
                {
                    Msg = "Attack enemy units/buildings.";
                }
                else if (TaskType == TaskManager.TaskTypes.Heal)
                {
                    Msg = "Heal unit(s).";
                }
                else if (TaskType == TaskManager.TaskTypes.Convert)
                {
                    Msg = "Convert enemy unit(s).";
                }
                else if (TaskType == TaskManager.TaskTypes.AttackTypeSelection)
                {
                    Msg = "Switch attack type.";
                }
                else if(TaskType == TaskManager.TaskTypes.ToggleWander)
                {
                    Msg = "Toggle wandering.";
                }

            }

            //show the task info on the tooltip:
            UIMgr.ShowTooltip(Msg);
        }

		//Cancel a task in progress:
		public void CancelInProgressTask ()
		{
			if (SelectionMgr.SelectedBuilding != null) {
				SelectionMgr.SelectedBuilding.CancelInProgressTask(ID);
			}
		}

		//Upgrade building:
		public void UpgradeBuilding ()
		{
			if (SelectionMgr.SelectedBuilding != null) {
				SelectionMgr.SelectedBuilding.CheckBuildingUpgrade ();

			}
		}
	}
}