using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum PlacementResult
{
	Failed = 0,
	Success,
	AmmoTaken,
	AmmoDestroyed,
	ItemSwapped,
}

public class InventorySpace
{
	public int objectType;
	public InventoryItem currentObject = null;
	public InventoryScript inventory;
	public bool cellChecked = false;
}

public class InventoryScript : MonoBehaviour
{
	[Serializable]
	class InventorySize
	{
		public int m_Width;
		public int m_Height;
		public int m_Cost;
	}

	// SCORING BONUSES
	const int SCORE_TYPE_ADJACENCY_BONUS = 15;
	const int SCORE_FULL_TYPE_ADJACENCY_BONUS = 50;
	const int SCORE_FULL_ADJACENCY_BONUS = 100;
	const int SCORE_TYPE_BOXED_BONUS = 50;

	const int SCORE_MINOR_ROTATION_BONUS = 5;
	const int SCORE_TYPE_ROTATION_BONUS = 50;
	const int SCORE_FULL_TYPE_ROTATION_BONUS = 50;
	const int SCORE_FULL_ROTATION_BONUS = 200;

	const int MINIMUM_AMOUNT_FOR_ADJACENT_BONUS = 1;
	const int MINIMUM_AMOUNT_FOR_ROTATION_BONUS = 1;

	const int MAXIMUM_RANDOM_PLACEMENT_TRIES = 25;
	
	// Inventory variables
	[SerializeField] private int m_Width;
	[SerializeField] private int m_Height;
	[SerializeField] private Texture2D m_LineTexture;
	[SerializeField] private int m_LineWidth;
	[SerializeField] private InventorySize[] m_SizeList;
	
	private InventorySpace[,] m_InventoryList;
	private bool m_AllowPartialAmmoCombining = true;
	private bool m_DrawLines = true;
	private float m_Spacing = 1.0f;
	private int m_TotalMoney = 5000;
	private int m_SizeIndex = 0;
	private int m_ItemCount = 0;
	private int m_NumFreeCells = 0;

	private bool m_UpdateTotalSellPrice = false;
	private int m_TotalSellPrice = 0;
	
	public void AddMoney(int amount){m_TotalMoney += amount;}
	public void TakeMoney(int amount){m_TotalMoney -= amount;}
	public int TotalMoney { get {return m_TotalMoney;}}
	public bool DrawLines { get {return m_DrawLines;} set {m_DrawLines = value;}}
	public bool AllowPartialAmmoCombining { get {return m_AllowPartialAmmoCombining;} set {m_AllowPartialAmmoCombining = value;}}

	public bool UpdateTotalSellPrice { get {return m_UpdateTotalSellPrice;} set {m_UpdateTotalSellPrice = value;}}
	public int TotalSellPrice { get {return m_TotalSellPrice;}}
	public int Size { get {return m_Width * m_Height;}}

	public bool IsUpgradeable { get {return m_SizeIndex < (m_SizeList.GetLength(0) - 1);}}
	public int UpgradeCost { get {return IsUpgradeable ? m_SizeList[m_SizeIndex + 1].m_Cost : 0;}}
	public string UpgradeSizeString { get {return IsUpgradeable ? "(" + m_SizeList[m_SizeIndex + 1].m_Width.ToString()
										+ " x " + m_SizeList[m_SizeIndex + 1].m_Height.ToString() + ")" : "(? x ?)";}}
	
	void Awake()
	{
		//DontDestroyOnLoad(gameObject);

		// Create the Inventory array and set the inventory of each cell to this
		m_InventoryList = new InventorySpace[m_Width, m_Height];
		for (int x = 0; x < m_Width; x++)
		{
			for (int y = 0; y < m_Height; y++)
			{
				m_InventoryList[x,y] = new InventorySpace();
				m_InventoryList[x,y].inventory = this;
			}
		}

		//m_SizeIndex = m_SizeList.GetLength(0) - 1;

		m_Width = m_SizeList[m_SizeIndex].m_Width;
		m_Height = m_SizeList[m_SizeIndex].m_Height;

		ResizeCollider();
	}

	void OnGUI()
	{
		/*if (m_DrawLines)
		{
			// Draw outlines for inventory grid spaces
			int height = -1;
			for (int i = 0; i <= m_Width; i++)
			{
				// Get the WorldSpace start and end for the line
				Vector3 start = transform.position;
				start.x += (float)i;

				if (height == -1)
				{
					Vector3 end = start;
					end.y -= m_Height;

					// Convert it to screen space to use in the GUI function
					start = Camera.main.WorldToScreenPoint(start);
					end = Camera.main.WorldToScreenPoint(end);
					height = (int)(start.y - end.y) + 1;
				}
				else
				{
					start = Camera.main.WorldToScreenPoint(start);
				}

				GUI.DrawTexture(new Rect(start.x, Screen.height - start.y, m_LineWidth, height), m_LineTexture);
			}

			int width = -1;
			for (int i = 0; i <= m_Height; i++)
			{
				// Get the WorldSpace start and end for the line
				Vector3 start = transform.position;
				start.y -= (float)i;

				if (width == -1)
				{
					Vector3 end = start;
					end.x += m_Width;
					
					// Convert it to screen space to use in the GUI function
					start = Camera.main.WorldToScreenPoint(start);
					end = Camera.main.WorldToScreenPoint(end);
					width = (int)(end.x - start.x) + 1;
				}
				else
				{
					start = Camera.main.WorldToScreenPoint(start);
				}
				
				GUI.DrawTexture(new Rect(start.x, Screen.height - start.y, width, m_LineWidth), m_LineTexture);
			}
		}*/

		/*if (CompareTag(Tags.MAININVENTORY))
		{
			GUI.TextArea(new Rect(0, 0, 20, 20), m_ItemCount.ToString());
		}
		else
		{
			GUI.TextArea(new Rect(0, 20, 20, 20), m_ItemCount.ToString());
		}*/

		/*if (m_Width != 5)
		{
			// Check for rotation bonus. Gives extra points for each item rotated the same way
			HashSet<InventoryItem> allItems = FindAllItemsWithComponent<InventoryItem>();
			int[] horizontalRotations = new int[4];
			int[] verticalRotations = new int[4];
			
			foreach (InventoryItem item in allItems)
			{
				// Check if we have a square shaped treasure, and if so ignore it
				if (item.Width == item.Height)
				{
					InventoryTreasure treasure = item.GetComponent<InventoryTreasure>();
					if (treasure != null)
					{
						continue;
					}
				}
				
				// Separate rotation amounts for items which start horizontal/vertical into different lists
				if (item.Width > item.Height)
				{
					// Add to relevant rotation amount
					horizontalRotations[item.Rotation] += 1;
				}
				else
				{
					verticalRotations[item.Rotation] += 1;
				}
			}
			
			// Check which rotation has the most items for both horizontal/vertical
			int highestHRotation = 0;
			int highestVRotation = 0;
			for (int i = 0; i < 4; i++)
			{
				if (horizontalRotations[i] > highestHRotation)
				{
					highestHRotation = i;
				}
				
				if (verticalRotations[i] > highestVRotation)
				{
					highestVRotation = i;
				}
			}
			
			// Combine relevant horizontal/vertical rotation amounts to give a more 'fair' results
			if (horizontalRotations[highestHRotation] > verticalRotations[highestVRotation])
			{
				if (highestHRotation == 0 || highestHRotation == 2)
				{
					horizontalRotations[highestHRotation] += verticalRotations[1] + verticalRotations[3];
				}
				else
				{
					horizontalRotations[highestHRotation] += verticalRotations[0] + verticalRotations[2];
				}
				
				// Multiply highest shared rotation by score bonus and add to score
				GUI.TextArea(new Rect(0, 0, 100, 20), (SCORE_MINOR_ROTATION_BONUS * horizontalRotations[highestHRotation]).ToString());
			}
			else
			{
				if (highestVRotation == 0 || highestVRotation == 2)
				{
					verticalRotations[highestVRotation] += horizontalRotations[1] + horizontalRotations[3];
				}
				else
				{
					verticalRotations[highestVRotation] += horizontalRotations[0] + horizontalRotations[2];
				}
				
				GUI.TextArea(new Rect(0, 0, 100, 20), (SCORE_MINOR_ROTATION_BONUS * verticalRotations[highestVRotation]).ToString());
			}
		}*/

		/*if (m_Width != 5)
		{
			// Check if items are boxed
			GUI.TextArea(new Rect(0, 0, 200, 20), AreItemsOfTypeBoxed<InventoryGrenade>() ? "Grenades boxed" : "Grenades not boxed!");
		}*/
	}
	
	// Update is called once per frame
	void Update()
	{
		/*if (Input.GetMouseButtonDown(2))
		{
			m_TotalMoney += 10000;
		}

		if (Input.GetMouseButtonDown(3))
		{
			UpgradeSize();
		}*/

		// Draw outlines for inventory grid spaces
		/*Vector3 pos = transform.position;
		
		for (int i = 0; i <= m_Width; i++)
		{
			Debug.DrawLine(new Vector3(pos.x + ((float)i * m_Spacing), pos.y), 
						   new Vector3(pos.x + ((float)i * m_Spacing), pos.y - ((float)m_Height * m_Spacing)), Color.white);
		}
		
		for (int i = 0; i <= m_Height; i++)
		{
			Debug.DrawLine(new Vector3(pos.x, pos.y - ((float)i * m_Spacing)), 
						   new Vector3(pos.x + ((float)m_Width * m_Spacing), pos.y - ((float)i * m_Spacing)), Color.white);
		}*/
	}

	private void ResizeCollider()
	{
		BoxCollider2D collider = GetComponent<BoxCollider2D>();
		if (collider != null)
		{
			Vector2 realSize = new Vector2(m_Width + 1, m_Height + 1);
			collider.size = realSize;
			realSize *= 0.5f;
			realSize.x -= 0.5f;
			realSize.y = -(realSize.y - 0.5f);
			collider.offset = realSize;
		}
	}
	
	private void ResetCells()
	{
		// Go through all the spaces in the inventory
		for (int y = 0; y < m_Height; y++)
		{
			for (int x = 0; x < m_Width; x++)
			{
				m_InventoryList[x,y].cellChecked = false;
			}
		}
	}
	
	private Vector2 GetCellPositionOfItem(InventoryItem item)
	{
		Vector2 position = new Vector2(-1.0f, -1.0f);
		InventorySpace itemSpace = item.FirstSpace;
		
		// Go through all the spaces in the inventory
		for (int y = 0; y < m_Height; y++)
		{
			for (int x = 0; x < m_Width; x++)
			{
				if (m_InventoryList[x,y] == itemSpace)
				{
					position.x = (float)x;
					position.y = (float)y;
					return position;
				}
			}
		}
		
		return position;
	}
	
	public bool FindAvailableSpace(InventoryItem item)
	{
		int timesChecked = 0;
		while (timesChecked < 2)
		{
			int width = item.RotatedWidth;
			int height = item.RotatedHeight;
			int rotation = item.Rotation;
			
			// Go through all the spaces in the inventory
			for (int y = 0; y < (m_Height - (height - 1)); y++)
			{
				for (int x = 0; x < (m_Width - (width - 1)); x++)
				{
					// Check each inventory square the item would occupy for any obstructions
					bool spaceFree = true;
					for (int j = 0; j < height && spaceFree; j++)
					{
						for (int i = 0; i < width && spaceFree; i++)
						{
							if (m_InventoryList[x+i,y+j].currentObject != null)
							{
								spaceFree = false;
							}
						}
					}
					
					// If there are no obstructions then set the item position and place it
					if (spaceFree)
					{
						Vector3 pos = transform.position;
						pos.x += (float)x * m_Spacing;
						pos.y -= (float)y * m_Spacing;
						
						if (rotation == 1)
						{
							pos.x += (float)width;
						}
						else if (rotation == 2)
						{
							pos.x += (float)width;
							pos.y -= (float)height;
						}
						else if (rotation == 3)
						{
							pos.y -= (float)height;
						}
						
						item.transform.position = pos;
						InventoryItem swapItem = null;
						PlaceItem(item, out swapItem);
						return true;
					}
				}
			}
			
			item.Rotate(-90.0f, item.transform.position);
			timesChecked++;
		}
		
		item.Reset();
		return false;
	}

	public bool FindRandomSpace(InventoryItem item)
	{
		int timesChecked = 0;
		while (timesChecked < MAXIMUM_RANDOM_PLACEMENT_TRIES)
		{
			// Pick a random cell to try placing
			int cellX = UnityEngine.Random.Range(0, m_Width);
			int cellY = UnityEngine.Random.Range(0, m_Height);

			// Pick a random rotation and rotate the item
			int rotation = UnityEngine.Random.Range(0, 4);
			for (int i = 0; i < rotation; i++)
			{
				item.Rotate(-90.0f, item.transform.position);
			}

			//int width = item.RotatedWidth;
			//int height = item.RotatedHeight;
					
			for (int i = 0; i < 4; i++)
			{
				Vector3 pos = transform.position;
				pos.x += (float)cellX * m_Spacing;
				pos.y -= (float)cellY * m_Spacing;
				
				item.transform.position = pos;
				if (TryPlaceItem(item) == PlacementResult.Success)
				{
					InventoryItem swapItem = null;
					PlaceItem(item, out swapItem);
					return true;
				}

				item.Rotate(-90.0f, item.transform.position);
			}

			// If space is not free then pick another random cell and rotation and try again
			timesChecked++;
		}
		
		item.Reset();
		return false;
	}

