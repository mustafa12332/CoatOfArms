using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.AI;

/* Multiplayer Faction Manager: script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
    public class MFactionManager_UNET : NetworkBehaviour {

        //Server only.
        //The lockstep cycle represents the time at which the host/server sends collected input commands to all clients.
		public float LockstepCycle = 0.2f; //the cycle's length, at which the player can send inputs.
        float CycleTimer;
        public GameObject rpgPlayerOne;
        public GameObject rpgPlayerTwo;
        public GameObject teamOneRpgSpy;
        public GameObject teamTwoRpgSpy;
        public Camera rpgCamera;
        public Canvas rpgCanvas;
        public List<InputVars> InputActions = new List<InputVars>(); //Collceted input actions are stored here.
        
        //Amount of players who have loaded the map scene and are ready to start the game.
        int ReadyPlayers = 0;
        bool AllPlayersReady = false; //when all players are ready, this is true

        int ServerTurn = 0; //a counter how many lockstep cycle have passed in the server
        int CurrentTurn = 0; //a counter for how many lockstep cycles have passed in the client
        int SyncedTurns = 0; //a counter for how many lockstep cycles have been synced successfully to all clients

        //Monitoring clients state and synced game turn in order not to leave one or more clients behind when the game progresses.
        public class ClientsInfoVars
        {
            public int FactionID; //Faction ID associated to this client
            public int Turn = 0; //The client's last reported synced turn
            public bool Disconneted = false; //Is the client connected or not?

            //Time
            public bool Timeout = false; //Is the client timing out? 
            public float KickTimer = 0.0f; //time before the client gets kicked for timing out
        }
        public List<ClientsInfoVars> ClientsInfo = new List<ClientsInfoVars>();
        public float TimeoutTime = 5.0f; //If clients don't respond within this time, they will be kicked

        //Timer to check if all clients are in the same level of sync: we'll call it the sync test
        public float SyncTestReload = 0.6f;
        public int SyncTestTriggerTurn = 2; //when the server turn hits this value, the sync test will start
        float SyncTestTimer;
        bool SyncTestEnabled = false; //is the sync test running?
        //Are all clients in the expected sync turn:
        bool AllClientsSynced = true;

        //Client related:
        [HideInInspector]
        [SyncVar] //Because the server sets the faction ID when this component is created, so we need to sync this to all clients.
        public int FactionID = 0; //The faction ID that this script manages. Each faction has its own MFactionManager component.

        bool IsFactionSpawned = false; //has the faction associated with this manager spawned or not?

        //Other components:
        GameManager GameMgr;
        NetworkManager_UNET NetworkMgr;
        private bool IsRpgSpawned = false;

        void Start()
        {
            IsFactionSpawned = false; //faction is marked as not spawned, yet.

            GameMgr = GameManager.Instance;
            GameMgr.Factions[FactionID].MFactionMgr_UNET = this;
            if(isServer) //only for the server
            {
                GameMgr.Factions[FactionID].ConnID_UNET = connectionToClient.connectionId; //set the unique connection ID
            }
            
            //get the lobby manager:
            NetworkMgr = NetworkManager_UNET.NetworkMgr;
            NetworkMgr.CanvasObj.SetActive(false); //hide the multiplayer menu UI as this object is spawned when the map is loaded.
            NetworkMgr.LastDisconnectionType = NetworkManager_UNET.DisconnectionTypes.Timeout; //if the player disconnects unexpectedly, it's then a timeout
        }

        //Letting the server/host know that a player is ready:
        [Command]
        public void CmdOnClientSceneReady()
        {
            //Register the new client:
            ClientsInfoVars NewClient = new ClientsInfoVars();
            NewClient.FactionID = FactionID;
            NewClient.Turn = 0;
            NewClient.Disconneted = false;
            NewClient.Timeout = false;
            GameMgr.Factions[GameManager.MasterFactionID].MFactionMgr_UNET.ClientsInfo.Add(NewClient);

            //inform the server/host that one of the player's scene is now ready
            GameMgr.Factions[GameManager.MasterFactionID].MFactionMgr_UNET.OnClientSceneReady();
        }

        //Whenever a player's scene is ready this is called in the server/host:
        public void OnClientSceneReady()
        {
            ReadyPlayers++;
            if (ReadyPlayers == GameMgr.Factions.Count) //if all registered players are ready.
            {
                AllPlayersReady = true; //this will allow the server to start sending input commands.
                AllClientsSynced = true; //intially all clients are synced in
                SyncTestEnabled = false; //sync test initially is disabled
            }
        }

        //Clients use this command to send input to the server/host:
        [Command]
        public void CmdSendInput(byte SourceMode, byte TargetMode, int SourceID, string GroupSourceID, int TargetID, Vector3 InitialPos, Vector3 TargetPos, int Value, int FactionID)
        {
            InputVars Input = new InputVars();
            Input.SourceMode = SourceMode;
            Input.TargetMode = TargetMode;
            Input.SourceID = SourceID;
            Input.GroupSourceID = GroupSourceID;
            Input.TargetID = TargetID;
            Input.InitialPos = InitialPos;
            Input.TargetPos = TargetPos;
            Input.Value = Value;
            Input.FactionID = FactionID;

            //add this input command in the list:
            GameMgr.Factions[GameManager.MasterFactionID].MFactionMgr_UNET.InputActions.Add(Input);
        }

        //Rpc event called whenever the server sends a command to a client
        [ClientRpc]
        public void RpcOnCommandReceived(byte SourceMode, byte TargetMode, int SourceID, string GroupSourceID, int TargetID, Vector3 InitialPos, Vector3 TargetPos, int Value, int FactionID)
        {
            InputVars Command = new InputVars();
            
            Command.SourceMode = SourceMode;
            Command.TargetMode = TargetMode;
            Command.SourceID = SourceID;
            Command.GroupSourceID = GroupSourceID;
            Command.TargetID = TargetID;
            Command.InitialPos = InitialPos;
            Command.TargetPos = TargetPos;
            Command.Value = Value;
            Command.FactionID = FactionID;
            Command.Team = GameMgr.Factions[FactionID].team;
            InputManager.Instance.LaunchCommand(Command);
        }
        [Command]
        public void CmdSpawnSpy()
        {
            GameManager.FactionInfo faction = GameMgr.Factions[FactionID];
            if(faction.team == 0) {
                Vector3 Pos = new Vector3(88, 2, 106);

                teamOneRpgSpy = Instantiate(teamOneRpgSpy);
            teamOneRpgSpy.SetActive(false);
            teamOneRpgSpy.transform.position = Pos;
            teamOneRpgSpy.SetActive(true);
            teamOneRpgSpy.transform.GetChild(1).gameObject.SetActive(false);
            NetworkServer.SpawnWithClientAuthority(teamOneRpgSpy, connectionToClient);
            }
            else
            {
                Vector3 Pos = new Vector3(22, 2, 34.3f);

                teamTwoRpgSpy = Instantiate(teamTwoRpgSpy);
                teamTwoRpgSpy.SetActive(false);
                teamTwoRpgSpy.transform.position = Pos;
                teamTwoRpgSpy.SetActive(true);
                teamTwoRpgSpy.transform.GetChild(1).gameObject.SetActive(false);

                NetworkServer.SpawnWithClientAuthority(teamTwoRpgSpy, connectionToClient);

            }
        }
        [Command]
        public void CmdCheckPlayerType()
        {
            
            GameManager.FactionInfo faction = GameMgr.Factions[FactionID];


            if (faction.playerType == "rpg" && faction.team== 0)
            {
                Vector3 Pos = new Vector3(88, 2, 106);
                
                rpgPlayerOne = Instantiate(rpgPlayerOne);
                rpgPlayerOne.SetActive(false);
                rpgPlayerOne.transform.position = Pos;
                rpgPlayerOne.SetActive(true);
                rpgPlayerOne.GetComponent<stats>().FactionId = FactionID;
                rpgPlayerOne.GetComponent<stats>().Team = faction.team; ;
                rpgPlayerOne.GetComponent<stats>().StartPosition = Pos;
                NetworkServer.SpawnWithClientAuthority(rpgPlayerOne, connectionToClient);

                RpcCheckPlayerType(1);
            }
            else if (faction.playerType == "rpg" && faction.team == 1)
            {
                
                Vector3 Pos = new Vector3(22, 2, 34.3f);
                rpgPlayerTwo = Instantiate(rpgPlayerTwo);
                rpgPlayerTwo.SetActive(false);
                rpgPlayerTwo.transform.position = Pos;
                rpgPlayerTwo.SetActive(true);
                rpgPlayerTwo.GetComponent<stats>().FactionId = FactionID;
                rpgPlayerTwo.GetComponent<stats>().FactionId = FactionID;
                rpgPlayerTwo.GetComponent<stats>().Team = faction.team; ;
                rpgPlayerTwo.GetComponent<stats>().StartPosition = Pos;
                NetworkServer.SpawnWithClientAuthority(rpgPlayerTwo, connectionToClient);

                RpcCheckPlayerType(2);
            }

        }
        [Command]
        public void CmdAttackRpgList(string unitIdString,int team)
        {
          
            RpcAttackRpgList(unitIdString, team);
        }
        [ClientRpc]
        public void RpcAttackRpgList(string unitIdString,int team)
        {
            List<Unit> unitList = InputManager.StringToUnitList(unitIdString);
            GameObject TargetObj = GameObject.FindObjectOfType<stats>().gameObject;
            MovementManager.Instance.LaunchAttackLocal(unitList, TargetObj, MovementManager.AttackModes.Full);
        }
        [Command]
        public void CmdAttackRpg(int unitId,int team)
        {
            RpcAttackRpg(unitId, team);
        }
        [ClientRpc]
        public void RpcAttackRpg(int unitId,int team)
        {
            if(unitId >= 0) { 
                GameObject refUnit = InputManager.Instance.SpawnedObjects[unitId];
                GameObject TargetObj = GameObject.FindObjectOfType<stats>().gameObject;

                MovementManager.Instance.LaunchAttackLocal(refUnit.GetComponent<Unit>(), TargetObj, MovementManager.AttackModes.Change);
            }

        }
        [ClientRpc]
        public void RpcCheckPlayerType(int team)
        {
            
            if (FactionID == GameManager.PlayerFactionID)
                if (GameMgr.Factions[FactionID].playerType == "rpg" && GameMgr.Factions[FactionID].team == team - 1)
                {
                    print("entered");

                    Camera.main.gameObject.SetActive(false);
                    rpgCamera = Instantiate(rpgCamera);
                    rpgCamera.GetComponent<ThirdPersonCamera>().teamNo = team;
                    rpgCanvas = Instantiate(rpgCanvas);

                    Transform[] childs = rpgCanvas.GetComponentsInChildren<Transform>();
                    PlayerController[] players = FindObjectsOfType<PlayerController>();
                    GameObject tempPlayer = null;
                    foreach (PlayerController player in players)
                    {
                        if (player.tag == "team" + team + "Rpg")
                        {
                            player.GetComponent<stats>().Team = team - 1;
                            tempPlayer = player.gameObject;
                            rpgCamera.GetComponent<ThirdPersonCamera>().Target = player.gameObject.transform.GetChild(0);
                            player.GetComponent<stats>().Canvas = rpgCanvas;
                        }
                    }
                  foreach (Transform child in childs)
                    {
                        if (child.gameObject.tag == "spy")
                        {
                            child.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                            {
                                 tempPlayer.GetComponent<AbilityUse>().onAbilityUse(child.gameObject);
                            });
                        }
                        else if (child.gameObject.tag == "firearrow")
                        {
                            child.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                            {
                                tempPlayer.GetComponent<fireArrowAbilityUse>().onAbilityUse(child.gameObject);
                            });
                        }
                    }

                    Destroy(GameMgr.SelectionMgr);
                    Destroy(GameObject.FindGameObjectWithTag("strategyCanvas"));

                }
        }
        void Update ()
		{
			//Lockstep multiplayer:
			if (isLocalPlayer) { //if this is the local player:
				//faction init:
				if (IsFactionSpawned == false && GameManager.AllFactionsReady == true && GameMgr.Factions[FactionID].playerType == "strategy") { //as long as the faction hasn't spawned yet
					if (ClientScene.ready == true) { //as soon as the connection is ready
                        //set as MFaction Manager for the Input Manager:
                        InputManager.UNET_Mgr = this;
                        //now spawn the faction capital:
                       
                            CmdSendInput((byte)InputSourceMode.FactionSpawn, (byte)InputTargetMode.None, -1, "", -1, GameMgr.Factions[FactionID].CapitalPos, Vector3.zero, 0, FactionID);
                        IsFactionSpawned = true; //mark faction as spawned
                        //inform the server that this player is ready to start receiving commands.
                        GameMgr.Factions [FactionID].MFactionMgr_UNET.CmdOnClientSceneReady ();
					}
				}
                else if(IsRpgSpawned == false && GameMgr.Factions[FactionID].playerType == "rpg")
                {
                   if(ClientScene.ready == true) {
                        InputManager.UNET_Mgr = this;
                        CmdCheckPlayerType();
                        CmdSpawnSpy();
                        IsRpgSpawned = true;
                        GameMgr.Factions[FactionID].MFactionMgr_UNET.CmdOnClientSceneReady();

                    }
                }
				if (isServer == true && AllPlayersReady == true) { //if this is the server and all players are ready and synced in
                    
                    //When the game is frozen, this needs to keep running:
                    float DeltaTime = Time.deltaTime;
                    if (Time.timeScale == 0.000001f)
                    {
                        DeltaTime *= 1000000;
                    }

                    //if all clients are synced in
                    if (AllClientsSynced == true)
                    {
                        //the lockstep cycle:
                        CycleTimer = CycleTimer + DeltaTime;

                        while (CycleTimer > LockstepCycle)
                        {
                            ServerTurn++; //increase the turn amount

                            //if the server already collected input:
                            if (InputActions.Count > 0)
                            {
                                //go through all collected input commands
                                foreach (InputVars Input in InputActions)
                                {
                                    //send them to all clients
                                    RpcOnCommandReceived(Input.SourceMode, Input.TargetMode, Input.SourceID, Input.GroupSourceID, Input.TargetID, Input.InitialPos, Input.TargetPos, Input.Value, Input.FactionID);
                                }
                                InputActions.Clear(); //clear the list
                            }

                            //trigger this RPC call to all clients:
                            RpcOnCommandsReceived();

                            //if this is the first lockstep turn to sync:
                            if (ServerTurn == SyncTestTriggerTurn)
                            {
                                //then start the sync test timer:
                                SyncTestTimer = 0.0f;
                                SyncTestEnabled = true;
                            }

                            CycleTimer = CycleTimer - LockstepCycle; //reset the input cycle's timer:
                        }
                    }
                    else
                    {
                        //if not all clients are synced in
                        //go through all clients
                        foreach(ClientsInfoVars Client in ClientsInfo)
                        {
                            if(Client.Timeout == true && Client.Disconneted == false) //if this client is timing out and still connected
                            {
                                //keep the kick timer going
                                Client.KickTimer -= DeltaTime;
                                if(Client.KickTimer <= 0.0f) //if the timer is over
                                {
                                    //kick the client
                                    if (GameMgr.Factions[Client.FactionID].MFactionLobby_UNET != null)
                                         GameMgr.Factions[Client.FactionID].MFactionMgr_UNET.connectionToClient.Disconnect();
                                    Client.Disconneted = true;
                                }
                            }
                        }
                    }

                    //Sync test:

                    if (SyncTestEnabled == true) //if the sync test is enabled
                    {
                        SyncTestTimer = SyncTestTimer + DeltaTime;

                        //when the timer is over
                        while (SyncTestTimer > SyncTestReload)
                        {
                            //perform a sync test
                            SyncTest();
                        }
                    }
                }
			}
		}

        //Checking if all clients are on the same turn:
        //after the server sends commands to clients, this rpc call is sent:
        [ClientRpc]
        public void RpcOnCommandsReceived()
        {
            //increase the current turn:
            CurrentTurn++;
            //in return each client sends a message to the server informing him the commands have been successfully received:
            GameMgr.Factions[GameManager.PlayerFactionID].MFactionMgr_UNET.CmdReportSuccessfulTurn(); //make sure to send the local faction ID
        }

        //triggered by the client in the server when the client successfully received all commands in a turn and wants to 
        [Command]
        public void CmdReportSuccessfulTurn ()
        {
            GameMgr.Factions[GameManager.MasterFactionID].MFactionMgr_UNET.UpdateClientTurn(FactionID);
        }

        //increment a client turn in the client info list (meaning that the client has reported he has successfully synced to that turn)
        public void UpdateClientTurn(int ID)
        {
            //now the server will increment the synced turns of the client with the provided Faction ID:
            int i = 0;

            //look through all client infos
            while (i < ClientsInfo.Count)
            {
                //if the reportee faction is this one
                if (ClientsInfo[i].FactionID == ID)
                {
                    ClientsInfo[i].Turn++; //increase the turn
                    
                    //as soon as a client reports a successful turn while the game is frozen and waiting for all players to sync in
                    if (AllClientsSynced == false)
                    {
                        //do the sync test:
                        SyncTest();
                    }
                    return;
                }
                i++;
            }
        }


        //The sync test: check all client info and see if they are all in the expected sync turn:
        public void SyncTest ()
        {
            AllClientsSynced = true; //if all clients are synced 
            int i = 0;

            //go through all client infos
            while (i < ClientsInfo.Count)
            {
                //make sure the client is not disconnected:
                if (ClientsInfo[i].Disconneted == false)
                {
                    //if this client current turn is smaller or equal to the reported synced turns:
                    if (ClientsInfo[i].Turn <= SyncedTurns)
                    {
                        //this means that this client is still behind:
                        AllClientsSynced = false; //not all clients are now synced in
                        //if the client is not already timing out:
                        if (ClientsInfo[i].Timeout == false)
                        {
                            ClientsInfo[i].KickTimer = TimeoutTime; //client will be kicked when this is timer is over if the client stays timed out
                            ClientsInfo[i].Timeout = true; //this client is now timing out
                        }
                    }
                    else if(ClientsInfo[i].Timeout == true) //if client was behind but still marked as timing out
                    {
                        //mark as not timing out:
                        ClientsInfo[i].Timeout = false;
                    }
                }
                i++;
            }

            if(AllClientsSynced == (GameManager.GameState == GameStates.Frozen)) //if there's a change in the sync state:
            {
                RpcFreezeGame(!AllClientsSynced); //(un)freeze the game depedning on the sync state
            }

            if(AllClientsSynced == true) //if all clients are synced in
            {
                SyncedTurns++; //increase the synced tunrs
            }

            SyncTestTimer = SyncTestTimer - SyncTestReload; //reload the timer
        }

        //Freezing the game:
        [ClientRpc]
        public void RpcFreezeGame (bool Freeze)
        {
            //set the new game state
            GameManager.GameState = (Freeze == true) ? GameStates.Frozen : GameStates.Running;
            //set the time scale:
            Time.timeScale = (Freeze == false) ? 1.0f : 0.000001f;
            //show/hide the game freeze message
            try {    
            GameMgr.UIMgr.FreezeMenu.gameObject.SetActive(Freeze);
            }
            catch(MissingReferenceException)
            {
                return; 
            }
        }
    }
}