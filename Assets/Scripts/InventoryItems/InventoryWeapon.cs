using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum WeaponType
{
	Magnum = 0,
	Pistol,
	Shotgun,
	Rifle,
	Automatic,
	//Explosive,
	//Rocket,
	Grenade,
};

public enum WeaponName
{
	Magnum = 0,
	Pistol,
	Shotgun,
	SniperRifle,
	Grenade,
	FlashGrenade,
	IncendiaryGrenade,
	SMG,
	Stiletto,
	Striker,
	CombatShotgun,
};

public class InventoryWeapon : MonoBehaviour
{
	public const int NUM_WEAPON_TYPES = 8;
	
	// Weapon stats
	[SerializeField] private Transform m_ParticlePrefab;
	[SerializeField] private Texture2D m_ActionTexture;
	[SerializeField] private Texture2D m_OutlineTexture;
	[SerializeField] private Color m_OutlineColour;
	[SerializeField] protected WeaponType m_WeaponType;
	[SerializeField] protected WeaponName m_WeaponName;
	[SerializeField] protected float m_EffectiveRange;
	[SerializeField] protected float m_IdealRange;
	[SerializeField] protected int m_CritChance;
	[SerializeField] protected int m_Accuracy;
	
	protected float m_FiringSpeed;
	protected float m_ReloadSpeed;
	protected float m_Damage;
	protected float m_Radius;
	protected int m_Capacity;
	protected int m_Penetration;
	
	// Weapon upgrade levels
	protected WeaponUpgradeStats m_WeaponStats;
	protected int m_CapacityUpgradeLevel = 0;
	protected int m_DamageUpgradeLevel = 0;
	protected int m_FiringSpeedUpgradeLevel = 0;
	protected int m_ReloadSpeedUpgradeLevel = 0;
	protected int m_PenetrationUpgradeLevel = 0;
	protected int m_RadiusUpgradeLevel = 0;

	protected float m_FiringTimer = 0.0f;
	protected float m_ReloadTimer = 0.0f;
	protected bool m_WaitToReload = false;
	protected int m_AmmoTaken = 0;
	
	// Gameplay variables
	protected bool m_IsEquipped = false;
	protected InventoryItem m_BaseItem;
	protected int m_AmmoLoaded = 0;

	public float IdealRange { get {return m_IdealRange;}}
	public float EffectiveRange { get {return m_EffectiveRange;}}
	public WeaponType WeaponType { get {return m_WeaponType;}}
	public WeaponName WeaponName { get {return m_WeaponName;}}
	public InventoryItem BaseItem { get {return m_BaseItem;}}
	
	// Weapon stat getters
	public int Capacity { get {return m_Capacity;}}
	public float Damage { get {return m_Damage;}}
	public float FiringSpeed { get {return m_FiringSpeed;}}
	public float ReloadSpeed { get {return m_ReloadSpeed;}}
	public float Radius { get {return m_Radius;}}
	public int Penetration { get {return m_Penetration;}}
	
	public int CapacityLevel { get {return m_CapacityUpgradeLevel;}}
	public int DamageLevel { get {return m_DamageUpgradeLevel;}}
	public int FiringSpeedLevel { get {return m_FiringSpeedUpgradeLevel;}}
	public int ReloadSpeedLevel { get {return m_ReloadSpeedUpgradeLevel;}}
	public int PenetrationLevel { get {return m_PenetrationUpgradeLevel;}}
	public int RadiusLevel { get {return m_RadiusUpgradeLevel;}}
	
	public bool IsEquipped() {return m_IsEquipped;}
	public void SetEquipped(bool equipped){m_IsEquipped = equipped;}// m_FiringTimer = m_FiringSpeed;}
	
	void Awake()
	{
		m_BaseItem = GetComponent<InventoryItem>();
		
		m_WeaponStats = GameObject.FindGameObjectWithTag(Tags.WEAPONSTATS).GetComponent<WeaponUpgradeStats>();
		
		// Get other weapon values based on upgrade level
		m_Capacity = m_WeaponStats.GetCapacity(m_WeaponName, m_CapacityUpgradeLevel);
		m_Damage = m_WeaponStats.GetDamage(m_WeaponName, m_DamageUpgradeLevel);
		m_FiringSpeed = m_WeaponStats.GetFiringSpeed(m_WeaponName, m_FiringSpeedUpgradeLevel);
		m_ReloadSpeed = m_WeaponStats.GetReloadSpeed(m_WeaponName, m_ReloadSpeedUpgradeLevel);
		m_Penetration = m_WeaponStats.GetPenetration(m_WeaponName, m_PenetrationUpgradeLevel);
		m_Radius = m_WeaponStats.GetRadius(m_WeaponName, m_RadiusUpgradeLevel);

		m_AmmoLoaded = m_Capacity;
		transform.FindChild("ItemText").guiText.text = m_AmmoLoaded.ToString();
	}

