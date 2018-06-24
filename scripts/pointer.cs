
using System.Collections;
using UnityEngine;

public class pointer : MonoBehaviour {
    RaycastHit hit;  // where the ray of the mouse is hitting
    public GameObject selectedObject;
    private Vector3 mouseClick;
    public static ArrayList selectedObjets = new ArrayList();
   public  GameObject hitObject;
    void Awake()
    {
        mouseClick = Vector3.zero;

    }
	// Update is called once per frame
	void Update () {

        // GameObject Target = GameObject.Find("Target"); // get the object named target
  
      
    
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // the ray follows the mousr
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
         //   GameObject hitObject = new GameObject();
            Debug.Log(hit.collider.name);
            hitObject = hit.transform.root.gameObject;
           
        }
        //  Debug.Log(hitObject.name);
      
      if (Input.GetMouseButtonDown(0)) //left buttn mouse clicked
        {
         
           mouseClick = hit.point;
            SelectObject(hitObject);
   
        }
     if (hit.collider.name == "Terrain")
        {
           
         //  Target.transform.position = hit.point;//shows the Target on Screen where the mouse pointing
            //left mouse is 0
           if (Input.GetMouseButton(1)) {//if right mouse was clicked
                GameObject Target = Resources.Load("Target") as GameObject;
                GameObject TargetObject = Instantiate(Target, hit.point, Quaternion.identity) as GameObject; //clonoes the object and keeps on screen
                TargetObject.name =  "Targetint"; // the game object created is name "Target"
                //GameObject objPrefab = Resources.Load("Target") as GameObject;
              //  GameObject obj = Instantiate(objPrefab) as GameObject;
            }
        }
        
     //   Debug.DrawRay(ray.origin, ray.direction * Mathf.Infinity, Color.green);

 
    }

    void SelectObject(GameObject obj)
    {

        if (ShiftKeyDown() && obj.name != "Terrain" && obj != null)
        {

            obj.transform.Find("selected").gameObject.active = true;
            selectedObjets.Add(obj);
            selectedObject = obj;

        }
       else if (selectedObject != null)
        {
           
            if (obj == selectedObject)
                return;
            else if (obj.name == "Terrain")
            {
                if (selectedObjets != null)
                {
         
                    foreach (GameObject Objj in selectedObjets)
                        Objj.transform.Find("selected").gameObject.active = false;
                        
                   
                }
                selectedObjets.Clear();
                selectedObject.transform.Find("selected").gameObject.active = false;
                selectedObject = null;
               // selectedObject = null;

            }
            else
            {
                foreach (GameObject Objj in selectedObjets)
                    Objj.transform.Find("selected").gameObject.active = false;
                selectedObjets.Clear();
                selectedObject = obj;
                Debug.Log("RE SELECTED  " + selectedObject.name);
                obj.transform.Find("selected").gameObject.active = true;
                selectedObjets.Add(obj);
            }
        }
        else
        {
            obj.transform.Find("selected").gameObject.active = true;
            selectedObjets.Add(obj);
            selectedObject = obj;
  
        }
       

/*else if (obj == null || obj.name == "Terrain")
               obj = null;
           else
               selectedObject = obj;
           Debug.Log("motha fucka");*/
        // selectedObject = obj;


        /*  Renderer[] rs = selectedObject.GetComponentsInChildren<Renderer>();
          foreach (Renderer r in rs) {
              Material m = r.material;
              m.color = Color.green;
              r.material = m;*/



        // }

        

    }
    void clearSelection()
    {
        selectedObject = null;

    }
    bool ShiftKeyDown() {
    
        if (Input.GetKey("left shift") || Input.GetKey("right shift"))
            return true;
        else
            return false;
    }
}