	public PlacementResult TryPlaceItem(InventoryItem item, bool allowSwapping = false)
	{
		// Get the item position relative to this inventory
		// and divide it by spacing to get the cell position
		Vector3 pos = item.transform.position;
		int rotation = item.Rotation;
		pos -= transform.position;
		pos /= m_Spacing;
		
		// Get width/height of object based on rotation
		int width = rotation == 0 || rotation == 2 ? item.Width : item.Height;
		int height = rotation == 0 || rotation == 2 ? item.Height : item.Width;
		width = rotation == 1 || rotation == 2 ? -width : width;
		height = rotation == 3 || rotation == 2 ? -height : height;
		
		// Get position of the top left corner of the item
		Vector3 topLeftPos = pos;
		if (width < 0)
		{
			topLeftPos.x += (float)width;
			width = -width;
		}
		if (height < 0)
		{
			topLeftPos.y -= (float)height;
			height = -height;
		}
		
		// If the object is outside the bounds of the inventory (Plus a small margin) return false
		if (topLeftPos.x < -0.5f || (topLeftPos.x + (float)width) >= ((float)m_Width + 0.5f))
		{
			return PlacementResult.Failed;
		}
		
		if (topLeftPos.y > 0.5f || (topLeftPos.y - (float)height) <= ((float)-m_Height - 0.5f))
		{
			return PlacementResult.Failed;
		}
		
		topLeftPos.x = Mathf.Max(topLeftPos.x, 0.0f);
		topLeftPos.y = Mathf.Min(topLeftPos.y, 0.0f);
		
		// Work out the actual cell position of the item
		Vector3 newPos = topLeftPos;
		int xPos = (int)newPos.x;
		int yPos = (int)-newPos.y;
		float decimalX = newPos.x - (float)xPos;
		float decimalY = -newPos.y - (float)yPos;
		
		if (decimalX >= 0.5f)
		{
			xPos++;
		}
		if (decimalY >= 0.5f)
		{
			yPos++;
		}
		
		// Check to see if the space needed for the item is free
		//bool spaceFree = true;
		HashSet<InventoryItem> overlappingItems = new HashSet<InventoryItem>();
		PlacementResult placementResult = PlacementResult.Success;
		InventoryAmmo ammoComponent = item.GetComponent<InventoryAmmo>();
		for (int y = yPos; y < yPos + height; y++)
		{
			for (int x = xPos; x < xPos + width; x++)
			{
				if (m_InventoryList[x,y].currentObject != null && m_InventoryList[x,y].currentObject != item)
				{
					// Check if both objects are ammo of the same type
					if (ammoComponent != null)
					{
						InventoryAmmo tempAmmoComponent = m_InventoryList[x,y].currentObject.GetComponent<InventoryAmmo>();
						if (tempAmmoComponent != null)
						{
							if (ammoComponent.WeaponType == tempAmmoComponent.WeaponType)
							{
								// If the ammo pack has room for more ammo, move ammo into it
								// AMMO NO LONGER NEEDS TO BE COMBINED, IS ALWAYS FULL
								/*int ammoNeeded = tempAmmoComponent.MaxCapacity - tempAmmoComponent.Amount;
								if (ammoNeeded == 0)
								{
									//spaceFree = false;
									placementResult = PlacementResult.Failed;
								}
								else if (!m_AllowPartialAmmoCombining && ammoNeeded < ammoComponent.Amount)
								{
									//spaceFree = false;
									placementResult = PlacementResult.Failed;
								}
								else if (ammoNeeded >= ammoComponent.Amount)
								{
									placementResult = PlacementResult.AmmoDestroyed;
									return placementResult;
								}
								else if (ammoNeeded < ammoComponent.Amount)
								{
									placementResult = PlacementResult.AmmoTaken;
									return placementResult;
								}*/
							}
						}
					}

					overlappingItems.Add(m_InventoryList[x,y].currentObject);
				}
			}
		}

		if (overlappingItems.Count > (allowSwapping ? 1 : 0))
		{
			placementResult = PlacementResult.Failed;
		}
		else if (allowSwapping && overlappingItems.Count == 1)
		{
			placementResult = PlacementResult.ItemSwapped;
		}

		return placementResult;
	}
	
	public PlacementResult PlaceItem(InventoryItem item, out InventoryItem swapItem, bool allowSwapping = false)
	{
		swapItem = null;

		// Get the item position relative to this inventory
		// and divide it by spacing to get the cell position
		Vector3 pos = item.transform.position;
		int rotation = item.Rotation;
		pos -= transform.position;
		pos /= m_Spacing;
		
		// Get width/height of object based on rotation
		int width = rotation == 0 || rotation == 2 ? item.Width : item.Height;
		int height = rotation == 0 || rotation == 2 ? item.Height : item.Width;
		width = rotation == 1 || rotation == 2 ? -width : width;
		height = rotation == 3 || rotation == 2 ? -height : height;
		
		// Get position of the top left corner of the item
		Vector3 topLeftPos = pos;
		if (width < 0)
		{
			topLeftPos.x += (float)width;
			width = -width;
		}
		if (height < 0)
		{
			topLeftPos.y -= (float)height;
			height = -height;
		}
		
		// If the object is outside the bounds of the inventory (Plus a small margin) return false
		if (topLeftPos.x < -0.5f || (topLeftPos.x + (float)width) > ((float)m_Width + 0.5f))
		{
			return PlacementResult.Failed;
		}
		
		if (topLeftPos.y > 0.5f || (topLeftPos.y - (float)height) < ((float)-m_Height - 0.5f))
		{
			return PlacementResult.Failed;
		}
		
		topLeftPos.x = Mathf.Max(topLeftPos.x, 0.0f);
		topLeftPos.y = Mathf.Min(topLeftPos.y, 0.0f);
		
		// Work out the actual cell position of the item
		Vector3 newPos = topLeftPos;//transform.position + topLeftPos;
		int xPos = (int)newPos.x;
		int yPos = (int)-newPos.y;
		float decimalX = newPos.x - (float)xPos;
		float decimalY = -newPos.y - (float)yPos;
		
		if (decimalX >= 0.5f)
		{
			xPos++;
		}
		if (decimalY >= 0.5f)
		{
			yPos++;
		}
		
		// Check to see if the space needed for the item is free
		bool spaceFree = true;
		bool ammoTaken = false;
		HashSet<InventoryItem> overlappingItems = new HashSet<InventoryItem>();
		InventoryAmmo ammoComponent = item.GetComponent<InventoryAmmo>();
		for (int y = yPos; y < yPos + height; y++)
		{
			for (int x = xPos; x < xPos + width; x++)
			{
				if (m_InventoryList[x,y].currentObject != null && m_InventoryList[x,y].currentObject != item)
				{
					// Check if both objects are ammo of the same type
					if (ammoComponent != null)
					{
						InventoryAmmo tempAmmoComponent = m_InventoryList[x,y].currentObject.GetComponent<InventoryAmmo>();
						if (tempAmmoComponent != null)
						{
							if (ammoComponent.WeaponType == tempAmmoComponent.WeaponType)
							{
								// If the ammo pack has room for more ammo, move ammo into it
								int ammoNeeded = tempAmmoComponent.MaxCapacity - tempAmmoComponent.Amount;
								if (ammoNeeded != 0)
								{
									if (m_AllowPartialAmmoCombining || ammoNeeded >= ammoComponent.Amount)
									{
										int amountTaken = ammoComponent.TakeAmmo(ammoNeeded);
										tempAmmoComponent.AddAmmo(amountTaken);
										ammoTaken = true;
										
										// Ammo was taken and the ammo is now empty, so destroy it and return
										if (ammoComponent.Amount == 0)
										{
											//ammoComponent.BaseItem.FirstSpace.inventory.DestroyItem(ammoComponent.BaseItem);
											Destroy(ammoComponent.BaseItem.gameObject);
											if (m_UpdateTotalSellPrice)
											{
												m_TotalSellPrice = GetTotalSellValue();
											}
											return PlacementResult.AmmoDestroyed;
										}
									}
									else
									{
										spaceFree = false;
									}
								}
							}
						}
					}
					spaceFree = false;
					overlappingItems.Add(m_InventoryList[x,y].currentObject);
				}
			}
		}
		
		// Ammo was taken, but there is still ammo remaining, so it continues to be carried
		if (ammoTaken)
		{
			if (m_UpdateTotalSellPrice)
			{
				m_TotalSellPrice = GetTotalSellValue();
			}
			return PlacementResult.AmmoTaken;
		}
		
		// If the space isn't free, the item can't be placed here, return false
		//InventoryItem swapItem = null;
		if (spaceFree == false)
		{
			if (overlappingItems.Count > 1 || !allowSwapping)
			{
				item.IsCarried = false;
				return PlacementResult.Failed;
			}
			else
			{
				HashSet<InventoryItem>.Enumerator enumerator = overlappingItems.GetEnumerator();
				enumerator.MoveNext();
				swapItem = enumerator.Current;
				swapItem.FirstSpace.inventory.RemoveItem(swapItem);
			}
		}

		// If there is space free, remove the item from its current inventory position if it has one
		if (item.FirstSpace != null)
		{
			item.FirstSpace.inventory.RemoveItem(item);
		}
		
		// Then set the currentObject for each space in this inventory to the item
		item.FirstSpace = m_InventoryList[xPos,yPos];
		for (int y = yPos; y < yPos + height; y++)
		{
			for (int x = xPos; x < xPos + width; x++)
			{
				m_InventoryList[x,y].currentObject = item;
			}
		}
		
		// Set the new position and readjust for the origin point of the object
		newPos = new Vector3((float)xPos + transform.position.x, (float)-yPos + transform.position.y, newPos.z);// + 0.5f);
		if (rotation == 1)
		{
			newPos.x += (float)width;
		}
		else if (rotation == 2)
		{
			newPos.x += (float)width;
			newPos.y -= (float)height;
		}
		else if (rotation == 3)
		{
			newPos.y -= (float)height;
		}
		item.PlaceItem(newPos);

		// If in the quick sell menu, update the total sell value
		item.IsCarried = false;
		if (m_UpdateTotalSellPrice)
		{
			m_TotalSellPrice = GetTotalSellValue();
		}

		if (CompareTag(Tags.MAININVENTORY))
		{
			InventoryArmour armour = item.GetComponent<InventoryArmour>();
			if (armour != null)
			{
				GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<CharacterScript>().ModifyArmour(armour.ArmourModifier);
			}
		}

		CalculateFreeCells();

		m_ItemCount++;

		if (swapItem == null)
		{
			return PlacementResult.Success;
		}
		else
		{
			item = swapItem;
			swapItem.IsCarried = true;
			return PlacementResult.ItemSwapped;
		}
	}
	
	public void RemoveItem(InventoryItem item)
	{
		// Get the item position relative to this inventory
		// and divide it by spacing to get the cell position
		//Vector3 pos = item.transform.position;
		//int rotation = item.Rotation;
		Vector3 pos = item.PreviousPosition;
		int rotation = item.PreviousRotation;
		pos -= transform.position;
		pos /= m_Spacing;
		
		// Get width/height of object based on rotation
		int width = rotation == 0 || rotation == 2 ? item.Width : item.Height;
		int height = rotation == 0 || rotation == 2 ? item.Height : item.Width;
		width = rotation == 1 || rotation == 2 ? -width : width;
		height = rotation == 3 || rotation == 2 ? -height : height;
		
		// Get position of the top left corner of the item
		Vector3 topLeftPos = pos;
		if (width < 0)
		{
			topLeftPos.x += (float)width;
			width = -width;
		}
		if (height < 0)
		{
			topLeftPos.y -= (float)height;
			height = -height;
		}
		
		// Work out the actual cell position of the item
		Vector3 newPos = topLeftPos;//transform.position + topLeftPos;
		int xPos = (int)newPos.x;
		int yPos = (int)-newPos.y;
		
		for (int y = yPos; y < yPos + height; y++)
		{
			for (int x = xPos; x < xPos + width; x++)
			{
				m_InventoryList[x,y].currentObject = null;
			}
		}
		
		item.FirstSpace = null;

		if (m_UpdateTotalSellPrice)
		{
			m_TotalSellPrice = GetTotalSellValue();
		}

		if (CompareTag(Tags.MAININVENTORY))
		{
			InventoryArmour armour = item.GetComponent<InventoryArmour>();
			if (armour != null)
			{
				GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<CharacterScript>().ModifyArmour(1.0f / armour.ArmourModifier);
			}
		}

		m_ItemCount--;
	}

	public int GetNumberOfItems()
	{
		HashSet<InventoryItem> itemList = FindAllItemsWithComponent<InventoryItem>();
		return itemList.Count;
		//return m_ItemCount;
	}
	
	public T FindItemWithComponent<T>(bool allowSubtypes = false) where T : UnityEngine.Component
	{
		// Search through all inventory spaces
		for (int y = 0; y < m_Height; y++)
		{
			for (int x = 0; x < m_Width; x++)
			{
				// If the space has an item in it and the item has the correct component
				if (m_InventoryList[x,y].currentObject != null)
				{
					T component = m_InventoryList[x,y].currentObject.GetComponent<T>();
					if (component != null)
					{
						if (allowSubtypes || component.GetType() == typeof(T))
						{
							return component;
						}
					}
				}
			}
		}
		
		return null;
	}
	
