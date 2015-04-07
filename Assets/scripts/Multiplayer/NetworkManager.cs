using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(PhotonView))]
public class NetworkManager : MonoBehaviour, StaticGuiObject
{
  private static readonly GameLogger logger = GameLogger.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

  enum State
  {
    Idle,
    Connected,
    WaitingInRoom,
    Started
  };

  // -------------------------------------------------------------------------------------------------------------------

  private State                             m_state;
  private int                               m_nofPlayersToStart = 2;
  static private bool                       multiplayerMode = false;
  
  private Dictionary<int, PhotonPlayer>     m_players;
  public  Dictionary<int, PhotonPlayer>     players     {get {return m_players;}}
  public  int                               playerCount {get {return m_players.Count;}}

  // -------------------------------------------------------------------------------------------------------------------
  
  public  delegate void                     NetworkManagerEvent ();
  public  NetworkManagerEvent               OnEnterMultiplayer;
  public  NetworkManagerEvent               OnConnectedToGame;
  public  NetworkManagerEvent               OnGameSetup;

  private PhotonView                        m_pv;
  public  PhotonView                        photonView {get {return m_pv;}}

  // Singleton
  private static NetworkManager             m_instance;
  public  static NetworkManager             instance {get { return m_instance;}}

  // -------------------------------------------------------------------------------------------------------------------

  void Awake()
  {
    m_instance = this;
    m_state    = State.Idle;
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  private void Start()
  {
    m_pv      = GetComponent<PhotonView> ();
    
    m_players = new Dictionary<int, PhotonPlayer> (m_nofPlayersToStart);

    GuiManager.instance.addStaticGuiObject (this);

    // TODO: Fullösning för att kunna kombinera mult/single player tills vidare.
    if (multiplayerMode && OnEnterMultiplayer != null)
      OnEnterMultiplayer ();
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  private void OnDisconnectedFromPhoton   () {logger.Debug ("Disconnected from photon");}
  private void OnFailedToConnectToPhoton  () {logger.Debug ("Failed to connect to photon");}
  private void OnConnectionFail           () {logger.Debug ("Connection to photon failed");}

  private void OnConnectedToPhoton        () {logger.Debug ("Connected to photon");}
  private void OnCreatedRoom              () {logger.Debug ("Room created");}
  private void OnConnectedToMaster        () {logger.Debug ("Connected to photon master server");}

  // -------------------------------------------------------------------------------------------------------------------
  
  private void OnJoinedLobby ()             
  {
    logger.Debug ("Joined lobby");

    m_state = State.Connected;
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  private void OnJoinedRoom ()
  {
    logger.Debug ("Room joined");

    logger.Debug ("OnJoinedRoom, playerID:" + PhotonNetwork.player.ID);
    if (PhotonNetwork.isMasterClient)
      m_players.Add (PhotonNetwork.player.ID, PhotonNetwork.player);
        
    //Notify any delegates that we are connected to the game
    if (OnConnectedToGame != null)
      OnConnectedToGame ();
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  private void OnPhotonPlayerConnected (PhotonPlayer player)
  {
    logger.Debug ("OnPhotonPlayerConnected, playerID:" + player.ID);
    if (!PhotonNetwork.isMasterClient)
      return;

    m_players.Add (player.ID, player);
    logger.Debug ("Player Count : " + m_players.Count);
    
    //Once all expected players have joined, send all clients the list of players
    if (m_players.Count == m_nofPlayersToStart)
    {
      foreach (PhotonPlayer p in m_players.Values)
      {
        logger.Debug ("Calling RegisterPlayerAll...");
        m_pv.RPC ("RegisterPlayerAll", PhotonTargets.Others, p);
      }
      
      //start the game
      m_pv.RPC ("StartGame", PhotonTargets.All);
    }
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  // Called on clients only. Passes all connected players to be added to the players dictionary.
  [RPC]
  public void RegisterPlayerAll (PhotonPlayer player)
  {
    logger.Debug ("Register Player All called for " + player.ID);
    m_players.Add (player.ID, player);
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  [RPC]
  public void StartGame()
  {
    //send the start of game event
    if (OnGameSetup != null)
      OnGameSetup ();

    m_state = State.Started;
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  void OnDisconnectedFromServer(NetworkDisconnection info)
  {
    if (PhotonNetwork.isMasterClient)
      logger.Debug ("Local server connection disconnected");
    else if (info == NetworkDisconnection.LostConnection)
      logger.Debug ("Lost connection to the server");
    else
      logger.Debug ("Successfully diconnected from the server");
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  public float buildGui ()
  {
    float btnXSpace = 0;
    float btnYSpace = 5;
    float btnW      = 100;
    float btnH      = 30;

    float currentY  = 0;

    if (!multiplayerMode)
    {
      if (GuiManager.instance.menuButton (new Rect(btnXSpace, currentY, btnW, btnH), "Multiplayer"))
      {
        // TODO: Fullösning för att kunna kombinera mult/single player tills vidare.
        logger.Debug ("Reload level");
        UnityEngine.Random.seed = 123;
        Application.LoadLevel (Application.loadedLevel);

        logger.Debug ("Connecting to Photon Server");
        PhotonNetwork.ConnectUsingSettings ("0.1");

        multiplayerMode = true;
      }

      return btnYSpace*2 + btnH;
    }

    currentY = btnYSpace;

    if (m_state == State.Connected)
    {
      if (GuiManager.instance.menuButton (new Rect(btnXSpace, currentY, btnW, btnH), "Create/Join"))
      {
        RoomOptions ro = new RoomOptions ();
        ro.isOpen = ro.isVisible = ro.cleanupCacheOnLeave = true;
        ro.maxPlayers = 4;
        PhotonNetwork.JoinOrCreateRoom ("Capture the defense - Room 1", ro, new TypedLobby ("Super lobby", LobbyType.Default));

        m_state = State.WaitingInRoom;
      }
      currentY += btnH + btnYSpace;
    }

    return currentY;
  }
}
