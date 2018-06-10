using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/* Faction UI Info: script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
	public class FactionUIInfo : MonoBehaviour {

		[HideInInspector]
		public SinglePlayerManager.FactionVars Faction; //linked the faction's slot in the map manager script

		//all the below objects are children of the FactionUI object:
		public Image ColorImg; //UI Image that shows the faction's color.
		public InputField FactionNameInput; //UI InputField used to input/show the faction's name.
		public InputField PopulationInput; //UI InputField used to input/show the faction's initial amount of population slots.
		public Dropdown DifficultyMenu; //UI Dropdown used to display the difficulty levels as options for the difficulty menu, allowing the player to pick the difficulty level for a NPC faction
		public Dropdown FactionTypeMenu; //UI Dropdown used to display the list of possible faction types that can be used in the currently selected maps.
		public GameObject RemoveFactionButton; //The object that holds the button that has the "RemoveFaction" event on "Click ()".

		[HideInInspector]
		public SinglePlayerManager Mgr; //the map manager.

		//Changing the difficulty level of the NPC:
		public void OnDifficultyLevelChange ()
		{
			int Level = DifficultyMenu.value;
			//making sure that the chosen level exists.
			if (Level < Mgr.DifficultyLevels.Length) {
				//update it:
				Mgr.Factions [Mgr.Factions.IndexOf (Faction)].NPCDifficulty = Level;
			} else {
				Debug.LogError ("Level Mgr.Factions.IndexOf (Faction) "+Level.ToString()+" has not been defined in the map manager!");
			}
		}

		//changing the faction type:
		public void OnFactionTypeChange ()
		{
			int FactionType = FactionTypeMenu.value;
			//making sure that the chosen level exists.
			if (FactionType < Mgr.Maps[Mgr.CurrentMapID].FactionTypes.Length) {
				//update it:
				Mgr.Factions [Mgr.Factions.IndexOf (Faction)].FactionCode = Mgr.Maps[Mgr.CurrentMapID].FactionTypes[FactionType].Code;
			} else {
				Debug.LogError ("Level Mgr.Factions.IndexOf (Faction) "+FactionType.ToString()+" has not been defined in the map manager!");
			}
		}

		//Updating the faction color:
		public void ChangeColor ()
		{
			//each time the ColorImg has been clicked, we move to the next color:
			if (Mgr.AllowedColors.Length - 1 > Mgr.Factions [Mgr.Factions.IndexOf (Faction)].FactionColorID) {
				Mgr.Factions [Mgr.Factions.IndexOf (Faction)].FactionColorID++;
			} else {
				Mgr.Factions [Mgr.Factions.IndexOf (Faction)].FactionColorID = 0;
			}

			//inform the map manager about this faction's new color.
			Mgr.Factions [Mgr.Factions.IndexOf (Faction)].FactionColor = Mgr.AllowedColors [Mgr.Factions [Mgr.Factions.IndexOf (Faction)].FactionColorID];

			//update the actual color:
			ColorImg.color = Mgr.Factions [Mgr.Factions.IndexOf (Faction)].FactionColor;
		}

		//Updating the faction name
		public void OnFactionNameChange ()
		{
			if (FactionNameInput.text != "") { //if the new name is valid:
				//let the map manager know we updated the name:
				Mgr.Factions [Mgr.Factions.IndexOf (Faction)].FactionName = FactionNameInput.text;
			}
		}

		//Updating the initial amount of population slots:
		public void OnPopulationChange ()
		{
			int Amount;

			bool Result = System.Int32.TryParse (PopulationInput.text, out Amount);
			if (Result == true) { //making sure the new amount is valid:
				//make sure we don't go below the allowed limit:
				if (Amount >= Mgr.MinPopulation) {
					Mgr.Factions [Mgr.Factions.IndexOf (Faction)].InitialPopulation = Amount;
				} else {
					PopulationInput.text = Mgr.MinPopulation.ToString ();
				}
			} else {
				PopulationInput.text = Mgr.Factions [Mgr.Factions.IndexOf (Faction)].InitialPopulation.ToString ();
			}
		}

		//remove the faction from the list:
		public void RemoveFaction ()
		{
			Mgr.RemoveFaction (Mgr.Factions.IndexOf (Faction));
		}
	}
}