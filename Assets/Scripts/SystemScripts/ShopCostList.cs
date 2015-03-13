using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class ItemCostInfo
{
	[SerializeField] private int m_ItemCost;
	
	[SerializeField] private List<int> m_PenetrationCost;
	[SerializeField] private List<int> m_FiringSpeedCost;
	[SerializeField] private List<int> m_ReloadSpeedCost;
	[SerializeField] private List<int> m_CapacityCost;
	[SerializeField] private List<int> m_DamageCost;
	[SerializeField] private List<int> m_RadiusCost;
	
	public List<int> PenetrationCost { get {return m_PenetrationCost;}}
	public List<int> FiringSpeedCost { get {return m_FiringSpeedCost;}}
	public List<int> ReloadSpeedCost { get {return m_ReloadSpeedCost;}}
	public List<int> CapacityCost { get {return m_CapacityCost;}}
	public List<int> DamageCost { get {return m_DamageCost;}}
	public List<int> RadiusCost { get {return m_RadiusCost;}}
	public int ItemCost { get {return m_ItemCost;}}
}

public class ShopCostList : MonoBehaviour
{
	[SerializeField] private List<ItemCostInfo> m_ItemPriceList;

	public List<ItemCostInfo> ItemPriceList { get {return m_ItemPriceList;}}

	void Awake()
	{
		//DontDestroyOnLoad(gameObject);
	}
	
	public int GetCostFromPrefabID(int prefabID)
	{
		//prefabID--;
		prefabID -= 3;
		if (m_ItemPriceList.Count > prefabID)
		{
			return m_ItemPriceList[prefabID].ItemCost;
		}
		return 0;
	}
	
	public int GetCapacityUpgradeCost(int prefabID, int upgradeLevel)
	{
		prefabID -= 3;
		if (m_ItemPriceList.Count > prefabID)
		{
			if (m_ItemPriceList[prefabID].CapacityCost.Count > upgradeLevel)
			{
				return m_ItemPriceList[prefabID].CapacityCost[upgradeLevel];
			}
		}
		return 0;
	}
	
	public int GetDamageUpgradeCost(int prefabID, int upgradeLevel)
	{
		prefabID -= 3;
		if (m_ItemPriceList.Count > prefabID)
		{
			if (m_ItemPriceList[prefabID].DamageCost.Count > upgradeLevel)
			{
				return m_ItemPriceList[prefabID].DamageCost[upgradeLevel];
			}
		}
		return 0;
	}
	
	public int GetFiringSpeedUpgradeCost(int prefabID, int upgradeLevel)
	{
		prefabID -= 3;
		if (m_ItemPriceList.Count > prefabID)
		{
			if (m_ItemPriceList[prefabID].FiringSpeedCost.Count > upgradeLevel)
			{
				return m_ItemPriceList[prefabID].FiringSpeedCost[upgradeLevel];
			}
		}
		return 0;
	}
	
	public int GetReloadSpeedUpgradeCost(int prefabID, int upgradeLevel)
	{
		prefabID -= 3;
		if (m_ItemPriceList.Count > prefabID)
		{
			if (m_ItemPriceList[prefabID].ReloadSpeedCost.Count > upgradeLevel)
			{
				return m_ItemPriceList[prefabID].ReloadSpeedCost[upgradeLevel];
			}
		}
		return 0;
	}
	
	public int GetPenetrationUpgradeCost(int prefabID, int upgradeLevel)
	{
		prefabID -= 3;
		if (m_ItemPriceList.Count > prefabID)
		{
			if (m_ItemPriceList[prefabID].PenetrationCost.Count > upgradeLevel)
			{
				return m_ItemPriceList[prefabID].PenetrationCost[upgradeLevel];
			}
		}
		return 0;
	}
	
	public int GetRadiusUpgradeCost(int prefabID, int upgradeLevel)
	{
		prefabID -= 3;
		if (m_ItemPriceList.Count > prefabID)
		{
			if (m_ItemPriceList[prefabID].RadiusCost.Count > upgradeLevel)
			{
				return m_ItemPriceList[prefabID].RadiusCost[upgradeLevel];
			}
		}
		return 0;
	}
}