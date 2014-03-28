using UnityEngine;
using System.Collections;

public class InventoryTreasure : MonoBehaviour
{
	public enum TreasureMaterial
	{
		Bronze = 0,
		Silver,
		Gold,
	}

	[SerializeField] private TreasureMaterial m_Material;
	[SerializeField] private int m_ScoreValue;

	private InventoryItem m_BaseItem;

	public TreasureMaterial MaterialType { get {return m_Material;}}
	public InventoryItem BaseItem { get {return m_BaseItem;}}
	public int ScoreValue { get {return m_ScoreValue;}}

	// Use this for initialization
	void Awake ()
	{
		m_BaseItem = GetComponent<InventoryItem>();
		//transform.FindChild("ItemText").guiText.enabled = false;
	}
}