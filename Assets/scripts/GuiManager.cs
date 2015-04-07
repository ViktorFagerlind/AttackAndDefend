using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuiManager : MonoBehaviour 
{
  private GuiObject m_guiObject;

  public float  m_topHeight   = 20;
  public float  m_leftWidth   = 200;

  public float  m_popupAngleDist = 40;
  public float  m_popupRadius    = 110;

  public float  m_buttonWidth        = 80;
  public float  m_buttonHeight       = 60;
  
  public GUISkin m_guiSkin;

  public GUIStyle m_guiMenuButtonStyle;

  public PlayerManager m_playerToShow;
  
  // -------------------------------------------------------------------------------------------------------------------

  private Texture2D               m_OK;
  private Texture2D               m_NOK;

  private List<StaticGuiObject>   m_staticGuiObjects;

  // -------------------------------------------------------------------------------------------------------------------
  
  public Texture2D buttonOkTexture  {get { return m_OK;}}
  public Texture2D buttonNokTexture {get { return m_NOK;}}

  // Singleton
  private static GuiManager m_instance;
  public  static GuiManager instance {get { return m_instance;}}
  
  // -------------------------------------------------------------------------------------------------------------------
  
  void Awake()
  {
    m_instance = this;

    m_staticGuiObjects = new List<StaticGuiObject> ();
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  void Start () 
  {
    m_OK  = Resources.Load ("icons/GreenOK")  as Texture2D;
    m_NOK = Resources.Load ("icons/RedNOK") as Texture2D;
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  public void addStaticGuiObject (StaticGuiObject o)
  {
   if (o is PlayerManager) {
      if (o.Equals(m_playerToShow)) {
        m_staticGuiObjects.Add (o);
      }
    } else {
      m_staticGuiObjects.Add (o);

    }


  }

  // -------------------------------------------------------------------------------------------------------------------
  
  private Vector2 getRelativePos (int itemNr, int total, float radius)
  {
    if (itemNr < 0 || total <= 0)
      Debug.LogError ("Erroneous parameters!");
    
    float startAngle = -m_popupAngleDist * (float)(total - 1) / 2.0f;
    
    float angle = startAngle + itemNr * m_popupAngleDist;

    return radius * new Vector2 ( Mathf.Sin (angle * Mathf.PI / 180.0f), 
                                 -Mathf.Cos (angle * Mathf.PI / 180.0f));
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  public bool popupButton (Vector3 objectPos, int itemNr, int total, Texture2D texture)
  {
    int[] itemNrs = {itemNr};
    int[] totals  = {total};

    return popupButtonLevels (objectPos, itemNrs, totals, texture);
  }
   
  // -------------------------------------------------------------------------------------------------------------------
  
  public bool popupButtonL2 (Vector3 objectPos, int itemNr_1, int total_1,  int itemNr_2, int total_2, Texture2D texture)
  {
    int[] itemNrs = {itemNr_1,  itemNr_2};
    int[] totals  = {total_1,   total_2};
    
    return popupButtonLevels (objectPos, itemNrs, totals, texture);
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  private bool popupButtonLevels (Vector3 objectPos, int[] itemNrs, int[] totals, Texture2D texture)
  {
    Vector3 screenCenterPos = Camera.main.WorldToScreenPoint (objectPos);

    Vector2 relativePos = getRelativePos (itemNrs[0], totals[0], m_popupRadius);
    for (int i=1; i < itemNrs.Length; i++)
      relativePos += getRelativePos (itemNrs[i], totals[i], m_popupRadius * 0.8f);

    Vector2 pos = relativePos + new Vector2 (screenCenterPos.x, Screen.height - screenCenterPos.y);

    return GUI.Button (new Rect (pos.x - m_buttonWidth/2.0f, pos.y - m_buttonHeight/2.0f, m_buttonWidth, m_buttonHeight), 
                       texture);
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  public bool menuButton (Rect position, string content)
  {
    return menuButton (position, new GUIContent (content));
  }

  public bool menuButton (Rect position, GUIContent content)
  {
    return GUI.Button (position, content, m_guiMenuButtonStyle);
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  public bool ObjectSelected (GuiObject guiObject) 
  {
    // Check if changing selection is allowed
    if (!((MonoBehaviour)guiObject).enabled || (m_guiObject != null) && !m_guiObject.unmarkSelected ())
      return false;

    m_guiObject = guiObject;
    m_guiObject.markSelected ();

    return true;
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  void OnGUI () 
  {
    Rect groupBox;

    GUI.skin = m_guiSkin;

    GUI.color = new Color (GUI.color.r, GUI.color.g, GUI.color.b, 0.85f);


    // Draw status bar
    GUI.Box (new Rect (0, 0, Screen.width, m_topHeight), "");
    GUI.Label (new Rect (30,15,150,20), "Money: " + m_playerToShow.getCurrentMoney() + " / " + m_playerToShow.getMoneyGoal());
    GUI.Label (new Rect (160,15,100,20), "Food: " + m_playerToShow.getFood());

    
    // Draw left hand gui
    Rect box = new Rect (0, m_topHeight, m_leftWidth, Screen.height-m_topHeight);
    GUI.Box (box, "Build Menu");

    groupBox = new Rect (box.x+m_guiSkin.box.border.left, 
                         box.y+m_guiSkin.box.border.top, 
                         box.xMax-m_guiSkin.box.border.left - m_guiSkin.box.border.right,
                         box.yMax-m_guiSkin.box.border.top  - m_guiSkin.box.border.bottom);


    //GUI.color = new Color (GUI.color.r, GUI.color.g, GUI.color.b, 0.5f);
    
    foreach (StaticGuiObject sgo in m_staticGuiObjects)
    {
      GUI.BeginGroup (groupBox, "");

      float height = sgo.buildGui ();
      groupBox.Set (groupBox.x, groupBox.y + height, groupBox.xMax, groupBox.yMax + height);

      GUI.EndGroup ();
    }


    // Draw  selected items
    GUI.color = new Color (GUI.color.r, GUI.color.g, GUI.color.b, 0.85f);

    if (m_guiObject != null)
      m_guiObject.buildGui ();
  }

  // -------------------------------------------------------------------------------------------------------------------

}
