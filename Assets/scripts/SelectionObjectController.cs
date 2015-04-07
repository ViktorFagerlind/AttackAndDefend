using UnityEngine;
using System.Collections;

public class SelectionObjectController : MonoBehaviour 
{
  Renderer  m_renderer;
  float     m_scale;

  public float scale 
  {
    get {return m_scale;}
    set 
    {
      m_scale = value;
      transform.localScale = new Vector3 (m_scale, m_scale, m_scale);
    }
  }

  void Start ()
  {
    m_renderer          = GetComponentInChildren <Renderer> ();
    m_renderer.enabled  = false;
    m_scale             = transform.localScale.x;
  }
   
  void Update ()
  {
  }

  public void select ()
  {
    m_renderer.enabled = true;
  }

  public void unselect ()
  {
    m_renderer.enabled = false;
  }
  
}
