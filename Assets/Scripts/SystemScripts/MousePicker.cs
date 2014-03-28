using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MousePicker : MonoBehaviour
{
	[SerializeField] private InventoryScript m_MainInventory;
	[SerializeField] private InventoryScript m_HoldingArea;
	[SerializeField] private Transform m_IdealRangeHighlight;
	[SerializeField] private Transform m_EffectiveRangeHighlight;
	[SerializeField] private Transform m_OutOfRangeHighlight;
	[SerializeField] private Texture2D m_DiscardIcon;
	[SerializeField] private Transform m_DiscardPrefab;

	private bool m_Enabled = true;
	private bool m_IsCarrying = false;
	private InventoryItem m_CurrentObject = null;
	private Vector3 m_ItemOffset = new Vector3(-1.0f, -1.0f, -1.0f);
	private InventoryWeapon m_HoverWeapon = null;
	private InventoryItem m_HoverItem = null;
	
	private float m_DoubleClickStart;
	private InventoryItem m_LastClickedItem;

	private RaycastHit2D[] m_TopRaycastList = new RaycastHit2D[10];
	private RaycastHit2D[] m_BottomRaycastList = new RaycastHit2D[10];
	private int m_MouseOverArea = 0;

	public bool IsCarrying { get {return m_IsCarrying;}}

	public bool Enabled
	{
		get
		{
			return m_Enabled;
		}
		set
		{
			m_Enabled = value;
			if (!value)
			{
				m_HoverItem = null;
				m_HoverWeapon = null;
				m_ItemOffset = new Vector3(-1.0f, -1.0f, -1.0f);
			}
		}
	}

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		DontDestroyOnLoad(m_IdealRangeHighlight.gameObject);
		DontDestroyOnLoad(m_EffectiveRangeHighlight.gameObject);
		DontDestroyOnLoad(m_OutOfRangeHighlight.gameObject);
	}

	void OnGUI()
	{
		if (m_MouseOverArea == 3)
		{
			if (IsCarrying)
			{
			#if UNITY_ANDROID
				if (Input.touchCount > 0)
				{
					Touch firstTouch = Input.GetTouch(0);
					GUI.DrawTexture(new Rect(firstTouch.position.x - (m_DiscardIcon.width / 2), 
					                         (Screen.height - firstTouch.position.y) - m_DiscardIcon.height / 2, 
					                         m_DiscardIcon.width, m_DiscardIcon.height), m_DiscardIcon);
				}
			#else
				GUI.DrawTexture(new Rect(Input.mousePosition.x - (m_DiscardIcon.width / 2), 
				                         (Screen.height - Input.mousePosition.y) - m_DiscardIcon.height / 2, 
				                         m_DiscardIcon.width, m_DiscardIcon.height), m_DiscardIcon);
			#endif
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (m_Enabled)
		{
			// Get the mouse position on the z=0 plane for moving items later
			float distance;
			Plane plane = new Plane(Vector3.back, 0.0f);
			#if UNITY_ANDROID
			Ray ray = new Ray();
			if (Input.touchCount > 0)
			{
				Touch firstTouch = Input.GetTouch(0);
				ray = Camera.main.ScreenPointToRay(firstTouch.position);
			}
			#else
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			#endif

			Vector3 pos = transform.position;
			if (plane.Raycast(ray, out distance))
			{
				pos = ray.GetPoint(distance);
				//transform.position = pos;
			}

			// Find the inventory item the mouse is over (If there is one)
			RaycastHit hit;
			m_HoverItem = null;
			if (Physics.Raycast(ray, out hit))
			{
				// Check if the collided object has the InventoryItem tag
				if (hit.collider.gameObject.tag == Tags.INVENTORYITEM)
				{
					m_HoverItem = hit.collider.gameObject.GetComponent<InventoryItem>();
				}
			}

			// Check if the item being hovered over is a weapon, and show the weapon range highlights if it is
			if (m_HoverItem != null)
			{
				InventoryWeapon weapon = m_HoverItem.GetComponent<InventoryWeapon>();
				if (weapon != null && weapon.GetType() == typeof(InventoryWeapon))
				{
					m_HoverWeapon = weapon;
					Vector3 startPos = GameObject.FindGameObjectWithTag(Tags.PLAYER).transform.position;
					//startPos.x += 0.25f;
					//startPos.y += 0.5f;
					startPos.x += 0.8f;
					startPos.y += 1.6f;

					m_IdealRangeHighlight.transform.position = startPos;
					m_IdealRangeHighlight.transform.localScale = new Vector3(m_HoverWeapon.IdealRange, 3.0f, 1.0f);
					m_IdealRangeHighlight.GetComponentInChildren<MeshRenderer>().enabled = true;

					startPos.x += m_HoverWeapon.IdealRange;
					m_EffectiveRangeHighlight.transform.position = startPos;
					m_EffectiveRangeHighlight.transform.localScale = new Vector3(m_HoverWeapon.EffectiveRange - m_HoverWeapon.IdealRange, 3.0f, 1.0f);
					m_EffectiveRangeHighlight.GetComponentInChildren<MeshRenderer>().enabled = true;

					startPos.x += m_HoverWeapon.EffectiveRange - m_HoverWeapon.IdealRange;
					m_OutOfRangeHighlight.transform.position = startPos;
					m_OutOfRangeHighlight.transform.localScale = new Vector3(64.0f, 3.0f, 1.0f);
					m_OutOfRangeHighlight.GetComponentInChildren<MeshRenderer>().enabled = true;
				}
				else
				{
					if (m_HoverWeapon != null)
					{
						m_HoverWeapon = null;
						m_IdealRangeHighlight.GetComponentInChildren<MeshRenderer>().enabled = false;
						m_EffectiveRangeHighlight.GetComponentInChildren<MeshRenderer>().enabled = false;
						m_OutOfRangeHighlight.GetComponentInChildren<MeshRenderer>().enabled = false;
					}
				}
			}
			else
			{
				if (m_HoverWeapon != null)
				{
					m_HoverWeapon = null;
					m_IdealRangeHighlight.GetComponentInChildren<MeshRenderer>().enabled = false;
					m_EffectiveRangeHighlight.GetComponentInChildren<MeshRenderer>().enabled = false;
					m_OutOfRangeHighlight.GetComponentInChildren<MeshRenderer>().enabled = false;
				}
			}

			// Cast a ray to find any 2D collider in the top part of the screen (Such as item drops)
			ItemDrop itemDrop = null;
			Vector2 topMousePos = new Vector2();
			#if UNITY_ANDROID
			if (Input.touchCount > 0)
			{
				topMousePos = GameObject.FindGameObjectWithTag(Tags.ACTIONCAMERA).GetComponent<Camera>().ScreenToWorldPoint(Input.GetTouch(0).position);
			}
			#else
			topMousePos = GameObject.FindGameObjectWithTag(Tags.ACTIONCAMERA).GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
			#endif

			int topHitObjects = Physics2D.RaycastNonAlloc(topMousePos, Vector2.zero, m_TopRaycastList);
			for (int i = 0; i < topHitObjects; i++) //(RaycastHit2D rayHit in m_TopRaycastList)
			{
				if (m_TopRaycastList[i].collider.gameObject.tag == Tags.ITEMDROP)
				{
					itemDrop = m_TopRaycastList[i].collider.gameObject.GetComponent<ItemDrop>();
					break;
				}
			}
			
			// Cast a ray to find the inventory on the bottom part of the screen
			Vector2 bottomMousePos = new Vector2();
			#if UNITY_ANDROID
			if (Input.touchCount > 0)
			{
				bottomMousePos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
			}
			#else
			bottomMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			#endif
			m_MouseOverArea = 0;
			int bottomHitObjects = Physics2D.RaycastNonAlloc(bottomMousePos, Vector2.zero, m_BottomRaycastList);
			for (int i = 0; i < bottomHitObjects; i++)
			{
				// Set the mouse over area to the relevant area
				if (m_BottomRaycastList[i].collider.tag == Tags.MAININVENTORY)
				{
					m_MouseOverArea = 1;
				}
				else if (m_BottomRaycastList[i].collider.tag == Tags.HOLDINGAREA)
				{
					m_MouseOverArea = 2;
				}
				else if (m_BottomRaycastList[i].collider.tag == Tags.DISCARDAREA)
				{
					m_MouseOverArea = 3;
				}
			}

			// If currently carrying an object make it follow the mouse and handle rotation
			bool currentSpaceFree = false;
			PlacementResult tryResult = PlacementResult.Failed;
			if (m_CurrentObject != null)
			{
				if (m_IsCarrying)
				{
					m_CurrentObject.FollowMouse(pos, m_ItemOffset);
					
					if (Input.GetMouseButtonDown(1))
					{
						m_CurrentObject.Rotate(-90.0f, pos);
						m_ItemOffset = pos - m_CurrentObject.transform.position;
					}
					
					//bool enableHighlight = false;
					if (m_MouseOverArea == 1)
					{
						tryResult = m_MainInventory.TryPlaceItem(m_CurrentObject);
					}
					else if (m_MouseOverArea == 2)
					{
						tryResult = m_HoldingArea.TryPlaceItem(m_CurrentObject);
					}

					if (tryResult != PlacementResult.Failed)
					{
						m_CurrentObject.transform.FindChild("MovingHighlight").GetComponent<MeshRenderer>().enabled = true;
					}
					else
					{
						m_CurrentObject.transform.FindChild("MovingHighlight").GetComponent<MeshRenderer>().enabled = false;
					}

					if (tryResult == PlacementResult.Success || tryResult == PlacementResult.AmmoDestroyed)
					{
						currentSpaceFree = true;
					}
				}
			}
			else
			{
				// If not carrying an item and hovering over an item enable the hovering highlight
				#if !UNITY_ANDROID
				if (m_HoverItem != null)
				{
					m_HoverItem.transform.FindChild("MouseOverHighlight").GetComponent<MeshRenderer>().enabled = true;
				}
				#endif
			}
			
			// If the player presses the LMB pick up the relevant item
			if (Input.GetMouseButtonDown(0) && m_CurrentObject == null)
			{
				if (m_HoverItem != null)
				{
					m_CurrentObject = m_HoverItem;
					m_CurrentObject.transform.FindChild("MovingHighlight").GetComponent<MeshRenderer>().enabled = true;
					m_CurrentObject.transform.FindChild("MouseOverHighlight").GetComponent<MeshRenderer>().enabled = false;
					
					InventoryWeapon weapon = m_CurrentObject.GetComponent<InventoryWeapon>();
					if (weapon != null)
					{
						m_CurrentObject.transform.FindChild("EquippedHighlight").GetComponent<MeshRenderer>().enabled = false;
					}
					
					if (m_ItemOffset == new Vector3(-1.0f, -1.0f, -1.0f))
					{
						m_ItemOffset = pos - m_CurrentObject.transform.position;
						// NO LONGER REMOVES ITEM ON PICKUP, ONLY REMOVED WHEN A NEW POSITION IS FOUND!
						//if (m_CurrentObject.FirstSpace != null)
						//{
						//	m_CurrentObject.FirstSpace.inventory.RemoveItem(m_CurrentObject);
						//}
					}
					m_IsCarrying = true;
					m_CurrentObject.IsCarried = true;
				}
				else if (itemDrop != null)
				{
					// Handle creation of inventory item based on drop
					if (itemDrop.PickupDrop())
					{
						// Destroy the drop if there's space in holding area
						Destroy(itemDrop.gameObject);
					}
				}
			}
			// If the player releases the LMB place the item into the inventory
			else if (Input.GetMouseButtonUp(0))
			{
				if (m_CurrentObject != null)
				{
					if (m_IsCarrying)
					{
						bool placedInHolding = false;
						PlacementResult result = PlacementResult.Failed;
						if (currentSpaceFree)
						{
							if (m_CurrentObject.FirstSpace != null)
							{
								m_CurrentObject.FirstSpace.inventory.RemoveItem(m_CurrentObject);
							}
						}

						// Place the item based on which inventory the mouse is over
						if (m_MouseOverArea == 1)
						{
							result = m_MainInventory.PlaceItem(m_CurrentObject);
						}
						else if (m_MouseOverArea == 2)
						{
							result = m_HoldingArea.PlaceItem(m_CurrentObject);
							placedInHolding = true;
						}
						else if (m_MouseOverArea == 3)
						{
							if (m_HoverWeapon != null)
							{
								m_HoverWeapon = null;
								m_IdealRangeHighlight.GetComponentInChildren<MeshRenderer>().enabled = false;
								m_EffectiveRangeHighlight.GetComponentInChildren<MeshRenderer>().enabled = false;
								m_OutOfRangeHighlight.GetComponentInChildren<MeshRenderer>().enabled = false;
							}

							m_CurrentObject.FirstSpace.inventory.DestroyItem(m_CurrentObject);
							//Destroy(m_CurrentObject.gameObject);
							m_CurrentObject = null;
							m_IsCarrying = false;
							//m_CurrentObject.IsCarried = false;
							m_ItemOffset = new Vector3(-1.0f, -1.0f, -1.0f);

							Vector3 position = GameObject.FindGameObjectWithTag(Tags.PLAYER).transform.position + new Vector3(-0.32f, 3.0f, 0.0f);
							Instantiate(m_DiscardPrefab, position, Quaternion.identity);
						}

						if (m_CurrentObject != null)
						{
							#if UNITY_ANDROID
							if (result == PlacementResult.Failed || result == PlacementResult.AmmoTaken)
							#else
							if (result == PlacementResult.Failed)
							#endif
							{
								// If it didn't fit into either reset it to the previous position
								// and check against both again
								m_CurrentObject.Reset();
								m_IsCarrying = false;
								m_CurrentObject.IsCarried = false;
								m_ItemOffset = new Vector3(-1.0f, -1.0f, -1.0f);
							}
							else if (result != PlacementResult.AmmoTaken)
							{
								m_IsCarrying = false;
								m_CurrentObject.IsCarried = false;
								m_ItemOffset = new Vector3(-1.0f, -1.0f, -1.0f);
							}

							if (result == PlacementResult.AmmoDestroyed)
							{
								m_CurrentObject = null;
							}
						}

						#if UNITY_ANDROID
						if (m_CurrentObject != null)
						#else
						if (m_CurrentObject != null && result != PlacementResult.AmmoTaken)
						#endif
						{
							// If the current object is a weapon, and it's equipped, re-enable the equip highlight
							InventoryWeapon weapon = m_CurrentObject.GetComponent<InventoryWeapon>();
							if (weapon != null)
							{
								if (weapon.IsEquipped())
								{
									// If it was placed in the holding area, unequip the weapon
									if (placedInHolding)
									{
										GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<CharacterScript>().EquippedWeapon = null;
										weapon.SetEquipped(false);
									}
									else
									{
										m_CurrentObject.transform.FindChild("EquippedHighlight").GetComponent<MeshRenderer>().enabled = true;
									}
								}
							}
							
							m_CurrentObject.transform.FindChild("MovingHighlight").GetComponent<MeshRenderer>().enabled = false;
							m_CurrentObject = null;
						}
					}
				}
			}
			
			// If the player double clicks a weapon, equip it on the character
			if (m_HoverItem != null)
			{
				if (IsDoubleClick())
				{
					//m_HoverItem.FirstSpace.inventory.RemoveItem(m_HoverItem);
					//m_HoldingArea.FindAvailableSpace(m_HoverItem);
					if (m_HoverItem.FirstSpace.inventory == m_MainInventory)
					{
						InventoryGrenade grenade = m_HoverItem.GetComponent<InventoryGrenade>();
						if (grenade != null)
						{
							GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<CharacterScript>().ThrowGrenade(grenade.GrenadeType);
							m_HoverItem.FirstSpace.inventory.DestroyItem(m_HoverItem);
						}
						else
						{
							InventoryWeapon weapon = m_HoverItem.GetComponent<InventoryWeapon>();
							if (weapon != null)
							{
								InventoryWeapon oldWeapon = GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<CharacterScript>().EquippedWeapon;
								if (oldWeapon != null)
								{
									oldWeapon.transform.FindChild("EquippedHighlight").GetComponent<MeshRenderer>().enabled = false;
									oldWeapon.SetEquipped(false);
								}
								
								GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<CharacterScript>().EquippedWeapon = weapon;
								m_HoverItem.transform.FindChild("EquippedHighlight").GetComponent<MeshRenderer>().enabled = true;
								weapon.SetEquipped(true);
							}
						}
					}
					
					InventoryHealth health = m_HoverItem.GetComponent<InventoryHealth>();
					if (health != null)
					{
						GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<CharacterScript>().AlterHealth(health.HealAmount);
						m_HoverItem.FirstSpace.inventory.DestroyItem(m_HoverItem);
					}
				}
			}
		}
	}
	
	bool IsDoubleClick()
	{
		if (Input.GetMouseButtonUp(0))
		{
			if ((Time.realtimeSinceStartup - m_DoubleClickStart) < 0.3f && m_HoverItem == m_LastClickedItem)
			{
				m_DoubleClickStart = -1.0f;
				return true;
			}
			else
			{
				m_DoubleClickStart = Time.realtimeSinceStartup;
				m_LastClickedItem = m_HoverItem;
			}
		}
		
		return false;
	}
}