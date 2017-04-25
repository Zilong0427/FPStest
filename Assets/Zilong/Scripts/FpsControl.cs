using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FpsControl : MonoBehaviour {
	public Transform ShootPos;
	public GameObject bullet;
	public float BulletSpeed;
	public float Hp;
	public Image img_Hp;
	public float Att;
	public GameObject Fire;

	float MaxHP;
	// Use this for initialization
	void Start () 
	{
		MaxHP = Hp;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetMouseButtonDown(0)) {
			Shoot ();
		}
		if (Input.GetKeyDown (KeyCode.F)) {
			Fire.SetActive (true);
		}
		if (Input.GetKeyUp (KeyCode.F)) {
			Fire.SetActive (false);
		}
		img_Hp.fillAmount = (float)(Hp / MaxHP);
	}
	void Shoot()
	{
		//print (ShootPos.forward);
		Vector3 v = ShootPos.forward.normalized;
		GameObject tempBullet = (GameObject)Instantiate (bullet, ShootPos.position, Quaternion.identity);
		tempBullet.GetComponent<Rigidbody> ().velocity = v * BulletSpeed;
		tempBullet.GetComponent<BulletControl> ().Att = Att;
		tempBullet.GetComponent<BulletControl> ().Shooter = this.gameObject;
	}
	public void Hit(float dam){
		Hp -= dam;
	}
}
