using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InventoryAmmo : MonoBehaviour
{
	[SerializeField] private List<int> m_Amounts;
	[SerializeField] private int m_MaxCapacity;
	[SerializeField] private WeaponType m_WeaponType;
	[SerializeField] private int m_Amount = 0;
	
	private InventoryItem m_BaseItem;
	
	public int Amount { get {return m_Amount;} set {m_Amount = value; transform.FindChild("ItemText").guiText.text = m_Amount.ToString();}}
	public int MaxCapacity { get {return m_MaxCapacity;}}
	public WeaponType WeaponType { get {return m_WeaponType;}}
	public InventoryItem BaseItem { get {return m_BaseItem;}}
	
	void Awake()
	{
		/*if (m_Amount == 0)
		{
			if (m_Amounts.Count > 0)
			{
				int chance = Random.Range(0, 100);

				if (chance >= 95)
				{
					m_Amount = m_Amounts[2];
				}
				else if (chance >= 70)
				{
					m_Amount = m_Amounts[1];
				}
				else
				{
					m_Amount = m_Amounts[0];
				}
			}
		}*/

		m_Amount = m_MaxCapacity;
		m_BaseItem = GetComponent<InventoryItem>();
		transform.FindChild("ItemText").guiText.text = m_Amount.ToString();
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
	
	public int TakeAmmo(int ammoNeeded)
	{
		int ammoTaken = 0;
		
		if (ammoNeeded <= m_Amount)
		{
			ammoTaken = ammoNeeded;
			m_Amount -= ammoNeeded;
		}
		else
		{
			ammoTaken = m_Amount;
			m_Amount = 0;
		}
		
		transform.FindChild("ItemText").guiText.text = m_Amount.ToString();
		return ammoTaken;
	}
	
	public void AddAmmo(int amount)
	{
		m_Amount += amount;
		transform.FindChild("ItemText").guiText.text = m_Amount.ToString();
	}
}