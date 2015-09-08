using UnityEngine;
using System.Collections;

public class ProjectileFlashGrenade : MonoBehaviour
{
	[SerializeField] private GrenadeType m_GrenadeType;
	private float m_ExplosionRadius;
	private float m_StunTime;
	
	private int m_RadiusUpgradeLevel = 0;
	private int m_DamageUpgradeLevel = 0;
	private float m_Timer = 3.0f;
	
	public GrenadeType GrenadeType { get {return m_GrenadeType;}}
	public int RadiusUpgradeLevel { set {m_RadiusUpgradeLevel = value;}}
	public int DamageUpgradeLevel { set {m_DamageUpgradeLevel = value;}}
	
	void Awake()
	{
		transform.GetComponent<Rigidbody2D>().velocity = new Vector3(24.0f, 6.4f, 0.0f);
		UpdateStats();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Time.timeScale > 0.0f)
		{
			if (m_Timer > 0.0f)
			{
				m_Timer -= Time.deltaTime;
			}
			else
			{
				Explode();
			}
		}
	}

	void OnCollisionEnter2D(Collision2D coll)
	{
		if (coll.gameObject.CompareTag(Tags.ENEMY))
		{
			Explode();
		}
	}
	
	void Explode()
	{
		Collider2D[] m_Colliders = Physics2D.OverlapCircleAll((Vector2)transform.position, m_ExplosionRadius);
		for (int i = 0; i < m_Colliders.Length; i++)
		{
			EnemyAI enemy = m_Colliders[i].GetComponent<EnemyAI>();
			if (enemy != null)
			{
				enemy.Stun(m_StunTime);
			}
		}
		
		GameObject.Destroy(gameObject);
	}

	public void UpdateStats()
	{
		WeaponUpgradeStats weaponStats = GameObject.FindGameObjectWithTag(Tags.WEAPONSTATS).GetComponent<WeaponUpgradeStats>();
		m_ExplosionRadius = weaponStats.GetRadius(WeaponName.FlashGrenade, m_RadiusUpgradeLevel);
		m_StunTime = weaponStats.GetDamage(WeaponName.FlashGrenade, m_DamageUpgradeLevel);
	}
}