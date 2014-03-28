using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum ShopState
{
	MainMenu = 0,
	Buy,
	Upgrade,
	Sell,
	Placing,
	QuickSell,
};

public class ShopMenu : MonoBehaviour
{
	[SerializeField] private ShopCostList m_CostListScript;
	[SerializeField] private InventoryScript m_Inventory;
	[SerializeField] private InventoryScript m_HoldingArea;
	[SerializeField] private WeaponUpgradeStats m_WeaponStats;
	[SerializeField] private MousePicker m_MousePicker;
	
	[SerializeField] private List<Texture2D> m_ItemPreviews;
	[SerializeField] private Texture2D m_CasePreview;
	[SerializeField] private Texture2D m_MainBackgroundTexture;
	[SerializeField] private Texture2D m_FullBackgroundTexture;
	[SerializeField] private Texture2D m_WeaponUpgradeEmpty;
	[SerializeField] private Texture2D m_WeaponUpgradeFull;
	[SerializeField] private List<int> m_BuyableItemList;
	
	private Vector2 m_ScrollPosition = Vector2.zero;
	private ShopState m_ShopState = ShopState.MainMenu;
	private bool m_DisplayShop = false;
	
	private HashSet<InventoryItem> m_SellableItemList;
	private HashSet<InventoryWeapon> m_UpgradableItemList;
	private SortedDictionary<WeaponType, int> m_AmmoToSell;
	private SortedDictionary<int, int> m_StackedItemsToSell;
	private InventoryWeapon m_SelectedWeapon = null;
	private InventoryItem m_SelectedSellItem = null;
	private InventoryAmmo m_SelectedAmmo = null;
	
	private int m_SelectedItem = 0;
	private int m_Amount = 1;
	private int m_SellItemCost = 0;
	private string m_DescriptionString = "";
	private int m_ItemCost = 0;
	private bool m_StackedItemSelected = false;

	private List<Transform> m_BuyingItemList;
	private bool m_AllItemsPlaced = false;
	private bool m_OldItemsInHolding = false;

	private bool m_ButtonPressed = false;
	private bool m_AllowPress = true;
	private float m_ButtonTimer = 0.8f;
	
	public void ToggleShop() {m_DisplayShop = !m_DisplayShop;}// m_MousePicker.Enabled = !m_DisplayShop;}

