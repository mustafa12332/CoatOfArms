using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSEngine
{
	public class TaskManager : MonoBehaviour {

		[HideInInspector]
		public GameManager GameMgr;

		//task type
		public enum TaskTypes {
			Null,
			Mvt,
			PlaceBuilding, Build,
			ResourceGen, Collect,
			Convert,
			Heal,
			APCRelease, APCCall, 
			CreateUnit, DestroyBuilding, Research, TaskUpgrade, UpgradeBuilding, //building related tasks:
			ToggleInvisibility, //invisibility tasks
			AttackTypeSelection, Attack, //attack tasks
            ToggleWander //Wandering tasks
		};

		public void LaunchTask (Building Building, int TaskID, int NPCUnitSpawnerID, TaskTypes TaskType)
		{
			if (TaskType == TaskTypes.ResourceGen) {
                GameMgr.UIMgr.HideTooltip();

				//if the task is a resource gen one:
				ResourceGenerator Gen = Building.gameObject.GetComponent<ResourceGenerator> (); //look for the generator
				int ResourceID = Gen.ReadyToCollect [TaskID]; //get the resource id
				GameMgr.ResourceMgr.AddResource (Building.FactionID, Gen.Resources [ResourceID].Name, Gen.Resources [ResourceID].Amount); //add the resource to the faction
				Gen.Resources[ResourceID].Amount = 0;
				Gen.Resources [ResourceID].MaxAmountReached = false; //launch the timer again
				Gen.ReadyToCollect.Remove (TaskID);

				if(GameManager.PlayerFactionID == Building.FactionID) //if this is the local player:
				{
					if (GameMgr.UIMgr.SelectionMgr.SelectedBuilding == Building) { //if the building is selected.
						GameMgr.UIMgr.UpdateInProgressTasksUI (Building); //update the UI:
						GameMgr.UIMgr.UpdateBuildingTasks (Building);
					}
					AudioManager.PlayAudio (GameMgr.GeneralAudioSource.gameObject, Gen.Resources[ResourceID].CollectionAudioClip, false); //Launched task audio.
				}
			} else {
				if (TaskType == TaskTypes.CreateUnit) {
					GameMgr.Factions [Building.FactionID].CurrentPopulation++; //add population
					if (GameManager.PlayerFactionID == Building.FactionID)
						GameMgr.UIMgr.UpdatePopulationUI (); //if it's the local player then change the population UI.
                    //update the limits list:
                    GameMgr.Factions[Building.FactionID].FactionMgr.IncrementLimitsList(Building.BuildingTasksList[TaskID].UnitPrefab.Code);
                } else {
                    GameMgr.UIMgr.HideTooltip();
                }

				//Add the new task to the building's task queue
				Building.PendingTasksInfo Item = new Building.PendingTasksInfo ();
				Item.ID = TaskID;
				Item.UnitSpawnerID = NPCUnitSpawnerID;
				Item.Upgrade = (TaskType == TaskTypes.TaskUpgrade);
				Building.TasksQueue.Add (Item);

				if (Item.Upgrade == false) { //if the task is to upgrade

					//Launch the timer if there was no other tasks, else, the timer will launch automatically.
					if (Building.TasksQueue.Count == 1) {
						Building.TaskQueueTimer = Building.BuildingTasksList [TaskID].ReloadTime;
					}
					//Take the required resources:
					GameMgr.ResourceMgr.TakeResources (Building.BuildingTasksList [TaskID].RequiredResources, Building.FactionID);

				} else {
					//Launch the timer if there was no other tasks, else, the timer will launch automatically.
					if (Building.TasksQueue.Count == 1) {
						Building.TaskQueueTimer = Building.BuildingTasksList [TaskID].Upgrades [Building.BuildingTasksList [TaskID].CurrentUpgradeLevel].UpgradeReload;
					}
					//take the upgrade's resource and launch it.
					GameMgr.ResourceMgr.TakeResources (Building.BuildingTasksList [TaskID].Upgrades [Building.BuildingTasksList [TaskID].CurrentUpgradeLevel].UpgradeResources, Building.FactionID);
				}

				//custom events:
				GameMgr.Events.OnTaskLaunched (Building, Building.BuildingTasksList [TaskID]);

				if(GameManager.PlayerFactionID == Building.FactionID) //if this is the local player:
				{
					if (GameMgr.UIMgr.SelectionMgr.SelectedBuilding == Building) { //if the building is selected.
						GameMgr.UIMgr.UpdateInProgressTasksUI (Building); //update the UI:
						GameMgr.UIMgr.UpdateBuildingTasks (Building);
					}
					AudioManager.PlayAudio (GameMgr.GeneralAudioSource.gameObject, Building.LaunchTaskAudio, false); //Launched task audio.

					if(Item.Upgrade == true || Building.BuildingTasksList [TaskID].TaskType == Building.BuildingTasks.Research)
					{
						Building.BuildingTasksList [TaskID].Active = true;
						Building.CheckTaskUpgrades (TaskID, true, false);

						//if this building is selected.
						if (GameMgr.SelectionMgr.SelectedBuilding == Building) {
							//update the selection panel UI to show that this task is no longer in progress.
							GameMgr.UIMgr.UpdateInProgressTasksUI (Building);
							GameMgr.UIMgr.UpdateBuildingTasks (Building);
						}
					}
				}
			}
		}

		//this method gets the building task and returns what it means in the task manager.
		public static TaskManager.TaskTypes BuildingTaskType (Building.BuildingTasks Task)
		{
			TaskManager.TaskTypes Result = TaskManager.TaskTypes.CreateUnit;

			switch (Task) {
			case Building.BuildingTasks.Destroy:
				Result = TaskManager.TaskTypes.DestroyBuilding;
				break;
			case Building.BuildingTasks.Research:
				Result = TaskManager.TaskTypes.Research;
				break;
			case Building.BuildingTasks.CreateUnit:
				Result = TaskManager.TaskTypes.CreateUnit;
				break;
			}

			return Result;
		}
		//making the building tasks attribut type to task TaskManager.TaskTypes 
	}
}