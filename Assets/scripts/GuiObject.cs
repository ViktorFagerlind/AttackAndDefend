using UnityEngine;
using System.Collections;

public interface GuiObject
{
  void buildGui ();

  void markSelected ();

  bool unmarkSelected ();
}
