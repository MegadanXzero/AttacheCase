using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class WeaponInfo
{
	// Weapon Stats
	[SerializeField] private WeaponName m_Name;
	[SerializeField] private WeaponType m_Type;
	[SerializeField] private List<int> m_Capacity;
	[SerializeField] private List<int> m_Penetration;
	[SerializeField] private List<float> m_FiringSpeed;
	[SerializeField] private List<float> m_ReloadSpeed;
	[SerializeField] private List<float> m_Damage;
	[SerializeField] private List<float> m_Radius;
	
	public int GetCapacity(int upgradeLevel)
	{
		if (m_Capacity.Count > upgradeLevel)
		{
			return m_Capacity[upgradeLevel];
		}
		return 0;
	}
	
	public int GetPenetration(int upgradeLevel)
	{
		if (m_Penetration.Count > upgradeLevel)
		{
			return m_Penetration[upgradeLevel];
		}
		return 0;
	}
	
	public float GetFiringSpeed(int upgradeLevel)
	{
		if (m_FiringSpeed.Count > upgradeLevel)
		{
			return m_FiringSpeed[upgradeLevel];
		}
		return 0.0f;
	}
	
	public float GetReloadSpeed(int upgradeLevel)
	{
		if (m_ReloadSpeed.Count > upgradeLevel)
		{
			return m_ReloadSpeed[upgradeLevel];
		}
		return 0.0f;
	}
	
	public float GetDamage(int upgradeLevel)
	{
		if (m_Damage.Count > upgradeLevel)
		{
			return m_Damage[upgradeLevel];
		}
		return 0.0f;
	}
	
	public float GetRadius(int upgradeLevel)
	{
		if (m_Radius.Count > upgradeLevel)
		{
			return m_Radius[upgradeLevel];
		}
		return 0.0f;
	}
	
	public WeaponName Name { get {return m_Name;}}
	public WeaponType Type { get {return m_Type;}}
	
	public int CapacityLevels { get {return m_Capacity.Count;}}
	public int DamageLevels { get {return m_Damage.Count;}}
	public int FiringSpeedLevels { get {return m_FiringSpeed.Count;}}
	public int ReloadSpeedLevels { get {return m_ReloadSpeed.Count;}}
	public int PenetrationLevels { get {return m_Penetration.Count;}}
	public int RadiusLevels { get {return m_Radius.Count;}}
}

public class WeaponUpgradeStats : MonoBehaviour
{	
	// Weapon Stats
	[SerializeField] private List<WeaponInfo> m_WeaponInfoList;

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
	
	public int GetCapacity(WeaponName weapon, int upgradeLevel)
	{
		foreach(WeaponInfo weaponInfo in m_WeaponInfoList)
		{
			if (weaponInfo.Name == weapon)
			{
				return weaponInfo.GetCapacity(upgradeLevel);
			}
		}
		return 0;
	}
	
	public int GetPenetration(WeaponName weapon, int upgradeLevel)
	{
		foreach(WeaponInfo weaponInfo in m_WeaponInfoList)
		{
			if (weaponInfo.Name == weapon)
			{
				return weaponInfo.GetPenetration(upgradeLevel);
			}
		}
		return 0;
	}
	
	public float GetDamage(WeaponName weapon, int upgradeLevel)
	{
		foreach(WeaponInfo weaponInfo in m_WeaponInfoList)
		{
			if (weaponInfo.Name == weapon)
			{
				return weaponInfo.GetDamage(upgradeLevel);
			}
		}
		return 0.0f;
	}
	
	public float GetRadius(WeaponName weapon, int upgradeLevel)
	{
		foreach(WeaponInfo weaponInfo in m_WeaponInfoList)
		{
			if (weaponInfo.Name == weapon)
			{
				return weaponInfo.GetRadius(upgradeLevel);
			}
		}
		return 0.0f;
	}
	
	public float GetFiringSpeed(WeaponName weapon, int upgradeLevel)
	{
		foreach(WeaponInfo weaponInfo in m_WeaponInfoList)
		{
			if (weaponInfo.Name == weapon)
			{
				return weaponInfo.GetFiringSpeed(upgradeLevel);
			}
		}
		return 0.0f;
	}
	
	public float GetReloadSpeed(WeaponName weapon, int upgradeLevel)
	{		
		foreach(WeaponInfo weaponInfo in m_WeaponInfoList)
		{
			if (weaponInfo.Name == weapon)
			{
				return weaponInfo.GetReloadSpeed(upgradeLevel);
			}
		}
		return 0.0f;
	}
	
	public int NumCapacityUpgrades(WeaponName weapon)
	{
		foreach(WeaponInfo weaponInfo in m_WeaponInfoList)
		{
			if (weaponInfo.Name == weapon)
			{
				return weaponInfo.CapacityLevels;
			}
		}
		return 0;
	}
	
	public int NumDamageUpgrades(WeaponName weapon)
	{
		foreach(WeaponInfo weaponInfo in m_WeaponInfoList)
		{
			if (weaponInfo.Name == weapon)
			{
				return weaponInfo.DamageLevels;
			}
		}
		return 0;
	}
	
	public int NumFiringSpeedUpgrades(WeaponName weapon)
	{
		foreach(WeaponInfo weaponInfo in m_WeaponInfoList)
		{
			if (weaponInfo.Name == weapon)
			{
				return weaponInfo.FiringSpeedLevels;
			}
		}
		return 0;
	}
	
	public int NumReloadSpeedUpgrades(WeaponName weapon)
	{
		foreach(WeaponInfo weaponInfo in m_WeaponInfoList)
		{
			if (weaponInfo.Name == weapon)
			{
				return weaponInfo.ReloadSpeedLevels;
			}
		}
		return 0;
	}
	
	public int NumPenetrationUpgrades(WeaponName weapon)
	{
		foreach(WeaponInfo weaponInfo in m_WeaponInfoList)
		{
			if (weaponInfo.Name == weapon)
			{
				return weaponInfo.PenetrationLevels;
			}
		}
		return 0;
	}
	
	public int NumRadiusUpgrades(WeaponName weapon)
	{
		foreach(WeaponInfo weaponInfo in m_WeaponInfoList)
		{
			if (weaponInfo.Name == weapon)
			{
				return weaponInfo.RadiusLevels;
			}
		}
		return 0;
	}
}