	public HashSet<T> FindAllItemsWithComponent<T>(bool allowSubtypes = false, bool includeGhostObjects = false) where T : UnityEngine.Component
	{
		HashSet<T> itemList = new HashSet<T>();
		
		// Search through all inventory spaces
		for (int y = 0; y < m_Height; y++)
		{
			for (int x = 0; x < m_Width; x++)
			{
				// If the space has an item in it and the item has the correct component
				if (m_InventoryList[x,y].currentObject != null)
				{
					if (!m_InventoryList[x,y].currentObject.IsCarried || includeGhostObjects)
					{
						T component = m_InventoryList[x,y].currentObject.GetComponent<T>();
						if (component != null)
						{
							if (allowSubtypes || component.GetType() == typeof(T))
							{
								itemList.Add(component);
							}
						}
					}
				}
			}
		}
		
		return itemList;
	}

	public bool AreItemsOfTypeBoxed<T>()
	{
		// Bounds of rectangle containing all items of this type
		Vector2 topLeft = new Vector2(99.0f, 99.0f);
		Vector2 bottomRight = new Vector2(-1.0f, -1.0f);

		// Search through all inventory spaces
		for (int y = 0; y < m_Height; y++)
		{
			for (int x = 0; x < m_Width; x++)
			{
				// If the space has an item in it and the item has the correct component
				if (m_InventoryList[x,y].currentObject != null)
				{
					if (!m_InventoryList[x,y].currentObject.IsCarried)
					{
						T component = m_InventoryList[x,y].currentObject.GetComponent<T>();
						if (component != null)
						{
							if (component.GetType() == typeof(T))
							{
								// If the location of this cell is outside the bounds of the current rect, expand it
								if ((float)x < topLeft.x)
								{
									topLeft.x = (float)x;
								}

								if ((float)x > bottomRight.x)
								{
									bottomRight.x = (float)x;
								}

								if ((float)y < topLeft.y)
								{
									topLeft.y = (float)y;
								}
								
								if ((float)y > bottomRight.y)
								{
									bottomRight.y = (float)y;
								}
							}
						}
					}
				}
			}
		}

		// If the rect is less than 2 cells in either direction give no bonus
		Vector2 size = bottomRight - topLeft;
		if (size.x < 1.0f || size.y < 1.0f)
		{
			return false;
		}

		// Now search through the rect we just created and see if there are 
		// any cells without the correct item type inside it
		for (int y = (int)topLeft.y; y <= (int)bottomRight.y; y++)
		{
			for (int x = (int)topLeft.x; x <= (int)bottomRight.x; x++)
			{
				// If the space has an item in it and the item has the correct component
				if (m_InventoryList[x,y].currentObject != null)
				{
					if (!m_InventoryList[x,y].currentObject.IsCarried)
					{
						T component = m_InventoryList[x,y].currentObject.GetComponent<T>();
						if (component == null)
						{
							// Item is not the correct type, items are not boxed
							return false;
						}
						else
						{
							if (component.GetType() != typeof(T))
							{
								return false;
							}
						}
					}
				}
				else
				{
					// If a space is empty items are not boxed
					return false;
				}
			}
		}

		// If we didn't find any wrong items on the second pass the items are boxed
		return true;
	}

	public HashSet<InventorySpace> FindAllEmptyCells()
	{
		HashSet<InventorySpace> spaceList = new HashSet<InventorySpace>();

		// Search through all inventory spaces
		for (int y = 0; y < m_Height; y++)
		{
			for (int x = 0; x < m_Width; x++)
			{
				// If the space has no item in it, add to the list
				if (m_InventoryList[x,y].currentObject == null)
				{
					spaceList.Add(m_InventoryList[x,y]);
				}
			}
		}

		return spaceList;
	}

	public HashSet<InventoryItem> FindItemsWithPrefabID(int id)
	{
		HashSet<InventoryItem> itemList = new HashSet<InventoryItem>();
		
		// Search through all inventory spaces
		for (int y = 0; y < m_Height; y++)
		{
			for (int x = 0; x < m_Width; x++)
			{
				// If the space has an item in it and the item has the correct component
				if (m_InventoryList[x,y].currentObject != null)
				{
					InventoryItem component = m_InventoryList[x,y].currentObject.GetComponent<InventoryItem>();
					if (component != null)
					{
						if (component.PrefabID == id)
						{
							itemList.Add(component);
						}
					}
				}
			}
		}
		
		return itemList;
	}
	
	public HashSet<InventoryAmmo> FindAmmo(WeaponType weaponType)
	{
		HashSet<InventoryAmmo> itemList = new HashSet<InventoryAmmo>();
		
		// Search through all inventory spaces to find the correct ammo
		for (int y = 0; y < m_Height; y++)
		{
			for (int x = 0; x < m_Width; x++)
			{
				// If the space has an item in it
				if (m_InventoryList[x,y].currentObject != null)
				{
					// And that item has an Ammo component and is of the right ammo type
					InventoryAmmo ammoComponent = m_InventoryList[x,y].currentObject.GetComponent<InventoryAmmo>();
					if (ammoComponent != null)
					{
						if (ammoComponent.WeaponType == weaponType)
						{
							itemList.Add(ammoComponent);
						}
					}
				}
			}
		}
		
		return itemList;
	}
	
	public HashSet<InventoryGrenade> FindGrenades(GrenadeType grenadeType)
	{
		HashSet<InventoryGrenade> itemList = new HashSet<InventoryGrenade>();
		
		// Search through all inventory spaces to find the correct grenades
		for (int y = 0; y < m_Height; y++)
		{
			for (int x = 0; x < m_Width; x++)
			{
				// If the space has an item in it
				if (m_InventoryList[x,y].currentObject != null)
				{
					// And that item has a Grenade component and is of the right grenade type
					InventoryGrenade grenadeComponent = m_InventoryList[x,y].currentObject.GetComponent<InventoryGrenade>();
					if (grenadeComponent != null)
					{
						if (grenadeComponent.GrenadeType == grenadeType)
						{
							itemList.Add(grenadeComponent);
						}
					}
				}
			}
		}
		
		return itemList;
	}

	public HashSet<InventoryTreasure> FindTreasure(InventoryTreasure.TreasureMaterial materialType)
	{
		HashSet<InventoryTreasure> itemList = new HashSet<InventoryTreasure>();
		
		// Search through all inventory spaces to find the correct treasure
		for (int y = 0; y < m_Height; y++)
		{
			for (int x = 0; x < m_Width; x++)
			{
				// If the space has an item in it
				if (m_InventoryList[x,y].currentObject != null)
				{
					// And that item has a Treasure component and is of the right material type
					InventoryTreasure treasureComponent = m_InventoryList[x,y].currentObject.GetComponent<InventoryTreasure>();
					if (treasureComponent != null)
					{
						if (treasureComponent.MaterialType == materialType)
						{
							itemList.Add(treasureComponent);
						}
					}
				}
			}
		}
		
		return itemList;
	}
	
	public int TakeAmmo(WeaponType weaponType, int ammoToTake, out Vector3 position)
	{		
		int ammoTaken = 0;
		int ammoLeftToTake = ammoToTake;
		HashSet<InventoryAmmo> ammoList = FindAmmo(weaponType);
		InventoryAmmo lowestAmmo = null;
		position = transform.position;
		
		// Until we have all the ammo we need, or the list is empty
		while (ammoLeftToTake != 0 && ammoList.Count != 0)
		{
			// Go through the list of ammo checking for the one with the least ammo
			foreach (InventoryAmmo ammo in ammoList)
			{
				if (lowestAmmo != null)
				{
					if (ammo.Amount < lowestAmmo.Amount)
					{
						lowestAmmo = ammo;
					}
				}
				else
				{
					lowestAmmo = ammo;
				}
			}
			
			// Take the necessary amount of ammo
			int amountTaken = lowestAmmo.TakeAmmo(ammoLeftToTake);
			ammoTaken += amountTaken;
			ammoLeftToTake -= amountTaken;
			position = lowestAmmo.BaseItem.CentrePosition;
			
			// If the ammo pack is empty, destroy it
			if (lowestAmmo.Amount == 0)
			{
				ammoList.Remove(lowestAmmo);
				DestroyItem(lowestAmmo.BaseItem);
				//RemoveItem(lowestAmmo.BaseItem);
				//Destroy(lowestAmmo.gameObject);
				lowestAmmo = null;
			}
		}
		
		return ammoTaken;
	}
	
	public void DestroyItem(InventoryItem item)
	{
		RemoveItem(item);
		Destroy(item.gameObject);
	}
	
