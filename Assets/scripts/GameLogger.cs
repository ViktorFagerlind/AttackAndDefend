using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
using log4net.Config;
#endif


public class GameLogger
{
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
  log4net.ILog    m_log4netLog;
#else
  Stack<string>   m_contextStack;
  private string  m_className;
#endif

  public static GameLogger GetLogger (System.Type t)
  {
    return new GameLogger (t);
  }

  private GameLogger (System.Type t)
  {
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
    m_log4netLog = log4net.LogManager.GetLogger (t);
#else
    m_contextStack  = new Stack<string> ();
    m_className     = t.ToString ();
#endif
  }

#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
  public void Debug (string message)
  {
    m_log4netLog.Debug (message);
  }
  public void Warning (string message)
  {
    m_log4netLog.Warn (message);
  }
  public void Error (string message)
  {
    m_log4netLog.Error (message);
  }
  public void PushContext (string context)
  {
    log4net.NDC.Push (context);
  }
  public void PopContext ()
  {
    log4net.NDC.Pop ();
  }

#else

  private string getContext ()
  {
    string context = "";

    foreach (string ctx in m_contextStack)
      context += ctx;

    return context;
  }

  public void Debug (string message)
  {

    UnityEngine.Debug.Log ("DEBUG   " + m_className + "   [" + getContext () + "] - " + message);
  }

  public void Warning (string message)
  {
    UnityEngine.Debug.Log ("WARNING " + m_className + "   [" + getContext () + "] - " + message);
  }

  public void Error (string message)
  {
    UnityEngine.Debug.Log ("ERROR   " + m_className + "   [" + getContext () + "] - " + message);
  }
  public void PushContext (string context)
  {
    m_contextStack.Push (context);
  }
  public void PopContext ()
  {
    m_contextStack.Pop ();
  }

#endif
}
