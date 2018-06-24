using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text;
using System;


public class Unit
{

    public string unitName;
    public GameObject go;
    public Vector3 pos;
    public float rot;
   public  int state;
    public int id;
    public bool Enemy;
        }
public class Player {
    public int playerType; // rpg or strg
    public string playerName;
    public GameObject avatar;
    public int connectionId;
    public int unitState =0;
   public List<Unit> EnemyUnits;
   public List<Unit> myUnits;
    public List<Unit> EnemyBuildings;
    public List<Unit> myBuildings;
    public int teamNo;

}

public class Client : MonoBehaviour {
    public Boolean moving = false;
    public float speed = 15f;
    Vector3 velocity = Vector3.zero;
    private bool unitsLoaded = false;
    public static int rpgC = 1;
    public static int strgC = 2;
    private int count = 0;
    private const int MAX_CONNECTIONS = 8;
    private int hostid;
    private int wedId;
    private int host;
    private int port = 5888;
    private int reilableChnl;
    private int unReilableChnl;
    private static int ourClientId;
    private int connectionId;
    private float connectionTime;
    private bool isConnected = false;
    public static bool isStarted = false;
    private byte err;
    private string playerName;



    public GameObject playerPrefab;
    public GameObject rpgCamera;
    public GameObject otherStrg;
    public GameObject playerPre;
    public GameObject myPlayer;
    public GameObject camTarget;

    public GameObject Archer;
    public GameObject calv;

    public GameObject worker;
    public GameObject spearMan;
    public GameObject Soldier;
    public int playerType;
    public bool Test=true;
    Texture2D tex;
    bool spawnted = false;

    public Canvas lose;
    //private List<Unit> EnemyUnits = new List< Unit > ();

    private static int EnemtId=0;
    private static int EnemeyBuildId = 0;
    private static int myBuildingId = 0;

    private static Player myPlayerHandler;
    public Dictionary<int,Player> players = new Dictionary<int,Player>();

   // private Player myPlayer;
   
    public void Connect() {
  
        string pName = GameObject.Find("nameInput").GetComponent<InputField>().text;
        if (pName == ""){
            Debug.Log("Name empty please fill");
            return;
        }
        playerName = pName;

       string pType = GameObject.Find("Dropdown").GetComponentInChildren<Dropdown>().captionText.text;





        if (pType == "RPG Character") // Type of player in connection
            playerType = rpgC;
        
        else if (pType == "Strategy Charcter") 
            playerType = strgC;  
        else 
            return;

        // ---- Init and start connection
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();
        reilableChnl = cc.AddChannel(QosType.Reliable);
        unReilableChnl = cc.AddChannel(QosType.Unreliable);
        HostTopology topo = new HostTopology(cc, MAX_CONNECTIONS);
        hostid = NetworkTransport.AddHost(topo,0);
        connectionId = NetworkTransport.Connect(hostid,"127.0.0.1", port, 0, out err);
        connectionTime = Time.time;

        isConnected = true;
    }

    public void Update(){
        if (!isConnected)
            return;
      
        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[2048];
        int bufferSize = 2048;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

        switch (recData){
            case NetworkEventType.DataEvent: // Data Reception handling
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                string[] splitData = msg.Split('|');
                switch (splitData[0]){

                    case "SPAWNUNIT":
                        if (int.Parse(splitData[1]) != ourClientId)
                        {
                            print("RECV SPAWN FROM SEVER");
                            onSpawnUnit(splitData);
                        }
                        break;
                    case "ASKNAME":
                        OnAskName(splitData);
                        break;
                    case "CNN":
                        Vector3 yy = new Vector3(70f, 0 , 45f);
                     
                        SpawnPlayer(splitData[1], int.Parse(splitData[2]), yy);//, new Vector3(200,-16,150));
                        break;
                    case "DC":
                        PlayerDisconnected(int.Parse(splitData[1]));
                        break;
                    case "ASKPOSITION":
                        onAskPosition(splitData);
                        break;
                    case "BUILD":
                        if (getMyConId() != int.Parse(splitData[1]))
                            onBuild(splitData);
                        break;
                    case "KILL":
                        if (getMyConId() != int.Parse(splitData[1]))
                            onKill(splitData);
                        break;
                    case "SYNC":

                     if (getMyConId() == int.Parse(splitData[1]))
                   {
                            print("sync recived");
                            sync(splitData);

                     }
                        break;
                            default:
                        Debug.Log("Other thing happned");
                        break;
                }
                break;
        }



        if(isStarted)
            if(myPlayerHandler.myBuildings.Count == 0)
            {
                lose.gameObject.SetActive(true);
                print("LOOOOOOOOOOOOOOOOOOOSEEE MOTHAAAAAAAAA FUCKAAAAAAAAA");

            }
    }

