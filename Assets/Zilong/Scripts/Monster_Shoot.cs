using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster_Shoot : Monster {
	public GameObject bullet;
	public Transform ShootPos;
	public float BulletSpeed;
	// Use this for initialization
	protected override void Start () {
		base.Start ();
	}
	
	// Update is called once per frame
	protected override void Update ()
	{
		base.Update ();
		if (AttTarget != null) 
		{
			this.transform.LookAt (AttTarget.transform.position);
			if (!isAttack) 
			{
				isAttack = true;
				Attack ();
			}
		}
	}
	protected override void Attack ()
	{
		base.Attack ();
		StartCoroutine (DoAtt ());
	}
	IEnumerator DoAtt()
	{
		//isAttack = true;
		Shoot ();
		yield return new WaitForSeconds (1);
		isAttack = false;
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
}
