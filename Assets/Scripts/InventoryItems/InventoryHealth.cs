using UnityEngine;
using System.Collections;

public class InventoryHealth : MonoBehaviour
{
	public float tempHealAmount;
	
	private float m_HealAmount;
	private InventoryItem m_BaseItem;
	
	public float HealAmount { get {return m_HealAmount;}}
	public InventoryItem BaseItem { get {return m_BaseItem;}}
	
	void Awake()
	{
		m_BaseItem = GetComponent<InventoryItem>();
		m_HealAmount = tempHealAmount;
	}
	
	void Update ()
	{
		
	}
}