using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.Xml;
using System.Xml.Serialization;

[Serializable]
public abstract class Command
{
  private static readonly GameLogger logger = GameLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
  
  // -------------------------------------------------------------------------------------------------------------------

  private string m_objectNameToCommand;
  public  string objectNameToCommand { get {return m_objectNameToCommand;}}

  public Command (string objectNameToCommand)
  {
    m_objectNameToCommand = objectNameToCommand;

    Lockstep.instance.EnqueueCommand (this);
  }

  public void Execute ()
  {
    System.Object objectToCommand;
    
    // Some commands does not operate on an object...
    if (objectNameToCommand == "")
      objectToCommand = null;
    else
    {
      objectToCommand = CommandableDictionary.instance.get (objectNameToCommand);
      
      if (objectToCommand == null)
      {
        logger.Error ("Could not find commandable object " + objectNameToCommand);
        return;
      }
    }
    
    Perform (objectToCommand);
  }
  
  protected abstract void Perform (System.Object objectToCommand);

  public string Serialize ()
  {
    MemoryStream stream = new MemoryStream ();
    IFormatter formatter = new BinaryFormatter ();
    formatter.Serialize (stream, this);
    return Convert.ToBase64String (stream.ToArray ());
  }
  
  static public Command Deserialize (string serialization)
  {
    MemoryStream stream = new MemoryStream (Convert.FromBase64String (serialization));
    IFormatter formatter = new BinaryFormatter ();
    return (Command) formatter.Deserialize (stream);
  }


  public static string XmlSerialize<T>(T obj)
  {
    StringBuilder xml        = new StringBuilder ();
    XmlSerializer serializer = new XmlSerializer (typeof(T));

    using (TextWriter textWriter = new StringWriter(xml))
    {
      serializer.Serialize(textWriter, obj);
    }

    return xml.ToString();
  }

  public static T XmlDeserialize<T>(string xml)
  {
    T obj = default(T);
    XmlSerializer serializer = new XmlSerializer(typeof(T));
    
    using (TextReader textReader = new StringReader(xml))
    {
      obj = (T)serializer.Deserialize(textReader);
    }
    
    return obj;
  }

}
