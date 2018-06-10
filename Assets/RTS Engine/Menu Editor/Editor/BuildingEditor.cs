using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using RTSEngine;

/* Building Editor script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

[CustomEditor(typeof(Building))]
public class BuildingEditor : Editor {

	public SerializedProperty BuildingStates;
	public SerializedProperty BuildingResources;
	public SerializedProperty BuildingTasks;
    public SerializedProperty BonusResources;
	public SerializedProperty DestroyAward;
	public SerializedProperty DropOffResourceList;
	public SerializedProperty FactionColors;
	public SerializedProperty ConstructionStates;
	public SerializedProperty UpgradeBuildingResources;
	public SerializedProperty UpgradeRequiredBuildings;

	int TaskID = 0;

	private ReorderableList TestList;

	public override void OnInspectorGUI ()
	{
		Building Target = (Building)target;

		GUIStyle TitleGUIStyle = new GUIStyle ();
		TitleGUIStyle.fontSize = 20;
		TitleGUIStyle.alignment = TextAnchor.MiddleCenter;
		TitleGUIStyle.fontStyle = FontStyle.Bold;

		EditorGUILayout.LabelField ("Building:", TitleGUIStyle);
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		TitleGUIStyle.fontSize = 15;
		EditorGUILayout.LabelField ("General Building Settings:", TitleGUIStyle);
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		Target.Name = EditorGUILayout.TextField ("Building Name: ", Target.Name);
		Target.Code = EditorGUILayout.TextField ("Building Code: ", Target.Code);
		Target.Category = EditorGUILayout.TextField ("Building Category: ", Target.Category);
		Target.Description = EditorGUILayout.TextField ("Building Description: ", Target.Description);
		EditorGUILayout.LabelField ("Building Icon:");
		Target.Icon = EditorGUILayout.ObjectField (Target.Icon, typeof(Sprite), true) as Sprite;
		Target.FreeBuilding = EditorGUILayout.Toggle ("Free Building? (Belongs to no faction)", Target.FreeBuilding);
		Target.CanBeAttacked = EditorGUILayout.Toggle ("Can Be Attacked? ", Target.CanBeAttacked);
		Target.TaskPanelCategory = EditorGUILayout.IntField ("Task Panel Category: ", Target.TaskPanelCategory);
		Target.MinCenterDistance = EditorGUILayout.FloatField ("Minimum Center Distance: ", Target.MinCenterDistance);
        Target.ForceMinDistanceOnPlayer = EditorGUILayout.Toggle("Force Min Distance On Player?", Target.ForceMinDistanceOnPlayer);
	    Target.Radius = EditorGUILayout.FloatField ("Building Radius: ", Target.Radius);
		Target.FactionID = EditorGUILayout.IntField ("Faction ID: ", Target.FactionID);
		Target.PlacedByDefault = EditorGUILayout.Toggle ("Placed by default?", Target.PlacedByDefault);
		Target.AddPopulation = EditorGUILayout.IntField ("Population Slots To Add: ", Target.AddPopulation);

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Building Resource Settings:", TitleGUIStyle);
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		BuildingResources = serializedObject.FindProperty("BuildingResources");
		EditorGUILayout.PropertyField (BuildingResources, true);
		serializedObject.ApplyModifiedProperties();

		BonusResources = serializedObject.FindProperty("BonusResources");
		EditorGUILayout.PropertyField (BonusResources, true);
		serializedObject.ApplyModifiedProperties();

		Target.ResourceDropOff = EditorGUILayout.Toggle ("Is Resource Drop Off?", Target.ResourceDropOff);
		if (Target.ResourceDropOff == true) {
            EditorGUILayout.LabelField("Drop Off Position:");
            Target.DropOffPos = EditorGUILayout.ObjectField(Target.DropOffPos, typeof(Transform), true) as Transform;
			Target.AcceptAllResources = EditorGUILayout.Toggle ("Accept All Resources?", Target.AcceptAllResources);
			if (Target.AcceptAllResources == false) {
				DropOffResourceList = serializedObject.FindProperty("AcceptedResources");
				EditorGUILayout.PropertyField (DropOffResourceList, true);
				serializedObject.ApplyModifiedProperties();
			}
		}

		Target.PlaceNearResource = EditorGUILayout.Toggle ("Place Near Resource?", Target.PlaceNearResource);
		if (Target.PlaceNearResource == true) {
			Target.ResourceName = EditorGUILayout.TextField ("Resource Name:", Target.ResourceName);
			Target.ResourceRange = EditorGUILayout.FloatField ("Resource Range:", Target.ResourceRange);
		}

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Building Health Settings:", TitleGUIStyle);
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		Target.MaxHealth = EditorGUILayout.FloatField ("Maximum Building Health: ", Target.MaxHealth);
		Target.MinTaskHealth = EditorGUILayout.FloatField ("Minimum Health To Launch Task: ", Target.MinTaskHealth);
        Target.HoverHealthBarY = EditorGUILayout.FloatField("Hover Health Bar Height: ", Target.HoverHealthBarY);

        BuildingStates = serializedObject.FindProperty("BuildingStates");
		EditorGUILayout.LabelField ("Building States Parent Obj:");
		Target.BuildingStatesParent = EditorGUILayout.ObjectField (Target.BuildingStatesParent, typeof(GameObject), true) as GameObject;
		EditorGUILayout.PropertyField (BuildingStates, true);
		serializedObject.ApplyModifiedProperties();

		EditorGUILayout.LabelField ("Destruction Sound Effect:");
		Target.DestructionAudio = EditorGUILayout.ObjectField (Target.DestructionAudio, typeof(AudioClip), true) as AudioClip;
		EditorGUILayout.LabelField ("Destruction Object:");
		Target.DestructionObj = EditorGUILayout.ObjectField (Target.DestructionObj, typeof(GameObject), true) as GameObject;
		DestroyAward = serializedObject.FindProperty("DestroyAward");
		EditorGUILayout.PropertyField (DestroyAward, true);
		serializedObject.ApplyModifiedProperties();

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Building Upgrade Settings:", TitleGUIStyle);
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		Target.DirectUpgrade = EditorGUILayout.Toggle ("Upgrade Building Directly?", Target.DirectUpgrade);
		EditorGUILayout.LabelField ("Upgrade Building:");
		Target.UpgradeBuilding = EditorGUILayout.ObjectField (Target.UpgradeBuilding, typeof(Building), true) as Building;
		UpgradeBuildingResources = serializedObject.FindProperty("BuildingUpgradeResources");
		EditorGUILayout.PropertyField (UpgradeBuildingResources, true);
		serializedObject.ApplyModifiedProperties();
		UpgradeRequiredBuildings = serializedObject.FindProperty("UpgradeRequiredBuildings");
		EditorGUILayout.PropertyField (UpgradeRequiredBuildings, true);
		serializedObject.ApplyModifiedProperties();
		Target.BuildingUpgradeReload = EditorGUILayout.FloatField ("Building Upgrade Duration: ", Target.BuildingUpgradeReload);
		Target.UpgradeAllBuildings = EditorGUILayout.Toggle ("Upgrade All Buildings?", Target.UpgradeAllBuildings);

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Building Tasks:", TitleGUIStyle);
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		Target.MaxTasks = EditorGUILayout.IntField ("Max Simultaneous Tasks: ", Target.MaxTasks);

		BuildingTasks = serializedObject.FindProperty("BuildingTasksList");
		EditorGUILayout.PropertyField (BuildingTasks, true);
		serializedObject.ApplyModifiedProperties();

		/*//making sure there tasks to begin with:
		if(Target.BuildingTasksList.Count > 0)
		{
			//Tasks:
			//task to display:
			Building.BuildingTasksVars CurrentTask = Target.BuildingTasksList[TaskID];

            //settings that apply to all task types:
            CurrentTask.FactionSpecific = EditorGUILayout.Toggle("Faction Specific?", CurrentTask.FactionSpecific);
            if(CurrentTask.FactionSpecific == true)
            {
                CurrentTask.FactionCode = EditorGUILayout.TextField("Faction Code:", CurrentTask.FactionCode);
            }

            CurrentTask.Description = EditorGUILayout.TextField("Task Description:", CurrentTask.Description);
            CurrentTask.TaskPanelCategory = EditorGUILayout.IntField("Task Panel Category:", CurrentTask.TaskPanelCategory);
            EditorGUILayout.LabelField("Task Icon:");
            CurrentTask.TaskIcon = EditorGUILayout.ObjectField(CurrentTask.TaskIcon, typeof(Sprite), true) as Sprite;

            serializedObject.ApplyModifiedProperties();

            CurrentTask.TaskType = (Building.BuildingTasks)EditorGUILayout.EnumPopup("Task Type:", CurrentTask.TaskType);


            //show task settings depending on the task type:
            switch (CurrentTask.TaskType)
            {
                case Building.BuildingTasks.CreateUnit:
                    //upgrades:
                    CurrentTask.UpgradeTaskPanelCategory = EditorGUILayout.IntField("Task Panel Category:", CurrentTask.UpgradeTaskPanelCategory);
                    
                    break;
                case Building.BuildingTasks.Research:
                    break;
                case Building.BuildingTasks.Destroy:
                    break;
            }

		}*/

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Building Components:", TitleGUIStyle);
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

        EditorGUILayout.LabelField("Building Model:");
        Target.BuildingModel = EditorGUILayout.ObjectField(Target.BuildingModel, typeof(GameObject), true) as GameObject;
        EditorGUILayout.LabelField ("Building Plane:");
		Target.BuildingPlane = EditorGUILayout.ObjectField (Target.BuildingPlane, typeof(GameObject), true) as GameObject;
		EditorGUILayout.LabelField ("Building Selection Component:");
		Target.PlayerSelection = EditorGUILayout.ObjectField (Target.PlayerSelection, typeof(SelectionObj), true) as SelectionObj;
		EditorGUILayout.LabelField ("Construction Object:");
		Target.ConstructionObj = EditorGUILayout.ObjectField (Target.ConstructionObj, typeof(GameObject), true) as GameObject;
		ConstructionStates = serializedObject.FindProperty("ConstructionStates");
		EditorGUILayout.PropertyField (ConstructionStates, true);
		serializedObject.ApplyModifiedProperties();
		EditorGUILayout.LabelField ("Building Damage Effect:");
		Target.DamageEffect = EditorGUILayout.ObjectField (Target.DamageEffect, typeof(EffectObj), true) as EffectObj;
		EditorGUILayout.LabelField ("Units Spawn Position:");
		Target.SpawnPosition = EditorGUILayout.ObjectField (Target.SpawnPosition, typeof(Transform), true) as Transform;
		EditorGUILayout.LabelField ("Units Goto Position (right after spawning):");
		Target.GotoPosition = EditorGUILayout.ObjectField (Target.GotoPosition, typeof(Transform), true) as Transform;
		FactionColors = serializedObject.FindProperty("FactionColors");
		EditorGUILayout.PropertyField (FactionColors, true);
		serializedObject.ApplyModifiedProperties();

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		EditorGUILayout.LabelField ("Audio Clips:", TitleGUIStyle);
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		EditorGUILayout.LabelField ("Selection Sound Effect:");
		Target.SelectionAudio = EditorGUILayout.ObjectField (Target.SelectionAudio, typeof(AudioClip), true) as AudioClip;
		EditorGUILayout.LabelField ("Launch Task Sound Effect:");
		Target.LaunchTaskAudio = EditorGUILayout.ObjectField (Target.LaunchTaskAudio, typeof(AudioClip), true) as AudioClip;
		EditorGUILayout.LabelField ("Declined Task Sound Effect:");
		Target.DeclinedTaskAudio = EditorGUILayout.ObjectField (Target.DeclinedTaskAudio, typeof(AudioClip), true) as AudioClip;

		EditorUtility.SetDirty (Target);
	}

	public void ChangeTaskID (int Value, int Max)
	{
		int ProjectedID = TaskID + Value;
		if (ProjectedID < Max && ProjectedID >= 0) {
			TaskID = ProjectedID;
		}
	}
}