    private void onKill(string[] data)
    {
        int id = int.Parse(data[2]);
        foreach (Unit u in myPlayerHandler.myUnits)
            if (u!=null && u.id == id)
            {
                pointer.selectedObjets.Remove(u.go);
                pointer.unitsInDrag.Remove(u.go);
                pointer.UnitsOnScreen.Remove(u.go);
                myPlayerHandler.myUnits.Remove(u);
                Destroy(u.go);
            }
    }
    //  ------------------------------------------ FUNCTIONS --------------------------------------------- //
    private void onSpawnUnit(string[] data) { //SPAWN NEW ENEMY UNITS
       // Vector3 yy = new Vector3(UnityEngine.Random.Range(40.0f, 120.0f), 100, UnityEngine.Random.Range(15f, 50.0f));
        GameObject c;
        Renderer[] rs;
                 Unit u = new Unit();
        Vector3 pos1 = Vector3.zero;
        pos1.x = float.Parse(data[3]);
        pos1.y = float.Parse(data[4]);
        pos1.z = float.Parse(data[5]);

            if (data[2] == "Archer")
                c = Instantiate(Archer, pos1, transform.rotation);
            else if (data[2] == "Soldier")
                c = Instantiate(Soldier, pos1, transform.rotation);
        else if (data[2] == "cavalry")
            c = Instantiate(calv, pos1, transform.rotation);
        else if (data[2] == "spearMan")
            c = Instantiate(spearMan, pos1, transform.rotation);

        else
                c = Instantiate(worker, pos1, transform.rotation);

     //   print(data[0]   +  "   1= "   + data[1]    + "     "  +  data[2]);

        tex = getColors(int.Parse(data[1]));

        if (data[2] == "worker" || data[2] == "spearMan")
        {
            print("WORKER CREATION COLOR");
            rs = c.gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rs)
            {
                foreach (Material m in r.materials)
                    r.material.mainTexture = tex;
            }
        }
        else
        {
            rs = c.gameObject.GetComponentsInChildren<Renderer>();


            if (data[2] == "cavalry")
            {
                Material[] m = rs[0].materials;
                m[1].mainTexture = tex;
            
            }
            else// if (gameObject.name == "Soldier(Clone)")
            {
                Material m = rs[0].material;
                m.mainTexture = tex;

            }
        }


        u.unitName = data[2];
        u.pos.x = float.Parse(data[3]);
        u.pos.y = float.Parse(data[4]);
        u.rot = 0;
        u.pos.z = float.Parse(data[5]);
        u.pos = pos1;
        u.Enemy = true;
        u.id = EnemtId;
        EnemtId++;
            u.go = c;
            u.go.gameObject.GetComponent<unit>().TeamNo = int.Parse(data[1]);
            u.state = 0;
    