	void OnGUI()
	{
		if (GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>().ShowEffects)
		{
			if ((m_FiringTimer > 0.0f || m_ReloadTimer > 0.0f))
			{
				// Get width/height of object based on rotation
				int width = m_BaseItem.Rotation == 0 || m_BaseItem.Rotation == 2 ? m_BaseItem.Width : m_BaseItem.Height;
				int height = m_BaseItem.Rotation == 0 || m_BaseItem.Rotation == 2 ? m_BaseItem.Height : m_BaseItem.Width;
				width = m_BaseItem.Rotation == 1 || m_BaseItem.Rotation == 2 ? -width : width;
				height = m_BaseItem.Rotation == 3 || m_BaseItem.Rotation == 2 ? -height : height;

				// Get the boundaries of the object in screen space based on main camera
				Vector3 topLeft = Camera.main.WorldToScreenPoint(transform.position);
				Vector3 bottomRight = new Vector3(transform.position.x + (float)width, transform.position.y + (float)height);
				bottomRight = Camera.main.WorldToScreenPoint(bottomRight);

				// Calculate alpha of texture based on firing/reload timer
				float alpha = 0.0f;
				float time = 0.0f;
				if (m_FiringTimer > 0.0f)
				{
					time = m_FiringTimer / m_FiringSpeed;
				}
				else if (m_ReloadTimer > 0.0f)
				{
					time = m_ReloadTimer / m_ReloadSpeed;
				}
				alpha = Mathf.Lerp(0.0f, 1.0f, time);

				// Draw texture with relevant alpha
				Color tempColor = GUI.color;
				GUI.color = new Color(1.0f, 1.0f, 1.0f, alpha);
				GUI.DrawTexture(new Rect(topLeft.x, Screen.height - topLeft.y, bottomRight.x - topLeft.x, bottomRight.y - topLeft.y), m_ActionTexture);
				GUI.color = tempColor;
			}

			if (m_IsEquipped)
			{
				int horizontalLineWidth = 3;
				int verticalLineWidth = 3;

				// Get width/height of object based on rotation
				int width = m_BaseItem.Rotation == 0 || m_BaseItem.Rotation == 2 ? m_BaseItem.Width : m_BaseItem.Height;
				int height = m_BaseItem.Rotation == 0 || m_BaseItem.Rotation == 2 ? m_BaseItem.Height : m_BaseItem.Width;
				width = m_BaseItem.Rotation == 1 || m_BaseItem.Rotation == 2 ? -width : width;
				height = m_BaseItem.Rotation == 3 || m_BaseItem.Rotation == 2 ? -height : height;
				verticalLineWidth = m_BaseItem.Rotation == 1 || m_BaseItem.Rotation == 2 ? -verticalLineWidth : verticalLineWidth;
				horizontalLineWidth = m_BaseItem.Rotation == 3 || m_BaseItem.Rotation == 2 ? -horizontalLineWidth : horizontalLineWidth;
				
				// Get the boundaries of the object in screen space based on main camera
				Vector3 topLeft = Camera.main.WorldToScreenPoint(transform.position);
				Vector3 bottomRight = new Vector3(transform.position.x + (float)width, transform.position.y - (float)height);
				bottomRight = Camera.main.WorldToScreenPoint(bottomRight);

				GUI.color = m_OutlineColour;
				GUI.DrawTexture(new Rect(topLeft.x, Screen.height - topLeft.y, verticalLineWidth, topLeft.y - bottomRight.y), m_OutlineTexture);
				GUI.DrawTexture(new Rect(bottomRight.x, Screen.height - topLeft.y, -verticalLineWidth, topLeft.y - bottomRight.y), m_OutlineTexture);
				GUI.DrawTexture(new Rect(topLeft.x, Screen.height - topLeft.y, bottomRight.x - topLeft.x, horizontalLineWidth), m_OutlineTexture);
				GUI.DrawTexture(new Rect(topLeft.x, Screen.height - bottomRight.y, bottomRight.x - topLeft.x, -horizontalLineWidth), m_OutlineTexture);
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (Time.timeScale > 0.0f)
		{
			if (m_FiringTimer > 0.0f)
			{
				m_FiringTimer -= Time.deltaTime;
			}
			
			if (m_ReloadTimer > 0.0f)
			{
				m_ReloadTimer -= Time.deltaTime;
			}
			else if (m_WaitToReload)
			{
				Reload();
				m_WaitToReload = false;
			}
		}
	}
	
	public virtual float GetDamage(float distance)
	{
		// Check if the weapon has hit at this distance
		// Weapons have full accuracy in ideal range
		// In effective range accuracy falls off linearly to half at max range
		float damage = 0.0f;
		if (distance < m_IdealRange)
		{
			if (Random.Range(0, 100) < m_Accuracy)
			{
				damage = m_Damage;
			}
		}
		else if (distance < m_EffectiveRange)
		{
			float lerpAmount = (distance - m_IdealRange) / (m_EffectiveRange - m_IdealRange);
			int realAccuracy = (int)Mathf.Lerp((float)m_Accuracy, (float)(m_Accuracy / 2), lerpAmount);
			if (Random.Range(0, 100) < realAccuracy)
			{
				damage = m_Damage;
			}
		}
		
		if (Random.Range(0, 100) < m_CritChance)
		{
			damage *= 3.0f;
		}
		
		return damage;
	}
	
	public bool CanFire()
	{
		if (m_FiringTimer <= 0.0f && m_ReloadTimer <= 0.0f && 
			!m_WaitToReload && m_BaseItem.FirstSpace != null)
		{
			return true;
		}
		return false;
	}
	
	public bool Fire(ref float actionTime)
	{
		if (m_FiringTimer <= 0.0f && m_ReloadTimer <= 0.0f)
		{
			if (m_AmmoLoaded > 0)
			{
				m_FiringTimer = m_FiringSpeed;
				actionTime = m_FiringSpeed;
				m_AmmoLoaded--;
				transform.FindChild("ItemText").guiText.text = m_AmmoLoaded.ToString();
				return true;
			}
			else
			{
				actionTime = BeginReloading();
			}
		}
		return false;
	}
	
	public bool NeedsReloading()
	{
		if (m_AmmoLoaded != m_Capacity)
		{
			HashSet<InventoryAmmo> ammoList = m_BaseItem.FirstSpace.inventory.FindAmmo(m_WeaponType);
			return ammoList.Count != 0;
		}
		return false;
	}
	
	public float BeginReloading()
	{
		if (m_ReloadTimer <= 0.0f)
		{
			int ammoToTake = m_Capacity - m_AmmoLoaded;
			if (ammoToTake > 0)
			{
				Vector3 ammoPosition = new Vector3();
				m_AmmoTaken = m_BaseItem.FirstSpace.inventory.TakeAmmo(m_WeaponType, ammoToTake, out ammoPosition);
				
				if (m_AmmoTaken > 0)
				{
					for (int i = 0; i < m_AmmoTaken; i++)
					{
						ammoPosition.z = -5.0f;
						Transform trans = Instantiate(m_ParticlePrefab, ammoPosition, Quaternion.identity) as Transform;
						ParticleAttract particle = trans.GetComponent<ParticleAttract>();
						particle.WeaponType = m_WeaponType;
						particle.AttractTarget = m_BaseItem;

						// Automatic weapons use loads of ammo 
						// so halve the particles or shit's gonna get real
						if (m_WeaponType == WeaponType.Automatic)
						{
							i++;
						}
					}

					m_WaitToReload = true;
					m_ReloadTimer = m_ReloadSpeed;
					return m_ReloadSpeed;
				}
			}
		}
		return 0.0f;
	}
	
	private void Reload()
	{
		if (m_AmmoTaken > 0)
		{
			m_AmmoLoaded += m_AmmoTaken;
			m_AmmoTaken = 0;
			transform.FindChild("ItemText").guiText.text = m_AmmoLoaded.ToString();
		}
	}
	
	public void UpgradeCapacity()
	{
		m_CapacityUpgradeLevel++;
		m_Capacity = m_WeaponStats.GetCapacity(m_WeaponName, m_CapacityUpgradeLevel);
	}
	
	public void UpgradePenetration()
	{
		m_PenetrationUpgradeLevel++;
		m_Penetration = m_WeaponStats.GetPenetration(m_WeaponName, m_PenetrationUpgradeLevel);
	}
	
	public void UpgradeDamage()
	{
		m_DamageUpgradeLevel++;
		m_Damage = m_WeaponStats.GetDamage(m_WeaponName, m_DamageUpgradeLevel);
	}
	
	public void UpgradeFiringSpeed()
	{
		m_FiringSpeedUpgradeLevel++;
		m_FiringSpeed = m_WeaponStats.GetFiringSpeed(m_WeaponName, m_FiringSpeedUpgradeLevel);
	}
	
	public void UpgradeReloadSpeed()
	{
		m_ReloadSpeedUpgradeLevel++;
		m_ReloadSpeed = m_WeaponStats.GetReloadSpeed(m_WeaponName, m_ReloadSpeedUpgradeLevel);
	}
	
	public void UpgradeRadius()
	{
		m_RadiusUpgradeLevel++;
		m_Radius = m_WeaponStats.GetRadius(m_WeaponName, m_RadiusUpgradeLevel);
	}
}