using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GrenadeType
{
	Frag = 0,
	Flash,
	Incendiary,
};

public class InventoryGrenade : InventoryWeapon
{	
	public const int NUM_GRENADE_TYPES = 3;
	const int FRAG_PREFAB_ID = 5;
	const int FLASH_PREFAB_ID = 15;
	const int INCENDIARY_PREFAB_ID = 16;
	
	[SerializeField] private GrenadeType m_GrenadeType;
	private float m_ExplosionRadius = 3.0f;
	
	public GrenadeType GrenadeType { get {return m_GrenadeType;}}
	
	void Awake()
	{
		m_BaseItem = GetComponent<InventoryItem>();

		Transform itemText = transform.FindChild("ItemText");
		if (itemText != null)
		{
			itemText.GetComponent<GUIText>().enabled = false;
		}
		m_WeaponStats = GameObject.FindGameObjectWithTag(Tags.WEAPONSTATS).GetComponent<WeaponUpgradeStats>();
		
		// Get other weapon values based on upgrade level
		m_Damage = m_WeaponStats.GetDamage(m_WeaponName, m_DamageUpgradeLevel);
		m_FiringSpeed = m_WeaponStats.GetFiringSpeed(m_WeaponName, m_FiringSpeedUpgradeLevel);
		m_ReloadSpeed = m_WeaponStats.GetReloadSpeed(m_WeaponName, m_ReloadSpeedUpgradeLevel);
		m_Radius = m_WeaponStats.GetRadius(m_WeaponName, m_RadiusUpgradeLevel);
	}

	void Start()
	{
		/*if (m_BaseItem == null)
		{
			if (m_GrenadeType == GrenadeType.Frag)
			{
				m_BaseItem = PrefabIDList.GetPrefabWithID(FRAG_PREFAB_ID).GetComponent<InventoryItem>();
			}
			else if (m_GrenadeType == GrenadeType.Flash)
			{
				m_BaseItem = PrefabIDList.GetPrefabWithID(FLASH_PREFAB_ID).GetComponent<InventoryItem>();
			}
			else if (m_GrenadeType == GrenadeType.Incendiary)
			{
				m_BaseItem = PrefabIDList.GetPrefabWithID(INCENDIARY_PREFAB_ID).GetComponent<InventoryItem>();
			}
		}*/
	}
	
	public override float GetDamage(float distance)
	{
		// Check if the weapon has hit at this distance
		// Weapons have full accuracy in ideal range, half in effective range
		float damage = 0.0f;
		if (distance < 1.0f)
		{
			damage = m_Damage;
		}
		else if (distance < m_ExplosionRadius)
		{
			float damageScale = 1.0f - ((distance - 1.0f) / (m_ExplosionRadius - 1.0f));
			damageScale = Mathf.Max(0.0f, damageScale);
			damage = damageScale * m_Damage;
		}
		
		return damage;
	}
}