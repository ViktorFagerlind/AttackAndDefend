using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorkerAI : WalkToManyTargets
{
  public delegate void WorkerDoneDelegate(WorkerAI workerAi);

  // -------------------------------------------------------------------------------------------------------------------
  
  enum State
  {
    Idle,
    WalkingToTarget, 
    Haggling, 
    WalkingBack
  };

  // -------------------------------------------------------------------------------------------------------------------

  private PlayerManager  m_player;
  private State       m_state;
  private Vector3     m_spawningPoint;
  private Vector3     m_target;
  WorkerDoneDelegate  m_onWorkerDone;
  private List<GameObject> m_visitedBazaars;
  private int m_money;//Current amount of money
  private int m_maxmoney; //Amount of money received when reaching spawn point
  public GameObject m_moneyIndicator;

  public override void Start ()
  {
    base.Start ();

    m_visitedBazaars = new List<GameObject>();
    //m_moneyIndicator = GetComponentsInChildren<Renderer>();
    m_maxmoney = 100;
    setNewAmountOfMoney(m_maxmoney);
  }

  public void SetPlayer (PlayerManager newPlayer) {
    m_player = newPlayer;
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  public WorkerDoneDelegate workerDoneDelegate
  {
    set { m_onWorkerDone = value;}
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  public override void Awake()
  {
    base.Awake();

    m_target = GameObject.FindWithTag("Target").transform.position;
    m_state = State.Idle;
  }
  
  // -------------------------------------------------------------------------------------------------------------------

  public void MarchToTarget(List<Vector3> intermediatePoints)
  {
    if (m_state != State.Idle)
    {
      //Debug.LogError ("Not in idle state when commanded to start");
      return;
    }

    // positionOffset = new Vector3 (Random.Range (-10, 10), Random.Range (-10, 10), 0);

    m_spawningPoint = transform.position;
    
    m_state = State.WalkingToTarget;

    if (intermediatePoints == null)
      SetTarget(m_target);
    else
    {
      List<Vector3> points = new List<Vector3>(intermediatePoints);
      points.Add(m_target);
      SetTargets(points);
    }
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  protected override void OnAllTargetsReached()
  {
    //Debug.Log ("State: " + m_state);
    if (m_state == State.WalkingToTarget)
    {
      //Debug.Log ("Reached target!");
      //TODO här ska vi anropa någon övre funktion och säga hur mycket pengar vi stoppar i stora peninghögen
      m_player.incrCurrentMoney(m_money);
      setNewAmountOfMoney(0);
      walkBackToSpawnPoint();
      return;
    } 

    if (m_state == State.WalkingBack)
    {
      //Debug.Log ("Back and ready to work.");
      setNewAmountOfMoney(m_maxmoney); 
      m_state = State.Idle;
      m_onWorkerDone(this);
      return;
    }

    //Debug.LogError ("Illegal state for OnAllTargetsReached.");
  }

  // -------------------------------------------------------------------------------------------------------------------

  void walkBackToSpawnPoint () 
  {      
    m_state = State.WalkingBack;
    SetTarget(m_spawningPoint);
  }

  // -------------------------------------------------------------------------------------------------------------------

  public override void UpdateSimulation (float dt)
  {
    if (m_state != State.Haggling)
      Move (dt);
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  public bool startHaggling(GameObject bazaarOfMerchant)
  {
    if (m_state != State.WalkingToTarget)
      return false;

    m_state = State.Haggling;
    m_visitedBazaars.Add(bazaarOfMerchant);

    return true;
  }

  // -------------------------------------------------------------------------------------------------------------------
  public void setNewAmountOfMoney(int newAmountOfMoney) 
  {
    m_money = newAmountOfMoney;
    m_moneyIndicator.transform.localScale = new Vector3(4,10*m_money/m_maxmoney,4);
  }
  
  public void finishHaggling()
  {
    if (m_state == State.Haggling)
    {
      if (m_money > 0.01) {
        //We have money left, huzzah!
        m_state = State.WalkingToTarget;
      } else {
        //We have no money left! We need to turn back!
        walkBackToSpawnPoint();
      }

    } 
  }
  
  public bool purchaseObject(int cost)
  {
    //TODO visa en snygg liten bild som visar att det pågår
    if (m_money > cost)
    {
      setNewAmountOfMoney(m_money - cost);
      return true;
    } 
    else
    {
      setNewAmountOfMoney(0);
      return false;
    }
  }

  public bool hasVisitedBazaar(GameObject bazaarToCheck)
  {
    return m_visitedBazaars.Contains(bazaarToCheck);
  }

  // -------------------------------------------------------------------------------------------------------------------
  public void setColor(Color theNewColor) {
    Transform tmp = transform.Find ("Money Indicator");
    Renderer tmprenderer = tmp.GetComponent<Renderer>();
    tmprenderer.material.color = theNewColor;
  }

}