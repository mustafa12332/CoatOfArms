using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class pointer : MonoBehaviour {

    RaycastHit hit;  // where the ray of the mouse is hitting
    public static GameObject selectedObject;
    private Vector3 mouseClick;
    public static ArrayList selectedObjets = new ArrayList();
    public  GameObject hitObject;
    public GameObject townHall;
    public   Canvas c;
    private bool showC = false;
    public static int myConId; 

    
    public GameObject buildCanvas;
    public GameObject unitscanvas;
    public GameObject townCanvas;
    public GameObject unitsControler;

    public static ArrayList unitsInDrag = new ArrayList();
    public static ArrayList UnitsOnScreen = new ArrayList();

    public GUIStyle mouseDragSkin;

    public bool inBuilding = false;
    public bool finishDragOnThisFrame;
    private static Vector3 mouseDownPoint;
    private static Vector3 mouseUpPoint;
    private static Vector3 mouseCurrPoint;

    private static float limitToDeclareDrag = 1.0f;
    private static float TimeToDeclareDrag;
    private static Vector2 startDrag;
    private static float dragZone = 1.3f;
    public static bool isDrag = false;
    bool townH = false;
    private bool menuActive = false;
    float BoxLeft, BoxTop, BoxWidth, BoxHeight;
    private static Vector2 startBox;
    private static Vector2 endBox;
    public GameObject Target;
    GameObject go1;
    Client client;


    public Texture2D cursorTextureonEnemy;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;




    void Awake(){
        mouseClick = Vector3.zero;
        client = GameObject.Find("Client").gameObject.GetComponent<Client>();
    }
    private void Start()
    {

      

       
    }
    // Update is called once per frame
    void Update () {

    
     //   Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
     //   if (hit2 = Physics2D.Raycast(ray2.origin, new Vector2(0, 0)))
      //      Debug.Log(hit2.collider.name);

        if(Client.isStarted)
            c.gameObject.SetActive(true);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // the ray follows the mousr

        Physics.Raycast(ray, out hit, Mathf.Infinity);

        mouseCurrPoint = hit.point;

        
        GameObject hitObject = hit.transform.root.gameObject;
        Debug.Log(hitObject.name);

        if (hitObject.gameObject.GetComponent<unit>() != null && hitObject.gameObject.GetComponent<unit>().clientId == 0)
        {
            Cursor.SetCursor(cursorTextureonEnemy, hotSpot, cursorMode);
            Unit Enemy;
            foreach (Unit u in Client.getMyplayerHandler().EnemyUnits)
                if (hitObject.gameObject == u.go)
                {
                    Enemy = u;


                    if (Input.GetMouseButton(1))
                    {

                        if (selectedObjets != null)
                            foreach (GameObject go in selectedObjets)
                               SoldierScript.attackEnemy(Enemy, go);


                    }

                }
        }
        else
            Cursor.SetCursor(null, Vector2.zero, cursorMode);


        if (Input.GetMouseButtonDown(0)){
            if(hitObject.name!="strCam")
                if(!menuActive)
            TimeToDeclareDrag = limitToDeclareDrag;
            startDrag = Input.mousePosition;
            mouseDownPoint = hit.point;

            clearSelection();
        }
        else if (Input.GetMouseButton(0)) {
         
            if (!isDrag){
                TimeToDeclareDrag -= Time.deltaTime;
                if (TimeToDeclareDrag <= 0 || isClientDragging(startDrag, Input.mousePosition))
                    isDrag = true;
            }
        }

        else if (Input.GetMouseButtonUp(0)) {
            finishDragOnThisFrame = true;
            TimeToDeclareDrag = 0;
            isDrag = false;
        }

        if (Input.GetMouseButtonDown(0)){ //left buttn mouse clicked
            if (!isDrag) {           
                mouseClick = hit.point;
                SelectObject(hitObject);
            }
        }
        if (hit.collider.name == "Terrain" && !isDrag) {
   
            if (Input.GetMouseButtonUp(1)){
                GameObject TargetObject = Instantiate(Target, hit.point, Quaternion.identity) as GameObject; //clonoes the object and keeps on screen
                TargetObject.name = "Targetint"; // the game object created is name "Targetint"
            }
        }

        if (isDrag)
        {
           
          BoxWidth = Camera.main.WorldToScreenPoint(mouseDownPoint).x - Camera.main.WorldToScreenPoint(mouseCurrPoint).x;
            BoxHeight = Camera.main.WorldToScreenPoint(mouseDownPoint).y - Camera.main.WorldToScreenPoint(mouseCurrPoint).y;
            BoxLeft = Input.mousePosition.x;
            BoxTop = (Screen.height - Input.mousePosition.y) - BoxHeight;


            if (floatToBool(BoxWidth))
                if (floatToBool(BoxHeight))
                    startBox = new Vector2(Input.mousePosition.x, Input.mousePosition.y + BoxHeight);
                else
                    startBox = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            else
            if (!floatToBool(BoxWidth))
                if (floatToBool(BoxHeight))
                    startBox = new Vector2(Input.mousePosition.x + BoxWidth, Input.mousePosition.y + BoxHeight);
                else
                    startBox = new Vector2(Input.mousePosition.x + BoxWidth, Input.mousePosition.y);

            endBox = new Vector2(startBox.x + Unsigned(BoxWidth),
                startBox.y - Unsigned(BoxHeight));


        }

    }

    private void LateUpdate()
    {
        //   Debug.Log("LATE PDATE: " + UnitsOnScreen.Count);
     
        if ((isDrag ) && UnitsOnScreen.Count > 0)
        {
            clearSelection();
            if (unitsInDrag != null)
              cleardrag();

            for (int i = 0; i < UnitsOnScreen.Count; i++){

                GameObject unitObj = UnitsOnScreen[i] as GameObject;
                if (unitObj != null) { 
                    unit UnitScript = unitObj.GetComponent<unit>();
                    GameObject selectedObj = unitObj.transform.Find("selected").gameObject;
             //   Debug.Log(Client.getMyConId());
                    if (!UnitAlreadySelected(unitObj)){// && (myConId == Client.getMyConId())
                        if (UnitsInDragBox(UnitScript.ScreenPos)
                            && unitObj.gameObject.GetComponent<SoldierScript>().enabled == true) 
                        {
                            
                            selectedObj.active = true;
                            unitsInDrag.Add(unitObj);
                            selectedObjets.Add(unitObj);
                            hideCanvases();
                            if(unitObj.name=="worker(Clone)")
                                buildCanvas.SetActive(true);
                            else
                            unitsControler.SetActive(true);
                        }
                        else
                        {
                            if (UnitAlreadySelected(unitObj)) {
                                if (!UnitsInDragBox(UnitScript.ScreenPos))
                                    selectedObject.active = false;


                            }
                        }
                   }
                }
            }
        }
    }

    void SelectObject(GameObject obj)
    {
        
        if (obj != null) {
            if (obj.name == "worker(Clone)")
            {
                hideCanvases();
                buildCanvas.SetActive(true);
        

            }
            else if (obj.name == "townHallT")
            {
                hideCanvases();
                townCanvas.SetActive(true);
                TestS.theBuilding = obj;

            }
            else if (obj.name == "barracks(Clone)")
            {
                hideCanvases();
                unitscanvas.SetActive(true);
                TestS.theBuilding = obj;

            }
            else if (obj.gameObject.GetComponent<SoldierScript>() != null)
            {
                hideCanvases();
                unitsControler.SetActive(true);
            }
            /*       if (obj.GetComponent<KeepHere>() != null)
                   {
                       if (!showC)
                       {
                           c.gameObject.active = true;
                           showC = true;
                       }

                   }
                   if (obj.name == "BuildM" || obj.name == "BackG")
                   {

                       print("MOTAAAAAAAA FUCKAAAA");
                       return;


                   }
                   else if (showC && obj.GetComponent<KeepHere>() == null)
                   {
                       c.gameObject.active = false;
                       showC = false;
                   }*/
            int y = GameObject.Find("strCam").GetComponent<cameraController>().TeamNo;
            unit unintScript = obj.GetComponent<unit>();
            if (unintScript!=null&&unintScript.TeamNo != y){
                obj.transform.Find("selected").gameObject.active =false;
                return;

            }

            else if (ShiftKeyDown()) { 
           
            selectedObjets.Add(obj);
            selectedObject = obj;
        
            }
           
        }
       if (selectedObject != null){

            if (obj == selectedObject)
                return;
            else if (obj.name == "Terrain")
            {
                clearSelection();
            }
            else
            {
                clearSelection();
                selectedObject = obj;
                Debug.Log("RE SELECTED  " + selectedObject.name);
                obj.transform.Find("selected").gameObject.active = true;
                selectedObjets.Add(obj);
            }
        }
        else if(obj.name != "Terrain"){
      //      if (obj.name != "worker(Clone)")
      //      {
            //    hideCanvases();
           //     unitsControler.SetActive(true);
        //    }
            obj.transform.Find("selected").gameObject.active = true;
            selectedObjets.Add(obj);
            selectedObject = obj;
        }
    }





    public static bool UnitsInDragBox(Vector2 UnitScreenPos){
        if ((UnitScreenPos.x > startBox.x && UnitScreenPos.y < startBox.y)
            && (UnitScreenPos.x < endBox.x && UnitScreenPos.y > endBox.y))
            return true;
        return false;
    }

      public static bool UnitAlreadySelected(GameObject Unit) { 
        if (unitsInDrag.Count > 0){
            for (int i = 0; i < unitsInDrag.Count; i++){
                GameObject ArrayListUnit = unitsInDrag[i] as GameObject;

                if (ArrayListUnit == Unit)
                    return true;
            }
            return false;
        }

        return false;
    }

    public static void InsertToSelectedUnits(){

  
           selectedObjets.Clear();

        if (unitsInDrag.Count > 0){

            for (int i = 0; i < unitsInDrag.Count; i++) {

                GameObject unitObj = unitsInDrag[i] as GameObject;
                if (!UnitAlreadySelected(unitObj)) {

                    //    unitObj.GetComponent<unit>().Selected = true;
                    if (unitObj.gameObject.GetComponent<SoldierScript>().enabled == true)
                    {
                        selectedObjets.Add(unitObj);
                        unitObj.transform.Find("selected").gameObject.active = true;

                    }
                }
            }
            unitsInDrag.Clear();  
        }
    }


    public static bool unitsWithinScreenSpace(Vector2 unitScreenPos)
    {


      //  if ((unitScreenPos.x < Screen.width && unitScreenPos.y > Screen.height)
         //   && (unitScreenPos.x > 0f && unitScreenPos.y > 0f))
            return true;
      //  return false;
    }

    public static void RemoveFromScreenUnts(GameObject Unit)
    {

        for (int i = 0; i < UnitsOnScreen.Count; i++){

            GameObject unitObj = UnitsOnScreen[i] as GameObject;
            if (Unit == unitObj){
                selectedObjets.Remove(unitObj);
                unitObj.transform.Find("selected").gameObject.active = false;
                unitObj.GetComponent<unit>().OnScreen = false;
            //    unitObj.GetComponent<unit>().Selected = false;

             return;
            }

        }
        return;
    } 

    public bool isClientDragging(Vector2 DragStart, Vector2 newPoint)
    {

        if (newPoint.x > DragStart.x + dragZone || newPoint.x < DragStart.x - dragZone
            || newPoint.y > DragStart.y + dragZone || newPoint.y < DragStart.y - dragZone)
             return true;
        else
            return false;
    }


    private void OnGUI(){
        if (isDrag)
            GUI.Box(new Rect(BoxLeft, BoxTop, BoxWidth, BoxHeight), "", mouseDragSkin);
    }


    public static float Unsigned(float distance){
        if (distance < 0)
            distance = distance * -1;

        return distance;
    }


    public bool floatToBool(float num){
        if (num < 0f)
            return false;
        return true;
    }



    void clearSelection()
    {

        foreach (GameObject Objj in selectedObjets) 
           if(Objj!=null)
        Objj.transform.Find("selected").gameObject.active = false;


   
        foreach (GameObject Objj in unitsInDrag)
            if (Objj != null)
                Objj.transform.Find("selected").gameObject.active = false;

        if(selectedObject!=null)
            selectedObject.transform.Find("selected").gameObject.active = false;

        selectedObject = null;
        unitsInDrag.Clear();
        selectedObjets.Clear();
    }


   static bool ShiftKeyDown() {
    
        if (Input.GetKey("left shift") || Input.GetKey("right shift"))
            return true;
        else
            return false;
    }


    void cleardrag()
    {
        if (unitsInDrag != null)
            foreach (GameObject Objj in unitsInDrag)
                Objj.transform.Find("selected").gameObject.active = false;
        unitsInDrag.Clear();
    }



    void TaskOnClick(){
        Debug.Log("Clicked my button motha fucka now insta a new player");
    }

    private void hideCanvases() {
        buildCanvas.SetActive(false);
        townCanvas.SetActive(false);
        unitscanvas.SetActive(false);
        unitsControler.SetActive(false);

    }

    public static ArrayList getSelected()
    {
        foreach (GameObject go in selectedObjets)
            print(go.name);
        return selectedObjets;
    }


}

