using UnityEngine;
using System.Collections;

public class MerchantSellHandler : LockstepSimulation {
	enum State
	{
		Idle,
		Chasing,
		Haggling,
		Home
	};

	State state;
	Collider target;
	BazarController bazaar;
	Vector3 goal;
	float standStill;

	// Use this for initialization
	public override void Start () {
		base.Start ();
		GoHome ();
	}
	void GoHome() {
		target = null;
		state = State.Home;
		goal = bazaar.transform.position;
		standStill = 0;
	}
	void RandomGoal() {
		Vector3 bc = bazaar.transform.position;
		float br = bazaar.m_bazarRadius;
		Vector2 v = Random.insideUnitCircle * br;
		goal = bc + new Vector3(v.x, 0, v.y);
		state = State.Idle;
		standStill = 0;
	}
	void Stand() {
		state = State.Idle;
		standStill = 1.0f;
	}

	// Update is called once per frame
  public override void UpdateSimulation (float dt) {
		if (state == State.Chasing) {
			goal = target.transform.position;
		} else if (standStill > 0) {
			standStill -= dt;
			if (standStill <= 0) {
				if (state == State.Haggling) {
					WorkerAI tmp = target.GetComponent<WorkerAI>();
					tmp.finishHaggling();
					GoHome ();
				} else {
					RandomGoal();
				}
			}
		} else {
			float tmpDistance = Vector3.Distance(transform.position,goal);
			if (tmpDistance < 0.1) {
				Stand ();
			}
		}
		goal.y = 0;

		// Walk towards goal
		float speed = state == State.Chasing ? 110f : 40f;
		Vector3 direction = Vector3.MoveTowards(transform.position,goal,speed*dt);
		//if (state == MerchState.Chasing) Debug.Log ("Chasing from" + transform.position.ToString() + " to " + direction.ToString());
		direction.y = 0.0f;
		transform.LookAt (new Vector3(goal.x, 0, goal.z));
		transform.position = direction;

	}

	void OnTriggerEnter(Collider other) {
		if (! other.CompareTag("Worker"))
			return;
		if (other != target)
			return;
		
		// Debug.Log ("Merchant caught WORKER");

		WorkerAI tmp = other.GetComponent<WorkerAI>();
		if (tmp.startHaggling (bazaar.gameObject)) {
			state = State.Haggling;
			standStill = 2.0f;
		} else {
			GoHome ();
		}
	}

	public void ChaseTarget(Collider worker) {
		target = worker;
		state = State.Chasing;
		//Debug.Log ("Merchant chasing WORKER");
	}
	public void ForgetTarget(Collider worker) {
		if (worker == target) {
			RandomGoal();
			//Debug.Log ("Merchant stopped chasing WORKER");
		}	
	}
	public void SetBazaar (BazarController b) {
		bazaar = b;
	}
	public bool isBusy() {
		return state != State.Idle;
	}
}
  
