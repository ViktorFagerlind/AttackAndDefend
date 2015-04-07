using UnityEngine;
using System.Collections;
using System;

public class GeneralCommands : MonoBehaviour 
{
  // -------------------------------------------------------------------------------------------------------------------
  
  [Serializable] public class EmptyCommand : Command
  {
    public EmptyCommand () 
      : base ("") 
    {
    }
    protected override void Perform (System.Object objectToCommand)
    {
      // Do nothing
    }
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  [Serializable] public class InstantiatePrefabCommand : Command
  {
    private string    m_prefabPath;
    private Position  m_pos;
    static int        m_intantiationCounter = 0;

    public InstantiatePrefabCommand (string objectNameToCommand, string prefabPath, Vector3 position) 
      : base (objectNameToCommand) 
    {
      m_prefabPath  = prefabPath;
      m_pos = new Position (position);
    }
    protected override void Perform (System.Object objectToCommand)
    {
      GameObject go = (GameObject)Instantiate(Resources.Load (m_prefabPath), new Vector3(m_pos.x, m_pos.y, m_pos.z), Quaternion.identity);
      go.name = go.name.Replace ("(Clone)", " " + m_intantiationCounter);;

      m_intantiationCounter++;
    }
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  private void Start ()
  {
    // Register as commandable objects
    CommandableDictionary.instance.register (GetType ().FullName, this);
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  private void OnDestroy ()
  {
    // Remove from commandable objects
    CommandableDictionary.instance.deRegister (name);
  }
}
