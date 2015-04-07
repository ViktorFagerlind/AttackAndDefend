using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BazarController : MonoBehaviour, GuiObject
{
  // -------------------------------------------------------------------------------------------------------------------

  [Serializable] class BuildMerchantCommand : Command
  {
    public BuildMerchantCommand (string objectNameToCommand) : base (objectNameToCommand){}
    protected override void Perform (System.Object objectToCommand) {((BazarController)objectToCommand).incrNumberOfMerchants(1);}
  }

  [Serializable] class IncreaseItemCostCommand : Command
  {
    public IncreaseItemCostCommand (string objectNameToCommand) : base (objectNameToCommand){}
    protected override void Perform (System.Object objectToCommand) {((BazarController)objectToCommand).incrCostOfItems(1);}
  }
  
  [Serializable] class IncreaseRadiusCommand : Command
  {
    public IncreaseRadiusCommand (string objectNameToCommand) : base (objectNameToCommand){}
    protected override void Perform (System.Object objectToCommand) {((BazarController)objectToCommand).incrRadius(15);}
  }
  
  // -------------------------------------------------------------------------------------------------------------------

  /* Upgradable bazaar properties*/
  public float            m_bazarRadius;
  public int              m_numberOfMerchants;
  public float            m_costOfItems;
  public TextMesh         m_debugText;
  public GameObject       m_merchantPrefab;
  public List<GameObject> m_merchantList;

  Texture2D               m_levelUpMerchantTexture;
  Texture2D               m_increaseSellpriceTexture;
  Texture2D               m_increaseRadiusTexture;

  SelectionObjectController m_selectionObjectController;

  // Use this for initialization
  void Start()
  {
    m_merchantList = new List<GameObject>();
    
    m_selectionObjectController = GetComponentInChildren<SelectionObjectController> ();

    setRadius(m_bazarRadius);
    setNumberOfMerchants(3);
    setCostOfItems(3);

    m_levelUpMerchantTexture    = Resources.Load("icons/LevelUp")            as Texture2D;
    m_increaseSellpriceTexture  = Resources.Load("icons/IncreaseSellprice")  as Texture2D;
    m_increaseRadiusTexture     = Resources.Load("icons/IncreaseRadius")     as Texture2D;

    // Register as commandable objects
    CommandableDictionary.instance.register (name, this);
  }
  
  // Update is called once per frame
  void Update()
  {
    //setRadius (bazarRadius+0.1f);
  }

  void setNumberOfMerchants(int newNumberOfMerchants)
  {
    m_numberOfMerchants = newNumberOfMerchants;
    createMerchants();
    updateDebugText();
  }

  public void incrNumberOfMerchants(int incrNumberOfMerchants)
  {
    setNumberOfMerchants(m_numberOfMerchants + incrNumberOfMerchants);
  }

  void setCostOfItems(float newCostOfItems)
  {
    m_costOfItems = newCostOfItems;
    updateDebugText();
  }

  public void incrCostOfItems(float incrCostOfItems)
  {
    setCostOfItems(m_costOfItems + incrCostOfItems);
  }

  void setRadius(float newRadius)
  {
    m_bazarRadius = newRadius;
    m_selectionObjectController.scale = m_bazarRadius * 13f/60f;

    CapsuleCollider tmpCollider = GetComponentInChildren<CapsuleCollider> ();
    tmpCollider.radius = m_bazarRadius;
  }

  public void incrRadius(float incrRadius)
  {
    setRadius (m_bazarRadius + incrRadius);
  }

  void updateDebugText()
  {
    m_debugText.text = "M: " + m_numberOfMerchants.ToString() + " C: " + m_costOfItems.ToString();
  }

  void createMerchants()
  {
    for (int i=m_merchantList.Count; i<m_numberOfMerchants; i++)
    {
      GameObject obj = (GameObject)Instantiate(m_merchantPrefab);
      obj.transform.parent = transform;
      obj.name = "Merchant " + i.ToString();
      //obj.transform.localScale -= new Vector3(0.25f,0.25f,0);
        
      //obj.transform.Translate(p,Space.World);
      Vector3 startLocation = new Vector3(transform.position.x + i / 2.0f, transform.position.y, transform.position.z - 30f);
      //z: -1,5
      obj.transform.Translate(startLocation, Space.World);

      m_merchantList.Add(obj);
      MerchantSellHandler m = obj.GetComponent<MerchantSellHandler>();
      m.SetBazaar(this);
    }
  }

  public void setColor(Color newColor)
  {
    Component[] renderers = GetComponentsInChildren<Renderer>();
    foreach (Renderer rendu in renderers)
    { //TODO: gör det här effektivare DOY
      if (rendu.name == "tent_exterior")
      {
        rendu.material.color = newColor;
      }
    } 
  }
  
  void OnMouseUp()
  {
    GuiManager.instance.ObjectSelected(this);
  }

  public void markSelected()
  {
    m_selectionObjectController.select ();
  }
  
  public bool unmarkSelected()
  {
    m_selectionObjectController.unselect ();

    return true;
  }
  
  public void buildGui()
  {
    int nofItems = 3;

    if (GuiManager.instance.popupButton(transform.position, 0, nofItems, m_levelUpMerchantTexture))
      new BuildMerchantCommand    (name);
      
    if (GuiManager.instance.popupButton(transform.position, 1, nofItems, m_increaseSellpriceTexture))
      new IncreaseItemCostCommand (name);

    if (GuiManager.instance.popupButton(transform.position, 2, nofItems, m_increaseRadiusTexture))
      new IncreaseRadiusCommand   (name);
  }
}
