using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RTSEngine { 
public class theiving : NetworkBehaviour {
        int team;
        public GameObject thievingUi;
        public GameObject instaniatedThievingUi;
        public GameObject Canvas;
        public Animator anim;
        public AnimatorOverrideController animatorOverrideController;
        public bool nearCapital = false;
        private int theivedfactionId = -1;
        private int enemyTeam;
        private int amountStolen=0;
        public AnimationClip run;
        protected AnimationClipOverrides clipOverrides;
        // Use this for initialization
        public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
        {
            public AnimationClipOverrides(int capacity) : base(capacity) { }

            public AnimationClip this[string name]
            {
                get { return this.Find(x => x.Key.name.Equals(name)).Value; }
                set
                {
                    int index = this.FindIndex(x => x.Key.name.Equals(name));
                    if (index != -1)
                        this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
                }
            }
        }
        void Start () {
            anim = GetComponent<Animator>();
            animatorOverrideController = new AnimatorOverrideController(anim.runtimeAnimatorController);
            anim.runtimeAnimatorController = animatorOverrideController;
            clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
            animatorOverrideController.GetOverrides(clipOverrides);
            if (tag == "team1RpgSpy")
            {
                team = 0;
                enemyTeam = 1;
            }
            else
            {
                team = 1;
                enemyTeam = 0;
            }
		
	}
	
	// Update is called once per frame
	void Update () {
            if (amountStolen > 0)
            {
                clipOverrides["WK_worker_03_run"] = run;
                animatorOverrideController.ApplyOverrides(clipOverrides);
            }

            else { 
                clipOverrides["WK_worker_03_run"] = null;
                animatorOverrideController.ApplyOverrides(clipOverrides);

            }


            if (nearCapital)
                if (Input.GetKeyDown(KeyCode.E) && theivedfactionId != -1)

                {
                    int amount = GameManager.Instance.ResourceMgr.GetResourceAmount(theivedfactionId, "Metal");
                    print(amount + "amount");
                    if (amount < 100)
                    {
                        CmdDeductResource(enemyTeam, amount);
                    }
                    else
                        CmdDeductResource(enemyTeam, 100);

                    amountStolen = amount;

                }


        }
       

        [Command]
        void CmdDeductResource(int team,int amount)
        {
            RpcDeductResource(team,amount);
        }
        [ClientRpc]
        void RpcDeductResource(int team,int amount)
        {
            print("deduct" + amount);
            if(team == GameManager.Instance.Factions[GameManager.PlayerFactionID].team && GameManager.Instance.Factions[GameManager.PlayerFactionID].playerType=="strategy")
              GameManager.Instance.ResourceMgr.AddResource(GameManager.PlayerFactionID, "Metal", -amount);
        }
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Building>())
            {
                if(other.GetComponent<Building>().Name == "Capital" && other.GetComponent<Building>().team != team && amountStolen == 0)
                {
                    print(other.name);

                    if (!instaniatedThievingUi && GameObject.FindGameObjectWithTag("rpgCanvas"))
                    {
                        nearCapital = true;
                        theivedfactionId = other.GetComponent<Building>().FactionID;
                        print("instaniated canvas");
                        instaniatedThievingUi = Instantiate(thievingUi, GameObject.FindGameObjectWithTag("rpgCanvas").transform);
                    }
                   
                }
                else if (other.GetComponent<Building>().Name == "Capital" && other.GetComponent<Building>().team == team && amountStolen>0)
                {
                    CmdDeductResource(other.GetComponent<Building>().team, -amountStolen);
                    amountStolen = 0;

                }
            }
    }
        private void OnTriggerExit(Collider other)
        {
            print("exited " + other.name);
            if (other.GetComponent<Building>())
            {
                if (other.GetComponent<Building>().Name == "Capital" && other.GetComponent<Building>().team != team)
                {
                    nearCapital = false;
                    theivedfactionId = -1;
                    if(instaniatedThievingUi)
                    Destroy(instaniatedThievingUi);
                }
            }
        }
    }
}