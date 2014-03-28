using UnityEngine;
using System.Collections;

public class InventoryItem : MonoBehaviour
{
	[SerializeField] private string m_ItemName;
	[SerializeField] private string m_ItemDescription;
	[SerializeField] private int m_PrefabID;
	[SerializeField] private int m_Width = 1;
	[SerializeField] private int m_Height = 1;
	private InventorySpace m_FirstSpace = null;
	
	private int m_Rotation = 0;
	private Vector3 m_PreviousPosition;
	private Quaternion m_PreviousQuaternion;
	private int m_PreviousRotation = 0;
	private Vector3 m_PlaneOffset;
	private bool m_IsCarried = false;
	
	public InventorySpace FirstSpace { get {return m_FirstSpace;} set {m_FirstSpace = value;}}
	public Quaternion PreviousQuaternion { get {return m_PreviousQuaternion;}}
	public Vector3 PreviousPosition { get {return m_PreviousPosition;}}
	public int PreviousRotation { get {return m_PreviousRotation;}}
	public int Rotation { get {return m_Rotation;}}
	public int Width { get {return m_Width;}}
	public int Height { get {return m_Height;}}
	public int RotatedWidth { get {return m_Rotation == 1 || m_Rotation == 3 ? m_Height : m_Width;}}
	public int RotatedHeight { get {return m_Rotation == 1 || m_Rotation == 3 ? m_Width : m_Height;}}
	
	public int PrefabID { get {return m_PrefabID;}}
	public string ItemName { get {return m_ItemName;}}
	public string ItemDescription { get {return m_ItemDescription;}}
	public bool IsCarried { get {return m_IsCarried;} set {m_IsCarried = value;}}

