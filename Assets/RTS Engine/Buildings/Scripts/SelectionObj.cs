using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/* Selection Obj script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */
namespace RTSEngine
{
	public class SelectionObj : MonoBehaviour {

		//This script gets added to an empty object that only have a collider and a rigidbody, the collider represents the boundaries of the object (building or resource) that can be selected by the player.
		//The main collider of the object will only be used for placement.

        public enum ObjTypes { Unit, Building, Resource,Rpg}
        public ObjTypes ObjType;

        public float MinimapIconSize = 0.5f;

		[HideInInspector]
		public GameObject MainObj; //The object that we're actually selecting.
        //main object components:
        [HideInInspector]
        public Unit UnitMgr;
        [HideInInspector]
        public Building BuildingMgr;
        [HideInInspector]
        public Resource ResourceMgr;
        [HideInInspector]
        public stats rpgManager;

        //the minimap icon here:
        [HideInInspector]
        public GameObject MinimapIcon;

        //other scripts
        SelectionManager SelectionMgr;
        UIManager UIMgr;

        void Start()
        {
            //get the components below
            SelectionMgr = GameManager.Instance.SelectionMgr;
            UIMgr = GameManager.Instance.UIMgr;

            //get the main object main component depending on the object type:
            switch(ObjType)
            {
                case ObjTypes.Unit:
                    UnitMgr = MainObj.GetComponent<Unit>();
                    break;
                case ObjTypes.Building:
                    BuildingMgr = MainObj.GetComponent<Building>();
                    break;
                case ObjTypes.Resource:
                    ResourceMgr = MainObj.GetComponent<Resource>();
                    break;
                case ObjTypes.Rpg:
                    rpgManager = MainObj.GetComponent<stats>();
                    break;
                default:
                    break;
            }

            //ask the minimap icon manager to create the icons for the main obj:
            MinimapIconManager.Instance.AssignIcon(this);

            gameObject.layer = 0; //Setting it to the default layer because raycasting ignores building and resource layers.

			//In order for collision detection to work, we must assign these settings to the collider and rigidbody.
			GetComponent<Collider> ().isTrigger = true;
            GetComponent<Collider>().enabled = true; //enable it

            if (GetComponent<Rigidbody> () == null) {
				gameObject.AddComponent<Rigidbody> ();
			}
			GetComponent<Rigidbody> ().isKinematic = true;
			GetComponent<Rigidbody> ().useGravity = false;
		}

		// Update is called once per frame
		public void SelectObj () {
			if (MainObj != null) { //Making sure we have linked an object or a resource object to this script:
				if (!EventSystem.current.IsPointerOverGameObject () && BuildingPlacement.IsBuilding == false) { //Make sure that the mouse is not over any UI element
                    switch (ObjType)
                    {
                        case ObjTypes.Unit: //in case it's a unit
                            UnitMgr.SelectUnit();
                            break;
                        case ObjTypes.Building: //If the object to select is a building:
                            if (BuildingMgr.Placed == true)
                            {
                                //Only select the building when it's already placed and when we are not attempting to place any building on the map:
                                BuildingMgr.FlashTime = 0.0f;
                                BuildingMgr.CancelInvoke("SelectionFlash");

                                SelectionMgr.SelectBuilding(BuildingMgr);
                            }
                            break;
                        case ObjTypes.Resource: //in case it's a resource
                            ResourceMgr.FlashTime = 0.0f;
                            ResourceMgr.CancelInvoke("SelectionFlash");

                            SelectionMgr.SelectResource(ResourceMgr);
                            break;
                        case ObjTypes.Rpg:
                            Debug.Log("hello");
                            rpgManager.FlashTime = 0.0f;
                            rpgManager.CancelInvoke("SelectionFlash");

                            SelectionMgr.selectRpg(rpgManager);
                            break;
                        default:
                            break;
                    }
				}
			}
		}

        //Hover health bar:

        //when the mouse is over this object
        void OnMouseEnter()
        {
            //if this is related to a resource
            if (ObjType == ObjTypes.Resource)
            {
                //we have no business here
                return;
            }
            //if the hover health bar feature is enabled
            if (UIMgr.EnableHoverHealthBar == true)
            {
                if (MainObj != null) //the main object is valid
                {
                    //Making sure we have linked an object or a resource object to this script:
                    if (!EventSystem.current.IsPointerOverGameObject() && BuildingPlacement.IsBuilding == false)
                    { //Make sure that the mouse is not over any UI  and we're not placing any building
                        float Health = 0.0f;
                        float MaxHealth = 0.0f;
                        float PosY = 0.0f;

                        int FactionID = -1;

                        switch (ObjType)
                        {
                            case ObjTypes.Unit: //in case it's a unit
                                if (UnitMgr.enabled == true)
                                { //make sure the unit is enabled
                                  //get the health stats from the unit:
                                    Health = UnitMgr.Health;
                                    MaxHealth = UnitMgr.MaxHealth;
                                    PosY = UnitMgr.HoverHealthBarY;
                                    FactionID = UnitMgr.FactionID;
                                }
                                break;
                            case ObjTypes.Building: //If the object to select is a building:
                                if (BuildingMgr.Placed == true && BuildingMgr.enabled == true)
                                { //if the building component is enabled and the actual building is placed
                                  //get the health stats from the building:
                                    Health = BuildingMgr.Health;
                                    MaxHealth = BuildingMgr.MaxHealth;
                                    PosY = BuildingMgr.HoverHealthBarY;
                                    FactionID = BuildingMgr.FactionID;
                                }
                                break;
                            default:
                                break;
                        }

                        //if this is not the player faction ID and we're only allowed to show hover health bars for friendly units/buildings:
                        if (FactionID != GameManager.PlayerFactionID && UIMgr.PlayerFactionOnly == true)
                        {
                            return; //STOP BEFORE WE GO FURTHER!
                        }

                        //enable the hover health bar:
                        UIMgr.TriggerHoverHealthBar(true, this, PosY);
                        //if the hover health bar was successfully enabled:
                        if (UIMgr.IsHoverSource(this))
                        {
                            //update the health:
                            UIMgr.UpdateHoverHealthBar(Health, MaxHealth);
                        }
                    }
                }
            }
        }

        //if the mouse leaves this object
        void OnMouseExit()
        {
            //if the hover health bar feature is enabled
            if (UIMgr.EnableHoverHealthBar == true)
            {
                //disable the hover health bar:
                UIMgr.TriggerHoverHealthBar(false, this, 0.0f);
            }
        }

    }
}