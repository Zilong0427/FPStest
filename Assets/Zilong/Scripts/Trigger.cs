using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour {
	public Monster m_Monster;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnTriggerEnter(Collider other) 
	{
		if (other.GetComponent<FpsControl> ())
			m_Monster.AttTarget = other.gameObject;
	}
}
