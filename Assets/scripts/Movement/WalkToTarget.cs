using UnityEngine;
using System.Collections;
using Pathfinding;

public abstract class WalkToTarget : LockstepSimulation
{
  // -------------------------------------------------------------------------------------------------------------------

  public float    m_speed = 100;  // The AI's speed per second
  public float    m_changeWaypointDistance = 10;  // The max distance from the AI to a waypoint for it to continue to the next waypoint

  // -------------------------------------------------------------------------------------------------------------------
  
  private Path                m_path = null; // Currently calculated path
  private Seeker              m_seeker;
  private int                 m_currentWaypoint = 0;  // The waypoint we are currently moving towards
  private Vector3             m_positionOffset;


  // -------------------------------------------------------------------------------------------------------------------
  
  public Vector3 positionOffset
  {
    set {m_positionOffset = value;}
    get {return m_positionOffset;}
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  public virtual void Awake ()
  {
    m_positionOffset = new Vector3 ();

    m_seeker = GetComponent<Seeker> ();

    //Debug.Log ("Setting m_seeker: " + m_seeker);
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  protected void SetTarget (Vector3 target)
  {
    //Debug.Log ("Using m_seeker: " + m_seeker);

    m_seeker.StartPath (transform.position, target, OnPathComplete);
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  protected void Move (float dt)
  {
    if (m_path == null) 
      return;

    Vector3 currentWaypointPosition = m_path.vectorPath [m_currentWaypoint] + positionOffset;

    Vector3 dir = (currentWaypointPosition - transform.position).normalized; // Direction to the next waypoint
    Vector3 velocity = dir * m_speed;
    // m_controller.SimpleMove (velocity);
    transform.Translate (velocity * dt,Space.World);


   // transform.Translate (velocity);


//    Debug.Log ("Wp(" + currentWaypoint + "): " + currentWaypointPosition + ", dir: " + dir + ", pos: " + transform.position);

    // Proceed to follow the next waypoint if close enough
    if (Vector3.Distance (transform.position, currentWaypointPosition) < m_changeWaypointDistance) 
    {
      m_currentWaypoint++;

      if (m_currentWaypoint == m_path.vectorPath.Count) 
      {
        //Debug.Log ("End Of Path Reached");
        
        m_path = null;

        OnTargetReached ();
        return;
      } else {
        // look at the next waypoint
        transform.LookAt (m_path.vectorPath [m_currentWaypoint] + positionOffset);
      }
    }
  }

  // -------------------------------------------------------------------------------------------------------------------
  
  protected virtual void OnTargetReached ()
  {
  }


  // -------------------------------------------------------------------------------------------------------------------
  
  private void OnPathComplete (Path p)
  {
    if (p.error) 
    {
      //Debug.LogError ("Error!");
      return;
    }
    
    //Debug.Log ("Got a path!");
    
    m_path = p;
    m_currentWaypoint = 0; // Reset the waypoint counter
  }
  
  // -------------------------------------------------------------------------------------------------------------------
  
}