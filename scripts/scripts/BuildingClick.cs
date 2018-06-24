using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BuildingClick : MonoBehaviour
{
    public Button yourButton;
    public GameObject swordsMan;

    private static int IdCount = 0;
    private GameObject g;

    void Start()
    {
        yourButton.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        print(yourButton.name);

      BuildBuilding b =  GameObject.Find("Client").GetComponent<BuildBuilding>();

        switch (yourButton.name)
        {

            case "barracks":
                b.barracksFunc = true;
                b.inBuilding = true;
                b.wait = false;
                b.canvasButton = true;

                break;
            case "townhall":
                b.townhallFunc = true;
                b.inBuilding = true;
                b.wait = false;
                b.canvasButton = true;
                break;
            case "stable":
                b.stableFunc = true;
                b.inBuilding = true;
                b.wait = false;
                b.canvasButton = true;
                break;
            case "farm":
                b.farmFunc = true;
                b.inBuilding = true;
                b.wait = false;
                b.canvasButton = true;
                break;
            case "forge":
                b.forgeFunc = true;
                b.inBuilding = true;
                b.wait = false;
                b.canvasButton = true;
                break;



        }

       /* Vector3 yy = new Vector3(UnityEngine.Random.Range(40.0f, 120.0f), 88.16667f, UnityEngine.Random.Range(15f, 50.0f));

        var angles = transform.rotation.eulerAngles;
        angles.z = 0;
        angles.x = 0;
        angles.y = 0;


        g = Instantiate(swordsMan, new Vector3(yy.x, yy.y, yy.z), Quaternion.Euler(angles));


        Unit unit = new Unit();
        unit.go = g;
        unit.unitName = swordsMan.name;
        unit.Enemy = false;
        unit.pos.x = yy.x;
        unit.pos.y = yy.y;
        unit.pos.z = yy.z;
        unit.rot = Quaternion.Euler(angles).y;
        unit.state = 0;

        unit.id = IdCount;
        IdCount += 1;
        GameObject c = GameObject.Find("Client").gameObject;
        Client cc = c.GetComponent<Client>();
        unit.go.GetComponent<ClickToMove>().enabled = true;
        unit.go.GetComponent<SoldierScript>().enabled = true;
        if (Input.GetMouseButtonUp(0))
            cc.CreateUnit(unit);  */



    }


    public static int getCountId()
    {

        return IdCount;
    }

}