	public bool ShopShowing { get {return m_DisplayShop;}}
	
	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		m_BuyingItemList = new List<Transform>();
		//m_CostListScript = GameObject.FindGameObjectWithTag(Tags.SHOPCOSTLIST).GetComponent<ShopCostList>();
	}
	
	void OnGUI()
	{
		#if UNITY_ANDROID
		GUI.skin.verticalScrollbar.fixedWidth = 30.0f;
		GUI.skin.verticalScrollbarThumb.fixedWidth = 30.0f;
		#endif

		if (m_DisplayShop)
		{
			// Draw the main shop menu
			switch (m_ShopState)
			{
			case ShopState.MainMenu:
				DrawMainMenu();
				break;
				
			case ShopState.Buy:
				DrawShopMenu();
				break;
				
			case ShopState.Sell:
				DrawShopMenu();
				break;
				
			case ShopState.Upgrade:
				DrawShopMenu();
				break;
				
			case ShopState.Placing:
				DrawPlacingGUI();
				break;

			case ShopState.QuickSell:
				DrawQuickSellGUI();
				break;
			}
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		/*if (Input.GetMouseButtonDown(2))
		{
			if (m_DisplayShop)
			{
				m_MousePicker.Enabled = true;
				m_DisplayShop = false;
				m_Inventory.DrawLines = true;
				m_HoldingArea.DrawLines = true;
				
				Time.timeScale = 1.0f;
			}
			else
			{
				m_MousePicker.Enabled = false;
				m_DisplayShop = true;
				m_Inventory.DrawLines = false;
				m_HoldingArea.DrawLines = false;

				Time.timeScale = 0.0f;
			}
		}*/
	}

	void FixedUpdate()
	{
		if (m_ShopState == ShopState.Placing)
		{
			if (m_HoldingArea.GetNumberOfItems() == 0 && m_MousePicker.IsCarrying == false)
			{
				m_Inventory.TakeMoney(m_ItemCost);
				m_ShopState = ShopState.Buy;
				m_MousePicker.Enabled = false;

				m_Inventory.DrawLines = false;
				m_Inventory.AllowPartialAmmoCombining = true;
				m_HoldingArea.DrawLines = false;
				GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>().ShowEffects = false;


				// Clear the list of items being bought
				m_BuyingItemList.Clear();
			}
			else
			{
				// Check to see if all items being bought have been placed in the inventory
				m_AllItemsPlaced = true;
				List<InventoryItem> boughtItemList = new List<InventoryItem>();
				foreach(Transform item in m_BuyingItemList)
				{
					InventoryItem itemComponent = item.GetComponent<InventoryItem>();
					boughtItemList.Add(itemComponent);
					if (itemComponent != null)
					{
						if (itemComponent.FirstSpace != null)
						{
							if (itemComponent.FirstSpace.inventory != m_Inventory)
							{
								m_AllItemsPlaced = false;
							}
						}
					}
				}

				// Check if any non-bought items are in the holding area
				HashSet<InventoryItem> itemList = m_HoldingArea.FindAllItemsWithComponent<InventoryItem>();
				m_OldItemsInHolding = false;
				foreach(InventoryItem item in itemList)
				{
					bool boughtItem = false;
					foreach(InventoryItem itemComp in boughtItemList)
					{
						if (item == itemComp)
						{
							boughtItem = true;
							break;
						}
					}

					if (!boughtItem)
					{
						m_OldItemsInHolding = true;
						break;
					}
				}
			}
		}

		if (m_ButtonPressed)
		{
			m_ButtonTimer -= Time.fixedDeltaTime;

			if (m_ButtonTimer <= 0.0f)
			{
				m_AllowPress = true;
			}
			m_ButtonPressed = false;
		}
		else
		{
			m_AllowPress = true;
			m_ButtonTimer = 0.5f;
		}
	}
	
	private void DrawMainMenu()
	{
		/*GUI.DrawTexture(new Rect((Screen.width / 2) - (m_MainBackgroundTexture.width / 2),
								 (Screen.height / 2) - (m_MainBackgroundTexture.height / 2),
								  m_MainBackgroundTexture.width, m_MainBackgroundTexture.height), 
								  m_MainBackgroundTexture);
		
		// Button for each menu option, which simply changes the menu state
		if (GUI.Button(new Rect((Screen.width / 2) - (m_MainBackgroundTexture.width / 2) + 20,
								(Screen.height / 2) - (m_MainBackgroundTexture.height / 2) + 20, 470, 100), "BUY"))
		{
			m_ShopState = ShopState.Buy;
		}
		
		if (GUI.Button(new Rect((Screen.width / 2) - (m_MainBackgroundTexture.width / 2) + 20,
								(Screen.height / 2) - (m_MainBackgroundTexture.height / 2) + 143, 470, 100), "SELL"))
		{
			PrepareSellMenu();
		}
		
		if (GUI.Button(new Rect((Screen.width / 2) - (m_MainBackgroundTexture.width / 2) + 20,
								(Screen.height / 2) - (m_MainBackgroundTexture.height / 2) + 267, 470, 100), "UPGRADE"))
		{
			PrepareUpgradeMenu();
		}
		
		if (GUI.Button(new Rect((Screen.width / 2) - (m_MainBackgroundTexture.width / 2) + 20,
								(Screen.height / 2) - (m_MainBackgroundTexture.height / 2) + 390, 470, 100), "BACK"))
		{
			ToggleShop();
			//m_MousePicker.Enabled = true;
			//m_DisplayShop = false;
			//Time.timeScale = 1.0f;
		}*/

		int top = Screen.height / 4;
		GUI.DrawTexture(new Rect(0, top , Screen.width, Screen.height), m_FullBackgroundTexture);
		
		int screenCentreX = Screen.width / 2;
		int screenCentreY = top + ((top * 3) / 2);
		int screenQuarterX = Screen.width / 4;

		if (GUI.Button(new Rect(screenQuarterX - 100, screenCentreY - 50, 200, 100), "BUY"))
		{
			m_ShopState = ShopState.Buy;
		}

		if (GUI.Button(new Rect(screenCentreX - 100, screenCentreY - 50, 200, 100), "SELL"))
		{
			PrepareSellMenu();
		}
		
		if (GUI.Button(new Rect((screenQuarterX * 3) - 100, screenCentreY - 50, 200, 100), "UPGRADE"))
		{
			PrepareUpgradeMenu();
		}

		if (GUI.Button(new Rect(screenCentreX - 100, screenCentreY - 200, 200, 100), "QUICK SELL"))
		{
			m_ShopState = ShopState.QuickSell;
			m_MousePicker.Enabled = true;			
			m_Inventory.DrawLines = true;
			m_HoldingArea.DrawLines = true;
			m_HoldingArea.UpdateTotalSellPrice = true;
			GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>().ShowEffects = true;
		}

		if (GUI.Button(new Rect(screenCentreX - 100, screenCentreY + 100, 200, 100), "BACK"))
		{
			ToggleShop();
		}
	}

	private void PrepareSellMenu()
	{
		m_SellableItemList = m_Inventory.FindAllItemsWithComponent<InventoryItem>();
		m_AmmoToSell = new SortedDictionary<WeaponType, int>();
		m_StackedItemsToSell = new SortedDictionary<int, int>();
		List<InventoryItem> removeList = new List<InventoryItem>();
		
		// Check for ammo in the sell list
		foreach (InventoryItem item in m_SellableItemList)
		{
			InventoryAmmo ammo = item.GetComponent<InventoryAmmo>();
			if (ammo != null)
			{
				removeList.Add(item);
				if (m_AmmoToSell.ContainsKey(ammo.WeaponType))
				{
					m_AmmoToSell[ammo.WeaponType] += ammo.Amount;
				}
				else
				{
					m_AmmoToSell.Add(ammo.WeaponType, ammo.Amount);
				}
			}
		}

		// Remove any ammo from the itemList
		foreach (InventoryItem item in removeList)
		{
			m_SellableItemList.Remove(item);
		}
		removeList.Clear();

		// Check for stackable items in the sell list
		foreach (InventoryItem item in m_SellableItemList)
		{
			InventoryWeapon weapon = item.GetComponent<InventoryWeapon>();
			if (weapon != null)
			{
				if (weapon.GetType() != typeof(InventoryWeapon))
				{
					weapon = null;
				}
			}

			if (weapon == null)
			{
				removeList.Add(item);
				if (m_StackedItemsToSell.ContainsKey(item.PrefabID))
				{
					m_StackedItemsToSell[item.PrefabID] += 1;
				}
				else
				{
					m_StackedItemsToSell.Add(item.PrefabID, 1);
				}
			}
		}

		// Remove any stackable items from the item list (Only weapons should remain)
		foreach (InventoryItem item in removeList)
		{
			m_SellableItemList.Remove(item);
		}

		// Only transition to (Or stay in) the sell menu if there are things to sell
		if (m_SellableItemList.Count + m_AmmoToSell.Count > 0)
		{
			m_ShopState = ShopState.Sell;
		}
		else
		{
			m_ShopState = ShopState.MainMenu;
		}
		
		m_Amount = 1;
	}
	
	private void PrepareUpgradeMenu()
	{
		// Only transition to (Or stay in) the upgrade menu if there are things to upgrade
		m_UpgradableItemList = m_Inventory.FindAllItemsWithComponent<InventoryWeapon>(false);
		if (m_UpgradableItemList.Count > 0)
		{
			m_ShopState = ShopState.Upgrade;
		}
		else
		{
			m_ShopState = ShopState.MainMenu;
		}
	}
	
	private void DrawShopMenu()
	{
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), m_FullBackgroundTexture);

		#if UNITY_ANDROID
		int listItemWidth = (Screen.width / 2) - 65;
		#else
		int listItemWidth = (Screen.width / 2) - 50;
		#endif

		if (m_ShopState == ShopState.Buy)
		{
			// Everything between here is in the scrollable list vvvvvvvvvv
			m_ScrollPosition = GUI.BeginScrollView(new Rect((Screen.width / 2) + 10, 20, (Screen.width / 2) - 30, ((Screen.height / 3) * 2) - 40),
			                                       					m_ScrollPosition, new Rect(0, 0, 0, (m_Inventory.IsUpgradeable ? 
			                                       					(m_BuyableItemList.Count * 55) + 55 : m_BuyableItemList.Count * 55) - 5));

			if (m_Inventory.IsUpgradeable)
			{
				if (GUI.Button(new Rect(0, 0, listItemWidth, 50), "Attache Case Upgrade " + m_Inventory.UpgradeSizeString + " - £" + m_Inventory.UpgradeCost.ToString()))
				{
					m_DescriptionString = "Upgrade your Attache Case to a larger size to allow you to carry more items!";
					m_SelectedItem = 1;
					m_Amount = 1;
					m_SelectedWeapon = null;
					m_SelectedAmmo = null;
				}
			}
			
			// Populate Buy menu with buyable items
			for (int i = 0; i < m_BuyableItemList.Count; i++)
			{
				InventoryItem item = PrefabIDList.GetPrefabWithID(m_BuyableItemList[i]).GetComponent<InventoryItem>();
				if (item != null)
				{
					// Get cost (If an ammo pack, multiply by amount of ammo)
					int itemCost = m_CostListScript.GetCostFromPrefabID(m_BuyableItemList[i]);
					InventoryAmmo ammo = item.GetComponent<InventoryAmmo>();
					/*if (ammo != null)
					{
						itemCost *= ammo.Amount;
					}*/
					
					if (GUI.Button(new Rect(0, m_Inventory.IsUpgradeable ? (i * 55) + 55 : i * 55, listItemWidth, 50), item.ItemName + " - £" + itemCost.ToString()))
					{
						m_DescriptionString = item.ItemDescription.Replace("\\n", "\n");
						m_SelectedItem = m_BuyableItemList[i];
						m_Amount = 1;
						
						InventoryWeapon weapon = item.GetComponent<InventoryWeapon>();
						if (weapon != null)// && weapon.GetType() == typeof(InventoryWeapon))
						{
							m_SelectedWeapon = weapon;
						}
						else
						{
							m_SelectedWeapon = null;
						}

						if (ammo != null)
						{
							m_SelectedAmmo = ammo;
						}
						else
						{
							m_SelectedAmmo = null;
						}
					}
				}
			}
		}
		else if (m_ShopState == ShopState.Sell)
		{
			// Everything between here is in the scrollable list vvvvvvvvvv
			m_ScrollPosition = GUI.BeginScrollView(new Rect((Screen.width / 2) + 10, 20, (Screen.width / 2) - 30, ((Screen.height / 3) * 2) - 40),
													m_ScrollPosition, new Rect(0, 0, 0, ((m_SellableItemList.Count + 
			                                      	m_AmmoToSell.Count + m_StackedItemsToSell.Count) * 55) - 5));
			
			// Populate Sell menu with sellable items
			int i = 0;
			foreach (InventoryItem item in m_SellableItemList)
			{
				// Get selling price of item (Sell cost is half of buying cost)
				// For weapons, increase the sell price by half of the cost of each upgrade level purchased
				InventoryWeapon weapon = item.GetComponent<InventoryWeapon>();
				int itemCost = m_CostListScript.GetCostFromPrefabID(item.PrefabID) / 2;
				if (weapon != null)
				{
					// Damage //
					int upgradeLevel = weapon.DamageLevel;
					for (int level = 0; level < upgradeLevel; level++)
					{
						itemCost += m_CostListScript.GetDamageUpgradeCost(item.PrefabID, level) / 2;
					}
					
					// Capacity //
					upgradeLevel = weapon.CapacityLevel;
					for (int level = 0; level < upgradeLevel; level++)
					{
						itemCost += m_CostListScript.GetCapacityUpgradeCost(item.PrefabID, level) / 2;
					}
					
					// Firing Speed //
					upgradeLevel = weapon.FiringSpeedLevel;
					for (int level = 0; level < upgradeLevel; level++)
					{
						itemCost += m_CostListScript.GetFiringSpeedUpgradeCost(item.PrefabID, level) / 2;
					}
					
					// Reload Speed //
					upgradeLevel = weapon.ReloadSpeedLevel;
					for (int level = 0; level < upgradeLevel; level++)
					{
						itemCost += m_CostListScript.GetReloadSpeedUpgradeCost(item.PrefabID, level) / 2;
					}
					
					// Penetration //
					upgradeLevel = weapon.PenetrationLevel;
					for (int level = 0; level < upgradeLevel; level++)
					{
						itemCost += m_CostListScript.GetPenetrationUpgradeCost(item.PrefabID, level) / 2;
					}
					
					// Radius //
					upgradeLevel = weapon.RadiusLevel;
					for (int level = 0; level < upgradeLevel; level++)
					{
						itemCost += m_CostListScript.GetRadiusUpgradeCost(item.PrefabID, level) / 2;
					}
				}
				
				if (GUI.Button(new Rect(0, i * 55, listItemWidth, 50), item.ItemName + " - £" + itemCost.ToString()))
				{
					m_DescriptionString = item.ItemDescription;
					m_SelectedSellItem = item;
					m_SelectedItem = item.PrefabID;
					m_SellItemCost = itemCost;
					m_SelectedAmmo = null;
					m_StackedItemSelected = false;
					
					if (weapon != null)// && weapon.GetType() == typeof(InventoryWeapon))
					{
						m_SelectedWeapon = weapon;
					}
					else
					{
						m_SelectedWeapon = null;
					}
				}
				i++;
			}

			foreach (KeyValuePair<int, int> pair in m_StackedItemsToSell)
			{
				HashSet<InventoryItem> itemList = m_Inventory.FindItemsWithPrefabID(pair.Key);
				HashSet<InventoryItem>.Enumerator enumerator = itemList.GetEnumerator();
				enumerator.MoveNext();
				InventoryItem item = enumerator.Current;
				
				int itemCost = m_CostListScript.GetCostFromPrefabID(item.PrefabID) / 2;
				if (GUI.Button(new Rect(0, i * 55, listItemWidth, 50), item.ItemName + " x" + pair.Value + " - £" + itemCost.ToString()))
				{
					m_DescriptionString = item.ItemDescription;
					m_SelectedSellItem = item;
					m_SelectedItem = item.PrefabID;
					m_SelectedAmmo = null;
					m_SelectedWeapon = null;
					m_StackedItemSelected = true;
					m_Amount = 1;
				}
				i++;
			}
			
			foreach (KeyValuePair<WeaponType, int> pair in m_AmmoToSell)
			{
				HashSet<InventoryAmmo> ammoList = m_Inventory.FindAmmo(pair.Key);
				HashSet<InventoryAmmo>.Enumerator enumerator = ammoList.GetEnumerator();
				enumerator.MoveNext();
				InventoryItem item = enumerator.Current.BaseItem;
				
				int itemCost = m_CostListScript.GetCostFromPrefabID(item.PrefabID) / 2;
				if (GUI.Button(new Rect(0, i * 55, listItemWidth, 50), item.ItemName + " x" + pair.Value + " - £" + itemCost.ToString()))
				{
					m_DescriptionString = item.ItemDescription;
					m_SelectedSellItem = item;
					m_SelectedItem = item.PrefabID;
					m_SelectedAmmo = enumerator.Current;
					m_Amount = 1;
					m_SelectedWeapon = null;
					m_StackedItemSelected = false;
				}
				i++;
			}
		}
		else if (m_ShopState == ShopState.Upgrade)
		{
			// Everything between here is in the scrollable list vvvvvvvvvv
			m_ScrollPosition = GUI.BeginScrollView(new Rect((Screen.width / 2) + 10, 20, (Screen.width / 2) - 30, ((Screen.height / 3) * 2) - 40),
													m_ScrollPosition, new Rect(0, 0, 0, (m_UpgradableItemList.Count + 3 * 55) - 5));
			
			// Populate Upgrade menu with upgradable items
			int i = 0;
			foreach (InventoryWeapon weapon in m_UpgradableItemList)
			{
				if (GUI.Button(new Rect(0, i * 55, listItemWidth, 50), weapon.BaseItem.ItemName))
				{
					m_DescriptionString = weapon.BaseItem.ItemDescription;
					m_SelectedItem = weapon.BaseItem.PrefabID;
					m_SelectedWeapon = weapon;
				}
				i++;
			}

			if (GUI.Button(new Rect(0, i * 55, listItemWidth, 50), "Frag Grenade"))
			{
				InventoryGrenade frag = GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<CharacterScript>().FragUpgrader;
				m_DescriptionString = frag.BaseItem.ItemDescription;
				m_DescriptionString += "\nGrenade upgrades will apply to all grenades of this type once bought!";
				m_SelectedItem = frag.BaseItem.PrefabID;
				m_SelectedWeapon = frag;
			}
			i++;

			if (GUI.Button(new Rect(0, i * 55, listItemWidth, 50), "Flash Grenade"))
			{
				InventoryGrenade flash = GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<CharacterScript>().FlashUpgrader;
				m_DescriptionString = flash.BaseItem.ItemDescription;
				m_DescriptionString += "\nGrenade upgrades will apply to all grenades of this type once bought!";
				m_SelectedItem = flash.BaseItem.PrefabID;
				m_SelectedWeapon = flash;
			}
			i++;

			if (GUI.Button(new Rect(0, i * 55, listItemWidth, 50), "Incendiary Grenade"))
			{
				InventoryGrenade incendiary = GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<CharacterScript>().IncendiaryUpgrader;
				m_DescriptionString = incendiary.BaseItem.ItemDescription;
				m_DescriptionString += "\nGrenade upgrades will apply to all grenades of this type once bought!";
				m_SelectedItem = incendiary.BaseItem.PrefabID;
				m_SelectedWeapon = incendiary;
			}
		}
		
		GUI.EndScrollView(true);
		// Everything between here is in the scrollable list ^^^^^^^^^^
		
		// Description box, gets description based on item selected
		GUI.TextArea(new Rect((Screen.width / 2) + 10, (Screen.height / 3) * 2, (Screen.width / 2) - 30, (Screen.height / 3) - 90), m_DescriptionString);
		
		// Item preview box
		GUI.TextArea(new Rect(20, 20, (Screen.width / 2) - 30, (Screen.height / 2) - 20), "");
		if (m_SelectedItem != 0)
		{
			// Special case for the case upgrade preview, otherwise use relevant image from preview list 
			if (m_SelectedItem == 1)
			{
				GUI.DrawTexture(new Rect(70, 70, (Screen.width / 2) - 130, (Screen.height / 2) - 120), m_CasePreview, ScaleMode.ScaleToFit);
			}
			else
			{
				GUI.DrawTexture(new Rect(70, 70, (Screen.width / 2) - 130, (Screen.height / 2) - 120), m_ItemPreviews[m_SelectedItem - 3], ScaleMode.ScaleToFit);
			}
		}
		
		// Item stats box
		int statBoxWidth = (Screen.width / 2) - 30;
		int statBoxHeight = (Screen.height / 2) - 90;
		//GUI.TextArea(new Rect(20, (Screen.height / 2), statBoxWidth, statBoxHeight), "");

		if (m_SelectedWeapon != null)
		{
			// Draw the current and next upgraded weapon stats
			//----------------------// Damage //----------------------//
			int numUpgrades = m_WeaponStats.NumDamageUpgrades(m_SelectedWeapon.WeaponName);
			int upgradeLevel = m_SelectedWeapon.DamageLevel;
			if (m_ShopState != ShopState.Upgrade || upgradeLevel >= numUpgrades - 1)
			{
				GUI.TextArea(new Rect(20, (Screen.height / 2), statBoxWidth / 2, statBoxHeight / 3),
					"Damage: " + m_WeaponStats.GetDamage(m_SelectedWeapon.WeaponName, upgradeLevel).ToString() + "\n\nLevel:");
			}
			else
			{
				GUI.TextArea(new Rect(20, (Screen.height / 2), (statBoxWidth / 2) - 70, statBoxHeight / 3),
					"Damage: " + m_SelectedWeapon.Damage.ToString() + " >>> " + 
					m_WeaponStats.GetDamage(m_SelectedWeapon.WeaponName, upgradeLevel + 1).ToString() + "\n\nLevel:");
				
				// Create a button to upgrade this stat
				int upgradeCost = m_CostListScript.GetDamageUpgradeCost(m_SelectedWeapon.BaseItem.PrefabID, upgradeLevel);
				if (GUI.Button(new Rect((statBoxWidth / 2) - 50, (Screen.height / 2), 70, statBoxHeight / 3), upgradeCost.ToString())
					&& m_Inventory.TotalMoney >= upgradeCost)
				{
					m_Inventory.TakeMoney(upgradeCost);
					m_SelectedWeapon.UpgradeDamage();
				}
			}
			
			// Draw the damage upgrade level nubs
			for (int i = 0; i <= upgradeLevel; i++)
			{
				GUI.DrawTexture(new Rect((i * 24) + 60, (Screen.height / 2) + 32, 
					m_WeaponUpgradeFull.width, m_WeaponUpgradeFull.height), m_WeaponUpgradeFull);
			}
			for (int i = upgradeLevel + 1; i < numUpgrades; i++)
			{
				GUI.DrawTexture(new Rect((i * 24) + 60, (Screen.height / 2) + 32, 
					m_WeaponUpgradeEmpty.width, m_WeaponUpgradeEmpty.height), m_WeaponUpgradeEmpty);
			}
			
			//----------------------// Capacity //----------------------//
			numUpgrades = m_WeaponStats.NumCapacityUpgrades(m_SelectedWeapon.WeaponName);
			upgradeLevel = m_SelectedWeapon.CapacityLevel;
			if (m_ShopState != ShopState.Upgrade || upgradeLevel >= numUpgrades - 1)
			{
				GUI.TextArea(new Rect(20, (Screen.height / 2) + (statBoxHeight / 3), statBoxWidth / 2, statBoxHeight / 3),
					"Capacity: " + m_WeaponStats.GetCapacity(m_SelectedWeapon.WeaponName, upgradeLevel).ToString() + "\n\nLevel:");
			}
			else
			{
				GUI.TextArea(new Rect(20, (Screen.height / 2) + (statBoxHeight / 3), (statBoxWidth / 2) - 70, statBoxHeight / 3),
					"Capacity: " + m_SelectedWeapon.Capacity.ToString() + " >>> " +
					m_WeaponStats.GetCapacity(m_SelectedWeapon.WeaponName, upgradeLevel + 1).ToString() + "\n\nLevel:");
				
				// Create a button to upgrade this stat
				int upgradeCost = m_CostListScript.GetCapacityUpgradeCost(m_SelectedWeapon.BaseItem.PrefabID, upgradeLevel);
				if (GUI.Button(new Rect((statBoxWidth / 2) - 50, (Screen.height / 2) + (statBoxHeight / 3), 70, statBoxHeight / 3), upgradeCost.ToString())
					&& m_Inventory.TotalMoney >= upgradeCost)
				{
					m_Inventory.TakeMoney(upgradeCost);
					m_SelectedWeapon.UpgradeCapacity();
				}
			}
			
			// Draw the capacity upgrade level nubs
			for (int i = 0; i <= upgradeLevel; i++)
			{
				GUI.DrawTexture(new Rect((i * 24) + 60, (Screen.height / 2) + (statBoxHeight / 3) + 32, 
					m_WeaponUpgradeFull.width, m_WeaponUpgradeFull.height), m_WeaponUpgradeFull);
			}
			for (int i = upgradeLevel + 1; i < numUpgrades; i++)
			{
				GUI.DrawTexture(new Rect((i * 24) + 60, (Screen.height / 2) + (statBoxHeight / 3) + 32, 
					m_WeaponUpgradeEmpty.width, m_WeaponUpgradeEmpty.height), m_WeaponUpgradeEmpty);
			}
			
			//----------------------// Penetration //----------------------//
			numUpgrades = m_WeaponStats.NumPenetrationUpgrades(m_SelectedWeapon.WeaponName);
			upgradeLevel = m_SelectedWeapon.PenetrationLevel;
			if (m_ShopState != ShopState.Upgrade || upgradeLevel >= numUpgrades - 1)
			{
				GUI.TextArea(new Rect(20, (Screen.height / 2) + ((statBoxHeight / 3) * 2), statBoxWidth / 2, statBoxHeight / 3),
					"Penetration: " + m_WeaponStats.GetPenetration(m_SelectedWeapon.WeaponName, upgradeLevel).ToString() + "\n\nLevel:");
			}
			else
			{
				GUI.TextArea(new Rect(20, (Screen.height / 2) + ((statBoxHeight / 3) * 2), (statBoxWidth / 2) - 70, statBoxHeight / 3),
					"Penetration: " + m_SelectedWeapon.Penetration.ToString() + " >>> " + 
					m_WeaponStats.GetPenetration(m_SelectedWeapon.WeaponName, upgradeLevel + 1).ToString() + "\n\nLevel:");
				
				// Create a button to upgrade this stat
				int upgradeCost = m_CostListScript.GetPenetrationUpgradeCost(m_SelectedWeapon.BaseItem.PrefabID, upgradeLevel);
				if (GUI.Button(new Rect((statBoxWidth / 2) - 50, (Screen.height / 2) + ((statBoxHeight / 3) * 2), 70, statBoxHeight / 3), upgradeCost.ToString())
					&& m_Inventory.TotalMoney >= upgradeCost)
				{
					m_Inventory.TakeMoney(upgradeCost);
					m_SelectedWeapon.UpgradePenetration();
				}
			}
			
			// Draw the capacity upgrade level nubs
			for (int i = 0; i <= upgradeLevel; i++)
			{
				GUI.DrawTexture(new Rect((i * 24) + 60, (Screen.height / 2) + ((statBoxHeight / 3) * 2) + 32, 
					m_WeaponUpgradeFull.width, m_WeaponUpgradeFull.height), m_WeaponUpgradeFull);
			}
			for (int i = upgradeLevel + 1; i < numUpgrades; i++)
			{
				GUI.DrawTexture(new Rect((i * 24) + 60, (Screen.height / 2) + ((statBoxHeight / 3) * 2) + 32, 
					m_WeaponUpgradeEmpty.width, m_WeaponUpgradeEmpty.height), m_WeaponUpgradeEmpty);
			}
			
			//----------------------// Firing Speed //----------------------//
			numUpgrades = m_WeaponStats.NumFiringSpeedUpgrades(m_SelectedWeapon.WeaponName);
			upgradeLevel = m_SelectedWeapon.FiringSpeedLevel;
			if (m_ShopState != ShopState.Upgrade || upgradeLevel >= numUpgrades - 1)
			{
				GUI.TextArea(new Rect(20 + (statBoxWidth / 2), (Screen.height / 2), statBoxWidth / 2, statBoxHeight / 3),
					"Firing Speed: " + m_WeaponStats.GetFiringSpeed(m_SelectedWeapon.WeaponName, upgradeLevel).ToString() + "\n\nLevel:");
			}
			else
			{
				GUI.TextArea(new Rect(20 + (statBoxWidth / 2), (Screen.height / 2), (statBoxWidth / 2) - 70, statBoxHeight / 3),
					"Firing Speed: " + m_SelectedWeapon.FiringSpeed.ToString() + " >>> " + 
					m_WeaponStats.GetFiringSpeed(m_SelectedWeapon.WeaponName, upgradeLevel + 1).ToString() + "\n\nLevel:");
				
				// Create a button to upgrade this stat
				int upgradeCost = m_CostListScript.GetFiringSpeedUpgradeCost(m_SelectedWeapon.BaseItem.PrefabID, upgradeLevel);
				if (GUI.Button(new Rect(statBoxWidth - 50, (Screen.height / 2), 70, statBoxHeight / 3), upgradeCost.ToString())
					&& m_Inventory.TotalMoney >= upgradeCost)
				{
					m_Inventory.TakeMoney(upgradeCost);
					m_SelectedWeapon.UpgradeFiringSpeed();
				}
			}
			
			// Draw the capacity upgrade level nubs
			for (int i = 0; i <= upgradeLevel; i++)
			{
				GUI.DrawTexture(new Rect((statBoxWidth / 2) + (i * 24) + 60, (Screen.height / 2) + 32, 
					m_WeaponUpgradeFull.width, m_WeaponUpgradeFull.height), m_WeaponUpgradeFull);
			}
			for (int i = upgradeLevel + 1; i < numUpgrades; i++)
			{
				GUI.DrawTexture(new Rect((statBoxWidth / 2) + (i * 24) + 60, (Screen.height / 2) + 32, 
					m_WeaponUpgradeEmpty.width, m_WeaponUpgradeEmpty.height), m_WeaponUpgradeEmpty);
			}
			
			//----------------------// Reload Speed //----------------------//
			numUpgrades = m_WeaponStats.NumReloadSpeedUpgrades(m_SelectedWeapon.WeaponName);
			upgradeLevel = m_SelectedWeapon.ReloadSpeedLevel;
			if (m_ShopState != ShopState.Upgrade || upgradeLevel >= numUpgrades - 1)
			{
				GUI.TextArea(new Rect(20 + (statBoxWidth / 2), (Screen.height / 2) + (statBoxHeight / 3), statBoxWidth / 2, statBoxHeight / 3),
					"Reload Speed: " + m_WeaponStats.GetReloadSpeed(m_SelectedWeapon.WeaponName, upgradeLevel).ToString() + "\n\nLevel:");
			}
			else
			{
				GUI.TextArea(new Rect(20 + (statBoxWidth / 2), (Screen.height / 2) + (statBoxHeight / 3), (statBoxWidth / 2) - 70, statBoxHeight / 3),
					"Reload Speed: " + m_SelectedWeapon.ReloadSpeed.ToString() + " >>> " + 
					m_WeaponStats.GetReloadSpeed(m_SelectedWeapon.WeaponName, upgradeLevel + 1).ToString() + "\n\nLevel:");
				
				// Create a button to upgrade this stat
				int upgradeCost = m_CostListScript.GetReloadSpeedUpgradeCost(m_SelectedWeapon.BaseItem.PrefabID, upgradeLevel);
				if (GUI.Button(new Rect(statBoxWidth - 50, (Screen.height / 2) + (statBoxHeight / 3), 70, statBoxHeight / 3), upgradeCost.ToString())
					&& m_Inventory.TotalMoney >= upgradeCost)
				{
					m_Inventory.TakeMoney(upgradeCost);
					m_SelectedWeapon.UpgradeReloadSpeed();
				}
			}
			
			// Draw the capacity upgrade level nubs
			for (int i = 0; i <= upgradeLevel; i++)
			{
				GUI.DrawTexture(new Rect((statBoxWidth / 2) + (i * 24) + 60, (Screen.height / 2) + (statBoxHeight / 3) + 32, 
					m_WeaponUpgradeFull.width, m_WeaponUpgradeFull.height), m_WeaponUpgradeFull);
			}
			for (int i = upgradeLevel + 1; i < numUpgrades; i++)
			{
				GUI.DrawTexture(new Rect((statBoxWidth / 2) + (i * 24) + 60, (Screen.height / 2) + (statBoxHeight / 3) + 32, 
					m_WeaponUpgradeEmpty.width, m_WeaponUpgradeEmpty.height), m_WeaponUpgradeEmpty);
			}
			
			//----------------------// Radius //----------------------//
			numUpgrades = m_WeaponStats.NumRadiusUpgrades(m_SelectedWeapon.WeaponName);
			upgradeLevel = m_SelectedWeapon.RadiusLevel;
			float radius = m_WeaponStats.GetRadius(m_SelectedWeapon.WeaponName, upgradeLevel);
			if (radius != 0.0f)
			{
				if (m_ShopState != ShopState.Upgrade || upgradeLevel >= numUpgrades - 1)
				{
					GUI.TextArea(new Rect(20 + (statBoxWidth / 2), (Screen.height / 2) + ((statBoxHeight / 3) * 2), statBoxWidth / 2, statBoxHeight / 3),
						"Explosion Radius: " + radius.ToString() + "\n\nLevel:");
				}
				else
				{
					GUI.TextArea(new Rect(20 + (statBoxWidth / 2), (Screen.height / 2) + ((statBoxHeight / 3) * 2), (statBoxWidth / 2) - 70, statBoxHeight / 3),
						"Explosion Radius: " + radius.ToString() + " >>> " +
						m_WeaponStats.GetRadius(m_SelectedWeapon.WeaponName, upgradeLevel + 1).ToString() + "\n\nLevel:");
					
					// Create a button to upgrade this stat
					int upgradeCost = m_CostListScript.GetRadiusUpgradeCost(m_SelectedWeapon.BaseItem.PrefabID, upgradeLevel);
					if (GUI.Button(new Rect(statBoxWidth - 50, (Screen.height / 2) + ((statBoxHeight / 3) * 2), 70, statBoxHeight / 3), upgradeCost.ToString())
						&& m_Inventory.TotalMoney >= upgradeCost)
					{
						m_Inventory.TakeMoney(upgradeCost);
						m_SelectedWeapon.UpgradeRadius();
					}
				}
				
				// Draw the capacity upgrade level nubs
				for (int i = 0; i <= upgradeLevel; i++)
				{
					GUI.DrawTexture(new Rect((statBoxWidth / 2) + (i * 24) + 60, (Screen.height / 2) + ((statBoxHeight / 3) * 2) + 32, 
						m_WeaponUpgradeFull.width, m_WeaponUpgradeFull.height), m_WeaponUpgradeFull);
				}
				for (int i = upgradeLevel + 1; i < numUpgrades; i++)
				{
					GUI.DrawTexture(new Rect((statBoxWidth / 2) + (i * 24) + 60, (Screen.height / 2) + ((statBoxHeight / 3) * 2) + 32, 
						m_WeaponUpgradeEmpty.width, m_WeaponUpgradeEmpty.height), m_WeaponUpgradeEmpty);
				}
			}
		}
		
		// Buttons at the bottom of the menu
		if (GUI.Button(new Rect(20, Screen.height - 70, 150, 50), "BACK"))
		{
			m_ShopState = ShopState.MainMenu;
			m_SelectedWeapon = null;
			m_SelectedAmmo = null;
			m_SelectedSellItem = null;
			m_DescriptionString = "";
			m_SelectedItem = 0;
			m_StackedItemSelected = false;
			m_ScrollPosition = new Vector2(0.0f, 0.0f);
		}
		
		if (m_ShopState == ShopState.Buy)
		{
			if (m_SelectedAmmo != null)
			{
				int maxAmount = m_SelectedAmmo.MaxCapacity * 5;
				if (GUI.RepeatButton(new Rect(Screen.width - 170, Screen.height - 70, 30, 50), "<"))
				{
					if (m_AllowPress)
					{
						if (m_Amount > 1)
						{
							m_Amount--;
						}
						else
						{
							m_Amount = maxAmount;
						}

						m_ButtonTimer += 0.05f;
						m_AllowPress = false;
					}
					m_ButtonPressed = true;
				}
				
				if (GUI.RepeatButton(new Rect(Screen.width - 50, Screen.height - 70, 30, 50), ">"))
				{
					if (m_AllowPress)
					{
						if (m_Amount < maxAmount)
						{
							m_Amount++;
						}
						else
						{
							m_Amount = 1;
						}

						m_ButtonTimer += 0.05f;
						m_AllowPress = false;
					}
					m_ButtonPressed = true;
				}

				m_ItemCost = m_CostListScript.GetCostFromPrefabID(m_SelectedItem) * m_Amount;
				if (GUI.Button(new Rect(Screen.width - 140, Screen.height - 70, 90, 50), 
				               "BUY " + m_Amount.ToString() + "\n(£" + m_ItemCost.ToString() + ")"))
				{
					// Take money from inventory and place bought item in holding area
					//m_ItemCost = m_CostListScript.GetCostFromPrefabID(m_SelectedItem) * m_Amount;
					if (m_Inventory.TotalMoney >= m_ItemCost)
					{
						int ammoPacks = m_Amount / m_SelectedAmmo.MaxCapacity;
						ammoPacks = m_Amount % m_SelectedAmmo.MaxCapacity != 0 ? ammoPacks + 1 : ammoPacks;
						int amountLeft = m_Amount;

						for (int i = 0; i < ammoPacks; i++)
						{
							Transform item = Instantiate(PrefabIDList.GetPrefabWithID(m_SelectedItem)) as Transform;
							InventoryItem itemComponent = item.GetComponent<InventoryItem>();
							InventoryAmmo ammoComponent = item.GetComponent<InventoryAmmo>();

							int packAmount = amountLeft > m_SelectedAmmo.MaxCapacity ? m_SelectedAmmo.MaxCapacity : amountLeft;
							amountLeft -= packAmount;
							ammoComponent.Amount = packAmount;
							
							// If the item is placed into the holding area switch to 
							// placing mode to let the player place it in their inventory
							if (m_HoldingArea.FindAvailableSpace(itemComponent))
							{
								m_ShopState = ShopState.Placing;
								m_MousePicker.Enabled = true;
								m_BuyingItemList.Add(item);

								m_Inventory.DrawLines = true;
								m_Inventory.AllowPartialAmmoCombining = false;
								m_HoldingArea.DrawLines = true;
								m_HoldingArea.UpdateTotalSellPrice = true;
								GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>().ShowEffects = true;
							}
							else
							{
								Destroy(item.gameObject);
							}
						}

						m_Amount = 1;
					}
				}
			}
			else
			{
				if (GUI.Button(new Rect(Screen.width - 170, Screen.height - 70, 150, 50), "BUY") && m_SelectedItem > 0)
				{
					if (m_SelectedItem == 1)
					{
						// Take money from inventory and upgrade Inventory
						m_ItemCost = m_Inventory.UpgradeCost;
						if (m_Inventory.TotalMoney >= m_ItemCost)
						{
							m_Inventory.UpgradeSize();
							m_Inventory.TakeMoney(m_ItemCost);

							if (!m_Inventory.IsUpgradeable)
							{
								m_DescriptionString = "";
								m_SelectedItem = 0;
							}
						}
					}
					else
					{
						// Take money from inventory and place bought item in holding area
						m_ItemCost = m_CostListScript.GetCostFromPrefabID(m_SelectedItem);
						if (m_Inventory.TotalMoney >= m_ItemCost)
						{
							Transform item = Instantiate(PrefabIDList.GetPrefabWithID(m_SelectedItem)) as Transform;
							InventoryItem itemComponent = item.GetComponent<InventoryItem>();

							// If the item is placed into the holding area switch to 
							// placing mode to let the player place it in their inventory
							if (m_HoldingArea.FindAvailableSpace(itemComponent))
							{
								//m_Inventory.TakeMoney(itemCost);
								m_ShopState = ShopState.Placing;
								m_MousePicker.Enabled = true;
								m_BuyingItemList.Add(item);

								m_Inventory.DrawLines = true;
								m_HoldingArea.DrawLines = true;
								m_HoldingArea.UpdateTotalSellPrice = true;
								GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>().ShowEffects = true;
							}
							else
							{
								Destroy(item.gameObject);
							}
						}
					}
				}
			}
		}
		else if (m_ShopState == ShopState.Sell)
		{
			// Sell button for ammo (Possibly other stacked items in future?)
			if (m_SelectedAmmo != null)
			{
				WeaponType weaponType = m_SelectedAmmo.WeaponType;
				if (GUI.RepeatButton(new Rect(Screen.width - 170, Screen.height - 70, 30, 50), "<") && m_SelectedSellItem != null)
				{
					if (m_AllowPress)
					{
						if (m_Amount > 1)
						{
							m_Amount--;
						}
						else
						{
							m_Amount = m_AmmoToSell[weaponType];
						}

						m_ButtonTimer += 0.05f;
						m_AllowPress = false;
					}
					m_ButtonPressed = true;
				}
				
				if (GUI.RepeatButton(new Rect(Screen.width - 50, Screen.height - 70, 30, 50), ">") && m_SelectedSellItem != null)
				{
					if (m_AllowPress)
					{
						if (m_Amount < m_AmmoToSell[weaponType])
						{
							m_Amount++;
						}
						else
						{
							m_Amount = 1;
						}

						m_ButtonTimer += 0.05f;
						m_AllowPress = false;
					}
					m_ButtonPressed = true;
				}
				
				int itemCost = (m_CostListScript.GetCostFromPrefabID(m_SelectedAmmo.BaseItem.PrefabID) / 2) * m_Amount;
				if (GUI.Button(new Rect(Screen.width - 140, Screen.height - 70, 90, 50), 
					"SELL " + m_Amount.ToString() + "\n(£" + itemCost.ToString() + ")") && m_SelectedSellItem != null)
				{
					// Add money to inventory and destroy sold item
					m_Inventory.AddMoney(itemCost);
					Vector3 tempPos = new Vector3();
					m_Inventory.TakeAmmo(m_SelectedAmmo.WeaponType, m_Amount, out tempPos);
					
					if (m_SelectedAmmo.Amount == 0)
					{
						// Check if the currently selected ammo pack has been depleted, and if there are any more to select
						m_SelectedAmmo = null;
						HashSet<InventoryAmmo> ammoList = m_Inventory.FindAmmo(weaponType);
						HashSet<InventoryAmmo>.Enumerator enumerator = ammoList.GetEnumerator();
						while (enumerator.MoveNext() && m_SelectedAmmo == null)
						{
							if (enumerator.Current.Amount != 0)
							{
								m_SelectedAmmo = enumerator.Current;
								m_SelectedSellItem = m_SelectedAmmo.BaseItem;
							}
						}
						
						if (m_SelectedAmmo == null)
						{
							m_SelectedSellItem = null;
							m_DescriptionString = "";
							m_SellItemCost = 0;
							m_SelectedItem = 0;
						}
					}
					
					m_Amount = 1;
					PrepareSellMenu();
				}
			}
			else if (m_StackedItemSelected)
			{
				if (GUI.RepeatButton(new Rect(Screen.width - 170, Screen.height - 70, 30, 50), "<") && m_SelectedSellItem != null)
				{
					if (m_AllowPress)
					{
						if (m_Amount > 1)
						{
							m_Amount--;
						}
						else
						{
							m_Amount = m_StackedItemsToSell[m_SelectedItem];
						}
						
						m_ButtonTimer += 0.05f;
						m_AllowPress = false;
					}
					m_ButtonPressed = true;
				}
				
				if (GUI.RepeatButton(new Rect(Screen.width - 50, Screen.height - 70, 30, 50), ">") && m_SelectedSellItem != null)
				{
					if (m_AllowPress)
					{
						if (m_Amount < m_StackedItemsToSell[m_SelectedItem])
						{
							m_Amount++;
						}
						else
						{
							m_Amount = 1;
						}
						
						m_ButtonTimer += 0.05f;
						m_AllowPress = false;
					}
					m_ButtonPressed = true;
				}
				
				int itemCost = (m_CostListScript.GetCostFromPrefabID(m_SelectedItem) / 2) * m_Amount;
				if (GUI.Button(new Rect(Screen.width - 140, Screen.height - 70, 90, 50), 
				               "SELL " + m_Amount.ToString() + "\n(£" + itemCost.ToString() + ")") && m_SelectedSellItem != null)
				{
					// Add money to inventory and destroy sold items
					m_Inventory.AddMoney(itemCost);
					HashSet<InventoryItem> itemList = m_Inventory.FindItemsWithPrefabID(m_SelectedItem);
					int i = 0;
					foreach (InventoryItem item in itemList)
					{
						if (i < m_Amount)
						{
							m_Inventory.DestroyItem(item);
						}
						else
						{
							m_SelectedSellItem = item;
							break;
						}
						i++;
					}

					// If you sold the last item remove all the references to it
					if (m_Amount == m_StackedItemsToSell[m_SelectedItem])
					{
						// Should try and make it select next item in list
						m_SelectedSellItem = null;
						m_DescriptionString = "";
						m_SellItemCost = 0;
						m_SelectedItem = 0;
						m_StackedItemSelected = false;
					}
					
					m_Amount = 1;
					PrepareSellMenu();
				}
			}
			else
			{
				if (GUI.Button(new Rect(Screen.width - 170, Screen.height - 70, 150, 50), "SELL\n(£" + m_SellItemCost.ToString() + ")") && m_SelectedSellItem != null)
				{
					// Add money to inventory and destroy sold item
					//int itemCost = m_CostListScript.GetCostFromPrefabID(m_SelectedSellItem.PrefabID) / 2;
					m_Inventory.AddMoney(m_SellItemCost);

					m_Inventory.DestroyItem(m_SelectedSellItem);
					m_SelectedSellItem = null;
					m_DescriptionString = "";
					m_Amount = 1;
					m_SellItemCost = 0;
					m_SelectedItem = 0;
					PrepareSellMenu();
				}
			}
		}
		
		GUI.TextField(new Rect((Screen.width / 2) + 10, Screen.height - 70, (Screen.width / 2) - 200, 50), "MONEY : " + m_Inventory.TotalMoney.ToString());
	}

	private void DrawPlacingGUI()
	{
		if (!m_AllItemsPlaced && !m_OldItemsInHolding)
		{
			// Only items being bought are in the holding area, so simply cancel the purchase
			if (GUI.Button(new Rect(Screen.width - 120, Screen.height - 70, 100, 50), "CANCEL"))
			{
				// If buying is cancelled destroy all the items being bought, and don't bother taking any money
				foreach (Transform item in m_BuyingItemList)
				{
					//Destroy(item.gameObject);
					//m_HoldingArea.DestroyItem(item.GetComponent<InventoryItem>());
					InventoryItem itemComponent = item.GetComponent<InventoryItem>();
					itemComponent.FirstSpace.inventory.DestroyItem(itemComponent);
				}
				m_BuyingItemList.Clear();

				m_ShopState = ShopState.Buy;
				m_MousePicker.Enabled = false;
				m_Inventory.DrawLines = false;
				m_HoldingArea.DrawLines = false;
				m_HoldingArea.UpdateTotalSellPrice = false;
				m_Inventory.AllowPartialAmmoCombining = true;
				GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>().ShowEffects = false;
			}
		}
		else if (m_AllItemsPlaced && m_OldItemsInHolding)
		{
			// All items being bought are in the inventory, but other items are in the holding area, so sell them
			int sellValue = m_HoldingArea.TotalSellPrice;
			if (GUI.Button(new Rect(Screen.width - 120, Screen.height - 70, 100, 50), "SELL\n(£" + sellValue.ToString() + ")"))
			{
				// Destroy all items left in the holding area, and take money for the bought items
				HashSet<InventoryItem> itemList = m_HoldingArea.FindAllItemsWithComponent<InventoryItem>();
				foreach (InventoryItem item in itemList)
				{
					m_HoldingArea.DestroyItem(item);
				}

				m_Inventory.AddMoney(sellValue);

				m_BuyingItemList.Clear();
				m_Inventory.TakeMoney(m_ItemCost);
				m_ShopState = ShopState.Buy;
				m_MousePicker.Enabled = false;
				m_Inventory.DrawLines = false;
				m_HoldingArea.DrawLines = false;
				m_HoldingArea.UpdateTotalSellPrice = false;
				m_Inventory.AllowPartialAmmoCombining = true;
				GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>().ShowEffects = false;
			}
		}
		else if (!m_AllItemsPlaced && m_OldItemsInHolding)
		{
			// Various items are in the holding area, so sell them and cancel the purchase
			int sellValue = m_HoldingArea.TotalSellPrice;
			foreach (Transform item in m_BuyingItemList)
			{
				InventoryItem itemComponent = item.GetComponent<InventoryItem>();
				if (itemComponent.FirstSpace.inventory == m_HoldingArea)
				{
					sellValue -= itemComponent.GetSellValue();
				}
			}

			if (GUI.Button(new Rect(Screen.width - 120, Screen.height - 70, 100, 50), "CANCEL\n + SELL\n(£" + sellValue.ToString() + ")"))
			{
				// Destroy items being bought
				foreach (Transform item in m_BuyingItemList)
				{
					//Destroy(item.gameObject);
					InventoryItem itemComponent = item.GetComponent<InventoryItem>();
					itemComponent.FirstSpace.inventory.DestroyItem(itemComponent);
				}
				m_BuyingItemList.Clear();

				// Destroy all items left in the holding area
				HashSet<InventoryItem> itemList = m_HoldingArea.FindAllItemsWithComponent<InventoryItem>();
				foreach (InventoryItem item in itemList)
				{
					m_HoldingArea.DestroyItem(item);
				}

				m_Inventory.AddMoney(sellValue);

				m_ShopState = ShopState.Buy;
				m_MousePicker.Enabled = false;
				m_Inventory.DrawLines = false;
				m_HoldingArea.DrawLines = false;
				m_HoldingArea.UpdateTotalSellPrice = false;
				m_Inventory.AllowPartialAmmoCombining = true;
				GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>().ShowEffects = false;
			}
		}
	}

	private void DrawQuickSellGUI()
	{
		if (m_HoldingArea.GetNumberOfItems() == 0)
		{
			// Nothing is in the holding area, so cancel selling and return to previous menu
			if (GUI.Button(new Rect(Screen.width - 120, Screen.height - 70, 100, 50), "CANCEL"))
			{				
				m_ShopState = ShopState.MainMenu;
				m_MousePicker.Enabled = false;
				m_Inventory.DrawLines = false;
				m_HoldingArea.DrawLines = false;
				m_HoldingArea.UpdateTotalSellPrice = false;
				GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>().ShowEffects = false;
			}
		}
		else
		{
			// Items are in the holding area, so destroy them and give the relevant amount of money
			int sellValue = m_HoldingArea.TotalSellPrice;
			if (GUI.Button(new Rect(Screen.width - 120, Screen.height - 70, 100, 50), "SELL\n(£" + sellValue.ToString() + ")"))
			{
				// Destroy all items left in the holding area
				HashSet<InventoryItem> itemList = m_HoldingArea.FindAllItemsWithComponent<InventoryItem>();
				foreach (InventoryItem item in itemList)
				{
					m_HoldingArea.DestroyItem(item);
				}

				m_Inventory.AddMoney(sellValue);

				/*m_ShopState = ShopState.MainMenu;
				m_MousePicker.Enabled = false;
				m_Inventory.DrawLines = false;
				m_HoldingArea.DrawLines = false;
				m_HoldingArea.UpdateTotalSellPrice = false;*/
			}
		}
	}
}