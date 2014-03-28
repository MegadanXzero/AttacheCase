using UnityEngine;
using System.Collections;

public class ItemDrop : MonoBehaviour
{
	public int m_ItemID;
	public int m_Amount;

	public bool PickupDrop()
	{
		// ItemID 0 is for Money, 1 is for Treasures, 2 is for Ammo, anything else is for other inventory items
		bool itemPickedUp = true;
		InventoryItem itemComponent = null;
		if (m_ItemID == 0)
		{
			GameObject.FindGameObjectWithTag(Tags.MAININVENTORY).GetComponent<InventoryScript>().AddMoney(m_Amount);
		}
		else if (m_ItemID == 1)
		{
			int prefabID = GameObject.FindGameObjectWithTag(Tags.TREASUREDROPPER).GetComponent<TreasureDropManager>().GetTreasureDrop();
			Transform item = Instantiate(PrefabIDList.GetPrefabWithID(prefabID)) as Transform;
			itemComponent= item.GetComponent<InventoryItem>();
		}
		else if (m_ItemID == 2)
		{
			int prefabID = GameObject.FindGameObjectWithTag(Tags.AMMODROPPER).GetComponent<AmmoDropManager>().GetAmmoType();
			Transform item = Instantiate(PrefabIDList.GetPrefabWithID(prefabID)) as Transform;
			itemComponent = item.GetComponent<InventoryItem>();
		}
		else
		{
			Transform item = Instantiate(PrefabIDList.GetPrefabWithID(m_ItemID)) as Transform;
			itemComponent = item.GetComponent<InventoryItem>();
		}

		if (itemComponent != null)
		{
			if (!GameObject.FindGameObjectWithTag(Tags.HOLDINGAREA).GetComponent<InventoryScript>().FindAvailableSpace(itemComponent))
			{
				Destroy(itemComponent.gameObject);
				itemPickedUp = false;
			}
		}

		return itemPickedUp;
	}
}