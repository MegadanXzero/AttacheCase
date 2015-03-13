using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AmmoDropManager : MonoBehaviour
{
	[Serializable]
	public class AmmoDropInfo
	{
		public int m_PrefabID;
		public int m_NativeDropChance;
		public int m_BonusDropChance;
	}
	
	[SerializeField] private AmmoDropInfo[] m_DropList;
	[SerializeField] private InventoryScript m_Inventory;

	private int m_TotalNativeDropChance = 0;
	private int[] m_ActualDropChance;

	void Awake()
	{
		//DontDestroyOnLoad(gameObject);

		if (m_Inventory == null)
		{
			m_Inventory = GameObject.FindGameObjectWithTag(Tags.MAININVENTORY).GetComponent<InventoryScript>();
		}

		foreach(AmmoDropInfo info in m_DropList)
		{
			m_TotalNativeDropChance += info.m_NativeDropChance;
		}

		m_ActualDropChance = new int[Enum.GetNames(typeof(WeaponType)).Length - 1];
	}

	public int GetAmmoType()
	{
		// Get all weapon types in the inventory and set the total range of drop chances
		HashSet<WeaponType> weaponTypes = m_Inventory.GetWeaponTypes();
		int totalRange = m_TotalNativeDropChance;

		// Set the actual drop chance of each ammo type to the native drop chance
		for (int i = 0; i < m_ActualDropChance.Length; i++)
		{
			m_ActualDropChance[i] = m_DropList[i].m_NativeDropChance;
		}

		// For each weapon type in the inventory add the bonus drop chance to the actual drop chance
		foreach(WeaponType type in weaponTypes)
		{
			totalRange += m_DropList[(int)type].m_BonusDropChance;
			m_ActualDropChance[(int)type] += m_DropList[(int)type].m_BonusDropChance;
		}

		int random = UnityEngine.Random.Range(0, totalRange);
		int dropChance = 0;
		for (int i = 0; i < m_ActualDropChance.Length; i++)
		{
			dropChance += m_ActualDropChance[i];
			if (random < dropChance)
			{
				return m_DropList[i].m_PrefabID;
			}
		}

		return 0;
	}
}