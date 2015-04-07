using UnityEngine;
using System.Collections;

public class WorkerPurchaseHandler : LockstepSimulation {
	private float invincibleTime;
	private float freezeTime;
	public GameObject targeta; //will be replaced by navmesh later?
	
	public GameObject targetb; //will be replaced by navmesh later?

	int currtarget;

	// Use this for initialization
	public override void Start () {
		base.Start ();
		invincibleTime = 0;
		freezeTime = 0;
		currtarget = 1;
	}
	
	// Update is called once per frame
  public override void UpdateSimulation (float dt) {


		if (freezeTime > 0) {
			freezeTime = freezeTime-dt;
		} else {
			Player tmpPlayerController = GameObject.FindObjectOfType<Player>();

			tmpPlayerController.SetSpeed(1);

			//Very temporary movement code, will be replaced by viktors awesomeness later.
			GetComponent<Renderer>().material.color = Color.blue;
			//Allow the object to move!
			/*Plane plane;
			plane = new Plane(Vector3.up, Vector3.zero);
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			float fDist = 0.0f;
			plane.Raycast (ray, out fDist); 
			Vector3 pointedpoint = ray.GetPoint (fDist);
			transform.position = pointedpoint;*/
			if (currtarget == 1){
				Vector3 direction = Vector3.MoveTowards(transform.position,targeta.transform.position,1*dt);
				direction.y = 0.0f;
				transform.position = direction;
				if (Vector3.Distance (transform.position,targeta.transform.position)<0.5) {
					currtarget = 2;
					transform.LookAt (targetb.transform.position);
				}
			} else {
				Vector3 direction = Vector3.MoveTowards(transform.position,targetb.transform.position,1*dt);
				direction.y = 0.0f;
				transform.position = direction;
				if (Vector3.Distance (transform.position,targetb.transform.position)<0.5) {
					currtarget = 1;
					transform.LookAt (targeta.transform.position);
				}

			}

		}


		if (invincibleTime > 0) {
			invincibleTime = invincibleTime-dt;
		} else {
			GetComponent<Renderer>().material.color = Color.white;		
			GetComponent<Collider>().enabled = true;
		}
	}

	public void doTransaction(float transactionTime) {
		//print ("Jag försöker nu köpa någonting");
		invincibleTime = 2+transactionTime;
		GetComponent<Renderer>().material.color = Color.red;
		GetComponent<Collider>().enabled = false;
		freezeTime = transactionTime;
		Player tmpPlayerController = GameObject.FindObjectOfType<Player>();
		tmpPlayerController.SetSpeed(0);
	}
}
