using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class unit : MonoBehaviour{

    private float lastMovmentUpdate;
    private float movmentUpdarteRate = 200f;
    private bool Dead;
    public Vector2 ScreenPos;
    public bool OnScreen;
    public float Health;
    public int TeamNo;
    public int type;
    Animator anime;
    public Texture2D tex;
    public int clientId;
    public int ID;
    private void Start()
    {
        
        anime = this.GetComponent<Animator>();
        Dead = false;
        int myTeamNo = GameObject.Find("strCam").GetComponent<cameraController>().TeamNo;
        this.TeamNo = myTeamNo;
        Health = 120;
    }

    private void Update()
    {
 

     /*   if (Health <= 0)
        {
            if (!Dead)
            {
                anime.SetBool("isDead", true);
                GameObject c = GameObject.Find("Client").gameObject;
                this.gameObject.GetComponent<ClickToMove>().enabled = false;
                this.gameObject.GetComponent<BoxCollider>().enabled = false;
                this.gameObject.GetComponent<SoldierScript>().enabled = false;

                unit us = this.gameObject.GetComponent<unit>();
                Client cc = c.GetComponent<Client>();
                cc.RemovePlayer(this.gameObject);
                us.enabled = false;


                Dead = true;
                lastMovmentUpdate = Time.time;
            }
            else if (Dead && Time.time - lastMovmentUpdate > movmentUpdarteRate)
            {

                kill();
            }

        }  */
        if (Client.getMyConId() == TeamNo)
        {
            ScreenPos = Camera.main.WorldToScreenPoint(this.transform.position);
            if (pointer.unitsWithinScreenSpace(ScreenPos))

                if (!OnScreen)
                {
                    OnScreen = true;
                    pointer.UnitsOnScreen.Add(this.gameObject);
                    pointer.selectedObjets.Add(this.gameObject);
                    //    this.transform.Find("selected").gameObject.active = true;
                }
            //     else  
            //      if (OnScreen)
            //     pointer.RemoveFromScreenUnts(this.gameObject);
        }
    }
    public void kill()
    {

        Destroy(this.gameObject);

    }

}

