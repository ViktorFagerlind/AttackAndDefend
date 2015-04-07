using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SpawningPointAI : LockstepSimulation, GuiObject
{
  // -------------------------------------------------------------------------------------------------------------------
  
  [Serializable] class LevelUpSpawningPointCommand : Command
  {
    public LevelUpSpawningPointCommand (string objectNameToCommand) : base (objectNameToCommand) {}
    protected override void Perform (System.Object objectToCommand) {((SpawningPointAI)objectToCommand).increaseLevel();}
  }
  
  [Serializable] class BuildWorkerCommand : Command
  {
    public BuildWorkerCommand (string objectNameToCommand) : base (objectNameToCommand) {}
    protected override void Perform (System.Object objectToCommand) {((SpawningPointAI)objectToCommand).m_maxWorkerCount++;}
  }
  
  [Serializable] class SetWaypointsCommand : Command
  {
    private List<Position> m_waypointsToSet;

    public SetWaypointsCommand (string objectNameToCommand, List<Vector3> waypointsToSet) : base (objectNameToCommand) 
    {
      m_waypointsToSet = new List<Position> ();
      foreach (Vector3 p in waypointsToSet)
        m_waypointsToSet.Add (new Position (p));
    }
    protected override void Perform (System.Object objectToCommand) 
    {
      List<Vector3> waypointsToSet = new List<Vector3> ();
      foreach (Position p in m_waypointsToSet)
        waypointsToSet.Add (new Vector3 (p.x, p.y, p.z));

      ((SpawningPointAI)objectToCommand).waypoints = waypointsToSet;
    }
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  enum State
  {
    SelectingPath,
    Idle
  };

  // -------------------------------------------------------------------------------------------------------------------

  public Transform m_workerPrefab;
  public float     m_intervalTime       = 1;
  public int       m_maxWorkerCount     = 5;

  // -------------------------------------------------------------------------------------------------------------------

  private float    m_timeSimulation;
  private float    m_lastCreationTime;
  private int      m_workerCount;
  private int      m_level;

  private float      m_incomeCounter;

  private Color    m_originalColor;

  private State    m_state;

  private List<Vector3> m_waypoints;
  private List<Vector3> m_tmpWaypoints;

  public  PlayerManager m_player;

  private SpawnPointVisualController m_spawnPointVisualController;

  Texture2D             m_setWaypointsTexture;
  Texture2D             m_addWorkerTexture;
  Texture2D             m_levelUpTexture;

  SelectionObjectController m_selectionObjectController;

  // -------------------------------------------------------------------------------------------------------------------

  public List<Vector3> waypoints 
  {
    set { m_waypoints = value; }
  }

  // -------------------------------------------------------------------------------------------------------------------

  public override void Start ()
  {
    base.Start ();

    m_timeSimulation    = 0;
    m_lastCreationTime  = 0;
    m_state             = State.Idle;
    m_workerCount       = 0;
    m_level             = 5;

    m_waypoints         = new List<Vector3> ();
    m_tmpWaypoints      = null;


    m_spawnPointVisualController = GetComponentInChildren<SpawnPointVisualController>();

    m_spawnPointVisualController.setColor(m_player.getPlayerColor());

    m_setWaypointsTexture = Resources.Load ("icons/SetWaypoints") as Texture2D;
    m_addWorkerTexture    = Resources.Load ("icons/AddWorker")    as Texture2D;
    m_levelUpTexture      = Resources.Load ("icons/LevelUp")      as Texture2D;

    // Register as commandable objects
    CommandableDictionary.instance.register (name, this);

    m_selectionObjectController = GetComponentInChildren<SelectionObjectController> ();
  }
    
  // -------------------------------------------------------------------------------------------------------------------
  
  override public void OnDestroy ()
  {
    base.OnDestroy ();
    // Remove from commandable objects
    CommandableDictionary.instance.deRegister (name);
  }

  // -------------------------------------------------------------------------------------------------------------------

  public override void UpdateSimulation (float dt)
  {
    CreateWorkers ();

    m_timeSimulation += dt;

    // Add food!
    m_incomeCounter+= dt;
    if (m_incomeCounter>2) {
      m_incomeCounter=0;
      m_player.AddFood(m_level);
    }

  }

  // -------------------------------------------------------------------------------------------------------------------
  
  private void OnWorkerDone (WorkerAI workerAi)
  {
    workerAi.MarchToTarget (m_waypoints); 
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  private void CreateWorkers ()
  {
    if (m_workerCount >= m_maxWorkerCount)
      return;

    // Should we create worker?
    if (m_lastCreationTime + m_intervalTime < m_timeSimulation)
    {
      Transform instance = (Transform)Instantiate (m_workerPrefab, transform.position, Quaternion.identity);

    	// Update material
    	//instance.gameObject.GetComponentInChildren<Renderer> ().material = GetComponentInChildren<Renderer> ().material;
     
      WorkerAI workerAi = instance.gameObject.GetComponentInChildren<WorkerAI> ();

      workerAi.setColor(m_player.getPlayerColor());

      workerAi.SetPlayer (m_player);

      workerAi.workerDoneDelegate = OnWorkerDone;

      workerAi.MarchToTarget (m_waypoints); 

      m_lastCreationTime = m_timeSimulation;

      m_workerCount++;
    }
  }


  private void increaseLevel ()
  {
    m_level++;
    m_spawnPointVisualController.setVisualLevel(m_level);
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  private void OnMouseUp() 
  {
    GuiManager.instance.ObjectSelected (this);
  }

  // -------------------------------------------------------------------------------------------------------------------
  // GuiObject methods
  // -------------------------------------------------------------------------------------------------------------------

  public void markSelected ()
  {
    m_selectionObjectController.select ();
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  public bool unmarkSelected ()
  {
    if (m_state == State.SelectingPath)
      return false;

    m_selectionObjectController.unselect ();
        
    return true;
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  public void buildGui ()
  {
    int nofItems = 3;

    GUI.enabled = m_state == State.Idle;

    if (GuiManager.instance.popupButton (transform.position, 0, nofItems, m_levelUpTexture))
      new LevelUpSpawningPointCommand (name);
    
    if (GuiManager.instance.popupButton (transform.position, 1, nofItems, m_setWaypointsTexture))
      InitiateWaypointSelection ();
        
    if (GuiManager.instance.popupButton (transform.position, 2, nofItems, m_addWorkerTexture))
      new BuildWorkerCommand (name);

    // Handle the accnowledgement of the route

    if (m_state == State.SelectingPath)
    {
      GUI.enabled = true;

      if (GuiManager.instance.popupButtonL2 (transform.position, 1, nofItems, 0, 2, GuiManager.instance.buttonOkTexture))
        EndWaypointSelection (true);
        
      if (GuiManager.instance.popupButtonL2 (transform.position, 1, nofItems, 1, 2, GuiManager.instance.buttonNokTexture))
        EndWaypointSelection (false);
    }

    GUI.enabled = true;
  }

  
  // -------------------------------------------------------------------------------------------------------------------
  
  private void InitiateWaypointSelection ()
  {
    GameObject[] waypointObjs = GameObject.FindGameObjectsWithTag ("Waypoint");
    
    m_tmpWaypoints = new List<Vector3> ();

    foreach (GameObject wpo in waypointObjs) 
    {
      wpo.GetComponentInChildren<Projector> ().enabled = true;
      wpo.GetComponent<Waypoint> ().waypointClickedDelegate = WaypointClicked;
      wpo.GetComponent<Waypoint> ().selected = false;
    }

    m_state = State.SelectingPath;
  }
    
  // -------------------------------------------------------------------------------------------------------------------
  
  private void EndWaypointSelection (bool Success)
  {
    GameObject[] waypointObjs = GameObject.FindGameObjectsWithTag ("Waypoint");
    
    foreach (GameObject wpo in waypointObjs)
    {
      wpo.GetComponentInChildren<Projector> ().enabled = false;
      wpo.GetComponent<Waypoint> ().waypointClickedDelegate = null;
    }

    if (Success)
    {
      new SetWaypointsCommand (name, m_tmpWaypoints);
      m_tmpWaypoints = null;
    }

    m_state = State.Idle;
  }
    
    
  // -------------------------------------------------------------------------------------------------------------------

  public void WaypointClicked (Vector3 position, bool add)
  {
    if (add)
      m_tmpWaypoints.Add (position);
    else
      m_tmpWaypoints.Remove (position);
  }

}
