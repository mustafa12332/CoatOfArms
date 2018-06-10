using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace RTSEngine{
    public class stats : NetworkBehaviour {
        public string Name;
        public string Description;
        public Sprite Icon;
        private int factionId;
        public float FlashTime = 0;
        private int team;
        private HealthSystem HSystem;
        private const int CURRENTHEALTH = 1000, MAXHEALTH = 1000;
        public GameObject DeadCanvas;
        public SelectionObj PlayerSelection;
        public GameObject rpgPlane;
        private Animator anim;
        private Canvas canvas;
        private GameObject thirdPersonCamera;
        private int respawnTime = 60;
        private bool deadFlag = false;
        private Vector3 startPosition;

        public Vector3 StartPosition
        {
            get { return startPosition; }
            set { startPosition = value; }
        }
    public bool DeadFlag
        {
            get { return deadFlag; }
            set { deadFlag = value; }
        }
    public int Team
        {
            get { return team; }
            set { team = value;}
        }
    public Canvas Canvas
        {
            get { return canvas; }
            set { canvas = value; }
        }
    public HealthSystem HealthSystem
        {
            get { return this.HSystem; }
            set { HSystem = value; }
        }
        public  int FactionId{
        get { return factionId; }
        set { factionId = value; }
        }
    private void Start()
    {
            PlayerSelection.MainObj = gameObject;
            HSystem = new HealthSystem(CURRENTHEALTH, MAXHEALTH);
            anim = GetComponent<Animator>();

    }
        public void Update()
        {

            if (FlashTime > 0)
            {
                FlashTime -= Time.deltaTime;
            }
            if (FlashTime < 0)
            {
                //if the flash timer is over:
                FlashTime = 0.0f;
                CancelInvoke("SelectionFlash");
                rpgPlane.gameObject.SetActive(false);
                rpgPlane.GetComponent<Renderer>().material.color = Color.green;
            }
        }
        public void SelectionFlash()
        {
            rpgPlane.gameObject.SetActive(!rpgPlane.gameObject.activeInHierarchy);
        }
        public void onTakingDamage(int amount)
        {
            print("entered here");
            CmdTakeDamage((int)amount);
        }
        [Command]
    public void CmdTakeDamage(int amount)
        {
            print(gameObject.name +"from host");
            RpcTakeDamage(amount);
        }
    [ClientRpc]
    public void RpcTakeDamage(int amount)
        {
            print(gameObject.name + "\t" + amount);
           bool dead = HSystem.doDamage(amount);
            if (!deadFlag) {
            if (dead)
            {
                anim.SetBool("isDead", true);
                if (canvas)
                {
                Transform deadCanvas = canvas.transform.GetChild(1);
                deadCanvas.gameObject.SetActive(true);
                deadCanvas.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { respawnPlayer(deadCanvas.gameObject); });
                deadCanvas.transform.GetChild(2).GetComponent<Button>().interactable = false;
                StartCoroutine(respawnTimer(deadCanvas.gameObject));
                }
                deadFlag = true;
            }
            else
            {
                anim.SetTrigger("takeingDamage");
             
            }
            }

        }
        public void takeDamageLocal(int amount)
        {
            bool dead = HSystem.doDamage(amount);
            if (!deadFlag)
            {
                if (dead)
                {
                    anim.SetBool("isDead", true);
                    if (canvas)
                    {
                        Transform deadCanvas = canvas.transform.GetChild(1);
                        deadCanvas.gameObject.SetActive(true);
                        deadCanvas.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { respawnPlayer(deadCanvas.gameObject); });
                        deadCanvas.transform.GetChild(2).GetComponent<Button>().interactable = false;
                        StartCoroutine(respawnTimer(deadCanvas.gameObject));
                    }
                    deadFlag = true;
                }
                else
                {
                    anim.SetTrigger("takeingDamage");

                }
            }
        }

        private void respawnPlayer(GameObject deadCanvas)
        {
            HSystem.CurrentHealth = MAXHEALTH;
            deadCanvas.SetActive(false);
            if(StartPosition!=null)
                gameObject.transform.position = StartPosition;
            anim.SetBool("isDead", false);
            respawnTime = 60;
            deadFlag = false;
        }

        public IEnumerator respawnTimer(GameObject DeadCanvas)
        {
            Text text = DeadCanvas.transform.GetChild(1).GetChild(1).GetComponent<Text>();
            while (respawnTime >0)
            {
             
                text.text = "You Will Be Able To Respawn In \n" + respawnTime;
                respawnTime -= 1;
                yield return new WaitForSeconds(1f);
            }
          
            text.text = "Press Respawn Button Inorder to respawn";
           
            Transform deadCanvas = canvas.transform.GetChild(1);
            deadCanvas.transform.GetChild(2).GetComponent<Button>().interactable=true;
            
        }

    }
}