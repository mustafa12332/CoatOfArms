using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TestS : MonoBehaviour {
   public Button yourButton;
    public GameObject swordsMan;

    public static GameObject theBuilding;

    public static int IdCount = 0;
    private GameObject g;

    private static float limitToNextClick =4.5f;
    private float TimeFirstClick =4.5f;
    private bool
        startCount = false;
    private bool startCreating = false;
    private bool isCreating = false;
    void Start () {
        yourButton.onClick.AddListener(TaskOnClick);

    }

    void TaskOnClick()
    {
        startCount = true;
        startCreating = true;

    }
    public static int getCountId()
    {

        return IdCount;
    }

    private void Update()
    {
        if (startCreating)
        {
            if (startCount)
            {
                TimeFirstClick = limitToNextClick;

                startCount = false;
            }
                 //      print("in building units     " + TimeFirstClick);
            if (TimeFirstClick > 0)
                TimeFirstClick -= Time.deltaTime;

            if (TimeFirstClick < 0)
            {
                isCreating = true;
                startCreating = false;
            }
  
        }
        if (isCreating)
        {

            pointer.selectedObject = theBuilding;
            Vector3 yy;
            if (theBuilding != null && theBuilding.name == "barracks(Clone)")
            {
                yy = theBuilding.transform.position; //new Vector3(UnityEngine.Random.Range(40.0f, 120.0f), 88.16667f, UnityEngine.Random.Range(15f, 50.0f));

                float choosePos = UnityEngine.Random.Range(0f, 50.0f);
                print(choosePos + "    the calue");
                if (choosePos > 25f)
                {
                    yy.z -= 60;
                    yy.x += UnityEngine.Random.Range(-30f, 70.0f);

                }
                else
                {
                    yy.x += 50;

                    yy.z += UnityEngine.Random.Range(-30f, 70.0f);
                }

            }
            else
            {
                yy = theBuilding.transform.position;
                float choosePos = UnityEngine.Random.Range(0f, 50.0f);
                print(choosePos + "    the calue");

                if (choosePos > 25f)
                {
                    yy.z -= 100;
                    yy.x += UnityEngine.Random.Range(-30f, 70.0f);

                }
                else
                {
                    yy.x += 80;

                    yy.z += UnityEngine.Random.Range(-30f, 70.0f);
                }

                //    yy = theBuilding.transform.position; //new Vector3(UnityEngine.Random.Range(40.0f, 120.0f), 88.16667f, UnityEngine.Random.Range(15f, 50.0f));
                //  yy.x += 80;
                //   yy.z += UnityEngine.Random.Range(-20f, 50.0f);
            }

            //   else
            //   yy = new Vector3(UnityEngine.Random.Range(40.0f, 120.0f), 88.16667f, UnityEngine.Random.Range(15f, 50.0f));


            var angles = transform.rotation.eulerAngles;
            angles.z = 0;
            angles.x = 0;// UnityEngine.Random.Range(35.0f,60.0f);
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
                unit.go.GetComponent<unit>().clientId = 1;
                unit.id = IdCount;
                IdCount += 1;
                GameObject c = GameObject.Find("Client").gameObject;
                Client cc = c.GetComponent<Client>();
                unit.go.GetComponent<ClickToMove>().enabled = true;
                unit.go.GetComponent<SoldierScript>().enabled = true;



                //       if (Input.GetMouseButtonUp(0))
                cc.CreateUnit(unit);

                isCreating = false;

        }
    }
}
