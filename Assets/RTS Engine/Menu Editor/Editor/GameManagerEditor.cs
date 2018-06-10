using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RTSEngine;

/* Game Manager Editor script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor {

	int FactionID;
	public SerializedProperty FactionTypes;

	public override void OnInspectorGUI ()
	{
		GameManager Target = (GameManager)target;

		GUIStyle TitleGUIStyle = new GUIStyle ();
		TitleGUIStyle.fontSize = 20;
		TitleGUIStyle.alignment = TextAnchor.MiddleCenter;
		TitleGUIStyle.fontStyle = FontStyle.Bold;


		EditorGUILayout.LabelField ("Factions:", TitleGUIStyle);
		EditorGUILayout.Space ();

		TitleGUIStyle.fontSize = 15;
		EditorGUILayout.LabelField ("Faction Types:", TitleGUIStyle);
		EditorGUILayout.Space ();
		EditorGUILayout.HelpBox ("Each map can have a certain amount of possible faction types. When the player is in the map menu he can choose, depending on the map, a faction type.", MessageType.Info);
		FactionTypes = serializedObject.FindProperty("FactionDef");
		EditorGUILayout.PropertyField (FactionTypes, true);
		serializedObject.ApplyModifiedProperties();

		EditorGUILayout.LabelField ("Factions Amount: " + Target.Factions.Count.ToString ());
		EditorGUILayout.Space ();

		if (GUILayout.Button ("Add Faction")) {
			GameManager.FactionInfo NewFaction = new GameManager.FactionInfo ();
			Target.Factions.Add (NewFaction);

			FactionID = Target.Factions.Count - 1;
		}


		EditorGUILayout.Space ();
		EditorGUILayout.HelpBox ("Make sure to create the maximum amount that this map can handle. When fewer factions play on the map, the rest will be automatically removed.", MessageType.Info);

		EditorGUILayout.Space ();
		if (GUILayout.Button (">>")) {
			ChangeFactionID (1, Target.Factions.Count);
		}
		if (GUILayout.Button ("<<")) {
			ChangeFactionID (-1, Target.Factions.Count);
		}

		EditorGUILayout.Space ();
		int DisplayID = FactionID + 1;
		TitleGUIStyle.fontSize = 15;
		EditorGUILayout.LabelField ("Faction ID: " + DisplayID.ToString (), TitleGUIStyle);
		EditorGUILayout.Space ();


		Target.Factions [FactionID].Name = EditorGUILayout.TextField ("Faction Name: ", Target.Factions [FactionID].Name);
		Target.Factions [FactionID].Code = EditorGUILayout.TextField ("Faction Code: ", Target.Factions [FactionID].Code);
		Target.Factions [FactionID].FactionColor = EditorGUILayout.ColorField ("Faction Color: ", Target.Factions [FactionID].FactionColor);

		Target.Factions [FactionID].PlayerControl = EditorGUILayout.Toggle ("Player Controlled?: ", Target.Factions [FactionID].PlayerControl);
		EditorGUILayout.HelpBox ("Make sure that only one team is controlled the player.", MessageType.Info);

		Target.Factions [FactionID].MaxPopulation = EditorGUILayout.IntField ("Initial Maximum Population: ", Target.Factions [FactionID].MaxPopulation);
		EditorGUILayout.LabelField ("Capital Building:");
		Target.Factions [FactionID].CapitalBuilding = EditorGUILayout.ObjectField (Target.Factions [FactionID].CapitalBuilding, typeof(Building), true) as Building;
		EditorGUILayout.LabelField ("Faction Manager:");
		Target.Factions [FactionID].FactionMgr = EditorGUILayout.ObjectField (Target.Factions [FactionID].FactionMgr, typeof(FactionManager), true) as FactionManager;

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		if (GUILayout.Button ("Remove Faction")) {
			if (Target.Factions.Count > 2) {
                Target.Factions.RemoveAt(FactionID);
                ChangeFactionID(-1, Target.Factions.Count);
            } else {
				Debug.LogError ("The minimum amount of factions to have in one map is: 2!");
			}
		}

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		EditorGUILayout.Space (); 
		TitleGUIStyle.fontSize = 20;
		EditorGUILayout.LabelField ("General Settings:", TitleGUIStyle);
		EditorGUILayout.Space ();

		Target.RandomizePlayerControl = EditorGUILayout.Toggle ("Random Player Faction?: ", Target.RandomizePlayerControl);
		Target.MainMenuScene = EditorGUILayout.TextField ("Main Menu Scene: ", Target.MainMenuScene);
		Target.MapSize = EditorGUILayout.FloatField ("Map Size: ", Target.MapSize);
		Target.PeaceTime = EditorGUILayout.FloatField ("Peace Time (seconds): ", Target.PeaceTime);
		EditorGUILayout.LabelField ("General Audio Source:");
		Target.GeneralAudioSource = EditorGUILayout.ObjectField (Target.GeneralAudioSource, typeof(AudioSource), true) as AudioSource;

		EditorUtility.SetDirty (Target);
	}

	public void ChangeFactionID (int Value, int Max)
	{
		int ProjectedID = FactionID + Value;
		if (ProjectedID < Max && ProjectedID >= 0) {
			FactionID = ProjectedID;
		}
	}
}
