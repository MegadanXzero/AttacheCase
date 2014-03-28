using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyAI : MonoBehaviour
{	
	enum AnimState
	{
		Walking = 0,
		Running,
		Attacking,
		Flinch,
		Stumble,
		Stunned,
	}

	[SerializeField] private float m_BaseHealth;
	[SerializeField] private float m_Damage;
	[SerializeField] private float m_Speed;
	[SerializeField] private float m_AttackSpeed;
	[SerializeField] private Transform m_DropPrefab;
	[SerializeField] private int m_MoneyDropAmount;
	
	[SerializeField] private List<int> m_ItemDropIDs = new List<int>();
	[SerializeField] private List<int> m_ItemDropChances = new List<int>();
	[SerializeField] private Dictionary<int, int> m_DropChances = new Dictionary<int, int>();
	[SerializeField] private float m_AttackDistance;

	private Animator m_Animator;
	private bool m_PlayerInRange = false;
	//private float m_AttackTimer;
	private CharacterScript m_Character;

	private float m_Health;
	private float m_StunTimer = 0.0f;
	private bool m_FullyStunned = false;
	private bool m_Stunned = false;

	private float m_BurnTimer = 0.0f;
	private bool m_OnFire = false;

	public float Health { get {return m_Health;}}
	
	void Awake()
	{
		m_Animator = GetComponentInChildren<Animator>();
		particleSystem.enableEmission = false;
		//m_AttackTimer = m_AttackSpeed;
		
		int j = 0;
		foreach (int i in m_ItemDropIDs)
		{
			if (m_ItemDropChances.Count > j)
			{
				m_DropChances.Add(i, m_ItemDropChances[j]);
			}
			else
			{
				m_DropChances.Add(i, 0);
			}
			j++;
		}
		
		m_ItemDropIDs.Clear();
		m_ItemDropChances.Clear();

		// In future there should be something that scales up health over time
		m_Health = m_BaseHealth;
		m_Animator.SetFloat("Health", m_Health);
	}
	
	void Update ()
	{
		if (Time.timeScale > 0.0f)
		{
			if (m_OnFire)
			{
				if (m_BurnTimer > 0.0f)
				{
					m_BurnTimer -= Time.deltaTime;
				}
				else
				{
					m_OnFire = false;
					particleSystem.enableEmission = false;
				}
			}

			if (m_FullyStunned)
			{
				if (m_StunTimer > 0.0f)
				{
					m_StunTimer -= Time.deltaTime;
				}
				else
				{
					m_FullyStunned = false;
					m_Animator.SetBool("Stunned", false);
				}
			}
			else if (m_Health > 0.0f && !m_PlayerInRange && !m_Stunned)
			{
				Vector3 pos = transform.position;
				pos.x -= m_Speed * Time.deltaTime;
				transform.position = pos;
			}
		}
	}

	void FixedUpdate()
	{
		if (m_OnFire)
		{
			m_Health -= 1.0f;
			m_Animator.SetFloat("Health", m_Health);
		}

		if (!m_PlayerInRange)
		{
			// Check if the player is within the attack distance
			Vector2 rayPos = (Vector2)transform.position;
			rayPos.y += 0.5f;
			RaycastHit2D[] hitArray = Physics2D.RaycastAll(rayPos, new Vector2(-1.0f, 0.0f), m_AttackDistance).
				OrderBy(h => (h.fraction * m_AttackDistance)).ToArray();

			foreach(RaycastHit2D hitInfo in hitArray)
			{
				if (hitInfo.collider.tag == Tags.PLAYER)
				{
					m_PlayerInRange = true;
					m_Character = hitInfo.collider.GetComponent<CharacterScript>();
					m_Animator.SetBool("NearPlayer", true);
				}
			}
		}
	}
	
	public void Attack()
	{
		if (m_Character != null)
		{
			m_Character.AlterHealth(-m_Damage);
		}
	}
	
	/*void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == Tags.PLAYER)
		{
			m_PlayerInRange = true;
			m_Character = other.GetComponent<CharacterScript>();
			m_Animator.SetBool("NearPlayer", true);
		}
	}
	
	void OnTriggerExit2D(Collider2D other)
	{
		if (other.tag == Tags.PLAYER)
		{
			m_PlayerInRange = false;
			//m_AttackTimer = m_AttackSpeed;
			m_Character = null;
			m_Animator.SetBool("NearPlayer", false);
		}
	}*/

	public void Stun(float stunTime)
	{
		m_StunTimer = stunTime;
		m_FullyStunned = true;
		m_Animator.SetBool("Stunned", true);
	}

	public void Ignite(float burnTime)
	{
		m_BurnTimer = burnTime;
		m_OnFire = true;
		particleSystem.enableEmission = true;
		//m_Animator.SetFloat("Damage", 1.0f);
		m_Animator.SetTrigger("Flinch");
		m_Stunned = true;
	}
	
	public void DealDamage(float amount)
	{
		m_Health -= amount;
		m_Animator.SetFloat("Health", m_Health);
		//m_Animator.SetFloat("Damage", amount);
		m_Stunned = true;

		if (amount >= m_BaseHealth * 0.5f)
		{
			m_Animator.SetTrigger("Stumble");
		}
		else
		{
			m_Animator.SetTrigger("Flinch");
		}

		if (m_FullyStunned)
		{
			m_StunTimer = 0.0f;
			m_FullyStunned = false;
			m_Animator.SetBool("Stunned", false);
		}
	}

	public void DropItem()
	{
		// Generate a random number and check against drop chances to find the item type to drop
		int itemID = -1;
		int random = Random.Range(0, 100);
		foreach(KeyValuePair<int, int> entry in m_DropChances)
		{
			if (itemID == -1 && random < entry.Value)
			{
				itemID = entry.Key;
			}
		}
		
		if (itemID >= 0)
		{
			// Create an item drop and set the itemID to the correct ID
			Transform itemDrop = Instantiate(m_DropPrefab, transform.position + new Vector3(0.0f, 1.0f, -2.0f), Quaternion.identity) as Transform;
			itemDrop.rigidbody2D.velocity = new Vector2(Random.Range(-4.8f, 4.8f), 8.0f);
			ItemDrop itemComponent = itemDrop.GetComponent<ItemDrop>();
			itemComponent.m_ItemID = itemID;
			itemComponent.m_Amount = m_MoneyDropAmount;
		}
	}

	public void Death()
	{
		GameObject.FindGameObjectWithTag(Tags.TREASUREDROPPER).GetComponent<TreasureDropManager>().Increase();

		// Destroy the enemy object
		Destroy(gameObject);
	}

	public void ResetDamage()
	{
		//m_Animator.SetFloat("Damage", 0.0f);
		m_Stunned = false;
	}

	public void SetScaling(float healthScale, float damageScale)
	{
		m_BaseHealth *= healthScale;
		m_Damage *= damageScale;

		m_Health = m_BaseHealth;
		m_Animator.SetFloat("Health", m_Health);
	}
}