	public Vector3 CentrePosition
	{
		get
		{
			// Get width/height of object based on rotation
			int width = Rotation == 0 || Rotation == 2 ? m_Width : m_Height;
			int height = Rotation == 0 || Rotation == 2 ? m_Height : m_Width;
			width = Rotation == 1 || Rotation == 2 ? -width : width;
			height = Rotation == 3 || Rotation == 2 ? height : -height;

			Vector3 centre = transform.position;
			centre.x += ((float)width * 0.5f);
			centre.y += ((float)height * 0.5f);
			return centre;
		}
	}
	
	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		m_PlaneOffset = new Vector3(((float)m_Width) * 0.5f, ((float)m_Height) * 0.5f, 0.0f);
		m_ItemDescription = m_ItemDescription.Replace("\\n", "\n");
	}

	void Start()
	{
		if (m_FirstSpace == null)
		{
			if (transform.position.x < 16.0f)
			{
				GameObject.FindGameObjectWithTag(Tags.MAININVENTORY).GetComponent<InventoryScript>().PlaceItem(this);
			}
			else
			{
				GameObject.FindGameObjectWithTag(Tags.HOLDINGAREA).GetComponent<InventoryScript>().FindAvailableSpace(this);
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		//RepositionObjects();
	}
	
	public void FollowMouse(Vector3 pos, Vector3 offset)
	{
		// Follows the given position, adding an offset based on the cursor position
		offset.z = 0.4f;
		transform.position = pos - offset;
		
		RepositionObjects();
	}
	
	void RepositionObjects()
	{
		// Convert GUI Text position to screen space to make it render correctly
		Transform textObject = transform.FindChild("ItemText");
		Vector3 textOffset = new Vector3((float)m_Width, (float)m_Height, 0.0f);
		if (m_Rotation == 1)
		{
			textOffset.x = 0.0f;
			textOffset.y = (float)m_Width;
		}
		else if (m_Rotation == 2)
		{
			textOffset.x = 0.0f;
			textOffset.y = 0.0f;
		}
		else if (m_Rotation == 3)
		{
			textOffset.x = (float)m_Height;
			textOffset.y = 0.0f;
		}
		
		Vector3 textPosition = Camera.main.WorldToViewportPoint
			(new Vector3(transform.position.x + textOffset.x, transform.position.y - textOffset.y, 0.0f));
		textPosition.z = 0.0f;
		textObject.position = textPosition;
		
		// Get the moving highlight and if enabled position it correctly based on the grid
		MeshRenderer movingHighlight = transform.FindChild("MovingHighlight").GetComponent<MeshRenderer>();
		if (movingHighlight.enabled)
		{
			Vector3 truePlaneOffset = m_PlaneOffset;
			if (m_Rotation == 1)
			{
				truePlaneOffset.x = -m_PlaneOffset.y;
				truePlaneOffset.y = m_PlaneOffset.x;
			}
			else if (m_Rotation == 2)
			{
				truePlaneOffset.x = -truePlaneOffset.x;
				truePlaneOffset.y = -truePlaneOffset.y;
			}
			else if (m_Rotation == 3)
			{
				truePlaneOffset.x = m_PlaneOffset.y;
				truePlaneOffset.y = -m_PlaneOffset.x;
			}
			
			int xPos = (int)transform.position.x;
			int yPos = (int)transform.position.y;
			float decimalX = transform.position.x - (float)xPos;
			float decimalY = transform.position.y - (float)yPos;
			
			if (decimalX >= 0.5f)
			{
				xPos++;
			}
			if (decimalY <= -0.5f)
			{
				yPos--;
			}
			
			movingHighlight.transform.position = new Vector3((float)xPos + truePlaneOffset.x, (float)yPos - truePlaneOffset.y, 0.0f);
		}
	}
	
	public void PlaceItem(Vector3 pos)
	{
		// Place the item into the closest sensible inventory slots (If possible)
		pos.z = 0.0f;
		transform.position = pos;
		m_PreviousPosition = pos;
		m_PreviousQuaternion = transform.rotation;
		m_PreviousRotation = m_Rotation;
		
		RepositionObjects();
	}
	
	public void Rotate(float angle, Vector3 offset)
	{
		m_Rotation++;
		if (m_Rotation > 3)
		{
			m_Rotation = 0;
		}
		
		transform.RotateAround(offset, new Vector3(0.0f, 0.0f, 1.0f), angle);
	}
	
	public void Reset()
	{
		transform.position = m_PreviousPosition;
		transform.rotation = m_PreviousQuaternion;
		m_Rotation = m_PreviousRotation;

		RepositionObjects();
	}

	#if !UNITY_ANDROID
	void OnMouseExit()
	{
		transform.FindChild("MouseOverHighlight").GetComponent<MeshRenderer>().enabled = false;
	}
	#endif

	public int GetSellValue()
	{
		// Get selling price of item (Sell cost is half of buying cost)
		// For weapons, increase the sell price by half of the cost of each upgrade level purchased
		ShopCostList costListScript = GameObject.FindGameObjectWithTag(Tags.SHOPCOSTLIST).GetComponent<ShopCostList>();
		int itemCost = costListScript.GetCostFromPrefabID(m_PrefabID) / 2;
		InventoryWeapon weapon = GetComponent<InventoryWeapon>();
		if (weapon != null)
		{
			// Damage //
			int upgradeLevel = weapon.DamageLevel;
			for (int level = 0; level < upgradeLevel; level++)
			{
				itemCost += costListScript.GetDamageUpgradeCost(m_PrefabID, level) / 2;
			}
			
			// Capacity //
			upgradeLevel = weapon.CapacityLevel;
			for (int level = 0; level < upgradeLevel; level++)
			{
				itemCost += costListScript.GetCapacityUpgradeCost(m_PrefabID, level) / 2;
			}
			
			// Firing Speed //
			upgradeLevel = weapon.FiringSpeedLevel;
			for (int level = 0; level < upgradeLevel; level++)
			{
				itemCost += costListScript.GetFiringSpeedUpgradeCost(m_PrefabID, level) / 2;
			}
			
			// Reload Speed //
			upgradeLevel = weapon.ReloadSpeedLevel;
			for (int level = 0; level < upgradeLevel; level++)
			{
				itemCost += costListScript.GetReloadSpeedUpgradeCost(m_PrefabID, level) / 2;
			}
			
			// Penetration //
			upgradeLevel = weapon.PenetrationLevel;
			for (int level = 0; level < upgradeLevel; level++)
			{
				itemCost += costListScript.GetPenetrationUpgradeCost(m_PrefabID, level) / 2;
			}
			
			// Radius //
			upgradeLevel = weapon.RadiusLevel;
			for (int level = 0; level < upgradeLevel; level++)
			{
				itemCost += costListScript.GetRadiusUpgradeCost(m_PrefabID, level) / 2;
			}
		}
		
		InventoryAmmo ammo = GetComponent<InventoryAmmo>();
		if (ammo != null)
		{
			itemCost *= ammo.Amount;
		}

		return itemCost;
	}
}