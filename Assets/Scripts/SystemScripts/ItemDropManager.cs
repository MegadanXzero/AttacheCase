using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDropManager : MonoBehaviour
{
	private const int MAX_DUPLICATE_ITERATIONS = 20;
	private const int MAX_ITERATIONS = 40;

	[SerializeField] private int[] m_WeaponList;
	[SerializeField] private int[] m_GrenadeList;
	[SerializeField] private int[] m_HealthList;
	[SerializeField] private int[] m_TreasureList;
	[SerializeField] private InventoryScript m_Inventory;
	[SerializeField] private InventoryScript m_HoldingArea;

	[SerializeField] private int m_MinWeaponSize = 20;
	[SerializeField] private int m_MaxWeaponSize = 30;

	[SerializeField] private int m_MinAmmo = 8;
	[SerializeField] private int m_MaxAmmo = 12;

	[SerializeField] private int m_MinGrenades = 2;
	[SerializeField] private int m_MaxGrenades = 6;

	[SerializeField] private int m_MinHealth = 1;
	[SerializeField] private int m_MaxHealth = 4;

	[SerializeField] private int m_MinTreasureSize = 5;
	[SerializeField] private int m_MaxTreasureSize = 10;

	private List<WeaponType> m_SpawnedTypeList;
	private List<InventoryItem> m_SpawnedWeapons;
	private List<InventoryItem> m_SpawnedItems;

	private AmmoDropManager m_AmmoDropManager;

	private int m_TotalItemSize = 0;
	
	void Awake()
	{
		DontDestroyOnLoad(gameObject);

		if (m_Inventory == null)
		{
			m_Inventory = GameObject.FindGameObjectWithTag(Tags.MAININVENTORY).GetComponent<InventoryScript>();
		}

		if (m_AmmoDropManager == null)
		{
			m_AmmoDropManager = GameObject.FindGameObjectWithTag(Tags.AMMODROPPER).GetComponent<AmmoDropManager>();
		}

		m_SpawnedTypeList = new List<WeaponType>();
		m_SpawnedWeapons = new List<InventoryItem>();
		m_SpawnedItems = new List<InventoryItem>();
	}
	
	public void IncreaseMinTotalSize(int increase)
	{
		m_MinWeaponSize += increase;
	}

	public void IncreaseMaxTotalSize(int increase)
	{
		m_MaxWeaponSize += increase;
	}

	public void SpawnItems()
	{
		m_TotalItemSize = 0;

		SpawnWeapons();
		SpawnTreasure();
		SpawnHealth();
		SpawnGrenades();
		SpawnAmmo();

		DistributeItems();

		m_SpawnedWeapons.Clear();
		m_SpawnedItems.Clear();
	}

	private void SpawnWeapons()
	{
		int totalCurrentSize = 0;

		// Until we have enough weapons to fill the minimum number of cells
		int iterations = 0;
		while (totalCurrentSize < m_MinWeaponSize && iterations < MAX_ITERATIONS)
		{
			int random = m_WeaponList[Random.Range(0, m_WeaponList.GetLength(0))];

			// Get a random weapon ID and check that the weapon fits in the max number of cells
			// and the number of free inventory cells
			InventoryItem item = PrefabIDList.GetPrefabWithID(random).GetComponent<InventoryItem>();
			if (item != null)
			{
				int size = item.Width * item.Height;
				if (totalCurrentSize + size <= m_MaxWeaponSize)
				{
					if (size <= m_Inventory.NumFreeCells())
					{
						// If a weapon of this type has not already been spawned
						// (Allow duplicates after MAX_DUPLICATE_ITERATIONS to prevent taking too long)
						InventoryWeapon weaponComponent = item.GetComponent<InventoryWeapon>();
						if (weaponComponent != null)
						{
							if (!m_SpawnedTypeList.Contains(weaponComponent.WeaponType) || iterations > MAX_DUPLICATE_ITERATIONS)
							{
								// If this weapon fits in theory create the actual weapon
								Transform weapon = Instantiate(PrefabIDList.GetPrefabWithID(random)) as Transform;
								InventoryItem itemComponent = weapon.GetComponent<InventoryItem>();

								// Then try actually placing it into the inventory
								if (m_Inventory.FindAvailableSpace(itemComponent))
								{
									totalCurrentSize += size;
									m_SpawnedTypeList.Add(weaponComponent.WeaponType);
									m_SpawnedWeapons.Add(itemComponent);
								}
								else
								{
									Destroy(itemComponent.gameObject);
								}
							}
						}
					}
				}
			}
			++iterations;
		}

		m_SpawnedTypeList.Clear();
		m_TotalItemSize += totalCurrentSize;
	}

	private void SpawnAmmo()
	{
		int ammoToSpawn = Random.Range(m_MinAmmo, m_MaxAmmo + 1);
		int maxItems = (m_Inventory.Size - m_TotalItemSize) / 2;

		if (ammoToSpawn > maxItems)
		{
			ammoToSpawn = maxItems;
		}

		for (int i = 0; i < ammoToSpawn; ++i)
		{
			// Get the ammo type to create from the ammo drop manager
			int prefabID = m_AmmoDropManager.GetAmmoType();
			Transform item = Instantiate(PrefabIDList.GetPrefabWithID(prefabID)) as Transform;
			InventoryItem itemComponent = item.GetComponent<InventoryItem>();
			
			// Then try actually placing it into the inventory
			if (!m_Inventory.FindAvailableSpace(itemComponent))
			{
				if (!m_HoldingArea.FindAvailableSpace(itemComponent))
				{
					Destroy(itemComponent.gameObject);
				}
				else
				{
					m_SpawnedItems.Add(itemComponent);
				}
			}
			else
			{
				m_SpawnedItems.Add(itemComponent);
			}
		}
	}

	private void SpawnGrenades()
	{
		int grenadesToSpawn = Random.Range(m_MinGrenades, m_MaxGrenades + 1);
		
		for (int i = 0; i < grenadesToSpawn; ++i)
		{
			// Get a random grenade type to spawn
			int prefabID = m_GrenadeList[Random.Range(0, m_GrenadeList.GetLength(0))];
			Transform item = Instantiate(PrefabIDList.GetPrefabWithID(prefabID)) as Transform;
			InventoryItem itemComponent = item.GetComponent<InventoryItem>();
			
			// Then try actually placing it into the inventory
			if (!m_Inventory.FindAvailableSpace(itemComponent))
			{
				if (!m_HoldingArea.FindAvailableSpace(itemComponent))
				{
					Destroy(itemComponent.gameObject);
				}
				else
				{
					m_SpawnedItems.Add(itemComponent);
				}
			}
			else
			{
				m_SpawnedItems.Add(itemComponent);
			}
		}

		m_TotalItemSize += grenadesToSpawn * 2;
	}

	private void SpawnHealth()
	{
		int healthToSpawn = Random.Range(m_MinHealth, m_MaxHealth + 1);
		
		for (int i = 0; i < healthToSpawn; ++i)
		{
			// Get a random health type to spawn
			int prefabID = m_HealthList[Random.Range(0, m_HealthList.GetLength(0))];
			Transform item = Instantiate(PrefabIDList.GetPrefabWithID(prefabID)) as Transform;
			InventoryItem itemComponent = item.GetComponent<InventoryItem>();
			
			// Then try actually placing it into the inventory
			if (!m_Inventory.FindAvailableSpace(itemComponent))
			{
				if (!m_HoldingArea.FindAvailableSpace(itemComponent))
				{
					Destroy(itemComponent.gameObject);
				}
				else
				{
					m_SpawnedItems.Add(itemComponent);
				}
			}
			else
			{
				m_SpawnedItems.Add(itemComponent);
			}
		}

		m_TotalItemSize += healthToSpawn * 2;
	}

	private void SpawnTreasure()
	{
		int totalCurrentSize = 0;
		
		// Until we have enough treasure to fill the minimum number of cells
		int iterations = 0;
		while (totalCurrentSize < m_MinTreasureSize && iterations < MAX_ITERATIONS)
		{
			int random = m_TreasureList[Random.Range(0, m_TreasureList.GetLength(0))];
			
			// Get a random treasure ID and check that the weapon fits in the max number of cells
			// and the number of free inventory cells
			InventoryItem item = PrefabIDList.GetPrefabWithID(random).GetComponent<InventoryItem>();
			if (item != null)
			{
				int size = item.Width * item.Height;
				if (totalCurrentSize + size <= m_MaxTreasureSize)
				{
					//if (size <= m_Inventory.NumFreeCells())
					{
						// If this treasure fits in theory create the actual treasure
						Transform treasure = Instantiate(PrefabIDList.GetPrefabWithID(random)) as Transform;
						InventoryItem itemComponent = treasure.GetComponent<InventoryItem>();
						
						// Then try actually placing it into the inventory
						if (m_Inventory.FindAvailableSpace(itemComponent))
						{
							totalCurrentSize += size;
							m_SpawnedItems.Add(itemComponent);
						}
						else
						{
							if (m_HoldingArea.FindAvailableSpace(itemComponent))
							{
								totalCurrentSize += size;
								m_SpawnedItems.Add(itemComponent);
							}
							else
							{
								Destroy(itemComponent.gameObject);
							}
						}
					}
				}
			}
			++iterations;
		}

		m_TotalItemSize += totalCurrentSize;
	}

	private void DistributeItems()
	{
		m_Inventory.ClearItems(false);
		m_HoldingArea.ClearItems(false);

		// Get half of the weapons (Rounded up)
		int mainWeapons = (int)((((float)m_SpawnedWeapons.Count) * 0.5f) + 0.5f);
		for (int i = 0; i < m_SpawnedWeapons.Count; i++)
		{
			if (i < mainWeapons)
			{
				// Place each in a random spot in the inventory
				if (!m_Inventory.FindRandomSpace(m_SpawnedWeapons[i]))
				{
					// Or if it doesn't fit just dump into the holding area
					m_HoldingArea.FindAvailableSpace(m_SpawnedWeapons[i]);
				}
			}
			else
			{
				// Place the rest in the holding area
				m_HoldingArea.FindAvailableSpace(m_SpawnedWeapons[i]);
			}
		}

		ShuffleItems();

		// Get half of the other items
		int mainItems = m_SpawnedItems.Count / 2;
		for (int i = 0; i < m_SpawnedItems.Count; i++)
		{
			if (i < mainItems)
			{
				// Place each in a random spot in the inventory
				if(!m_Inventory.FindRandomSpace(m_SpawnedItems[i]))
				{
					// Or if it doesn't fit just dump into the holding area
					m_HoldingArea.FindAvailableSpace(m_SpawnedItems[i]);
				}
			}
			else
			{
				// Place the rest in the holding area
				m_HoldingArea.FindAvailableSpace(m_SpawnedItems[i]);
			}
		}
	}

	private void ShuffleItems()
	{
		for (int i = 0; i < m_SpawnedItems.Count; i++)
		{
			int rand = Random.Range(i, m_SpawnedItems.Count);
			InventoryItem item = m_SpawnedItems[rand];
			m_SpawnedItems[rand] = m_SpawnedItems[i];
			m_SpawnedItems[i] = item;
		}
	}
}