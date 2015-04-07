using UnityEngine;
using System.Collections;
using System.IO;
using System;

#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
using log4net.Config;
#endif

public class Startup : MonoBehaviour
{
  private void Awake ()
  {
    // Forces a different code path in the BinaryFormatter that doesn't rely on run-time code generation (which would break on iOS).
    Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");

#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
    // Initialise log4net
    object obj = Resources.Load ("Settings/logConfig");
    if(obj != null) 
    {
      TextAsset configText = obj as TextAsset;
      if(configText != null) 
      {
        MemoryStream memStream = new MemoryStream (configText.bytes);
        XmlConfigurator.Configure(memStream); 
      }
    }
#endif
  }
}