	public int GetCurrentScore(List<HashSet<InventoryItem>> itemScoringList, List<HashSet<InventoryItem>> rotationScoringList, bool chaosMode)
	{
		int currentScore = 0;
		
		// Check if all ammo packs are adjacent to each other
		HashSet<InventoryAmmo> allAmmo = FindAllItemsWithComponent<InventoryAmmo>();
		if (allAmmo.Count > 1)
		{
			int adjacentAmmoCells = 0;
			int targetRotation = -1;
			bool rotationEqual = false;
			HashSet<InventoryItem> adjacentItemList = new HashSet<InventoryItem>();
			foreach (InventoryAmmo ammo in allAmmo)
			{
				// Get amount of adjacent ammo cells of same ammo type
				adjacentAmmoCells += GetNumAdjacentSameTypeCells(ammo.BaseItem, adjacentItemList);
				
				// Check if all ammo is rotated the same way
				if (targetRotation == -1)
				{
					targetRotation = ammo.BaseItem.Rotation;
					rotationEqual = true;
				}
				else
				{
					if (ammo.BaseItem.Rotation != targetRotation)
					{
						rotationEqual = false;
					}
				}
			}
			if (adjacentItemList.Count > 0)
			{
				itemScoringList.Add(adjacentItemList);
			}

			if (allAmmo.Count > MINIMUM_AMOUNT_FOR_ADJACENT_BONUS)
			{
				HashSet<InventoryAmmo>.Enumerator ammoEnumerator = allAmmo.GetEnumerator();
				ammoEnumerator.MoveNext();
				Vector2 firstAmmoPosition = GetCellPositionOfItem(ammoEnumerator.Current.BaseItem);
				if (firstAmmoPosition.x > -1.0f && firstAmmoPosition.y > -1.0f)
				{
					HashSet<InventoryAmmo> adjacentAmmo = CheckAdjacentItems<InventoryAmmo>(firstAmmoPosition);
					
					if (adjacentAmmo.SetEquals(allAmmo))
					{
						currentScore += SCORE_FULL_ADJACENCY_BONUS;
					}
				}
			}
			
			// Add relevant values to score
			currentScore += SCORE_TYPE_ADJACENCY_BONUS * adjacentAmmoCells;
			if (rotationEqual && allAmmo.Count > MINIMUM_AMOUNT_FOR_ROTATION_BONUS)
			{
				currentScore += SCORE_FULL_TYPE_ROTATION_BONUS;
			}
		}
		
		// Check if all ammo packs of each individual type are adjacent
		List<bool> ammoTypesFullyAdjacent = new List<bool>();
		HashSet<InventoryItem> rotationAmmoList = new HashSet<InventoryItem>();
		for (int i = 0; i < InventoryWeapon.NUM_WEAPON_TYPES - 1; i++)
		{
			bool fullyAdjacent = false;
			HashSet<InventoryAmmo> allAmmoType = FindAmmo((WeaponType)i);
			if (allAmmoType.Count > MINIMUM_AMOUNT_FOR_ADJACENT_BONUS)
			{
				HashSet<InventoryAmmo>.Enumerator ammoEnumerator = allAmmoType.GetEnumerator();
				ammoEnumerator.MoveNext();
				Vector2 firstAmmoPosition = GetCellPositionOfItem(ammoEnumerator.Current.BaseItem);
				if (firstAmmoPosition.x > -1.0f && firstAmmoPosition.y > -1.0f)
				{
					HashSet<InventoryAmmo> adjacentAmmo = CheckAdjacentAmmo(firstAmmoPosition, (WeaponType)i);
					
					if (adjacentAmmo.SetEquals(allAmmoType))
					{
						currentScore += SCORE_FULL_TYPE_ADJACENCY_BONUS;
						fullyAdjacent = true;
					}
				}
			}

			if (allAmmoType.Count > MINIMUM_AMOUNT_FOR_ROTATION_BONUS)
			{
				int targetRotation = -1;
				bool rotationEqual = false;
				foreach (InventoryAmmo ammo in allAmmoType)
				{					
					// Check if all ammo of this type is rotated the same way
					if (targetRotation == -1)
					{
						targetRotation = ammo.BaseItem.Rotation;
						rotationEqual = true;
					}
					else
					{
						if (ammo.BaseItem.Rotation != targetRotation)
						{
							rotationEqual = false;
						}
					}
				}
				
				// If rotation is all equal add bonus to score
				if (rotationEqual)
				{
					currentScore += SCORE_TYPE_ROTATION_BONUS;

					foreach (InventoryAmmo ammo in allAmmoType)
					{
						rotationAmmoList.Add(ammo.BaseItem);
					}
				}
			}
			
			ammoTypesFullyAdjacent.Add(fullyAdjacent);
		}
		if (rotationAmmoList.Count > 0)
		{
			rotationScoringList.Add(rotationAmmoList);
		}

		// Check if all treasure is adjacent to each other
		HashSet<InventoryTreasure> allTreasure = FindAllItemsWithComponent<InventoryTreasure>();
		if (allTreasure.Count > 0)
		{
			int adjacentTreasureCells = 0;
			int targetRotation = -1;
			bool rotationEqual = true;
			HashSet<InventoryItem> adjacentItemList = new HashSet<InventoryItem>();
			foreach (InventoryTreasure treasure in allTreasure)
			{
				// Treasure gives a set bonus to score for each item
				// If in chaos mode score is reduced to increase overall score
				/*if (chaosMode)
				{
					currentScore -= treasure.ScoreValue;
				}
				else
				{
					currentScore += treasure.ScoreValue;
				}*/
				
				// Get amount of adjacent treasure cells of same material type
				adjacentTreasureCells += GetNumAdjacentSameTypeCells(treasure.BaseItem, adjacentItemList);

				// Ignore rotation for square items
				if (treasure.BaseItem.Width != treasure.BaseItem.Height)
				{
					// Check if all treasure is rotated the same way
					if (targetRotation == -1)
					{
						targetRotation = treasure.BaseItem.Rotation;
						//rotationEqual = true;
					}
					else
					{
						if (treasure.BaseItem.Rotation != targetRotation)
						{
							rotationEqual = false;
						}
					}
				}
			}
			if (adjacentItemList.Count > 0)
			{
				itemScoringList.Add(adjacentItemList);
			}

			if (allTreasure.Count > MINIMUM_AMOUNT_FOR_ADJACENT_BONUS)
			{
				HashSet<InventoryTreasure>.Enumerator treasureEnumerator = allTreasure.GetEnumerator();
				treasureEnumerator.MoveNext();
				Vector2 firstTreasurePosition = GetCellPositionOfItem(treasureEnumerator.Current.BaseItem);
				if (firstTreasurePosition.x > -1.0f && firstTreasurePosition.y > -1.0f)
				{
					HashSet<InventoryTreasure> adjacentTreasure = CheckAdjacentItems<InventoryTreasure>(firstTreasurePosition);
					
					if (adjacentTreasure.SetEquals(allTreasure))
					{
						currentScore += SCORE_FULL_ADJACENCY_BONUS;
					}
				}
			}
			
			// Add relevant values to score
			currentScore += SCORE_TYPE_ADJACENCY_BONUS * adjacentTreasureCells;
			if (rotationEqual && allTreasure.Count > MINIMUM_AMOUNT_FOR_ROTATION_BONUS)
			{
				currentScore += SCORE_FULL_TYPE_ROTATION_BONUS;
			}
		}

		// Check if all treasure of each individual type is adjacent
		int enumLength = Enum.GetNames(typeof(InventoryTreasure.TreasureMaterial)).Length;
		HashSet<InventoryItem> rotationTreasureList = new HashSet<InventoryItem>();
		for (int i = 0; i < enumLength; i++)
		{
			HashSet<InventoryTreasure> allTreasureType = FindTreasure((InventoryTreasure.TreasureMaterial)i);
			if (allTreasureType.Count > MINIMUM_AMOUNT_FOR_ADJACENT_BONUS)
			{
				HashSet<InventoryTreasure>.Enumerator treasureEnumerator = allTreasureType.GetEnumerator();
				treasureEnumerator.MoveNext();
				Vector2 firstTreasurePosition = GetCellPositionOfItem(treasureEnumerator.Current.BaseItem);
				if (firstTreasurePosition.x > -1.0f && firstTreasurePosition.y > -1.0f)
				{
					HashSet<InventoryTreasure> adjacentTreasure = CheckAdjacentTreasure(firstTreasurePosition, (InventoryTreasure.TreasureMaterial)i);
					
					if (adjacentTreasure.SetEquals(allTreasureType))
					{
						currentScore += SCORE_FULL_TYPE_ADJACENCY_BONUS;
					}
				}
			}

			if (allTreasureType.Count > MINIMUM_AMOUNT_FOR_ROTATION_BONUS)
			{
				int targetRotation = -1;
				bool rotationEqual = true;
				foreach (InventoryTreasure treasure in allTreasureType)
				{
					// Ignore rotation for square items
					if (treasure.BaseItem.Width != treasure.BaseItem.Height)
					{
						// Check if all treasure of this type is rotated the same way
						if (targetRotation == -1)
						{
							targetRotation = treasure.BaseItem.Rotation;
							//rotationEqual = true;
						}
						else
						{
							if (treasure.BaseItem.Rotation != targetRotation)
							{
								rotationEqual = false;
							}
						}
					}
				}
				
				// If rotation is all equal add bonus to score
				if (rotationEqual)
				{
					currentScore += SCORE_TYPE_ROTATION_BONUS;

					foreach (InventoryTreasure treasure in allTreasureType)
					{
						rotationTreasureList.Add(treasure.BaseItem);
					}
				}
			}
		}
		if (rotationTreasureList.Count > 0)
		{
			rotationScoringList.Add(rotationTreasureList);
		}
		
		// Check if all health items are adjacent to each other
		HashSet<InventoryHealth> allHealth = FindAllItemsWithComponent<InventoryHealth>();
		HashSet<InventoryItem> rotationHealthList = new HashSet<InventoryItem>();
		if (allHealth.Count > 1)
		{
			int adjacentHealthCells = 0;
			int targetRotation = -1;
			bool rotationEqual = true;
			HashSet<InventoryItem> adjacentItemList = new HashSet<InventoryItem>();
			foreach (InventoryHealth health in allHealth)
			{
				// Get amount of adjacent health cells of same healing amount
				adjacentHealthCells += GetNumAdjacentSameTypeCells(health.BaseItem, adjacentItemList);

				// Ignore rotation for square items
				if (health.BaseItem.Width != health.BaseItem.Height)
				{
					// Check if all health is rotated the same way
					if (targetRotation == -1)
					{
						targetRotation = health.BaseItem.Rotation;
						//rotationEqual = true;
					}
					else
					{
						if (health.BaseItem.Rotation != targetRotation)
						{
							rotationEqual = false;
						}
					}
				}
			}
			if (adjacentItemList.Count > 0)
			{
				itemScoringList.Add(adjacentItemList);
			}

			if (allHealth.Count > MINIMUM_AMOUNT_FOR_ADJACENT_BONUS)
			{
				HashSet<InventoryHealth>.Enumerator healthEnumerator = allHealth.GetEnumerator();
				healthEnumerator.MoveNext();
				Vector2 firstHealthPosition = GetCellPositionOfItem(healthEnumerator.Current.BaseItem);
				if (firstHealthPosition.x > -1.0f && firstHealthPosition.y > -1.0f)
				{
					HashSet<InventoryHealth> adjacentHealth = CheckAdjacentItems<InventoryHealth>(firstHealthPosition);
					
					if (adjacentHealth.SetEquals(allHealth))
					{
						currentScore += SCORE_FULL_ADJACENCY_BONUS;
					}
				}
			}
			
			// Add relevant values to score
			currentScore += SCORE_TYPE_ADJACENCY_BONUS * adjacentHealthCells;
			if (rotationEqual && allHealth.Count > MINIMUM_AMOUNT_FOR_ROTATION_BONUS)
			{
				currentScore += SCORE_FULL_TYPE_ROTATION_BONUS;

				foreach (InventoryHealth health in allHealth)
				{
					rotationHealthList.Add(health.BaseItem);
				}
			}
		}
		if (rotationHealthList.Count > 0)
		{
			rotationScoringList.Add(rotationHealthList);
		}
		
		// Check if all grenades are adjacent to each other
		HashSet<InventoryGrenade> allGrenades = FindAllItemsWithComponent<InventoryGrenade>();
		if (allGrenades.Count > 1)
		{
			int adjacentGrenadeCells = 0;
			int targetRotation = -1;
			bool rotationEqual = false;
			HashSet<InventoryItem> adjacentItemList = new HashSet<InventoryItem>();
			foreach (InventoryGrenade grenade in allGrenades)
			{
				// Get amount of adjacent grenade cells of same grenade type
				adjacentGrenadeCells += GetNumAdjacentSameTypeCells(grenade.BaseItem, adjacentItemList);
				
				// Check if all grenades are rotated the same way
				if (targetRotation == -1)
				{
					targetRotation = grenade.BaseItem.Rotation;
					rotationEqual = true;
				}
				else
				{
					if (grenade.BaseItem.Rotation != targetRotation)
					{
						rotationEqual = false;
					}
				}
			}
			if (adjacentItemList.Count > 0)
			{
				itemScoringList.Add(adjacentItemList);
			}

			if (allGrenades.Count > MINIMUM_AMOUNT_FOR_ADJACENT_BONUS)
			{
				HashSet<InventoryGrenade>.Enumerator grenadeEnumerator = allGrenades.GetEnumerator();
				grenadeEnumerator.MoveNext();
				Vector2 firstGrenadePosition = GetCellPositionOfItem(grenadeEnumerator.Current.BaseItem);
				if (firstGrenadePosition.x > -1.0f && firstGrenadePosition.y > -1.0f)
				{
					HashSet<InventoryGrenade> adjacentGrenades = CheckAdjacentItems<InventoryGrenade>(firstGrenadePosition);
					
					if (adjacentGrenades.SetEquals(allGrenades))
					{
						currentScore += SCORE_FULL_ADJACENCY_BONUS;
					}
				}
			}
			
			// Add relevant values to score
			currentScore += SCORE_TYPE_ADJACENCY_BONUS * adjacentGrenadeCells;
			if (rotationEqual && allGrenades.Count > MINIMUM_AMOUNT_FOR_ROTATION_BONUS)
			{
				currentScore += SCORE_FULL_TYPE_ROTATION_BONUS;
			}
		}
		
		// Check if all grenades of each individual type are adjacent
		HashSet<InventoryItem> rotationGrenadeList = new HashSet<InventoryItem>();
		for (int i = 0; i < InventoryGrenade.NUM_GRENADE_TYPES; i++)
		{
			HashSet<InventoryGrenade> allGrenadeType = FindGrenades((GrenadeType)i);
			if (allGrenadeType.Count > MINIMUM_AMOUNT_FOR_ADJACENT_BONUS)
			{
				HashSet<InventoryGrenade>.Enumerator grenadeEnumerator = allGrenadeType.GetEnumerator();
				grenadeEnumerator.MoveNext();
				Vector2 firstGrenadePosition = GetCellPositionOfItem(grenadeEnumerator.Current.BaseItem);
				if (firstGrenadePosition.x > -1.0f && firstGrenadePosition.y > -1.0f)
				{
					HashSet<InventoryGrenade> adjacentGrenades = CheckAdjacentGrenades(firstGrenadePosition, (GrenadeType)i);
					
					if (adjacentGrenades.SetEquals(allGrenadeType))
					{
						currentScore += SCORE_FULL_TYPE_ADJACENCY_BONUS;
					}
				}
			}

			if (allGrenadeType.Count > MINIMUM_AMOUNT_FOR_ROTATION_BONUS)
			{
				int targetRotation = -1;
				bool rotationEqual = false;
				foreach (InventoryGrenade grenade in allGrenadeType)
				{					
					// Check if all grenades of this type are rotated the same way
					if (targetRotation == -1)
					{
						targetRotation = grenade.BaseItem.Rotation;
						rotationEqual = true;
					}
					else
					{
						if (grenade.BaseItem.Rotation != targetRotation)
						{
							rotationEqual = false;
						}
					}
				}
				
				// If rotation is all equal add bonus to score
				if (rotationEqual)
				{
					currentScore += SCORE_TYPE_ROTATION_BONUS;

					foreach (InventoryGrenade grenade in allGrenadeType)
					{
						rotationGrenadeList.Add(grenade.BaseItem);
					}
				}
			}
		}
		if (rotationGrenadeList.Count > 0)
		{
			rotationScoringList.Add(rotationGrenadeList);
		}

		// Check if all armour is adjacent to each other
		HashSet<InventoryArmour> allArmour = FindAllItemsWithComponent<InventoryArmour>();
		HashSet<InventoryItem> rotationArmourList = new HashSet<InventoryItem>();
		if (allArmour.Count > 1)
		{
			int adjacentArmourCells = 0;
			int targetRotation = -1;
			bool rotationEqual = false;
			HashSet<InventoryItem> adjacentItemList = new HashSet<InventoryItem>();
			foreach (InventoryArmour armour in allArmour)
			{
				// Get amount of adjacent armour cells
				adjacentArmourCells += GetNumAdjacentSameTypeCells(armour.BaseItem, adjacentItemList);
				
				// Check if all armour is rotated the same way
				if (targetRotation == -1)
				{
					targetRotation = armour.BaseItem.Rotation;
					rotationEqual = true;
				}
				else
				{
					if (armour.BaseItem.Rotation != targetRotation)
					{
						rotationEqual = false;
					}
				}
			}
			if (adjacentItemList.Count > 0)
			{
				itemScoringList.Add(adjacentItemList);
			}

			if (allArmour.Count >= MINIMUM_AMOUNT_FOR_ADJACENT_BONUS)
			{
				HashSet<InventoryArmour>.Enumerator armourEnumerator = allArmour.GetEnumerator();
				armourEnumerator.MoveNext();
				Vector2 firstArmourPosition = GetCellPositionOfItem(armourEnumerator.Current.BaseItem);
				if (firstArmourPosition.x > -1.0f && firstArmourPosition.y > -1.0f)
				{
					HashSet<InventoryArmour> adjacentArmour = CheckAdjacentItems<InventoryArmour>(firstArmourPosition);
					
					if (adjacentArmour.SetEquals(allArmour))
					{
						currentScore += SCORE_FULL_ADJACENCY_BONUS;
					}
				}
			}
			
			// Add relevant values to score
			currentScore += SCORE_TYPE_ADJACENCY_BONUS * adjacentArmourCells;
			if (rotationEqual && allArmour.Count > MINIMUM_AMOUNT_FOR_ROTATION_BONUS)
			{
				currentScore += SCORE_FULL_TYPE_ROTATION_BONUS;

				foreach (InventoryArmour armour in allArmour)
				{
					rotationArmourList.Add(armour.BaseItem);
				}
			}
		}
		if (rotationArmourList.Count > 0)
		{
			rotationScoringList.Add(rotationArmourList);
		}

		// Check if all empty inventory spaces are adjacent
		HashSet<InventorySpace> allEmpty = FindAllEmptyCells();
		if (allEmpty.Count > 1)
		{
			HashSet<InventorySpace> adjacentEmpty = CheckEmptyAdjacency();

			// If list of all empty cells matches all adjacent cells, add score based on number of empty
			if (adjacentEmpty.SetEquals(allEmpty))
			{
				currentScore += allEmpty.Count < 6 ? allEmpty.Count * SCORE_TYPE_ADJACENCY_BONUS : 90;
			}
		}
		
		// Check if all weapons are adjacent to each other
		HashSet<InventoryWeapon> allWeapons = FindAllItemsWithComponent<InventoryWeapon>();
		if (allWeapons.Count > 1)
		{
			HashSet<InventoryWeapon>.Enumerator weaponEnumerator = allWeapons.GetEnumerator();
			weaponEnumerator.MoveNext();
			Vector2 firstWeaponPosition = GetCellPositionOfItem(weaponEnumerator.Current.BaseItem);
			if (firstWeaponPosition.x > -1.0f && firstWeaponPosition.y > -1.0f)
			{
				HashSet<InventoryWeapon> adjacentWeapons = CheckAdjacentItems<InventoryWeapon>(firstWeaponPosition);
				
				if (adjacentWeapons.SetEquals(allWeapons))
				{
					currentScore += SCORE_FULL_ADJACENCY_BONUS;
				}
			}
		}
		
		// Get amount of adjacent weapon cells and ammo of same type
		int adjacentWeaponCells = 0;
		int targetWeaponRotation = -1;
		bool weaponRotationEqual = false;
		HashSet<InventoryItem> adjacentWeaponList = new HashSet<InventoryItem>();
		HashSet<InventoryItem> rotationWeaponList = new HashSet<InventoryItem>();
		foreach (InventoryWeapon weapon in allWeapons)
		{
			adjacentWeaponCells += GetNumAdjacentSameTypeCells(weapon.BaseItem, adjacentWeaponList);
			
			// Check if all ammo of the correct type is adjacent to weapon
			if (IsAmmoAdjacent(weapon))
			{
				if (ammoTypesFullyAdjacent[(int)weapon.WeaponType])
				{
					currentScore += SCORE_FULL_ADJACENCY_BONUS;
				}
			}
			
			// Check if all weapons are rotated the same way
			if (targetWeaponRotation == -1)
			{
				targetWeaponRotation = weapon.BaseItem.Rotation;
				weaponRotationEqual = true;
			}
			else
			{
				if (weapon.BaseItem.Rotation != targetWeaponRotation)
				{
					weaponRotationEqual = false;
				}
			}
		}
		if (adjacentWeaponList.Count > 0)
		{
			itemScoringList.Add(adjacentWeaponList);
		}
		
		// Add relevant values to score
		currentScore += SCORE_TYPE_ADJACENCY_BONUS * adjacentWeaponCells;
		if (weaponRotationEqual && allWeapons.Count > 1)
		{
			currentScore += SCORE_FULL_TYPE_ROTATION_BONUS;

			foreach (InventoryWeapon weapon in allWeapons)
			{
				rotationWeaponList.Add(weapon.BaseItem);
			}
		}
		if (rotationWeaponList.Count > 0)
		{
			rotationScoringList.Add(rotationWeaponList);
		}
		
		// Check if all items are rotated the same way
		/*HashSet<InventoryItem> allItems = FindAllItemsWithComponent<InventoryItem>();
		int targetTotalRotation = -1;
		bool totalRotationEqual = false;
		foreach (InventoryItem item in allItems)
		{
			if (targetTotalRotation == -1)
			{
				int width = item.Width;
				int height = item.Height;
				targetTotalRotation = item.Rotation;
				
				// Simplify rotation to horizontal/vertical
				if (targetTotalRotation == 2)
				{
					targetTotalRotation = 0;
				}
				else if (targetTotalRotation == 3)
				{
					targetTotalRotation = 1;
				}
				
				if (height > width)
				{
					targetTotalRotation++;
					if (targetTotalRotation > 1)
					{
						targetTotalRotation = 0;
					}
				}
				else if (height == width)
				{
					targetTotalRotation = -1;
				}
				
				totalRotationEqual = true;
			}
			else
			{
				int width = item.Width;
				int height = item.Height;
				int itemRotation = item.Rotation;
				
				// Simplify rotation to horizontal/vertical
				if (itemRotation == 2)
				{
					itemRotation = 0;
				}
				else if (itemRotation == 3)
				{
					itemRotation = 1;
				}
				
				if (height > width)
				{
					itemRotation++;
					if (itemRotation > 1)
					{
						itemRotation = 0;
					}
				}
				
				if (itemRotation != targetTotalRotation && height != width)
				{
					totalRotationEqual = false;
				}
			}
		}
		
		// If all items are rotated the same way add bonus to score
		if (totalRotationEqual && allItems.Count > MINIMUM_AMOUNT_FOR_ROTATION_BONUS)
		{
			currentScore += SCORE_FULL_ROTATION_BONUS;
		}*/

		// Check for rotation bonus. Gives extra points for each item rotated the same way
		HashSet<InventoryItem> allItems = FindAllItemsWithComponent<InventoryItem>();
		int[] horizontalRotations = new int[4];
		int[] verticalRotations = new int[4];

		foreach (InventoryItem item in allItems)
		{
			// Check if we have a square shaped treasure, and if so ignore it
			if (item.Width == item.Height)
			{
				InventoryTreasure treasure = item.GetComponent<InventoryTreasure>();
				if (treasure != null)
				{
					continue;
				}
			}

			// Separate rotation amounts for items which start horizontal/vertical into different lists
			if (item.Width > item.Height)
			{
				// Add to relevant rotation amount
				horizontalRotations[item.Rotation] += 1;
			}
			else
			{
				verticalRotations[item.Rotation] += 1;
			}
		}

		// Check which rotation has the most items for both horizontal/vertical
		int highestHRotation = 0;
		int highestVRotation = 0;
		for (int i = 0; i < 4; i++)
		{
			if (horizontalRotations[i] > highestHRotation)
			{
				highestHRotation = i;
			}

			if (verticalRotations[i] > highestVRotation)
			{
				highestVRotation = i;
			}
		}

		// Combine relevant horizontal/vertical rotation amounts to give a more 'fair' results
		if (horizontalRotations[highestHRotation] > verticalRotations[highestVRotation])
		{
			if (highestHRotation == 0 || highestHRotation == 2)
			{
				horizontalRotations[highestHRotation] += verticalRotations[1] + verticalRotations[3];
			}
			else
			{
				horizontalRotations[highestHRotation] += verticalRotations[0] + verticalRotations[2];
			}

			// Multiply highest shared rotation by score bonus and add to score
			currentScore += (SCORE_MINOR_ROTATION_BONUS * horizontalRotations[highestHRotation]);
		}
		else
		{
			if (highestVRotation == 0 || highestVRotation == 2)
			{
				verticalRotations[highestVRotation] += horizontalRotations[1] + horizontalRotations[3];
			}
			else
			{
				verticalRotations[highestVRotation] += horizontalRotations[0] + horizontalRotations[2];
			}

			currentScore += (SCORE_MINOR_ROTATION_BONUS * verticalRotations[highestVRotation]);
		}

		// Check if items are arranged into a box shape
		if (AreItemsOfTypeBoxed<InventoryWeapon>())
		{
			currentScore += SCORE_TYPE_BOXED_BONUS;
		}

		if (AreItemsOfTypeBoxed<InventoryAmmo>())
		{
			currentScore += SCORE_TYPE_BOXED_BONUS;
		}

		if (AreItemsOfTypeBoxed<InventoryGrenade>())
		{
			currentScore += SCORE_TYPE_BOXED_BONUS;
		}

		if (AreItemsOfTypeBoxed<InventoryHealth>())
		{
			currentScore += SCORE_TYPE_BOXED_BONUS;
		}

		if (AreItemsOfTypeBoxed<InventoryTreasure>())
		{
			currentScore += SCORE_TYPE_BOXED_BONUS;
		}
		
		return currentScore;
	}
	
