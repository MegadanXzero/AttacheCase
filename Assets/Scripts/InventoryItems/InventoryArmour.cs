using UnityEngine;
using System.Collections;

public class InventoryArmour : MonoBehaviour
{
	[SerializeField] private float m_ArmourModifier;

	private InventoryItem m_BaseItem;

	public float ArmourModifier { get {return m_ArmourModifier;}}
	public InventoryItem BaseItem { get {return m_BaseItem;}}

	// Use this for initialization
	void Awake ()
	{
		m_BaseItem = GetComponent<InventoryItem>();
	}
}