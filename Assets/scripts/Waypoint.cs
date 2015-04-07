using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Waypoint : MonoBehaviour
{
  public delegate void WaypointClickedDelegate (Vector3 position, bool add);
    
  WaypointClickedDelegate  m_onWaypointClickedDelegate;
  bool                     m_selected;

  // -------------------------------------------------------------------------------------------------------------------
  
  public WaypointClickedDelegate waypointClickedDelegate
  {
    set {m_onWaypointClickedDelegate = value;}
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  public bool selected
  {
    set 
    {
      m_selected = value; 
      Color color;

      if (m_selected)
        color = Color.white;
      else
        color = Color.grey;

      Projector p = GetComponentInChildren<Projector> ();
      p.material = new Material (p.material);
      p.material.color = color;
    }
    get {return m_selected;}
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  void OnMouseUp() 
  {
    selected = !selected;

    if (m_onWaypointClickedDelegate != null)
      m_onWaypointClickedDelegate (transform.position, selected);
  }

  // -------------------------------------------------------------------------------------------------------------------
  
}