	private HashSet<T> CheckAdjacentItems<T>(Vector2 cellPos) where T : UnityEngine.Component
	{
		// Add beginning cell to a list of neighbour cells
		HashSet<T> adjacentItemList = new HashSet<T>();
		List<Vector2> cellList = new List<Vector2>();
		
		// Double check the cell contains the item type we're looking for
		InventorySpace firstSpace = m_InventoryList[(int)cellPos.x, (int)cellPos.y];
		if (firstSpace.currentObject != null)
		{
			T itemComponent = firstSpace.currentObject.GetComponent<T>();
			if (itemComponent != null)
			{
				if (itemComponent.GetType() == typeof(T))
				{
					// Add the item to the adjacent list, as well as the cell to the neighbour list
					adjacentItemList.Add(itemComponent);
					cellList.Add(cellPos);
					firstSpace.cellChecked = true;
				}
				else
				{
					return adjacentItemList;
				}
			}
			else
			{
				return adjacentItemList;
			}
		}
		else
		{
			return adjacentItemList;
		}
		
		// While there are still unchecked cells in the list
		while (cellList.Count > 0)
		{			
			// Check space to the left
			InventorySpace tempSpace;
			if (cellList[0].x > 0)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x - 1, (int)cellList[0].y];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// And if the cell contains the item type we're looking for
						T itemComponent = tempSpace.currentObject.GetComponent<T>();
						if (itemComponent != null)
						{
							if (itemComponent.GetType() == typeof(T))
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentItemList.Add(itemComponent);
								cellList.Add(new Vector2(cellList[0].x - 1, cellList[0].y));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			// Check space above
			if (cellList[0].y > 0)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x, (int)cellList[0].y - 1];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// And if the cell contains the item type we're looking for
						T itemComponent = tempSpace.currentObject.GetComponent<T>();
						if (itemComponent != null)
						{
							if (itemComponent.GetType() == typeof(T))
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentItemList.Add(itemComponent);
								cellList.Add(new Vector2(cellList[0].x, cellList[0].y - 1));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			// Check space to the right
			if (cellList[0].x < m_Width - 1)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x + 1, (int)cellList[0].y];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// And if the cell contains the item type we're looking for
						T itemComponent = tempSpace.currentObject.GetComponent<T>();
						if (itemComponent != null)
						{
							if (itemComponent.GetType() == typeof(T))
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentItemList.Add(itemComponent);
								cellList.Add(new Vector2(cellList[0].x + 1, cellList[0].y));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			// Check space below
			if (cellList[0].y < m_Height - 1)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x, (int)cellList[0].y + 1];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// And if the cell contains the item type we're looking for
						T itemComponent = tempSpace.currentObject.GetComponent<T>();
						if (itemComponent != null)
						{
							if (itemComponent.GetType() == typeof(T))
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentItemList.Add(itemComponent);
								cellList.Add(new Vector2(cellList[0].x, cellList[0].y + 1));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			cellList.RemoveAt(0);
		}
		
		// Reset the checked value of all cells back to false
		ResetCells();
		return adjacentItemList;
	}
	
