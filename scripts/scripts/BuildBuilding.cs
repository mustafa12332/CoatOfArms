using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildBuilding : MonoBehaviour {

   // public Canvas buildingsM;
    public GameObject townHall;
    public GameObject Forge;
    public GameObject Barracks;
    public GameObject Farm;
    public GameObject Stable;

    private ClickToMove ctm;
  
    private GameObject worker;
    bool StableB = false;
   bool FarmB = false;
  public bool inBuilding = false;
    bool townH = false;
    bool BarracksB = false;
   bool ForgeB = false;
     bool inCreate = false;
    bool Build=false;
     public bool wait = false;

    bool ready = false;

    public bool barracksFunc = false;
    public bool stableFunc = false;
    public bool townhallFunc = false;
    public bool farmFunc = false;
    public bool forgeFunc = false;

    private static float limitToNextClick = 0.5f;
    private  float TimeFirstClick =0.5f;


    public bool canvasButton = false;

    bool clicked = false;


    GameObject c;
    GameObject go1;
    RaycastHit hit;
    Vector3 pos;
    private List<GameObject> myBuildings = new List<GameObject>();

    private void Awake() {
        Destroy(go1);
        Build = false;
        inBuilding = false;
        wait = false;
        pos = Vector3.zero;

     


    }
    // Update is called once per frame

    void Update() {
        if (pointer.selectedObjets == null)
        {
            print("null");
            return;
        }
        else
            foreach (GameObject go in pointer.selectedObjets)
                if (go.name == "worker(Clone)")
                {
             
                    worker = go;
                    //    buildingsM.gameObject.SetActive(true);
                    inBuilding = true;
                }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // the ray follows the mousr
        Physics.Raycast(ray, out hit, Mathf.Infinity);


        // temp.z = 10f; // Set this to be the distance you want the object to be placed in front of the camera.

        if (Input.GetKeyDown(KeyCode.B) || canvasButton)
        {
            canvasButton = false;
           Build = false;
         //   buildingsM.gameObject.SetActive(false);
            pos = Vector3.zero;
         

        }
  
        if ((Input.GetKeyDown(KeyCode.T) && inBuilding) || townhallFunc)
            {
                go1 = Instantiate(townHall) as GameObject;
                go1.transform.position = hit.point;
                PaintObjGreen(go1);
                townH = true;
          
            townhallFunc = false;
            }
        else if ((Input.GetKeyDown(KeyCode.F) && inBuilding) || forgeFunc) {
                go1 = Instantiate(Forge) as GameObject;
                go1.transform.position = hit.point;
                PaintObjGreen(go1);
                ForgeB = true;
            forgeFunc = false;
            }
        else if ((Input.GetKeyDown(KeyCode.U) && inBuilding) || barracksFunc)
        {
                go1 = Instantiate(Barracks) as GameObject;
                go1.transform.position = hit.point;
                PaintObjGreen(go1);
                BarracksB = true;
            barracksFunc = false;
            }
        else if ((Input.GetKeyDown(KeyCode.S) && inBuilding) || farmFunc) {
                go1 = Instantiate(Farm) as GameObject;
                go1.transform.position = hit.point;
                PaintObjGreen(go1);
                FarmB = true;
            farmFunc = false;
            }
        else if ((Input.GetKeyDown(KeyCode.H) && inBuilding) || stableFunc) {
                go1 = Instantiate(Stable) as GameObject;
                go1.transform.position = hit.point;
                PaintObjGreen(go1);
                StableB = true;
            stableFunc = false;
            }

        if ((inBuilding && !wait )&& go1!=null)
        {
            go1.transform.position = hit.point;
          //  Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
    

        }
        if (!ready&&inBuilding &&!worker.GetComponent<Animator>().GetBool("isAttack")) {


   

            if (!ready&&Input.GetMouseButtonUp(0) &&!clicked)
            {
                pointer.selectedObjets.Add(worker);
                TimeFirstClick = limitToNextClick;
            
                clicked = true;
          
            }
  
            //       RaycastHit2D hit2;

            if(clicked)
            TimeFirstClick -= Time.deltaTime;
           // if (TimeFirstClick != 0)
           // print(TimeFirstClick);





            if (!ready&&Input.GetMouseButtonDown(0) &&  (TimeFirstClick<0)&&worker != null)
            {
                ctm = worker.GetComponent<ClickToMove>();
                //    pointer.selectedObjets.Add(worker);
                pos = hit.point;
                if (ctm != null)
                    ctm.goTo(worker, pos);
                print("im in the funcktion  motha fucka");
       
                wait = true;
                Build = true;
            //    ready = true;
                clicked = false;
             //   if(ctm!=null&&worker!=null)
                   // ctm.goTo(worker, pos);
                TimeFirstClick = 0.5f;
           //     ready = true;

            }

            //   if (hit2 = Physics2D.Raycast(ray2.origin, new Vector2(0, 0)))
            //      Debug.Log(hit2.collider.name);

            //    if (hit2.collider.name != "BuildM")
            //       print("NULLLLLLLLLL");                
            //

            // Input.GetMouseButtonUp(0) && Physics2D.Raycast(ray2.origin, new Vector2(0, 0)).collider == null

            if (!ready&&ctm.stopUnits(worker.transform.position, 0) && Build && worker != null)
         {
                ctm = worker.GetComponent<ClickToMove>();
                ctm.stopUnit(worker);
                worker.GetComponent<Animator>().SetBool("isWalking", false);
                worker.GetComponent<Animator>().SetBool("isAttack", true);
                worker.GetComponent<ClickToMove>().enabled = false;
                ready = true;
                pointer.selectedObjets.Add(worker);
            }

            //  buildingsM.gameObject.SetActive(false);
            /*      pos = hit.point;
                     go1.transform.position = pos;

                     wait = true;
                     Build = false;
           //    Build = true; // to change the worker walking to the building here
                 }

             //      if (Build&&wait)
             //.   {*/

            if (ready) {
                if (!worker.GetComponent<Animator>().GetBool("isAttack"))
                {
                    worker.GetComponent<Animator>().SetBool("isWalking", false);
                    worker.GetComponent<Animator>().SetBool("isAttack", true);

                }
                pointer.selectedObjets.Add(worker);
                inBuilding = false;
                ready = false;
                clicked = false;
                Build = false;
                    wait = false;
                inCreate = false;
                    if (pos != Vector3.zero)
                    {
                    Destroy(go1);

                    if (townH)
                    {
                        c = Instantiate(townHall, pos, transform.rotation) as GameObject;
                        townH = false;
                    }
                    else if (ForgeB)
                    {
                        c = Instantiate(Forge, pos, transform.rotation) as GameObject;
                        ForgeB = false;
                    }
                    else if (BarracksB)
                    {
                        c = Instantiate(Barracks, pos, transform.rotation) as GameObject;
                        BarracksB = false;


                    }
                    else if (FarmB)
                    {
                         
                        c = Instantiate(Farm, pos, transform.rotation) as GameObject;
                        FarmB = false;


                    }
                    else if (StableB)
                    {
        
                        c = Instantiate(Stable, pos, transform.rotation) as GameObject;
                        StableB = false;

                    }
                    Unit u = new Unit();
                    u.go = c;
                    u.pos = c.transform.position;
                    u.id = Client.getMyBuildingId();
                    
                    Client.incBuildingID();
                    myBuildings.Add(c);
                    Build = false;
                    wait = false;
                    c.GetComponent<KeepHere>().worker = worker;
                    c.GetComponent<KeepHere>().enabled = true;
                    c.GetComponent<KeepHere>().fromFunc = true;
                    c.GetComponent<unit>().clientId = 1;
                    c.transform.Find("particles").gameObject.SetActive(true);
                    this.GetComponent<Client>().SendBuildingUpdate(c.name, pos, transform.rotation.y, Client.getMyConId());
                    Client.getMyplayerHandler().myBuildings.Add(u);
                    inBuilding = false;
                    worker = null;
                    pos = Vector3.zero;
                }
            }
        }
                if (Input.GetMouseButtonDown(1))
                {
            TimeFirstClick = 0.5f;
            wait = false;
            ready = false;
            clicked = false;
                    Build = false;
                    inBuilding = false;
                    StableB = false;
                    FarmB = false;
                    inBuilding = false;
                    townH = false;
                    BarracksB = false;
                    ForgeB = false;
                    Destroy(go1);

                }

    }
    void PaintObjGreen(GameObject obj) {

        Renderer[] rend = go1.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rend)
        {
            Material m = r.material;
            m.color = Color.green;
        }

    }
   public GameObject createBuilding(string bName, Vector3 pos,float rot) {
        Debug.Log("NAME:   " + bName);
     //  GameObject objPrefab = Resources.Load(bName) as GameObject;


        c = Instantiate(Resources.Load(bName), pos, transform.rotation) as GameObject;
        c.GetComponent<KeepHere>().enabled = true;
        c.GetComponent<KeepHere>().fromFunc = true;
        c.transform.Find("particles").gameObject.SetActive(true);


        return c;
        
    }


}
