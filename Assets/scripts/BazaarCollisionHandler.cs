using UnityEngine;
using System.Collections;

public class BazaarCollisionHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	void OnTriggerEnter(Collider other) {
		MerchantSellHandler m;
		if (!other.CompareTag ("Worker"))
			return;
		//Debug.Log ("Trigger enter WORKER");

		BazarController tmp = transform.parent.GetComponent<BazarController>();
		GameObject closestMerchant = null;
		float currDistance = float.MaxValue;
		foreach (GameObject iMerchant in tmp.m_merchantList) { 
			float tmpDistance = Vector3.Distance(iMerchant.transform.position,other.transform.position);
			m = iMerchant.GetComponent<MerchantSellHandler>();
			if (m.isBusy()) continue;
			if (tmpDistance < currDistance) {
				currDistance = tmpDistance;
				closestMerchant = iMerchant;
			}
			
		}
		if (closestMerchant == null)
			return;
		m = closestMerchant.GetComponent<MerchantSellHandler>();
		m.ChaseTarget(other);
	}

	void OnTriggerExit(Collider other) {
		if (!other.CompareTag("Worker"))
			return;
		//Debug.Log ("Trigger exit WORKER");
		
		BazarController tmp = transform.parent.GetComponent<BazarController>();
		foreach (GameObject iMerchant in tmp.m_merchantList) { 
			MerchantSellHandler m = iMerchant.GetComponent<MerchantSellHandler>();
			m.ForgetTarget(other);
		}
	}


}
