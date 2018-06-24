using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;




public class ServerClient{ //create list of connected clients to server
    public int connectionId;
    public string playerName;
    public List<Unit> myUnits;


    // Main Player Removal    will be for the rpg char
    public Vector3 pos;
    public float rotation;
    public int unitState;

}

public class Server : MonoBehaviour
{

    public Canvas c;

    private const int MAX_CONNECTIONS = 8;
    private int hostid;
    private int webHostId;
    private int port = 5888;
    private int reilableChnl;
    private int unReilableChnl;
    private bool isStarted = false;
    private byte err;
    private List<ServerClient> ClientList = new List<ServerClient>();
    private int playerType;
    private float lastMovmentUpdate;
    private float movmentUpdarteRate = 0.5f;

    private void Start() {
        Debug.Log("ON START");
        //init the server
     
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();
        reilableChnl = cc.AddChannel(QosType.Reliable);
        unReilableChnl = cc.AddChannel(QosType.Unreliable);
        HostTopology topo = new HostTopology(cc, MAX_CONNECTIONS);
        hostid = NetworkTransport.AddHost(topo, port, null);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port, null);
        isStarted = true;// the server is running
    }

    private void Update() {
        if (!isStarted)//if the server not running
            return;

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[2048];
        int bufferSize =2048;
        int dataSize;
        byte error;

       //the recived data from the clients 
       NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId,
                                           out channelId, recBuffer, bufferSize, out dataSize, out error);

        switch (recData) {
            //can add a "Nothing" that runs each time no other command


            case NetworkEventType.ConnectEvent:    // if there was a connection Rrequest to server
                Debug.Log("Player" + connectionId + "has connected");
                setCanvas("Connection Event conId: " + connectionId);
                 Onconnection(connectionId);
                break;


            case NetworkEventType.DataEvent:   // the recived message was Data type
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
           //     Debug.Log("Recv : " + msg);
                string[] splitData = msg.Split('|');
                switch (splitData[0])//type of data request like new palyer, message ...
                {
              
                    case "NameIs":
                        setCanvas(splitData[1] + "  Has Connected ");
                        OnNameIs(connectionId, splitData[1]);
                        break;
                    case "UNITCREATION":
                        onUnitCreate(connectionId, splitData);
                        break;
                    case "MYPOSITION":
      // Main Player Removal               //   onMyPosition(connectionId,float.Parse(splitData[1]), float.Parse(splitData[2]), float.Parse(splitData[3]),float.Parse(splitData[4]), int.Parse(splitData[5]));
                       updateUnits(connectionId, splitData);
                        break;
                    case "BUILD":
                        onBuild(splitData);
                        break;
                    case "UNITREMOVE":     
                        OnUnitRemove(splitData);
                        break;

                    default:
                        Debug.Log("Other thing happned");
                        break;
                }
                break;



            case NetworkEventType.DisconnectEvent: //the client has disconnected
                onDisconnection(connectionId);
                setCanvas( "Player Id Has Disonnected   " + connectionId);
                Debug.Log("  Player " + connectionId + "   has disconnected    ");
              
                break;
        }

        if (Time.time - lastMovmentUpdate > movmentUpdarteRate) {  //asks client for a current po
            lastMovmentUpdate = Time.time;           
            foreach (ServerClient sc in ClientList) {
                string m = "ASKPOSITION|" + sc.connectionId.ToString() +'%' + '0' ;
                // Main Player Removal         //    m += sc.connectionId.ToString() + '%' + sc.pos.x.ToString() + '%' + sc.pos.y.ToString() + '%' + sc.pos.z.ToString()
                //    + '%' + sc.rotation.ToString() + '%' + sc.unitState.ToString(); //added player name
                int j = 0;
                for (int i = 0; i < sc.myUnits.Count  ; i++)
                {
                    m += '%' + sc.myUnits[i].pos.x.ToString() + '%' + sc.myUnits[i].pos.y.ToString() + '%' + sc.myUnits[i].pos.z.ToString() + '%'
                                            + sc.myUnits[i].rot.ToString() + '%' + sc.myUnits[i].state.ToString();


             
                    if (j == 14)
                    {
                        Send(m, reilableChnl, ClientList);
                        j = 0;
                        m = "ASKPOSITION|" + sc.connectionId.ToString()+ '%' + (i+1).ToString();
                    }

                    j++;
                }
                //       m += '|';
                Send(m, reilableChnl, ClientList);
            }
           // m = m.Trim('|');
      //      Send(m, reilableChnl, ClientList);
        }
    }
    private void onBuild(string[] data) {
        string m = "BUILD|" + data[1] + "|" + data[2] + "|" + data[3] + "|" + data[4] +"|" + data[5] +"|" + data[6];
        ///  foreach (ServerClient sc in ClientList)

      
            Send(m,reilableChnl, ClientList);

    // Send(m, reilableChnl, int.Parse(data[1]));

    }
    private void OnUnitRemove(string[] data) {

        string m = "KILL|" + data[1] + "|" + data[2]; // connId + unity Id to remove
    List<Unit> temp = 
                  ClientList.Find(c => c.connectionId == int.Parse(data[1])).myUnits; //remove from server the unit
        Send(m, unReilableChnl, ClientList);

        foreach (Unit u in temp)
            if (u.id == int.Parse(data[2]))
                ClientList.Find(c => c.connectionId == int.Parse(data[1])).myUnits.Remove(u);
  
    }
    private void onMyPosition(int conId, float x, float y,float z,float r,int s) //this gonna be for strategy
    {

        ClientList.Find(c => c.connectionId == conId).pos = new Vector3(x, y, z);
        ClientList.Find(c => c.connectionId == conId).rotation = r;
        ClientList.Find(c => c.connectionId == conId).unitState = s;
    }
    private void onUnitCreate(int conId, string[] data) {

       /* for (int i = 0; i < ClientList.Count; i++)
            if (ClientList[i].connectionId == conId)
                myClient = ClientList[i]; */
       Unit units = new Unit();
        Vector3 pos;
       pos.x = float.Parse(data[3]);
         pos.y = float.Parse(data[4]);
         pos.z = float.Parse(data[5]);
        units.unitName = data[2];
         units.pos = pos;
         units.rot = 0;
         units.state = 0;
        ClientList.Find(c => c.connectionId == conId).myUnits.Add(units);
        print("SErver Send to Spawn");
        string m  = "SPAWNUNIT|" + data[1] + "|" + data[2] + "|" + data[3] + "|" + data[4] + "|" + data[5] + "|" + 0.ToString();

       // foreach (ServerClient sc in ClientList)
            Send(m, unReilableChnl, ClientList);

    }
    private void updateUnits(int conId,string[] data)
     {
        Vector3 pos;
        print(data[1]);
        int y = ( int.Parse(data[1]));
         for (int j = 2; j < data.Length; j+=5) { 
             pos.x = float.Parse(data[j]);
             pos.y = float.Parse(data[j+1]);
             pos.z = float.Parse(data[j+2]);
            ClientList.Find(c => c.connectionId == conId).myUnits[y].pos = pos;
            ClientList.Find(c => c.connectionId == conId).myUnits[y].rot =float.Parse(data[j + 3]);
            ClientList.Find(c => c.connectionId == conId).myUnits[y].state = int.Parse(data[j + 4]);
            y++;
        // Debug.Log("ID: " + conId + "  " + data);

     }
         

    }
    private void Onconnection(int conId) {

        //we add the new client on connection to a clients list
        // aand send the clients list to the new connected player

        // the connection and name registrion must be sent in diffrent 
        ServerClient c = new ServerClient();
        c.connectionId = conId;
        c.playerName = "Temp";
        c.myUnits = new List<Unit>();
        ClientList.Add(c);
        string msg = "ASKNAME|" + conId +'|';




     /*   foreach (ServerClient sc in ClientList)
        {
            //  string m = "ASKPOSITION|" + sc.connectionId.ToString() + '%' + '0';
            // Main Player Removal         //    m += sc.connectionId.ToString() + '%' + sc.pos.x.ToString() + '%' + sc.pos.y.ToString() + '%' + sc.pos.z.ToString()
            //    + '%' + sc.rotation.ToString() + '%' + sc.unitState.ToString(); //added player name
            int j = 0;
            for (int i = 0; i < sc.myUnits.Count; i++)
            {
           
                msg +=  sc.myUnits[i].pos.x.ToString() + '%' + sc.myUnits[i].pos.y.ToString() + '%' + sc.myUnits[i].pos.z.ToString() + '%'
                                        + sc.myUnits[i].rot.ToString() + '%' + sc.myUnits[i].state.ToString() +'%';
          
            }
                   msg.Trim('%');
        }

    */
        Send(msg, reilableChnl, conId);
    }
    private void syncUnits()
    {


    //    foreach (ServerClient sc in ClientList)
     //   {
            ServerClient sc = ClientList[0];
            string msg = "SYNC|" + sc.connectionId + '|';
            //  string m = "ASKPOSITION|" + sc.connectionId.ToString() + '%' + '0';
            // Main Player Removal         //    m += sc.connectionId.ToString() + '%' + sc.pos.x.ToString() + '%' + sc.pos.y.ToString() + '%' + sc.pos.z.ToString()
            //    + '%' + sc.rotation.ToString() + '%' + sc.unitState.ToString(); //added player name
            int j = 0;
            for (int i = 0; i < sc.myUnits.Count; i++)
            {

                msg += sc.myUnits[i].pos.x.ToString() + '%' + sc.myUnits[i].pos.y.ToString() + '%' + sc.myUnits[i].pos.z.ToString() + '%'
                                        + sc.myUnits[i].rot.ToString() + '%' + sc.myUnits[i].state.ToString() + '%';

            }
            msg.Trim('%');

        Send(msg, unReilableChnl, ClientList);
        //  }
    }

    private void Send(string message, int channelId, int conId) {
        //to avoid overloading the send with paramaters 
        //this function is pre actual send!
        List<ServerClient> c = new List<ServerClient>();
        c.Add(ClientList.Find(x => x.connectionId == conId));//find the client in the list

        Send(message, channelId, c); //the message sent from here
    }
    private void Send(string message, int channelId, List<ServerClient> c)
    {//the message sent 
       // Debug.Log("Sending : " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message); //encode message to 

        foreach (ServerClient sc in c) // sent to all the clients
            NetworkTransport.Send(hostid, sc.connectionId, channelId, msg, message.Length * sizeof(char), out err);
    }

    private void OnNameIs(int conId, string playerName) {//init the player name here

        ClientList.Find(x => x.connectionId == conId).playerName = playerName;

        for (int i = 0; i < 5; i++)
        {
            Unit units = new Unit();
            Vector3 pos = Vector3.zero;
            pos.x = 0;
            pos.y = 0;
            pos.z = 0;
            units.unitName = "worker(Clone)";
            units.pos = pos;
            units.rot = 0;
            units.state = 0;
            ClientList.Find(c => c.connectionId == conId).myUnits.Add(units);

        }

        Send("CNN|" + playerName + '|' + conId  , reilableChnl, ClientList);

        //  if (ClientList.Count > 1)
        //   {
        //      print("sending syc  "  + ClientList.Count);
        //     syncUnits();

        //  }
    }
    private void onDisconnection(int conId) {

        ClientList.Remove(ClientList.Find(x => x.connectionId == conId));

        Send("DC|" + conId, reilableChnl, ClientList);
    }
    private void setCanvas(string myStr) {

        c.GetComponentInChildren<Text>().text += myStr +'\n';

    }
}
