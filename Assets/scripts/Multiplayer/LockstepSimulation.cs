using UnityEngine;
using System.Collections;

public abstract class LockstepSimulation : MonoBehaviour
{
  
  // -------------------------------------------------------------------------------------------------------------------

  public virtual void Start ()
  {
    Lockstep.instance.AddObjectToSimulate (this);
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  public abstract void UpdateSimulation (float dt);

  // -------------------------------------------------------------------------------------------------------------------
  
  public virtual void OnDestroy ()
  {
    Lockstep.instance.RemoveObjectToSimulate (this);
  }
}
