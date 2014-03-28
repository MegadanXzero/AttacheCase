using UnityEngine;
using System.Collections;

public class EnemyAnim : MonoBehaviour
{
	private EnemyAI m_AIScript;

	void Awake ()
	{
		m_AIScript = transform.parent.GetComponent<EnemyAI>();
	}

	public void Attack()
	{
		m_AIScript.Attack();
	}

	public void DropItem()
	{
		m_AIScript.DropItem();
	}

	public void Death()
	{
		m_AIScript.Death();
	}

	public void ResetDamage()
	{
		m_AIScript.ResetDamage();
	}

	public void DisableCollider()
	{
		transform.parent.GetComponent<BoxCollider2D>().enabled = false;
	}
}