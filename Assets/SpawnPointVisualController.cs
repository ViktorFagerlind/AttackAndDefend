using UnityEngine;
using System.Collections;

public class SpawnPointVisualController : MonoBehaviour {

  public MeshRenderer m_meshrenderer;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

  public void setVisualLevel(int newLevel) {
    transform.localScale = new Vector3(40,40,25+newLevel*5);
  }

  public void setColor(Color newColor) {
    m_meshrenderer.materials[3].color = newColor;

  }
}
