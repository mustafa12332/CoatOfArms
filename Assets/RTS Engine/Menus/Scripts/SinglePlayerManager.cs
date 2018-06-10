using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* Map Manager script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
	public class SinglePlayerManager : MonoBehaviour {

        [Header("General:")]

        public string MainMenuScene; //the name of the scene that you want the player to get back to from the the single player menu:

		public int MinPopulation = 5; //The minimum amount of population to start with.

        //Faction types:
        [System.Serializable]
		public class FactionTypeVars
		{
			public string Code;
			public string Name;
		}

		//Maps' info:
		[System.Serializable]
		public class MapsVars
		{
			public string MapScene = ""; //Name of the scene that includes the map.
			public string MapName = "Map"; //Name of the map to show in the UI menu.
			public string MapDescription = "Description"; //When a map is selected, this description is displayed.
			public int MaxFactions = 4; //Maximum amount of factions that this map supports.
			public FactionTypeVars[] FactionTypes; //The available types of factions that can play in this map.
		}
        [Header("Maps & Factions:")]
        public MapsVars[] Maps;
		[HideInInspector]
		public int CurrentMapID = -1; //holds the currently selected map ID.

		//an array that holds the faction's info:
		[System.Serializable]
		public class FactionVars
		{
			public FactionUIInfo FactionUI; //The faction UI slot associated to this faction.
			public string FactionCode;
			public string FactionName; //holds the faction's name.
			public Color FactionColor = Color.blue; //holds the faction's color.
			public int FactionColorID = 0; //holds the faction's color ID.
			public bool ControlledByPlayer = false; //is the faction controlled by the player or not?
			public int InitialPopulation = 10; //the initial amount of the faction's population slots.
			public int NPCDifficulty = 0; //holds the NPC difficulty ID.
		}
		[HideInInspector]
		public List<FactionVars> Factions;

		public Color[] AllowedColors; //Array that holds all the allowed colors for the factions

		public GameObject[] DifficultyLevels; //Array that holds the possible difficulty level objects (each difficulty level object is an empty object that has 3 components only: NPCBuildingPlacement, NPCResource and NPCArmy).

        [Header("UI:")]
        //Map info UI:
        public Text MapDescription; //A UI Text that shows the selected map's description.
        public Text MapMaxFactions; //A UI Text that shows the selected map's maximum allowed amount of factions.
        public Dropdown MapDropDownMenu; //A UI object that has a Dropdown component allowing the player to pick the map.

        public Transform FactionUIParent; //UI object parent of all the objects holding the "FactionUIInfo.cs" script. Each child object represents a faction slot.
        public FactionUIInfo FactionUISample; //A child object of the "FactionUIParent", that includs the "FactionUIInfo.cs" component. This represents a faction slot and holds all the information of the faction (color, name, population, etc).
        //This object will be used a sample as it will be duplicated to the number of the faction that the player chooses.
        // Use this for initialization

        void Awake () {

			DontDestroyOnLoad (this.gameObject); //we want this object to be passed to the map's scene.

			int i = 0;

			//go through all the maps:
			if (Maps.Length > 0) {
				CurrentMapID = 0;
				List<string> MapNames = new List<string>();

				for (i = 0; i < Maps.Length; i++) {
					//If we forgot to put the map's scene or we have less than 2 as max players, then show an error:
					if (Maps [i].MapScene == null) {
						Debug.LogError ("Map ID " + i.ToString () + " is invalid!");
					}
					if (Maps [i].MaxFactions < 2) {
						Debug.LogError ("Map ID " + i.ToString () + " max factions (" + Maps [i].MaxFactions.ToString () + ") is lower than 2!");
					} 
					MapNames.Add (Maps [i].MapName);
				}

				//set the map drop down menu options to the names of the maps.
				if (MapDropDownMenu != null) {
					MapDropDownMenu.ClearOptions ();
					MapDropDownMenu.AddOptions (MapNames);
				} else {
					Debug.LogError ("You must add a drop down menu to pick the maps!");
				}
			} else {
				//At least one map must be included:
				Debug.LogError ("You must include at least one map in the map manager!");
			}

			//go through all the difficulty levels:
			if (DifficultyLevels.Length > 0) {
				for (i = 0; i < DifficultyLevels.Length; i++) {
					//Show an error if one of the difficulty levels components has not been assigned:
					if (DifficultyLevels [i] == null) {
						Debug.LogError ("Difficulty Level ID " + i.ToString () + " has not been defined in the map manager!");
					}
				}
			} else {
				//We need at least one difficulty level:
				Debug.LogError ("You must include at least one difficulty level in the map manager!");
			}


			Factions = new List<FactionVars>(); //Initialize the factions info:
			//We create two factions at the start of the scene because it's the minimal amount of factions:
			for(i = 0; i < 2; i++)
			{
				FactionVars NewFaction = new FactionVars ();
				//set the faction's info:
				NewFaction.FactionName = "Faction " + i.ToString ();
				NewFaction.InitialPopulation = MinPopulation;

				//Creating a UI panel for each faction:
				if (i == 0) {
					NewFaction.FactionUI = FactionUISample;
					NewFaction.FactionUI.DifficultyMenu.gameObject.SetActive (false); //hide the difficulty menu for the first faction (as it is always player controlled).
				} else {
					//taking the FactionUISample as base to create all the rest:
					GameObject FactionUIClone = (GameObject) Instantiate (FactionUISample.gameObject, Vector3.zero, Quaternion.identity); 
					FactionUIClone.transform.SetParent (FactionUIParent); //make sure to set the FactionUI objects to the same parent.
					NewFaction.FactionUI = FactionUIClone.GetComponent<FactionUIInfo>(); 
					FactionUIClone.GetComponent<RectTransform> ().localScale = new Vector3 (1.0f, 1.0f, 1.0f);

					NewFaction.FactionUI.DifficultyMenu.gameObject.SetActive (true); //activate the difficulty menu:
				}

				//rest of the UI settings:
				NewFaction.FactionUI.RemoveFactionButton.gameObject.SetActive (false);
				NewFaction.FactionUI.PopulationInput.text = NewFaction.InitialPopulation.ToString (); //initial population
				NewFaction.FactionUI.FactionNameInput.text = NewFaction.FactionName.ToString (); //name
				NewFaction.FactionColorID = i; //color id
				NewFaction.FactionColor = AllowedColors [i]; //color
				NewFaction.FactionUI.ColorImg.color = NewFaction.FactionColor;
				NewFaction.NPCDifficulty = 0; //difficulty level:

				NewFaction.FactionUI.Mgr = this;

				Factions.Add (NewFaction); //add a new faction to the list:

				NewFaction.FactionUI.Faction = Factions[Factions.Count-1]; //link the FactionUI object with the map manager.
			}

			//The first faction belongs to the faction controlled by the player:
			Factions[0].ControlledByPlayer = true;

			UpdateMap (); //update the map's settings
		}

		//a method that adds a new faction:
		public void AddFaction () {
			//if we haven't reached the maximum allowed amount of factions
			if (Factions.Count < Maps [CurrentMapID].MaxFactions) {
				//create a new faction
				FactionVars NewFaction = new FactionVars ();
				NewFaction.FactionName = "Faction " + Factions.Count.ToString ();
				NewFaction.InitialPopulation = MinPopulation;

				//taking the FactionUISample as base to create all the rest:
				GameObject FactionUIClone = (GameObject)Instantiate (FactionUISample.gameObject, Vector3.zero, Quaternion.identity);
				NewFaction.FactionUI = FactionUIClone.GetComponent<FactionUIInfo> (); 
				FactionUIClone.transform.SetParent (FactionUIParent);//make sure to set the FactionUI objects to the same parent.
				FactionUIClone.GetComponent<RectTransform> ().localScale = new Vector3 (1.0f, 1.0f, 1.0f);

				//set the faction's UI info:
				NewFaction.FactionUI.RemoveFactionButton.gameObject.SetActive (true); //activate this button to allow the player to discard this faction
				NewFaction.FactionUI.DifficultyMenu.gameObject.SetActive (true); //NPC faction so activate the difficulty menu.
				NewFaction.FactionUI.PopulationInput.text = NewFaction.InitialPopulation.ToString (); //population
				NewFaction.FactionUI.FactionNameInput.text = NewFaction.FactionName.ToString (); //name
				NewFaction.FactionUI.ColorImg.color = NewFaction.FactionColor; //color
				NewFaction.NPCDifficulty = 0;

				//the faction type:
				NewFaction.FactionUI.FactionTypeMenu.ClearOptions ();
				if (Factions [0].FactionUI.FactionTypeMenu.options.Count > 0) {
					NewFaction.FactionUI.FactionTypeMenu.AddOptions (Factions [0].FactionUI.FactionTypeMenu.options);
				}
				NewFaction.FactionUI.FactionTypeMenu.value = 0;
				NewFaction.FactionCode = Maps [CurrentMapID].FactionTypes [0].Code;

				NewFaction.FactionUI.Mgr = this;

				Factions.Add (NewFaction); 
				NewFaction.FactionUI.Faction = Factions [Factions.Count - 1]; //to link the FactionUI object with the map manager.

			} else {

			}
		}

		//a method that removes the faction from the list:
		public void RemoveFaction (int ID) {
			//We can't remove the faction controlled by the player and we must at least keep two factions:
			if (ID == 0 || Factions.Count == 2) {
				return;
			}

			if (ID < Factions.Count) {
				//destroy the faction's UI object and remove it from the list.
				Destroy (Factions [ID].FactionUI.gameObject);
				Factions.RemoveAt (ID);
			}
		}

		//a method that updates the map:
		public void UpdateMap ()
		{
			if (MapDropDownMenu != null) {
				int MapID = MapDropDownMenu.value;
				//Make sure the map ID is valid and defined
				if (MapID < Maps.Length) {
					CurrentMapID = MapID; //Set the new map ID

					//check if the amount of factions does surpass the max amount of factions allowed for this map:
					if (Factions.Count > Maps [CurrentMapID].MaxFactions) {
						//Remove factions until the max amount factions is reached.
						for (int i = (Factions.Count - Maps [CurrentMapID].MaxFactions); i > 0; i--) {
							RemoveFaction (Factions.Count-1);
						}
					}

					UpdateMapUI ();
				}
			}
		}

		//update the map's UI by:
		public void UpdateMapUI ()
		{
			if (MapDescription != null) {
				MapDescription.text = Maps [CurrentMapID].MapDescription; //showing the selected map's description.
			}
			if (MapMaxFactions != null) {
				MapMaxFactions.text = Maps [CurrentMapID].MaxFactions.ToString (); //showing the selected map's maximum amount of 
			}

			List<string> FactionTypes = new List<string> ();
			if (Maps [CurrentMapID].FactionTypes.Length > 0) { //if there are actually faction types to choose from:
				for(int i = 0; i < Maps [CurrentMapID].FactionTypes.Length ; i++) //create a list with the names with all possible faction types:
				{
					FactionTypes.Add (Maps [CurrentMapID].FactionTypes [i].Name);
				}
			}

			for(int i = 0; i < Factions.Count; i++) //for each present faction:
			{
				Factions [i].FactionUI.FactionTypeMenu.ClearOptions (); //clear all the faction type options.
				if (FactionTypes.Count > 0) {
					//Add the faction types' names as options:
					Factions [i].FactionUI.FactionTypeMenu.AddOptions(FactionTypes);
					Factions [i].FactionUI.FactionTypeMenu.value = 0;
					Factions [i].FactionCode = Maps [CurrentMapID].FactionTypes [0].Code;
				}
			}
		}

		//start the game and loads the map's scene
		public void StartGame ()
		{
			SceneManager.LoadScene (Maps [CurrentMapID].MapScene);
		}

		//go back to the main menu:
		public void MainMenu ()
		{
			SceneManager.LoadScene (MainMenuScene);
			Destroy (this.gameObject);
		}
	}
}