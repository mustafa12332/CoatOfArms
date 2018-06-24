using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSEngine
{
	public class UnitManager : MonoBehaviour {

		public static UnitManager Instance;

		[Header("Free Units:")]
		public Unit[] FreeUnits; //units that don't belong to any faction here.
		public Color FreeUnitSelectionColor = Color.black;

		//Task panel:
		[Header("Task Components:")]
		//icons for component tasks for units and their UI task button parent category (if you don't want to use task components, then simply don't assign the icons below):
		public Sprite MvtTaskIcon;
		public int MvtTaskCategory = 0;

		public Sprite BuildTaskIcon; 
		public int BuildTaskCategory = 0;

		public Sprite CollectTaskIcon; 
		public int CollectTaskCategory = 0;

		public Sprite AttackTaskIcon; 
		public int AttackTaskCategory = 0;

		public Sprite HealTaskIcon; 
		public int HealTaskCategory = 0;

		public Sprite ConvertTaskIcon; 
		public int ConvertTaskCategory = 0;

        public Sprite EnableWanderIcon;
        public Sprite DisableWanderIcon;
        public int WanderTaskCategory = 0;

		[HideInInspector]
		public TaskManager.TaskTypes AwaitingTaskType; //registers the pending task type:
		public bool ChangeMouseTexture = false; //change the mouse texture when having an awaiting task type?

        //a list of upgrades that can be applied to units:
        public class UpgradeListVars
		{
			public float Speed = 0.0f;
			public float UnitDamage = 0.0f;
			public float BuildingDamage = 0.0f;
			public float AttackReload = -0.2f;
			public float SearchRange = 3.0f;
			public float FoWSize = 3.0f;
			public float MaxHealth = 30.0f;
		}

		public class FactionUnitUpradesVars
		{
			//a list of units who receive upgrades in the current game:
			[HideInInspector]
			public List<string> UnitsToUpgrade = new List<string>();
			[HideInInspector]
			//list of upgrades for units in the list above:
			public List<UpgradeListVars> UpgradeList = new List<UpgradeListVars>();
		}
		public FactionUnitUpradesVars[] FactionUnitUpgrades; //each faction has a slot in this list

		void Awake ()
		{
			if (Instance == null) {
				Instance = this;
			} else if (Instance != this) {
				Destroy (gameObject);
			}

			AwaitingTaskType = TaskManager.TaskTypes.Null;
		}

		void Start ()
		{
			FactionUnitUpgrades = new FactionUnitUpradesVars[GameManager.Instance.Factions.Count]; //create a slot for each faction:
			for (int i = 0; i < GameManager.Instance.Factions.Count; i++) {
				FactionUnitUpgrades [i] = new FactionUnitUpradesVars ();

				//reset the unit- and upgrade list at the start:
				FactionUnitUpgrades [i].UnitsToUpgrade.Clear ();
				FactionUnitUpgrades [i].UpgradeList.Clear ();
			}

		}

		//Component Tasks:
		public void SetAwaitingTaskType (TaskManager.TaskTypes TaskType, Sprite Sprite)
		{
			AwaitingTaskType = TaskType; //set the new task type
			if (ChangeMouseTexture == true && Sprite != null) { //if it is allowed to change the mouse texture
				//change it:
				Cursor.SetCursor (Sprite.texture, Vector2.zero, CursorMode.Auto);
			}
		}

		//reset the awaiting task type:
		public void ResetAwaitingTaskType()
		{
			AwaitingTaskType = TaskManager.TaskTypes.Null;
			Cursor.SetCursor (null, Vector2.zero, CursorMode.Auto);
		}

        void OnEnable ()
		{
			CustomEvents.UnitCreated += OnUnitCreated;
			CustomEvents.UnitConverted += OnUnitConverted;
		}

		void OnDisable ()
		{
			CustomEvents.UnitCreated -= OnUnitCreated;
			CustomEvents.UnitConverted -= OnUnitConverted;
		}

		void OnUnitCreated (Unit Unit)
		{
            if (Unit.FreeUnit) //if this is a free unit then don't proceed
                return;

			//when a reseach on units is done, they are added to a list in this class and whenever they are created, they get their upgrades here:
			if (FactionUnitUpgrades[Unit.FactionID].UnitsToUpgrade.Contains (Unit.Code)) { //when the unit is in the upgrade list:
				int ID = FactionUnitUpgrades[Unit.FactionID].UnitsToUpgrade.IndexOf(Unit.Code); //get the ID of this unit inside the upgrade list
				Unit.Speed += FactionUnitUpgrades[Unit.FactionID].UpgradeList[ID].Speed;
				Unit.MaxHealth += FactionUnitUpgrades[Unit.FactionID].UpgradeList[ID].MaxHealth;
				Unit.Health = Unit.MaxHealth;
				if (Unit.gameObject.GetComponent<Attack> ()) {
					Unit.gameObject.GetComponent<Attack> ().UnitDamage += FactionUnitUpgrades[Unit.FactionID].UpgradeList [ID].UnitDamage;
					Unit.gameObject.GetComponent<Attack> ().BuildingDamage += FactionUnitUpgrades[Unit.FactionID].UpgradeList [ID].BuildingDamage;
					//NEED A WAY TO MAKE DAMAGE HIGHER FOR CUSTOM DAMAGE AS WELL
					Unit.gameObject.GetComponent<Attack> ().AttackReload += FactionUnitUpgrades[Unit.FactionID].UpgradeList [ID].AttackReload;
					Unit.gameObject.GetComponent<Attack> ().SearchRange += FactionUnitUpgrades[Unit.FactionID].UpgradeList [ID].SearchRange;
				}
			}
		}

		//Converter events:
		void OnUnitConverted (Unit Unit, Unit TargetUnit)
		{
			if (TargetUnit != null) {
				//if the unit is selected:
				if (GameManager.Instance.SelectionMgr.SelectedUnits.Contains (TargetUnit)) {
					//if this is the faction that the unit got converted to or this is the only unit that the player is selecting:
					if (TargetUnit.FactionID == GameManager.PlayerFactionID || GameManager.Instance.SelectionMgr.SelectedUnits.Count == 1) {
						//simply re-select the player:
						GameManager.Instance.SelectionMgr.SelectUnit (TargetUnit, false);
					} else { //this means this is the faction that the target belonged to before and that player is selecting multiple units including the newly converted target unit
						//deselect it:
						GameManager.Instance.SelectionMgr.DeselectUnit (TargetUnit);
					}
				}
			}
		}
    }
}