using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class Lockstep : Photon.MonoBehaviour
{
  private static readonly GameLogger logger = GameLogger.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

  // -------------------------------------------------------------------------------------------------------------------
  
  enum State
  {
    RunningSinglePlayer,
    SettingUpMultiplayer,
    RunningMultiplayer
  };

  // -------------------------------------------------------------------------------------------------------------------
  // Modifiable by GUI
  public  int                       m_lockStepSize = 10;

  private PhotonView                m_nv;
  private List<LockstepSimulation>  m_objectsToSimulate;
  private float                     m_simulationDeltaTime;
  private int                       m_simulationCount;
  private int                       m_lockStepTurnId;
  private List<int>                 m_readyPlayers;
  private List<int>                 m_playersConfirmedImReady;
  private bool                      m_initialized;

  private int                       m_numberOfPlayers;
  public  int                       numberOfPlayers { get {return m_numberOfPlayers;}}

  // TODO: Fullösning för att kunna kombinera mult/single player tills vidare.
  private State                     m_state;

  private PendingCommands           m_pendingCommands;
  private ConfirmedCommands         m_confirmedCommands;
  private Queue<Command>            m_commandQueueMine;

  // Singleton
  private static Lockstep           m_instance;
  public  static Lockstep           instance { get { return m_instance; } }

  #region Initialisation
  // -------------------------------------------------------------------------------------------------------------------
  
  private void Awake ()
  {
    m_instance = this;

    m_objectsToSimulate = new List<LockstepSimulation> ();
    m_commandQueueMine  = new Queue<Command> ();

    NetworkManager.instance.OnEnterMultiplayer += OnEnterMultiplayer;
    NetworkManager.instance.OnGameSetup        += OnGameSetup;

    m_initialized = false;

    m_state = State.RunningSinglePlayer;

    // Must be the same on all machines sharing a multiplayer game!
    m_simulationDeltaTime = 0.02f; // TODO: Should be Time.fixedDeltaTime, hard coded now to be able to simulate lag using fixedDeltaTime.
  }
  
  // -------------------------------------------------------------------------------------------------------------------

  private void Start ()
  {
    m_nv = NetworkManager.instance.photonView;
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  public void AddObjectToSimulate (LockstepSimulation O)
  {
    m_objectsToSimulate.Add(O);
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  public void RemoveObjectToSimulate (LockstepSimulation O)
  {
    m_objectsToSimulate.Remove(O);
  }
  
  #endregion

  #region StartGame
  // -------------------------------------------------------------------------------------------------------------------
  
  public void InitGameStartLists ()
  {
    if (m_initialized)
      return;

    m_readyPlayers            = new List<int> (m_numberOfPlayers);
    m_playersConfirmedImReady = new List<int> (m_numberOfPlayers);
    
    m_initialized = true;
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  void OnEnterMultiplayer ()
  {
    m_state = State.SettingUpMultiplayer;
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  // Called on all clients+server when NetworkManager has determined that the game is ready to go
  //
  // Initialises the class and tells all others that we are ready to start
  void OnGameSetup ()
  {
    logger.Debug ("Game setup, my Player ID: " + PhotonNetwork.player.ID);

    m_lockStepTurnId    = 0;
    m_simulationCount   = 0;
    m_numberOfPlayers   = NetworkManager.instance.playerCount;

    InitGameStartLists ();

    m_pendingCommands    = new PendingCommands   (m_numberOfPlayers);
    m_confirmedCommands  = new ConfirmedCommands (m_numberOfPlayers);

    
    m_nv.RPC ("ReadyToStart", PhotonTargets.OthersBuffered, PhotonNetwork.player.ID);
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  private void CheckGameStart ()
  {
    if (m_playersConfirmedImReady == null)
    {
      logger.Error ("ERROR!!! Unexpected null reference during game start. IsInit? " + m_initialized);
      return;
    }

    //check if all expected players confirmed our gamestart message
    if (m_playersConfirmedImReady.Count == m_numberOfPlayers - 1)
    {
      //check if all expected players sent their gamestart message
      if (m_readyPlayers.Count == m_numberOfPlayers - 1)
      {
        //we are ready to start
        logger.Debug ("All players are ready to start. Starting Game.");
        
        //we no longer need these lists
        m_playersConfirmedImReady = null;
        m_readyPlayers = null;
        
        GameStart();
      }
    }
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  private void GameStart ()
  {
    //start the LockStep Turn loop
    //LockStepTurn();
    //enabled = true;

    m_state = State.RunningMultiplayer;

    logger.Debug ("Game starting!");
    logger.PushContext ("T0");
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  // Called on all clients+server for each client+server (except itself) that is ready to go
  //
  // Adds ready player to m_readyPlayers, sends confirmation to server and checks if ready to start
  [RPC]
  public void ReadyToStart (int playerID)
  {
    logger.Debug ("Player " + playerID + " is ready to start the game.");
    
    //make sure initialization has already happened -incase another player sends game start before we are ready to handle it
    InitGameStartLists();

    m_readyPlayers.Add(playerID);
    
    if (PhotonNetwork.isMasterClient) //don't need an rpc call if we are the server
      ConfirmReadyToStartServer(PhotonNetwork.player.ID /*confirmingPlayerID*/, playerID /*confirmedPlayerID*/);
    else
      m_nv.RPC ("ConfirmReadyToStartServer", PhotonTargets.MasterClient, PhotonNetwork.player.ID /*confirmingPlayerID*/, playerID /*confirmedPlayerID*/);

    //Check if we can start the game
    CheckGameStart();
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  // Called on the server for each player pair that confirms the another one is ready to go
  //
  // Check for player validity and
  [RPC]
  public void ConfirmReadyToStartServer (int confirmingPlayerID, int confirmedPlayerID)
  {
    //workaround when multiple players running on same machine
    if (!PhotonNetwork.isMasterClient)
      return;

    logger.Debug ("Server Message: Player " + confirmingPlayerID + " is confirming Player " + confirmedPlayerID + " is ready to start the game.");
    
    //validate ID
    if (!NetworkManager.instance.players.ContainsKey (confirmingPlayerID))
    {
      //TODO: error handling
      logger.Error ("Server Message: WARNING!!! Unrecognized confirming playerID: " + confirmingPlayerID);
      return;
    }
    if (!NetworkManager.instance.players.ContainsKey (confirmedPlayerID))
    {
      //TODO: error handling
      logger.Error ("Server Message: WARNING!!! Unrecognized confirmed playerID: " + confirmingPlayerID);
    }
    
    // relay message to confirmed client
    if (PhotonNetwork.player.ID == confirmedPlayerID)
      ConfirmReadyToStart(confirmedPlayerID, confirmingPlayerID);  //don't need an rpc call if we are the player
    else
      m_nv.RPC ("ConfirmReadyToStart", PhotonTargets.OthersBuffered, confirmedPlayerID, confirmingPlayerID);
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  [RPC]
  public void ConfirmReadyToStart (int confirmedPlayerID, int confirmingPlayerID)
  {
    if (PhotonNetwork.player.ID != confirmedPlayerID)
      return;

    logger.Debug ("Player " + confirmingPlayerID + " confirmed I am ready to start the game.");
    m_playersConfirmedImReady.Add(confirmingPlayerID);
    
    //Check if we can start the game
    CheckGameStart ();
  }

  #endregion

  #region Lockstep

  // -------------------------------------------------------------------------------------------------------------------
  
  public void EnqueueCommand (Command command)
  {
    m_commandQueueMine.Enqueue (command);
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  private bool HandleOneOfMyCommands () 
  {
    // Check for an empty queue
    if (m_commandQueueMine.Count == 0)
      new GeneralCommands.EmptyCommand (); // This will add an empty command to the queue
    
    Command command = m_commandQueueMine.Dequeue ();

    m_pendingCommands.AddCommand (command, PhotonNetwork.player.ID, m_lockStepTurnId, m_lockStepTurnId);
    m_confirmedCommands.AddCommand (PhotonNetwork.player, m_lockStepTurnId, m_lockStepTurnId); // confirm our own action
        
    //send action to all other players
    logger.Debug ("Sent " + (command.GetType ().Name) + " command for turn " + m_lockStepTurnId);
    m_nv.RPC ("RecieveCommand", PhotonTargets.Others, m_lockStepTurnId, PhotonNetwork.player.ID, command.objectNameToCommand, command.Serialize ());
    
    return true;
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  [RPC]
  public void RecieveCommand (int commandsLockStepTurnId, int playerID, string objectToCommand, string serializedCommand) 
  {
    Command command = Command.Deserialize (serializedCommand);

    if(command == null) 
    {
      logger.Error ("Reception of command failed");
      return;
    }

    m_pendingCommands.AddCommand (command, playerID, m_lockStepTurnId, commandsLockStepTurnId);

    //send confirmation
    if(PhotonNetwork.isMasterClient) 
      ConfirmCommandServer (commandsLockStepTurnId, PhotonNetwork.player.ID, playerID);
    else 
    {
      // logger.Debug ("ConfirmCommandServer RPC Player " + playerID + " command confirmed for turn " + commandsLockStepTurnId);
      m_nv.RPC ("ConfirmCommandServer", PhotonTargets.MasterClient, commandsLockStepTurnId, PhotonNetwork.player.ID, playerID);
    }
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  [RPC]
  public void ConfirmCommandServer (int commandsLockStepTurnId, int confirmingPlayerID, int confirmedPlayerID)
  {
    // Workaround - if server and client on same machine
    if (!PhotonNetwork.isMasterClient)
      return;

    //we don't need an RPC call if this is the player
    if (PhotonNetwork.player.ID == confirmedPlayerID)
      ConfirmCommand (commandsLockStepTurnId, confirmingPlayerID);
    else
    {
      // logger.Debug ("ConfirmCommand RPC Player " + confirmedPlayerID + " command confirmed for turn " + commandsLockStepTurnId);
      m_nv.RPC ("ConfirmCommand", NetworkManager.instance.players [confirmedPlayerID], commandsLockStepTurnId, confirmingPlayerID);
    }
  }

  // -------------------------------------------------------------------------------------------------------------------

  [RPC]
  public void ConfirmCommand (int commandsLockStepTurnId, int confirmingPlayerID)
  {
//    if (confirmingPlayerID != PhotonNetwork.player.ID)
//      logger.Debug ("Received confirmation for command turn " + commandsLockStepTurnId);

    PhotonPlayer player = NetworkManager.instance.players [confirmingPlayerID];

    m_confirmedCommands.AddCommand (player, m_lockStepTurnId, commandsLockStepTurnId);
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  private void UpdateSimulation () 
  {
    logger.PushContext ("UpdateSimulation");

    // Update simulation
    foreach (LockstepSimulation ls in m_objectsToSimulate)
      ls.UpdateSimulation (m_simulationDeltaTime);
    
    m_simulationCount++;
    
    if ((m_simulationCount % 100) == 0)
      logger.Debug ("Current simulation count = " + m_simulationCount);

    logger.PopContext ();
  }
        
  // -------------------------------------------------------------------------------------------------------------------
  
  private void FixedUpdate () 
  {
    // TODO: Temporary solution for co-existance of single/multi player
    if (m_state == State.RunningSinglePlayer)
    {
      if (m_commandQueueMine.Count > 0)
        m_commandQueueMine.Dequeue ().Execute ();
     
      UpdateSimulation ();
      return;
    }
    else if (m_state == State.RunningMultiplayer)
    {
      if ((m_simulationCount % m_lockStepSize) == 0) // Time to perform lockstep (if ready)
      {
        // Do no simulation and do not update simulation count if we were not ready
        if(!m_confirmedCommands.ReadyForNextTurn (m_lockStepTurnId) || 
           !m_pendingCommands.ReadyForNextTurn (m_lockStepTurnId)) 
        {
          logger.Warning ("Lockstep turn failed, too fast?");
          return;
        }

        m_confirmedCommands.NewTurn ();
        m_pendingCommands.NewTurn ();
       
        if (m_lockStepTurnId >= 3)
          m_pendingCommands.ExecuteCurrentCommands ();

        m_lockStepTurnId++;
        logger.PopContext ();
        logger.PushContext ("T" + m_lockStepTurnId);

        HandleOneOfMyCommands ();
      }
      
      UpdateSimulation ();
    }
  }

  #endregion
}