	private HashSet<InventoryAmmo> CheckAdjacentAmmo(Vector2 cellPos, WeaponType ammoType)
	{
		// Add beginning cell to a list of neighbour cells
		HashSet<InventoryAmmo> adjacentAmmoList = new HashSet<InventoryAmmo>();
		List<Vector2> cellList = new List<Vector2>();
		
		// Double check the cell contains the item type we're looking for
		InventorySpace firstSpace = m_InventoryList[(int)cellPos.x, (int)cellPos.y];
		if (firstSpace.currentObject != null)
		{
			InventoryAmmo ammoComponent = firstSpace.currentObject.GetComponent<InventoryAmmo>();
			if (ammoComponent != null)
			{
				if (ammoComponent.WeaponType == ammoType)
				{
					// Add the item to the adjacent list, as well as the cell to the neighbour list
					adjacentAmmoList.Add(ammoComponent);
					cellList.Add(cellPos);
					firstSpace.cellChecked = true;
				}
				else
				{
					return adjacentAmmoList;
				}
			}
			else
			{
				return adjacentAmmoList;
			}
		}
		else
		{
			return adjacentAmmoList;
		}
		
		// While there are still unchecked cells in the list
		while (cellList.Count > 0)
		{			
			// Check space to the left
			InventorySpace tempSpace;
			if (cellList[0].x > 0)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x - 1, (int)cellList[0].y];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains the ammo type we're looking for
						InventoryAmmo ammoComponent = tempSpace.currentObject.GetComponent<InventoryAmmo>();
						if (ammoComponent != null)
						{
							if (ammoComponent.WeaponType == ammoType)
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentAmmoList.Add(ammoComponent);
								cellList.Add(new Vector2(cellList[0].x - 1, cellList[0].y));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			// Check space above
			if (cellList[0].y > 0)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x, (int)cellList[0].y - 1];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains the ammo type we're looking for
						InventoryAmmo ammoComponent = tempSpace.currentObject.GetComponent<InventoryAmmo>();
						if (ammoComponent != null)
						{
							if (ammoComponent.WeaponType == ammoType)
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentAmmoList.Add(ammoComponent);
								cellList.Add(new Vector2(cellList[0].x, cellList[0].y - 1));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			// Check space to the right
			if (cellList[0].x < m_Width - 1)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x + 1, (int)cellList[0].y];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains the ammo type we're looking for
						InventoryAmmo ammoComponent = tempSpace.currentObject.GetComponent<InventoryAmmo>();
						if (ammoComponent != null)
						{
							if (ammoComponent.WeaponType == ammoType)
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentAmmoList.Add(ammoComponent);
								cellList.Add(new Vector2(cellList[0].x + 1, cellList[0].y));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			// Check space below
			if (cellList[0].y < m_Height - 1)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x, (int)cellList[0].y + 1];	
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains the ammo type we're looking for
						InventoryAmmo ammoComponent = tempSpace.currentObject.GetComponent<InventoryAmmo>();
						if (ammoComponent != null)
						{
							if (ammoComponent.WeaponType == ammoType)
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentAmmoList.Add(ammoComponent);
								cellList.Add(new Vector2(cellList[0].x, cellList[0].y + 1));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			cellList.RemoveAt(0);
		}
		
		// Reset the checked value of all cells back to false
		ResetCells();
		return adjacentAmmoList;
	}
	
	private HashSet<InventoryGrenade> CheckAdjacentGrenades(Vector2 cellPos, GrenadeType grenadeType)
	{
		// Add beginning cell to a list of neighbour cells
		HashSet<InventoryGrenade> adjacentGrenadeList = new HashSet<InventoryGrenade>();
		List<Vector2> cellList = new List<Vector2>();
		
		// Double check the cell contains the item type we're looking for
		InventorySpace firstSpace = m_InventoryList[(int)cellPos.x, (int)cellPos.y];
		if (firstSpace.currentObject != null)
		{
			InventoryGrenade grenadeComponent = firstSpace.currentObject.GetComponent<InventoryGrenade>();
			if (grenadeComponent != null)
			{
				if (grenadeComponent.GrenadeType == grenadeType)
				{
					// Add the item to the adjacent list, as well as the cell to the neighbour list
					adjacentGrenadeList.Add(grenadeComponent);
					cellList.Add(cellPos);
					firstSpace.cellChecked = true;
				}
				else
				{
					return adjacentGrenadeList;
				}
			}
			else
			{
				return adjacentGrenadeList;
			}
		}
		else
		{
			return adjacentGrenadeList;
		}
		
		// While there are still unchecked cells in the list
		while (cellList.Count > 0)
		{			
			// Check space to the left
			InventorySpace tempSpace;
			if (cellList[0].x > 0)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x - 1, (int)cellList[0].y];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains the ammo type we're looking for
						InventoryGrenade grenadeComponent = tempSpace.currentObject.GetComponent<InventoryGrenade>();
						if (grenadeComponent != null)
						{
							if (grenadeComponent.GrenadeType == grenadeType)
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentGrenadeList.Add(grenadeComponent);
								cellList.Add(new Vector2(cellList[0].x - 1, cellList[0].y));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			// Check space above
			if (cellList[0].y > 0)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x, (int)cellList[0].y - 1];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains the ammo type we're looking for
						InventoryGrenade grenadeComponent = tempSpace.currentObject.GetComponent<InventoryGrenade>();
						if (grenadeComponent != null)
						{
							if (grenadeComponent.GrenadeType == grenadeType)
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentGrenadeList.Add(grenadeComponent);
								cellList.Add(new Vector2(cellList[0].x, cellList[0].y - 1));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			// Check space to the right
			if (cellList[0].x < m_Width - 1)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x + 1, (int)cellList[0].y];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains the ammo type we're looking for
						InventoryGrenade grenadeComponent = tempSpace.currentObject.GetComponent<InventoryGrenade>();
						if (grenadeComponent != null)
						{
							if (grenadeComponent.GrenadeType == grenadeType)
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentGrenadeList.Add(grenadeComponent);
								cellList.Add(new Vector2(cellList[0].x + 1, cellList[0].y));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			// Check space below
			if (cellList[0].y < m_Height - 1)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x, (int)cellList[0].y + 1];	
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains the ammo type we're looking for
						InventoryGrenade grenadeComponent = tempSpace.currentObject.GetComponent<InventoryGrenade>();
						if (grenadeComponent != null)
						{
							if (grenadeComponent.GrenadeType == grenadeType)
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentGrenadeList.Add(grenadeComponent);
								cellList.Add(new Vector2(cellList[0].x, cellList[0].y + 1));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			cellList.RemoveAt(0);
		}
		
		// Reset the checked value of all cells back to false
		ResetCells();
		return adjacentGrenadeList;
	}

