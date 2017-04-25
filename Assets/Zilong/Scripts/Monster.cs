using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Monster : MonoBehaviour { 
	public float HP;
	public float Att;
	public float Speed;
	public Image ImageHp;
	public Image ImageHp_BG;
	public GameObject target;
	public GameObject AttTarget;
	protected Collider m_Collider;
	protected Rigidbody m_Rigidbody;
	protected Animator m_Animator;
	public bool isDie = false;

	protected bool isAttack=false;
	protected float MaxHP;
	protected virtual void Start () {
		m_Rigidbody = GetComponent<Rigidbody> ();
		m_Animator = GetComponent<Animator> ();
		m_Collider = GetComponent<Collider> ();
		MaxHP = HP;
	}

	protected virtual void Attack(){}

	protected virtual void Update(){
		ImageHp.fillAmount = (float)(HP / MaxHP);
		ImageHp.transform.LookAt (target.transform.position);
		ImageHp_BG.transform.LookAt (target.transform.position);
		if (HP <= 0 && !isDie) 
		{
			DoDie ();
		}
	}
	public void Hit(float damage)
	{
		HP -= damage;
	}
	protected virtual void DoDie(){
		isDie = true;
	}
}
