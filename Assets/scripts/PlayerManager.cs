using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour, StaticGuiObject
{

  // -------------------------------------------------------------------------------------------------------------------
  
  enum State
  {
    Idle,
    PlacingBuilding,
    AcknowledgingBuilding
  };

  // -------------------------------------------------------------------------------------------------------------------
  
  public Transform  m_bazaarPrefab;

  // -------------------------------------------------------------------------------------------------------------------
  
  private Texture2D       m_buildBazaarTexture;
  private State           m_state;
  private GameObject      m_ongoingBazaar;
  private Vector3         m_ongoingBazaarPosition;

  private int             m_moneyGoal;
  private int             m_moneyCurrent;
  private int             m_food;

  private Color           m_playerColor;

  public int              m_playerId;

  // -------------------------------------------------------------------------------------------------------------------

  void Awake()
  {
    //TODO byt ut det här mot en singleton eller något liknande, som håller koll på sånahär globala saker
    Color[] colorArray = {Color.red, Color.blue, Color.yellow, Color.green};
    m_playerColor = colorArray[m_playerId];

  }

  void Start()
  {
    m_buildBazaarTexture = Resources.Load ("icons/BuildBazaar") as Texture2D;
    
    GuiManager.instance.addStaticGuiObject (this);

    m_state = State.Idle;

    m_moneyGoal = 10000;
    m_moneyCurrent = 0;
    m_food = 0;

  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  void Update()
  {
    switch (m_state)
    {
      case State.Idle:
        break;

      case State.PlacingBuilding:
        if (Input.GetButtonDown ("Fire1"))
        {
          Ray         ray;
          RaycastHit  hit;
          
          ray = Camera.main.ScreenPointToRay(Input.mousePosition);
          
          if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.tag == "Ground")
          {
            m_ongoingBazaarPosition = hit.point;
            m_ongoingBazaar = ((Transform)Instantiate (m_bazaarPrefab, m_ongoingBazaarPosition, Quaternion.identity)).gameObject;
            m_ongoingBazaar.GetComponent<BazarController> ().enabled = false;

            m_state = State.AcknowledgingBuilding;
          }
        }
        break;

      case State.AcknowledgingBuilding:

        break;
    }
  }

  // -------------------------------------------------------------------------------------------------------------------

  public Color getPlayerColor() {
    return m_playerColor;
  }

  public float buildGui ()
  {
    if (GuiManager.instance.menuButton (new Rect (10, 10, 80, 60), new GUIContent (m_buildBazaarTexture)) && m_state == State.Idle)
      m_state = State.PlacingBuilding;

    return 100;
  }

  public int getMoneyGoal() {
    return m_moneyGoal;
  }

  public void incrCurrentMoney(int moreMoney) {
    m_moneyCurrent = m_moneyCurrent + moreMoney;
  }
  
  public int getCurrentMoney() {
    return m_moneyCurrent;
  }

  public void AddFood(int foodToAdd) {
    m_food += foodToAdd;
  }
  
  public int getFood() {
    return m_food;
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  void OnGUI ()
  {
    if (m_state == State.AcknowledgingBuilding)
    {
      GUI.skin = GuiManager.instance.m_guiSkin;

      int nofItems = 2;

      bool ok     = GuiManager.instance.popupButton (m_ongoingBazaar.transform.position, 0, nofItems, GuiManager.instance.buttonOkTexture);
      bool cancel = GuiManager.instance.popupButton (m_ongoingBazaar.transform.position, 1, nofItems, GuiManager.instance.buttonNokTexture);

      if (ok)
        new GeneralCommands.InstantiatePrefabCommand ("GeneralCommands", "prefabs/BazaarPrefab", m_ongoingBazaarPosition);

      if (ok || cancel)
      {
        DestroyObject (m_ongoingBazaar.transform.root.gameObject);
        m_ongoingBazaar = null;
        m_state = State.Idle;
      }
    }
  }
}
