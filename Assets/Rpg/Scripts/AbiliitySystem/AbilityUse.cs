using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
namespace RTSEngine { 
public class AbilityUse : NetworkBehaviour {
   public GameObject abilityPrefab;
        private GameObject abilityObject;
        private bool abilityFlag = false;
    private SpyAbility fba;
    private Stopwatch abilityCooldownTimer;
    private Button button;
    private Image fillImage;
    private GameObject player;
    public GameObject particle;
    public AnimatorOverrideController anim;
    public bool otherAbilityUse = false;

        GameManager gameMgr;
	// Use this for initialization

    public void onAbilityUse(GameObject btn)
    {
            if (otherAbilityUse)
                return;
            print("enered ability use");
           
        fillImage = btn.transform.GetChild(0).gameObject.GetComponent<Image>();
        button = btn.GetComponent<Button>();
        button.interactable = false;
        fillImage.fillAmount = 1;
        abilityCooldownTimer = new Stopwatch();
        abilityCooldownTimer.Start();
                CmdUseAbility(gameObject.tag);
       Job.make(resetImage(120f,  button));
            
        }
        [Command]
        public void CmdUseAbility(string team)
        {
            RpcUseAbility(team);
        }
        [ClientRpc]
        public void RpcUseAbility(string team)
        {
            if (gameObject.tag == team) { 
            fba = new SpyAbility();
                print("entered usean");
            if(abilityObject)
            fba.useAbility(anim, particle, gameObject, abilityObject);
            }

        }
     IEnumerator resetImage(float amountOfSeconds,Button button)
    {
        Image fillImage = button.transform.GetChild(0).gameObject.GetComponent<Image>(); 
            while (fillImage.fillAmount>0)
            {
                float amountOfDeduction = 1f / amountOfSeconds;

                fillImage.fillAmount -= amountOfDeduction;
                yield return new WaitForSeconds(1.0f);
            }
            fillImage.fillAmount = 0;
            button.interactable = true;
            yield return null;
    }
	void Start () {

              
            

        }

        // Update is called once per frame
        void Update () {
            if (!abilityFlag) { 
            if (gameObject.tag == "team1Rpg" )

            {
                print(gameObject.name + "team1");
                abilityObject = GameObject.FindGameObjectWithTag("team1RpgSpy");

            }
            else
            {
                    print(gameObject.name + "team2");
                abilityObject = GameObject.FindGameObjectWithTag("team2RpgSpy");
                    print(abilityObject.tag +"hi");

            }
            if (abilityObject) {
                    abilityObject.transform.GetChild(1).gameObject.SetActive(false);
                abilityFlag = true;
                }
            }
        }
 
}
}