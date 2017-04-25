using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster_Near : Monster {

	Vector3 NowForward;
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
			NowForward = this.transform.forward;
			if (Vector3.Distance (this.transform.position, AttTarget.transform.position) > 3) {
				this.transform.Translate (NowForward.normalized * Speed * (-1));
			}
			if (Vector3.Distance (this.transform.position, AttTarget.transform.position) <= 3 && !isAttack) 
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
	protected override void DoDie ()
	{
		base.DoDie ();
		Destroy (this.gameObject);
	}
	IEnumerator DoAtt()
	{
		//isAttack = true;
		m_Animator.Play ("AttacK");
		yield return new WaitForSeconds (3);
		print ("att");
		isAttack = false;
	}
}
