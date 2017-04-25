using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour {
	public float Att;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnTriggerStay(Collider other) {
		Monster _Monster = other.GetComponent<Monster> ();
		if (_Monster != null)
		{
			//_Monster.AttTarget = Shooter;
			_Monster.Hit (Att);
			other.GetComponent<Monster_Near> ().CallFire ();
			//Destroy(this.gameObject);
		}
	}

}
