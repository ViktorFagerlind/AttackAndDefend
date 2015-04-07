using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class CommandableDictionary : MonoBehaviour
{
  // -------------------------------------------------------------------------------------------------------------------
  
  Dictionary<string,Object> m_dictionary;

  // -------------------------------------------------------------------------------------------------------------------
  
  // Singleton
  private static CommandableDictionary m_instance;
  public  static CommandableDictionary instance {get {return m_instance;}}

  // -------------------------------------------------------------------------------------------------------------------
  
  void Awake()
  {
    m_instance = this;

    m_dictionary = new Dictionary<string, Object> ();
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  public void register (string str, Object commandable)
  {
    m_dictionary.Add (str, commandable);
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
  public void deRegister (string str)
  {
    m_dictionary.Remove (str);
  }
    
  // -------------------------------------------------------------------------------------------------------------------
  
  public Object get (string str)
  {
    if (!m_dictionary.ContainsKey(str))
    {
      Debug.LogError ("Commandable '" + str + "' not registered in CommandableDictionary");
      return null;
    }

    return m_dictionary[str];
  }
  
}
