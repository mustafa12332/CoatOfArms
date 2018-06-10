using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class onbuttonpress : MonoBehaviour {
    public GameObject btn;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown((Int32.Parse(GetComponent< UnityEngine.UI.Text>().text)+KeyCode.Alpha0)))
        {
            if (btn.GetComponent<UnityEngine.UI.Button>().interactable)
                 btn.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        }
		
	}
   
}
