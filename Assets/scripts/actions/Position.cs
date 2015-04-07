using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Position
{
  public float x; 
  public float y; 
  public float z;

  public Position ()
  {
    x = y = z = 0;
  }

  public Position (Vector3 p)
  {
    Set (p);
  }

  void Set (Vector3 p)
  {
    x = p.x;
    y = p.y;
    z = p.z;
  }
}
