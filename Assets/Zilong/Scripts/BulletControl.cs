using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletControl : MonoBehaviour {
	public float Att;
	public GameObject Shooter;
	// Use this for initialization
	void Start () {
		Destroy (this.gameObject, 5);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnTriggerEnter(Collider other) {
		Monster _Monster = other.GetComponent<Monster> ();
		if (_Monster != null)
		{
			_Monster.AttTarget = Shooter;
			_Monster.Hit (Att);
			Destroy(this.gameObject);
		}



	}
}