	private HashSet<InventoryTreasure> CheckAdjacentTreasure(Vector2 cellPos, InventoryTreasure.TreasureMaterial treasureType)
	{
		// Add beginning cell to a list of neighbour cells
		HashSet<InventoryTreasure> adjacentTreasureList = new HashSet<InventoryTreasure>();
		List<Vector2> cellList = new List<Vector2>();
		
		// Double check the cell contains the item type we're looking for
		InventorySpace firstSpace = m_InventoryList[(int)cellPos.x, (int)cellPos.y];
		if (firstSpace.currentObject != null)
		{
			InventoryTreasure treasureComponent = firstSpace.currentObject.GetComponent<InventoryTreasure>();
			if (treasureComponent != null)
			{
				if (treasureComponent.MaterialType == treasureType)
				{
					// Add the item to the adjacent list, as well as the cell to the neighbour list
					adjacentTreasureList.Add(treasureComponent);
					cellList.Add(cellPos);
					firstSpace.cellChecked = true;
				}
				else
				{
					return adjacentTreasureList;
				}
			}
			else
			{
				return adjacentTreasureList;
			}
		}
		else
		{
			return adjacentTreasureList;
		}
		
		// While there are still unchecked cells in the list
		while (cellList.Count > 0)
		{			
			// Check space to the left
			InventorySpace tempSpace;
			if (cellList[0].x > 0)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x - 1, (int)cellList[0].y];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains the ammo type we're looking for
						InventoryTreasure treasureComponent = tempSpace.currentObject.GetComponent<InventoryTreasure>();
						if (treasureComponent != null)
						{
							if (treasureComponent.MaterialType == treasureType)
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentTreasureList.Add(treasureComponent);
								cellList.Add(new Vector2(cellList[0].x - 1, cellList[0].y));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			// Check space above
			if (cellList[0].y > 0)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x, (int)cellList[0].y - 1];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains the ammo type we're looking for
						InventoryTreasure treasureComponent = tempSpace.currentObject.GetComponent<InventoryTreasure>();
						if (treasureComponent != null)
						{
							if (treasureComponent.MaterialType == treasureType)
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentTreasureList.Add(treasureComponent);
								cellList.Add(new Vector2(cellList[0].x, cellList[0].y - 1));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			// Check space to the right
			if (cellList[0].x < m_Width - 1)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x + 1, (int)cellList[0].y];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains the ammo type we're looking for
						InventoryTreasure treasureComponent = tempSpace.currentObject.GetComponent<InventoryTreasure>();
						if (treasureComponent != null)
						{
							if (treasureComponent.MaterialType == treasureType)
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentTreasureList.Add(treasureComponent);
								cellList.Add(new Vector2(cellList[0].x + 1, cellList[0].y));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			// Check space below
			if (cellList[0].y < m_Height - 1)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x, (int)cellList[0].y + 1];	
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains the ammo type we're looking for
						InventoryTreasure treasureComponent = tempSpace.currentObject.GetComponent<InventoryTreasure>();
						if (treasureComponent != null)
						{
							if (treasureComponent.MaterialType == treasureType)
							{
								// Add the item to the adjacent list, as well as the cell to the neighbour list
								adjacentTreasureList.Add(treasureComponent);
								cellList.Add(new Vector2(cellList[0].x, cellList[0].y + 1));
								tempSpace.cellChecked = true;
							}
						}
					}
				}
			}
			
			cellList.RemoveAt(0);
		}
		
		// Reset the checked value of all cells back to false
		ResetCells();
		return adjacentTreasureList;
	}

	private HashSet<InventoryArmour> CheckAdjacentArmour(Vector2 cellPos)
	{
		// Add beginning cell to a list of neighbour cells
		HashSet<InventoryArmour> adjacentArmourList = new HashSet<InventoryArmour>();
		List<Vector2> cellList = new List<Vector2>();
		
		// Double check the cell contains the item type we're looking for
		InventorySpace firstSpace = m_InventoryList[(int)cellPos.x, (int)cellPos.y];
		if (firstSpace.currentObject != null)
		{
			InventoryArmour armourComponent = firstSpace.currentObject.GetComponent<InventoryArmour>();
			if (armourComponent != null)
			{
				// Add the item to the adjacent list, as well as the cell to the neighbour list
				adjacentArmourList.Add(armourComponent);
				cellList.Add(cellPos);
				firstSpace.cellChecked = true;
			}
			else
			{
				return adjacentArmourList;
			}
		}
		else
		{
			return adjacentArmourList;
		}
		
		// While there are still unchecked cells in the list
		while (cellList.Count > 0)
		{			
			// Check space to the left
			InventorySpace tempSpace;
			if (cellList[0].x > 0)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x - 1, (int)cellList[0].y];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains armour
						InventoryArmour armourComponent = tempSpace.currentObject.GetComponent<InventoryArmour>();
						if (armourComponent != null)
						{
							// Add the item to the adjacent list, as well as the cell to the neighbour list
							adjacentArmourList.Add(armourComponent);
							cellList.Add(new Vector2(cellList[0].x - 1, cellList[0].y));
							tempSpace.cellChecked = true;
						}
					}
				}
			}
			
			// Check space above
			if (cellList[0].y > 0)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x, (int)cellList[0].y - 1];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains armour
						InventoryArmour armourComponent = tempSpace.currentObject.GetComponent<InventoryArmour>();
						if (armourComponent != null)
						{
							// Add the item to the adjacent list, as well as the cell to the neighbour list
							adjacentArmourList.Add(armourComponent);
							cellList.Add(new Vector2(cellList[0].x, cellList[0].y - 1));
							tempSpace.cellChecked = true;
						}
					}
				}
			}
			
			// Check space to the right
			if (cellList[0].x < m_Width - 1)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x + 1, (int)cellList[0].y];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains armour
						InventoryArmour armourComponent = tempSpace.currentObject.GetComponent<InventoryArmour>();
						if (armourComponent != null)
						{
							// Add the item to the adjacent list, as well as the cell to the neighbour list
							adjacentArmourList.Add(armourComponent);
							cellList.Add(new Vector2(cellList[0].x + 1, cellList[0].y));
							tempSpace.cellChecked = true;
						}
					}
				}
			}
			
			// Check space below
			if (cellList[0].y < m_Height - 1)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x, (int)cellList[0].y + 1];	
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject != null)
					{
						// If the cell contains armour
						InventoryArmour armourComponent = tempSpace.currentObject.GetComponent<InventoryArmour>();
						if (armourComponent != null)
						{
							// Add the item to the adjacent list, as well as the cell to the neighbour list
							adjacentArmourList.Add(armourComponent);
							cellList.Add(new Vector2(cellList[0].x, cellList[0].y + 1));
							tempSpace.cellChecked = true;
						}
					}
				}
			}
			
			cellList.RemoveAt(0);
		}
		
		// Reset the checked value of all cells back to false
		ResetCells();
		return adjacentArmourList;
	}

	private HashSet<InventorySpace> CheckEmptyAdjacency()
	{
		// Add beginning cell to a list of neighbour cells
		HashSet<InventorySpace> adjacentEmptyList = new HashSet<InventorySpace>();
		List<Vector2> cellList = new List<Vector2>();

		// Search through all inventory spaces
		for (int y = 0; y < m_Height; y++)
		{
			for (int x = 0; x < m_Width; x++)
			{
				// If the space has no item in it, add to the list
				if (m_InventoryList[x,y].currentObject == null)
				{
					// Add the space to the adjacent list, as well as the cell location to the neighbour list
					adjacentEmptyList.Add(m_InventoryList[x,y]);
					m_InventoryList[x,y].cellChecked = true;
					cellList.Add(new Vector2((float)x, (float)y));
					x = m_Width;
					y = m_Height;
				}
			}
		}
		
		// While there are still unchecked cells in the list
		while (cellList.Count > 0)
		{			
			// Check space to the left
			InventorySpace tempSpace;
			if (cellList[0].x > 0)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x - 1, (int)cellList[0].y];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject == null)
					{
						// Add the item to the adjacent list, as well as the cell to the neighbour list
						adjacentEmptyList.Add(tempSpace);
						cellList.Add(new Vector2(cellList[0].x - 1, cellList[0].y));
						tempSpace.cellChecked = true;
					}
				}
			}
			
			// Check space above
			if (cellList[0].y > 0)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x, (int)cellList[0].y - 1];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject == null)
					{
						// Add the item to the adjacent list, as well as the cell to the neighbour list
						adjacentEmptyList.Add(tempSpace);
						cellList.Add(new Vector2(cellList[0].x, cellList[0].y - 1));
						tempSpace.cellChecked = true;
					}
				}
			}
			
			// Check space to the right
			if (cellList[0].x < m_Width - 1)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x + 1, (int)cellList[0].y];
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject == null)
					{
						// Add the item to the adjacent list, as well as the cell to the neighbour list
						adjacentEmptyList.Add(tempSpace);
						cellList.Add(new Vector2(cellList[0].x + 1, cellList[0].y));
						tempSpace.cellChecked = true;
					}
				}
			}
			
			// Check space below
			if (cellList[0].y < m_Height - 1)
			{
				// If cell hasn't already been checked
				tempSpace = m_InventoryList[(int)cellList[0].x, (int)cellList[0].y + 1];	
				if (!tempSpace.cellChecked)
				{
					if (tempSpace.currentObject == null)
					{
						// Add the item to the adjacent list, as well as the cell to the neighbour list
						adjacentEmptyList.Add(tempSpace);
						cellList.Add(new Vector2(cellList[0].x, cellList[0].y + 1));
						tempSpace.cellChecked = true;
					}
				}
			}
			
			cellList.RemoveAt(0);
		}
		
		// Reset the checked value of all cells back to false
		ResetCells();
		return adjacentEmptyList;
	}
	
	private int GetNumAdjacentSameTypeCells(InventoryItem baseItem, HashSet<InventoryItem> itemScoringList)
	{
		int numberOfAdjacentCells = 0;
		int width = baseItem.RotatedWidth;
		int height = baseItem.RotatedHeight;
		Vector2 itemPosition = GetCellPositionOfItem(baseItem);
		//HashSet<InventoryItem> adjacentItemList = new HashSet<InventoryItem>();
		
		InventoryGrenade grenadeComponent = baseItem.GetComponent<InventoryGrenade>();
		if (grenadeComponent != null)
		{
			// Check cells to the left & right
			for (int i = 0; i < height; i++)
			{
				InventoryItem item = null;
				if (itemPosition.x > 0.0f)
				{
					item = m_InventoryList[(int)itemPosition.x - 1, (int)itemPosition.y + i].currentObject;
				}
				
				for (int j = 0; j < 2; j++)
				{
					if (item != null)
					{
						// If there's an item in the cell and it has the grenade component
						InventoryGrenade itemComponent = item.GetComponent<InventoryGrenade>();
						if (itemComponent != null)
						{
							// If the grenade is of the same type increase the number of adjacent cells
							if (itemComponent.GrenadeType == grenadeComponent.GrenadeType)
							{
								if (numberOfAdjacentCells == 0)
								{
									itemScoringList.Add(grenadeComponent.BaseItem);
								}
								itemScoringList.Add(itemComponent.BaseItem);

								numberOfAdjacentCells++;
							}
						}
					}
					
					// Get cell to right of object or skip the second loop if there are no cells to the right
					if ((int)itemPosition.x + width < m_Width)
					{
						item = m_InventoryList[(int)itemPosition.x + width, (int)itemPosition.y + i].currentObject;
					}
					else
					{
						j++;
					}
				}
			}
			
			// Check cells to the top & bottom
			for (int i = 0; i < width; i++)
			{
				InventoryItem item = null;
				if (itemPosition.y > 0.0f)
				{
					item = m_InventoryList[(int)itemPosition.x + i, (int)itemPosition.y - 1].currentObject;
				}
				
				for (int j = 0; j < 2; j++)
				{
					if (item != null)
					{
						// If there's an item in the cell and it has the grenade component
						InventoryGrenade itemComponent = item.GetComponent<InventoryGrenade>();
						if (itemComponent != null)
						{
							// If the grenade is of the same type increase the number of adjacent cells
							if (itemComponent.GrenadeType == grenadeComponent.GrenadeType)
							{
								if (numberOfAdjacentCells == 0)
								{
									itemScoringList.Add(grenadeComponent.BaseItem);
								}
								itemScoringList.Add(itemComponent.BaseItem);

								numberOfAdjacentCells++;
							}
						}
					}
					
					// Get cell to bottom of object or skip the second loop if there are no cells to the bottom
					if ((int)itemPosition.y + height < m_Height)
					{
						item = m_InventoryList[(int)itemPosition.x + i, (int)itemPosition.y + height].currentObject;
					}
					else
					{
						j++;
					}
				}
			}

			return numberOfAdjacentCells;
		}
		
		InventoryAmmo ammoComponent = baseItem.GetComponent<InventoryAmmo>();
		if (ammoComponent != null)
		{
			// Check cells to the left & right
			for (int i = 0; i < height; i++)
			{
				InventoryItem item = null;
				if (itemPosition.x > 0.0f)
				{
					item = m_InventoryList[(int)itemPosition.x - 1, (int)itemPosition.y + i].currentObject;
				}
				
				for (int j = 0; j < 2; j++)
				{
					if (item != null)
					{
						// If there's an item in the cell and it has the ammo component
						InventoryAmmo itemComponent = item.GetComponent<InventoryAmmo>();
						if (itemComponent != null)
						{
							// If the ammo is of the same type increase the number of adjacent cells
							if (itemComponent.WeaponType == ammoComponent.WeaponType)
							{
								if (numberOfAdjacentCells == 0)
								{
									itemScoringList.Add(ammoComponent.BaseItem);
								}
								itemScoringList.Add(itemComponent.BaseItem);

								numberOfAdjacentCells++;
							}
						}
					}
					
					// Get cell to right of object or skip the second loop if there are no cells to the right
					if ((int)itemPosition.x + width < m_Width)
					{
						item = m_InventoryList[(int)itemPosition.x + width, (int)itemPosition.y + i].currentObject;
					}
					else
					{
						j++;
					}
				}
			}
			
			// Check cells to the top & bottom
			for (int i = 0; i < width; i++)
			{
				InventoryItem item = null;
				if (itemPosition.y > 0.0f)
				{
					item = m_InventoryList[(int)itemPosition.x + i, (int)itemPosition.y - 1].currentObject;
				}
				
				for (int j = 0; j < 2; j++)
				{
					if (item != null)
					{
						// If there's an item in the cell and it has the ammo component
						InventoryAmmo itemComponent = item.GetComponent<InventoryAmmo>();
						if (itemComponent != null)
						{
							// If the ammo is of the same type increase the number of adjacent cells
							if (itemComponent.WeaponType == ammoComponent.WeaponType)
							{
								if (numberOfAdjacentCells == 0)
								{
									itemScoringList.Add(ammoComponent.BaseItem);
								}
								itemScoringList.Add(itemComponent.BaseItem);

								numberOfAdjacentCells++;
							}
						}
					}
					
					// Get cell to bottom of object or skip the second loop if there are no cells to the bottom
					if ((int)itemPosition.y + height < m_Height)
					{
						item = m_InventoryList[(int)itemPosition.x + i, (int)itemPosition.y + height].currentObject;
					}
					else
					{
						j++;
					}
				}
			}

			return numberOfAdjacentCells;
		}

		InventoryTreasure treasureComponent = baseItem.GetComponent<InventoryTreasure>();
		if (treasureComponent != null)
		{
			// Check cells to the left & right
			for (int i = 0; i < height; i++)
			{
				InventoryItem item = null;
				if (itemPosition.x > 0.0f)
				{
					item = m_InventoryList[(int)itemPosition.x - 1, (int)itemPosition.y + i].currentObject;
				}
				
				for (int j = 0; j < 2; j++)
				{
					if (item != null)
					{
						// If there's an item in the cell and it has the treasure component
						InventoryTreasure itemComponent = item.GetComponent<InventoryTreasure>();
						if (itemComponent != null)
						{
							// If the treasure is of the same type increase the number of adjacent cells
							if (itemComponent.MaterialType == treasureComponent.MaterialType)
							{
								if (numberOfAdjacentCells == 0)
								{
									itemScoringList.Add(treasureComponent.BaseItem);
								}
								itemScoringList.Add(itemComponent.BaseItem);

								numberOfAdjacentCells++;
							}
						}
					}
					
					// Get cell to right of object or skip the second loop if there are no cells to the right
					if ((int)itemPosition.x + width < m_Width)
					{
						item = m_InventoryList[(int)itemPosition.x + width, (int)itemPosition.y + i].currentObject;
					}
					else
					{
						j++;
					}
				}
			}
			
			// Check cells to the top & bottom
			for (int i = 0; i < width; i++)
			{
				InventoryItem item = null;
				if (itemPosition.y > 0.0f)
				{
					item = m_InventoryList[(int)itemPosition.x + i, (int)itemPosition.y - 1].currentObject;
				}
				
				for (int j = 0; j < 2; j++)
				{
					if (item != null)
					{
						// If there's an item in the cell and it has the treasure component
						InventoryTreasure itemComponent = item.GetComponent<InventoryTreasure>();
						if (itemComponent != null)
						{
							// If the treasure is of the same type increase the number of adjacent cells
							if (itemComponent.MaterialType == treasureComponent.MaterialType)
							{
								if (numberOfAdjacentCells == 0)
								{
									itemScoringList.Add(treasureComponent.BaseItem);
								}
								itemScoringList.Add(itemComponent.BaseItem);

								numberOfAdjacentCells++;
							}
						}
					}
					
					// Get cell to bottom of object or skip the second loop if there are no cells to the bottom
					if ((int)itemPosition.y + height < m_Height)
					{
						item = m_InventoryList[(int)itemPosition.x + i, (int)itemPosition.y + height].currentObject;
					}
					else
					{
						j++;
					}
				}
			}

			return numberOfAdjacentCells;
		}
		
		InventoryWeapon weaponComponent = baseItem.GetComponent<InventoryWeapon>();
		if (weaponComponent != null)
		{
			// Check cells to the left & right
			for (int i = 0; i < height; i++)
			{
				InventoryItem item = null;
				if (itemPosition.x > 0.0f)
				{
					item = m_InventoryList[(int)itemPosition.x - 1, (int)itemPosition.y + i].currentObject;
				}
				
				for (int j = 0; j < 2; j++)
				{
					if (item != null)
					{
						// If there's an item in the cell and it has the weapon component
						InventoryWeapon itemComponent = item.GetComponent<InventoryWeapon>();
						if (itemComponent != null && itemComponent.GetType() == typeof(InventoryWeapon))
						{
							if (numberOfAdjacentCells == 0)
							{
								itemScoringList.Add(weaponComponent.BaseItem);
							}
							itemScoringList.Add(itemComponent.BaseItem);

							numberOfAdjacentCells++;
						}
						else
						{
							// If there's an ammo component of the same type in the cell increase adjacent cells
							InventoryAmmo itemAmmoComponent = item.GetComponent<InventoryAmmo>();
							if (itemAmmoComponent != null)
							{
								if (itemAmmoComponent.WeaponType == weaponComponent.WeaponType)
								{
									numberOfAdjacentCells += 2;
								}
							}
						}
					}
					
					// Get cell to right of object or skip the second loop if there are no cells to the right
					if ((int)itemPosition.x + width < m_Width)
					{
						item = m_InventoryList[(int)itemPosition.x + width, (int)itemPosition.y + i].currentObject;
					}
					else
					{
						j++;
					}
				}
			}
			
			// Check cells to the top & bottom
			for (int i = 0; i < width; i++)
			{
				InventoryItem item = null;
				if (itemPosition.y > 0.0f)
				{
					item = m_InventoryList[(int)itemPosition.x + i, (int)itemPosition.y - 1].currentObject;
				}
				
				for (int j = 0; j < 2; j++)
				{
					if (item != null)
					{
						// If there's an item in the cell and it has the weapon component
						InventoryWeapon itemComponent = item.GetComponent<InventoryWeapon>();
						if (itemComponent != null && itemComponent.GetType() == typeof(InventoryWeapon))
						{
							if (numberOfAdjacentCells == 0)
							{
								itemScoringList.Add(weaponComponent.BaseItem);
							}
							itemScoringList.Add(itemComponent.BaseItem);

							numberOfAdjacentCells++;
						}
						else
						{
							// If there's an ammo component of the same type in the cell increase adjacent cells
							InventoryAmmo itemAmmoComponent = item.GetComponent<InventoryAmmo>();
							if (itemAmmoComponent != null)
							{
								if (itemAmmoComponent.WeaponType == weaponComponent.WeaponType)
								{
									numberOfAdjacentCells += 2;
								}
							}
						}
					}
					
					// Get cell to bottom of object or skip the second loop if there are no cells to the bottom
					if ((int)itemPosition.y + height < m_Height)
					{
						item = m_InventoryList[(int)itemPosition.x + i, (int)itemPosition.y + height].currentObject;
					}
					else
					{
						j++;
					}
				}
			}

			return numberOfAdjacentCells;
		}
		
		InventoryHealth healthComponent = baseItem.GetComponent<InventoryHealth>();
		if (healthComponent != null)
		{
			// Check cells to the left & right
			for (int i = 0; i < height; i++)
			{
				InventoryItem item = null;
				if (itemPosition.x > 0.0f)
				{
					item = m_InventoryList[(int)itemPosition.x - 1, (int)itemPosition.y + i].currentObject;
				}
				
				for (int j = 0; j < 2; j++)
				{
					if (item != null)
					{
						// If there's an item in the cell and it has the health component
						InventoryHealth itemComponent = item.GetComponent<InventoryHealth>();
						if (itemComponent != null)
						{
							// If the heal amount of the two items is the same?
							// Temporary check until there are different healing item types
							if (itemComponent.HealAmount == healthComponent.HealAmount)
							{
								if (numberOfAdjacentCells == 0)
								{
									itemScoringList.Add(healthComponent.BaseItem);
								}
								itemScoringList.Add(itemComponent.BaseItem);

								numberOfAdjacentCells++;
							}
						}
					}
					
					// Get cell to right of object or skip the second loop if there are no cells to the right
					if ((int)itemPosition.x + width < m_Width)
					{
						item = m_InventoryList[(int)itemPosition.x + width, (int)itemPosition.y + i].currentObject;
					}
					else
					{
						j++;
					}
				}
			}
			
			// Check cells to the top & bottom
			for (int i = 0; i < width; i++)
			{
				InventoryItem item = null;
				if (itemPosition.y > 0.0f)
				{
					item = m_InventoryList[(int)itemPosition.x + i, (int)itemPosition.y - 1].currentObject;
				}
				
				for (int j = 0; j < 2; j++)
				{
					if (item != null)
					{
						// If there's an item in the cell and it has the health component
						InventoryHealth itemComponent = item.GetComponent<InventoryHealth>();
						if (itemComponent != null)
						{
							// If the heal amount of the two items is the same?
							// Temporary check until there are different healing item types
							if (itemComponent.HealAmount == healthComponent.HealAmount)
							{
								if (numberOfAdjacentCells == 0)
								{
									itemScoringList.Add(healthComponent.BaseItem);
								}
								itemScoringList.Add(itemComponent.BaseItem);

								numberOfAdjacentCells++;
							}
						}
					}
					
					// Get cell to bottom of object or skip the second loop if there are no cells to the bottom
					if ((int)itemPosition.y + height < m_Height)
					{
						item = m_InventoryList[(int)itemPosition.x + i, (int)itemPosition.y + height].currentObject;
					}
					else
					{
						j++;
					}
				}
			}

			return numberOfAdjacentCells;
		}

		InventoryArmour armourComponent = baseItem.GetComponent<InventoryArmour>();
		if (armourComponent != null)
		{
			// Check cells to the left & right
			for (int i = 0; i < height; i++)
			{
				InventoryItem item = null;
				if (itemPosition.x > 0.0f)
				{
					item = m_InventoryList[(int)itemPosition.x - 1, (int)itemPosition.y + i].currentObject;
				}
				
				for (int j = 0; j < 2; j++)
				{
					if (item != null)
					{
						// If there's an item in the cell and it has the armour component
						InventoryArmour itemComponent = item.GetComponent<InventoryArmour>();
						if (itemComponent != null)
						{
							// Currently don't bother to check for the same armour type
							// Players are unlikely to have multiple different armour types anyway...
							if (numberOfAdjacentCells == 0)
							{
								itemScoringList.Add(armourComponent.BaseItem);
							}
							itemScoringList.Add(itemComponent.BaseItem);

							numberOfAdjacentCells++;
						}
					}
					
					// Get cell to right of object or skip the second loop if there are no cells to the right
					if ((int)itemPosition.x + width < m_Width)
					{
						item = m_InventoryList[(int)itemPosition.x + width, (int)itemPosition.y + i].currentObject;
					}
					else
					{
						j++;
					}
				}
			}
			
			// Check cells to the top & bottom
			for (int i = 0; i < width; i++)
			{
				InventoryItem item = null;
				if (itemPosition.y > 0.0f)
				{
					item = m_InventoryList[(int)itemPosition.x + i, (int)itemPosition.y - 1].currentObject;
				}
				
				for (int j = 0; j < 2; j++)
				{
					if (item != null)
					{
						// If there's an item in the cell and it has the armour component
						InventoryArmour itemComponent = item.GetComponent<InventoryArmour>();
						if (itemComponent != null)
						{
							// Currently don't bother to check for the same armour type
							// Players are unlikely to have multiple different armour types anyway...
							if (numberOfAdjacentCells == 0)
							{
								itemScoringList.Add(armourComponent.BaseItem);
							}
							itemScoringList.Add(itemComponent.BaseItem);

							numberOfAdjacentCells++;
						}
					}
					
					// Get cell to bottom of object or skip the second loop if there are no cells to the bottom
					if ((int)itemPosition.y + height < m_Height)
					{
						item = m_InventoryList[(int)itemPosition.x + i, (int)itemPosition.y + height].currentObject;
					}
					else
					{
						j++;
					}
				}
			}

			return numberOfAdjacentCells;
		}
		
		return numberOfAdjacentCells;
	}
	
	private bool IsAmmoAdjacent(InventoryWeapon weapon)
	{
		int width = weapon.BaseItem.RotatedWidth;
		int height = weapon.BaseItem.RotatedHeight;
		Vector2 itemPosition = GetCellPositionOfItem(weapon.BaseItem);
		
		if (weapon != null)
		{
			// Check cells to the left & right
			for (int i = 0; i < height; i++)
			{
				InventoryItem item = null;
				if (itemPosition.x > 0.0f)
				{
					item = m_InventoryList[(int)itemPosition.x - 1, (int)itemPosition.y + i].currentObject;
				}
				
				for (int j = 0; j < 2; j++)
				{
					if (item != null)
					{
						// If there's an ammo component of the same type in the cell return true
						InventoryAmmo itemAmmoComponent = item.GetComponent<InventoryAmmo>();
						if (itemAmmoComponent != null)
						{
							if (itemAmmoComponent.WeaponType == weapon.WeaponType)
							{
								return true;
							}
						}
					}
					
					// Get cell to right of object or skip the second loop if there are no cells to the right
					if ((int)itemPosition.x + width < m_Width)
					{
						item = m_InventoryList[(int)itemPosition.x + width, (int)itemPosition.y + i].currentObject;
					}
					else
					{
						j++;
					}
				}
			}
			
			// Check cells to the top & bottom
			for (int i = 0; i < width; i++)
			{
				InventoryItem item = null;
				if (itemPosition.y > 0.0f)
				{
					item = m_InventoryList[(int)itemPosition.x + i, (int)itemPosition.y - 1].currentObject;
				}
				
				for (int j = 0; j < 2; j++)
				{
					if (item != null)
					{
						// If there's an ammo component of the same type in the cell return true
						InventoryAmmo itemAmmoComponent = item.GetComponent<InventoryAmmo>();
						if (itemAmmoComponent != null)
						{
							if (itemAmmoComponent.WeaponType == weapon.WeaponType)
							{
								return true;
							}
						}
					}
					
					// Get cell to bottom of object or skip the second loop if there are no cells to the bottom
					if ((int)itemPosition.y + height < m_Height)
					{
						item = m_InventoryList[(int)itemPosition.x + i, (int)itemPosition.y + height].currentObject;
					}
					else
					{
						j++;
					}
				}
			}
		}
		
		return false;
	}

	public HashSet<WeaponType> GetWeaponTypes()
	{
		HashSet<WeaponType> weaponTypeList = new HashSet<WeaponType>();
		HashSet<InventoryWeapon> weaponList = FindAllItemsWithComponent<InventoryWeapon>();

		foreach (InventoryWeapon weapon in weaponList)
		{
			weaponTypeList.Add(weapon.WeaponType);
		}

		return weaponTypeList;
	}

	public void UpgradeSize()
	{
		if (IsUpgradeable)
		{
			transform.FindChild("attachecase_" + m_SizeIndex.ToString()).GetComponent<SpriteRenderer>().enabled = false;
			m_SizeIndex++;
			transform.FindChild("attachecase_" + m_SizeIndex.ToString()).GetComponent<SpriteRenderer>().enabled = true;

			m_Width = m_SizeList[m_SizeIndex].m_Width;
			m_Height = m_SizeList[m_SizeIndex].m_Height;

			ResizeCollider();

			Vector3 distance = Vector3.zero;
			if (m_SizeIndex == 1)
			{
				distance = new Vector3(-1.0f, 0.0f);
			}
			else if (m_SizeIndex == 2)
			{
				distance = new Vector3(0.0f, 1.0f);
			}
			else if (m_SizeIndex == 3)
			{
				distance = new Vector3(-2.0f, 0.0f);
			}

			Vector3 pos = transform.position + distance;
			transform.position = pos;
			MoveItems(distance);
		}
	}

	private void MoveItems(Vector3 distance)
	{
		HashSet<InventoryItem> itemList = FindAllItemsWithComponent<InventoryItem>();

		foreach (InventoryItem item in itemList)
		{
			Vector3 pos = item.transform.position;
			pos += distance;
			item.transform.position = pos;

			InventoryItem swapItem = null;
			RemoveItem(item);
			PlaceItem(item, out swapItem);
		}
	}

	public void ClearItems(bool delete = true)
	{
		HashSet<InventoryItem> itemList = FindAllItemsWithComponent<InventoryItem>(includeGhostObjects: true);
		
		foreach (InventoryItem item in itemList)
		{
			if (delete)
			{
				DestroyItem(item);
			}
			else
			{
				RemoveItem(item);
			}
		}

		m_NumFreeCells = m_Width * m_Height;
	}

	private int GetTotalSellValue()
	{
		int totalValue = 0;
		HashSet<InventoryItem> itemList = FindAllItemsWithComponent<InventoryItem>();
		//ShopCostList costListScript = GameObject.FindGameObjectWithTag(Tags.SHOPCOSTLIST).GetComponent<ShopCostList>();
		foreach (InventoryItem item in itemList)
		{
			// Get selling price of item (Sell cost is half of buying cost)
			// For weapons, increase the sell price by half of the cost of each upgrade level purchased
			/*int itemCost = costListScript.GetCostFromPrefabID(item.PrefabID) / 2;
			InventoryWeapon weapon = item.GetComponent<InventoryWeapon>();
			if (weapon != null)
			{
				// Damage //
				int upgradeLevel = weapon.DamageLevel;
				for (int level = 0; level < upgradeLevel; level++)
				{
					itemCost += costListScript.GetDamageUpgradeCost(item.PrefabID, level) / 2;
				}
				
				// Capacity //
				upgradeLevel = weapon.CapacityLevel;
				for (int level = 0; level < upgradeLevel; level++)
				{
					itemCost += costListScript.GetCapacityUpgradeCost(item.PrefabID, level) / 2;
				}
				
				// Firing Speed //
				upgradeLevel = weapon.FiringSpeedLevel;
				for (int level = 0; level < upgradeLevel; level++)
				{
					itemCost += costListScript.GetFiringSpeedUpgradeCost(item.PrefabID, level) / 2;
				}
				
				// Reload Speed //
				upgradeLevel = weapon.ReloadSpeedLevel;
				for (int level = 0; level < upgradeLevel; level++)
				{
					itemCost += costListScript.GetReloadSpeedUpgradeCost(item.PrefabID, level) / 2;
				}
				
				// Penetration //
				upgradeLevel = weapon.PenetrationLevel;
				for (int level = 0; level < upgradeLevel; level++)
				{
					itemCost += costListScript.GetPenetrationUpgradeCost(item.PrefabID, level) / 2;
				}
				
				// Radius //
				upgradeLevel = weapon.RadiusLevel;
				for (int level = 0; level < upgradeLevel; level++)
				{
					itemCost += costListScript.GetRadiusUpgradeCost(item.PrefabID, level) / 2;
				}
			}

			InventoryAmmo ammo = item.GetComponent<InventoryAmmo>();
			if (ammo != null)
			{
				itemCost *= ammo.Amount;
			}

			totalValue += itemCost;*/

			totalValue += item.GetSellValue();
		}

		return totalValue;
	}

	private void CalculateFreeCells()
	{
		m_NumFreeCells = 0;
		
		for (int x = 0; x < m_Width; x++)
		{
			for (int y = 0; y < m_Height; y++)
			{
				if (m_InventoryList[x,y].currentObject == null)
				{
					m_NumFreeCells++;
				}
			}
		}
	}

	public int NumFreeCells()
	{
		return m_NumFreeCells;
	}
}