        myPlayerHandler.EnemyUnits.Add(u);

    }



    private void onAskPosition(string[] data){//update everyone else 
        if (!isStarted)
            return;
   
            for (int i = 1; i < data.Length; i++) {
            string[] d = data[i].Split('%');
          //  print(d);
            if (ourClientId != int.Parse(d[0])) {  // Update rest of player on my Client 

                // PLAYER REMOVAL
                /*      Vector3 pos = Vector3.zero;
                      pos.x = float.Parse(d[1]);
                      pos.y = float.Parse(d[2]);
                      pos.z = float.Parse(d[3]);


                      var angles = transform.rotation.eulerAngles;
                      angles.z = 0;
                      angles.x = 0;
                      angles.y = float.Parse(d[4]);
                      //  .Lerp
                   //   if(players[int.Parse(d[0])].avatar.GetComponent<multiAnime>()!=null)
                   //     players[int.Parse(d[0])].avatar.GetComponent<multiAnime>().setAnime(int.Parse(d[5])); 

                      players[int.Parse(d[0])].avatar.transform.position = pos;     
                      players[int.Parse(d[0])].unitState = int.Parse(d[5]);
                      players[int.Parse(d[0])].avatar.transform.rotation = Quaternion.Euler(angles);
                         */      // PLAYER REMOVAL
                int y = 0; int j = 2;
                // if (d.Length >0)
                //  print("in client sending units "  +  d[0] + "    " + d[1]);
                //   for(int l =0;j<myPlayerHandler.EnemyUnits.Count;l++)
                //   foreach (Unit u in myPlayerHandler.EnemyUnits)
                //   print("Client   " + d[1]);
                print("my Enemy Units Count   " + myPlayerHandler.EnemyUnits.Count);
                for (int o =( int.Parse(d[1])); o< myPlayerHandler.EnemyUnits.Count && y<15;o++)
                    {// int.Parse(d[1])
                 //    if (y < 15)
                 //     {

                    Unit u = myPlayerHandler.EnemyUnits[o];


                            Vector3 pos1 = Vector3.zero;
                            pos1.x = float.Parse(d[j]);
                            pos1.y = float.Parse(d[j + 1]);
                            pos1.z = float.Parse(d[j + 2]);

           
                            var angl = transform.rotation.eulerAngles;
                            angl.z = 0;
                            angl.x = 0;
                            angl.y = float.Parse(d[j + 3]);
                    StartCoroutine(FadeOut(u.go, u.go.transform.position, pos1, 0.5f));

                //    u.pos = pos1;
                    u.go.transform.rotation = Quaternion.Euler(angl);
                            u.go.GetComponent<multiAnime>().setAnime(int.Parse(d[j + 4]));
                            //  if (players[int.Parse(d[0])].avatar.GetComponent<multiAnime>() != null)                        
                            y++;
                            j += 5;
                   //     }
                    }
              
             //   players[int.Parse(d[0])].myUnits.Count
            }
        }
        // Send Data about my Players
        //  Vector3 myPos = players[ourClientId].avatar.transform.position;
        //     float rot = players[ourClientId].avatar.transform.eulerAngles.y;  
        //    int unitState = players[ourClientId].unitState;
        // Main Player Removal         string m = "MYPOSITION|"+ myPos.x.ToString() + '|' + myPos.y.ToString() + '|' + myPos.z.ToString() + '|' + rot.ToString()  +'|' + unitState.ToString();
        // added new    
        // for (int k = 0; k < players[ourClientId].myUnits.Count; k++)
        string m = "MYPOSITION|" + "0|";
        int k = 0;
      //  foreach (Unit u in myPlayerHandler.myUnits)
for(int o = 0; o< myPlayerHandler.myUnits.Count;o++)
            {
            print("my Units Count   " + myPlayerHandler.myUnits.Count);
            Unit u = myPlayerHandler.myUnits[o];

            if (u.go != null) {

      
                    Vector3 myPos1 = u.go.transform.position;
                    float rot1 = u.go.transform.eulerAngles.y;
                    int unitState1 = getState(u.go);

                    m += myPos1.x.ToString() + '|' + myPos1.y.ToString() + '|' + myPos1.z.ToString() + '|'
                                                                            + rot1.ToString() + '|' + unitState1.ToString() + '|';
   

                if (k == 14) {
                m = m.Trim('|');
                Send(m, reilableChnl);
                    k = 0;
                    m = "MYPOSITION|"  + (o+1).ToString() + '|';
                }
                k++;
            }
        }
        m = m.Trim('|');
        Send(m, reilableChnl);
    }



    private void onBuild(string[] data) {
        Vector3 tt = new Vector3(float.Parse(data[3]), float.Parse(data[4]), float.Parse(data[5]));
        BuildBuilding b = this.GetComponent<BuildBuilding>();
        Unit u = new Unit(); 
        u.pos = tt;
        u.id = EnemeyBuildId;
        EnemeyBuildId++;
        u.go =    b.createBuilding(data[2], tt, 0f);
        myPlayerHandler.EnemyBuildings.Add(u);
      
    }
    private int getState(GameObject go) {

        if (go.gameObject.GetComponent<Animator>().GetBool("isWalking") == true)
            return  1;

        if (go.gameObject.GetComponent<Animator>().GetBool("isAttack") == true)
            return 2;
        else
            return 0;

    }
    private IEnumerator FadeOut(GameObject go, Vector3 alphaStart, Vector3 alphaFinish, float time)
    {


        float elapsedTime = 0;


        while (elapsedTime < time)
        {
            go.transform.position = Vector3.Lerp(alphaStart, alphaFinish , (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private void OnAskName(string[] data) {  //Send name and init initail start screen

        ourClientId = int.Parse(data[1]);
   //     print("On Connection  data[1]   :   " + data[1]);
        Send("NameIs|" + playerName + playerType, reilableChnl);
     
  
        /*for (int i = 0; i < data.Length -1; i++) {
            string[] d = data[i].Split('%');
         //   Instantiate(worker) as GameObject;
         for(int j=0;j<d.Length;j++)
            print(j+ "    on ask name   "  + d[j]);
            */
       //     if (d[0] != "Temp")
       //  {
       //    SpawnPlayer(d[0], int.Parse(d[1]), ParseToVector(d[2]));


      //    } 
      
    }

    private void sync(string[] data) {
        print("in SYNC");
  //      myPlayerHandler.EnemyUnits = new List<Unit>();
        int clr;
        if (ourClientId == 1)
            clr = 2;
        else
            clr = 1;


        //    for (int i = 0; i < data.Length; i++)
        int j = 0;
        string[] d = data[2].Split('%');
        for (int i = 0; i < 5; i++)
        {
            //     print(i +"    " + d[i]);

            Unit u = new Unit();
            GameObject go = new GameObject();
            Vector3 pos = Vector3.zero;
            pos.x = float.Parse(d[j]);
            pos.y = float.Parse(d[j + 1]);
            pos.x = float.Parse(d[j + 2]);

            go = Instantiate(worker, new Vector3(pos.x, pos.y, pos.z), transform.rotation) as GameObject;

            u.go = go;
            u.unitName = "worker(Clone)";
            u.Enemy = true;
            u.state = 0;
            u.id = EnemtId;
            EnemtId++;

            Renderer[] rs;
            rs = go.gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rs)
            {
                foreach (Material m in r.materials)
                    r.material.mainTexture = getColors(clr);
            }



          myPlayerHandler.EnemyUnits.Add(u);
            j += 5;

            //        print(i + "    conn splitted   " + d[i]);

        }

    }
    private void SpawnPlayer(string playerName, int conId,Vector3 pos) {
      int type = int.Parse(playerName[playerName.Length - 1].ToString());
        //  Vector3 pos;
 
        if (type == rpgC && conId!= ourClientId ){
            GameObject go1;
            print("entered");
            go1 = Instantiate(playerPre,new Vector3(pos.x,pos.y,pos.z),transform.rotation) as GameObject;
            Player P = new Player();
              
            P.avatar = go1; 
            P.playerName = playerName;
            P.connectionId = conId;
            P.playerType = rpgC;
         //   P.avatar.GetComponentInChildren<TextMesh>().text = playerName;

            players.Add(conId, P);
     
        }
        else if(type == strgC){ // strategy player 
            Player P = new Player();
            //   P.teamNo = GameObject.Find("Team").GetComponentInChildren<Dropdown>().value;
            //  GameObject go;
            if (conId == ourClientId)
            {

                //   go = Instantiate(playerPrefab, new Vector3(pos.x, pos.y, pos.z), transform.rotation) as GameObject;
                P.teamNo = GameObject.Find("Team").GetComponentInChildren<Dropdown>().value;
                GameObject.Find("strCam").GetComponent<cameraController>().TeamNo = conId;
                //      go.GetComponent<unit>().TeamNo = conId;
                //    go.transform.Find("selected").gameObject.active = true;
                pointer.myConId = ourClientId;
                print("Client test" + P.teamNo + "   " + ourClientId);
                P.playerName = playerName;
                P.connectionId = conId;
                 P.myUnits = new List<Unit>();
                P.EnemyUnits = new List<Unit>();
                P.myBuildings = new List<Unit>();
                P.EnemyBuildings = new List<Unit>();
                // pos.y = 0;
        
                pos.y = 0;
                Vector3 EnemyPos = pos;

                if (ourClientId == 1)
                {
                    foreach (GameObject fooObj in GameObject.FindGameObjectsWithTag("townhall"))
                    {
                        if (fooObj.transform.position.z<500)
                        {

                            Unit u = new Unit();
                            u.go = fooObj;
                            u.pos = fooObj.transform.position;
                            u.id = myBuildingId;
                            myBuildingId++;
                            P.myBuildings.Add(u);
                            u.go.GetComponent<unit>().clientId = 1;
                        }
                     else
                        {
                            Unit u = new Unit();
                            u.go = fooObj;
                            u.pos = fooObj.transform.position;
                            u.id = EnemeyBuildId;
                            EnemeyBuildId++;
                            P.EnemyBuildings.Add(u);
                            u.go.GetComponent<unit>().clientId =0;
                        }
                    }


                    //  pos.x -=300;
                    EnemyPos.z += 2770;
                    EnemyPos.x += 1850;

                }

                //
                else
                {
                    // {
                    pos.z +=2770;
                    pos.x += 1850;
                    foreach (GameObject fooObj in GameObject.FindGameObjectsWithTag("townhall"))
                    {
                        if (fooObj.transform.position.z > 500)
                        {

                            Unit u = new Unit();
                            u.go = fooObj;
                            u.pos = fooObj.transform.position;
                            u.id = myBuildingId;
                            u.go.GetComponent<unit>().clientId = 1;
                            myBuildingId++;
                            P.myBuildings.Add(u);
                        }
                        if (fooObj.GetComponent<unit>().clientId == 1)
                        {

                            Unit u = new Unit();
                            u.go = fooObj;
                            u.pos = fooObj.transform.position;
                            u.id = EnemeyBuildId;
                            EnemeyBuildId++;
                            u.go.GetComponent<unit>().clientId = 0;
                            P.EnemyBuildings.Add(u);
                        }
                    }

                }
                //   pos.y = 0;

                //  }*/
                for (int i = 0; i < 5; i++)
                {


                    Vector3 testPos = Vector3.zero;


                    Unit u = new Unit();


                pos.x += ((i + 1) * 6);
                    GameObject go = Instantiate(worker,new Vector3(pos.x,pos.y,pos.z), transform.rotation) as GameObject;
             
                    u.go = go;
                    u.unitName = "worker(Clone)";
                    u.id = TestS.getCountId();
                    TestS.IdCount++;
                    u.unitName = go.name;
         
        
        
                    u.Enemy = false;
                    u.go.GetComponent<unit>().clientId = 1;
                    u.go.GetComponent<ClickToMove>().enabled = true;
                    u.go.GetComponent<SoldierScript>().enabled = true;
                    u.rot = 0;
              //      u.pos = pos;
                    P.myUnits.Add(u);


                }
                
               for (int i = 0; i < 5; i++)
                {
                    int clr;
                    if (conId == 1)
                        clr = 2;
                    else
                        clr = 1;

                  EnemyPos.x += ((i + 1) * 10);
                    GameObject go = Instantiate(worker, new Vector3(EnemyPos.x, EnemyPos.y, EnemyPos.z), transform.rotation) as GameObject;

                    Unit u = new Unit();
                    u.go = go;
            //        u.pos = EnemyPos;
                    u.Enemy = true;
                    u.id = EnemtId;
                        EnemtId++;

                 
                    u.rot = 0;

                    Renderer[] rs;
                    rs = go.gameObject.GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in rs)
                    {
                        foreach (Material m in r.materials)
                            r.material.mainTexture = getColors(clr);
                    }
                    P.EnemyUnits.Add(u);
                }

                myPlayerHandler = P;
                players.Add(conId, P);

            }


            if (P.teamNo != 0)
                {
                    // GameObject go = Instantiate(playerPrefab) as GameObject;
                    //   if()
                    GameObject cam = GameObject.Find("strCam").gameObject;
                    Vector3 camPos = cam.transform.position;
                    var camRot = cam.transform.localRotation.eulerAngles;
                    camRot.y += 180;
                    camRot.x = 57;
                    camRot.z = 0;
                    //  camRot.x += 56;

                    // camRot.z = cam.transform.localRotation.z;

                    cam.transform.eulerAngles = camRot;
                    camPos.z = 2900;
                camPos.x = 2100;
                cam.transform.position = camPos;
                    print(cam.transform.position + "    " + cam.transform.localRotation);

                }

            }
           
            else
            {   //no my id

                var angles = transform.rotation.eulerAngles;
                UnityEngine.Random.Range(0f, 360F);

                angles.y = UnityEngine.Random.Range(0f, 360F); ;
                //   go = Instantiate(otherStrg, new Vector3(pos.x, pos.y, pos.z), Quaternion.Euler(angles)) as GameObject;

            }//type strgc
          //  P.avatar = go;
     

        if (conId == ourClientId) { // remove entery page (name info)

            // go.AddComponent<playerMotor>();
           
            if (playerType == rpgC)//destroy old cam and create rpg + camera
            {
                Player P = new Player();
                GameObject go2;
                go2 = Instantiate(myPlayer) as GameObject;
                 rpgCamera = Instantiate(rpgCamera);

                P.teamNo = GameObject.Find("Team").GetComponentInChildren<Dropdown>().value;
          

                ThirdPersonCamera camera1= rpgCamera.transform.GetChild(0).GetComponent<ThirdPersonCamera>();
                ThirdPersonCamera camera2 = rpgCamera.transform.GetChild(1).GetComponent<ThirdPersonCamera>();
                camera1.target = go2.transform.GetChild(2);
                camera2.target = go2.transform.GetChild(2);
                P.avatar = go2;
                P.playerName = playerName;
                P.connectionId = conId;
                P.playerType = rpgC;
                //   P.avatar.GetComponentInChildren<TextMesh>().text = playerName;
                players.Add(conId, P);

                Destroy(GameObject.Find("strCam"));
            }
          
            GameObject.Find("Canvas").SetActive(false);
            isStarted = true;
      }
    }

    private void Send(string message, int channelId){  //send to server
        byte[] msg = Encoding.Unicode.GetBytes(message);    
            NetworkTransport.Send(hostid,connectionId, channelId, msg, message.Length * sizeof(char), out err);
    }
    private void PlayerDisconnected(int conId){
        Destroy(players[conId].avatar);
        players.Remove(conId);
    }

    public static int getMyConId() {
        return ourClientId;
    }

    public void SendBuildingUpdate(string Bname, Vector3 pos, float rot, int id){ //(upon creation this is sent
     
        string[] n = Bname.Split('(');
        Bname = n[0];

        string msg = "BUILD|" + id.ToString() + "|" + Bname + "|"
            + pos.x.ToString() + "|" + pos.y.ToString() + "|" + pos.z.ToString() + "|" + rot.ToString();
         
      Send(msg, reilableChnl);
    }

    private Vector3 ParseToVector(string data) { //string to vector

        Vector3 myVec = Vector3.zero;

        string trimed = data.Trim('(', ')');
        string[] v = trimed.Split(',');

         myVec.x = float.Parse(v[0]);
         myVec.y = float.Parse(v[1]);
         myVec.z = float.Parse(v[2]);

     return myVec;
    }
    public static Player getMyplayerHandler() {

        return myPlayerHandler;
    }
    private void syncUnits()
    {



    }

    public void CreateUnit(Unit unit)
    {

        myPlayerHandler.myUnits.Add(unit);

        string msg = "UNITCREATION|" + ourClientId.ToString() + '|' + unit.unitName + '|' + unit.pos.x.ToString() + '|' + unit.pos.y.ToString() + '|' + unit.pos.z.ToString() +
                '|' + unit.rot.ToString() + '|' + unit.state.ToString();

        Send(msg,reilableChnl);

 //    enemyUnits(unit);

    }
  

    public void RemovePlayer(GameObject g) {

        foreach (Unit unit in myPlayerHandler.EnemyUnits)
            if (unit.go!=null && unit.go.gameObject == g)
            {
                string msg = "UNITREMOVE|" + ourClientId.ToString() + '|'  +(unit.id).ToString();
          

                Send(msg, reilableChnl);

                myPlayerHandler.EnemyUnits.Remove(unit);
                Destroy(unit.go);
            }
    }
  
    void enemyUnits(Unit u)
    {

        GameObject c;
        Unit unit = new Unit();
      
        string[] data = new string[128];
        data[0] = "";
        data[1] = "2";
        data[2] = "spearMan";
        float x = u.pos.x + 30;
        float y = u.pos.y + 30;
        float z = u.pos.z + 30;
        Vector3 t = new Vector3(x, y, z);

        data[3] = x.ToString();
        data[4] = y.ToString();
        data[5] = z.ToString();


        onSpawnUnit(data);

    }
   private Texture2D getColors(int idcon)
    {
        Texture2D texture;

        if (idcon == 1)
            texture = (Texture2D)Resources.Load("White");
        else if (idcon == 2)
            texture = (Texture2D)Resources.Load("Red");
        else if (idcon == 3)
            texture = (Texture2D)Resources.Load("Blue");
        else
            texture = (Texture2D)Resources.Load("Green");

        return texture;

    }
    public static int getMyBuildingId()
    {

        return myBuildingId;
    }
    public static void incBuildingID()
    {
        myBuildingId++;

    }
}
