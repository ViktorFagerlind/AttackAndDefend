using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class WalkToManyTargets : WalkToTarget
{

  // -------------------------------------------------------------------------------------------------------------------
  
  private List<Vector3>  m_targets;
  private int            m_currentTarget;

  // -------------------------------------------------------------------------------------------------------------------
  
  public override void Awake ()
  {
    base.Awake ();
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  protected new void SetTarget (Vector3 target)
  {
    List<Vector3> targets = new List<Vector3> ();
    targets.Add (target);

    SetTargets (targets);
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  protected void SetTargets (List<Vector3> targets)
  {
    m_targets = targets;
    m_currentTarget = 0;

    base.SetTarget (m_targets [m_currentTarget]);
  }

  // -------------------------------------------------------------------------------------------------------------------

  protected override void OnTargetReached ()
  {
    m_currentTarget++;

    if (m_currentTarget < m_targets.Count)
      base.SetTarget (m_targets [m_currentTarget]);
    else
      OnAllTargetsReached ();
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  protected virtual void OnAllTargetsReached ()
  {
  }

  // -------------------------------------------------------------------------------------------------------------------
}