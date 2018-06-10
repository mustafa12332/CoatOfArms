using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace RTSEngine { 
public class playerHealth : MonoBehaviour {

    public UIProgressBar bar;
    private GameObject player;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            try { 
            if (bar == null)
                return;
            if (player == null)
            {
                if(GameManager.Instance)
                {
                    return;
                }
                if (GameManager.Instance.Factions[GameManager.PlayerFactionID].playerType == "strategy")
                    return;
                else
                {
                    if (GameManager.Instance.Factions[GameManager.PlayerFactionID].team == 0)
                    {
                        player = GameObject.FindGameObjectWithTag("team1Rpg");
                    }
                    else
                        player = GameObject.FindGameObjectWithTag("team2Rpg");
                }
            }
            if (player != null)
            {
                try
                {
                    this.bar.fillAmount = Mathf.Lerp(this.bar.fillAmount, (float)player.GetComponent<stats>().HealthSystem
                        .CurrentHealth / player.GetComponent<stats>().HealthSystem.MaxHealth, Time.deltaTime * 3);


                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        player.GetComponent<stats>().HealthSystem.doDamage(10);
                    }
                }
                catch(NullReferenceException e)
                {
                    return;
                }
            }
            }
            catch(NullReferenceException a)
            {
                return;
            }
        }
    }

}