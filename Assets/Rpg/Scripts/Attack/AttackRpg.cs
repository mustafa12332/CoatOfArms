using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RTSEngine { 
public class AttackRpg : NetworkBehaviour {

    public void onAttackUnit(GameObject other)
    {

                if (other.GetComponent<Unit>())
            {
                    if (GetComponent<stats>().Team != other.GetComponent<Unit>().team)
                    {
                        Unit enemy = other.GetComponent<Unit>();
                        int targetId = InputManager.Instance.SpawnedObjects.IndexOf(enemy.gameObject);
                        CmdLaunchAttack(targetId);
                    }
            }
            else if (other.GetComponent<stats>())
                {

                if (GameManager.Instance.Factions[GameManager.PlayerFactionID].team != GetComponent<stats>().Team)
                    return;
                if (other.GetComponent<stats>().isServer)
                {
                    print("server");
                    other.GetComponent<stats>().RpcTakeDamage((int)10);
                }
                else {
                    CmdAttackRpg(other.tag, 10);
                }
            }
            }
        public void onAttackBuilding(GameObject other)
        {
          
                Building enemyBuilding = other.GetComponent<Building>();
                int targetId = InputManager.Instance.SpawnedObjects.IndexOf(enemyBuilding.gameObject);
                CmdLaunchBuildingAttack(targetId);
            
        }
        [Command]
        public void CmdLaunchBuildingAttack(int targetId)
        {
            RpcLaunchBuildingAttack(targetId);
        }
        [ClientRpc]
        public void RpcLaunchBuildingAttack(int targetId)
        {
            if (targetId >= 0)
            {
                print("attacked building");
                GameObject enemyGameObject = InputManager.Instance.SpawnedObjects[targetId];
                enemyGameObject.GetComponent<Building>().AddHealthLocal(-10, gameObject);
                AttackManager.Instance.SpawnEffectObj(enemyGameObject.GetComponent<Building>().DamageEffect, enemyGameObject.gameObject, 0.0f, true);
            }
        }

        [Command]
        public void CmdAttackRpg(string tag,int damage)
        {
            RpcAttackRpg(tag, damage);
                }
        [ClientRpc]
        public void RpcAttackRpg(string tag,int damage)
        {
            GameObject rpgPlayer = GameObject.FindGameObjectWithTag(tag);
            if (rpgPlayer)
                rpgPlayer.GetComponent<stats>().takeDamageLocal(damage);
        }

        
        
        [Command]
        public void CmdLaunchAttack(int targetId)
        {
            RpcLaunchAttack(targetId);
        }
        [ClientRpc]
        void RpcLaunchAttack(int targetId)        {
            if (targetId >= 0)
            {
                GameObject enemyGameObject = InputManager.Instance.SpawnedObjects[targetId];
                enemyGameObject.GetComponent<Unit>().AddHealthLocal(-10, gameObject);
            }
        }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
}