using UnityEngine;
using System.Collections;
using System.Linq;

public class CharacterScript : MonoBehaviour
{	
	private const float INVINCIBLE_TIME = 0.0f;
	private const float FLASH_TIME = 0.25f;

	[SerializeField] private Transform m_GrenadePrefab;
	[SerializeField] private Transform m_FlashGrenadePrefab;
	[SerializeField] private Transform m_FireGrenadePrefab;
	[SerializeField] private InventoryGrenade m_FragGrenade;
	[SerializeField] private InventoryGrenade m_FlashGrenade;
	[SerializeField] private InventoryGrenade m_IncendiaryGrenade;

	[SerializeField] private Sprite[] m_SpriteList;

	[SerializeField] private Vector2 m_HealthBarPos = new Vector2(0, 0);
	[SerializeField] private Vector2 m_HealthBarSize = new Vector2(400, 40);
	[SerializeField] private Texture2D m_EmptyTex;
	[SerializeField] private Texture2D m_FullTex;
	[SerializeField] private float m_Speed;
	
	private GUIStyle m_BarStyle = new GUIStyle();
	private float m_ActionTimer = 0.0f;
	private float m_BarPercentage = 1.0f;
	private float m_TargetBarPercent = 1.0f;
	private float m_Health;
	private float m_MaxHealth = 100.0f;
	private float m_ArmourModifier = 1.0f;
	private bool m_Active = true;
	private bool m_Waiting = false;
	private bool m_Reloading = false;

	private bool m_Invincible = false;
	private float m_InvincibleTimer = 0.0f;
	private float m_FlashTimer = 0.0f;

	private RaycastHit2D[] m_RaycastHitList = new RaycastHit2D[20];

	//private Animator m_BodyAnimator;
	private Animator m_LegsAnimator;
	//private SpriteRenderer m_BodyRenderer;
	private SpriteRenderer m_LegsRenderer;
	private InventoryWeapon m_EquippedWeapon = null;
	private InventoryWeapon m_PreviousWeapon = null;

	public InventoryGrenade FragUpgrader { get {return m_FragGrenade;}}
	public InventoryGrenade FlashUpgrader { get {return m_FlashGrenade;}}
	public InventoryGrenade IncendiaryUpgrader { get {return m_IncendiaryGrenade;}}

	public bool Active { get {return m_Active;} set {m_Active = value;}}
	public bool WaitByDoor { get {return m_Waiting;} set {m_Waiting = value;}}

	public InventoryWeapon EquippedWeapon
	{
		get {return m_EquippedWeapon;} 

		set
		{
			if (m_EquippedWeapon != null)
			{
				if (m_EquippedWeapon.GetType() == typeof(InventoryWeapon))
				{
					m_PreviousWeapon = m_EquippedWeapon;
				}
			}
			m_EquippedWeapon = value;

			if (m_EquippedWeapon != null)
			{
				m_LegsRenderer.sprite = m_SpriteList[(int)m_EquippedWeapon.WeaponName];
			}
			else
			{
				m_LegsRenderer.sprite = m_SpriteList[m_SpriteList.Length - 1];
			}
		}
	}
	
	void Awake()
	{
		//DontDestroyOnLoad(gameObject);
		m_Health = m_MaxHealth;

		//m_BodyAnimator = transform.FindChild("Body").GetComponent<Animator>();
		//m_LegsAnimator = transform.FindChild("Legs").GetComponent<Animator>();
		m_LegsAnimator = transform.FindChild("Sprite").GetComponent<Animator>();

		//m_BodyRenderer = transform.FindChild("Body").GetComponent<SpriteRenderer>();
		//m_LegsRenderer = transform.FindChild("Legs").GetComponent<SpriteRenderer>();
		m_LegsRenderer = transform.FindChild("Sprite").GetComponent<SpriteRenderer>();
	}

	void OnLevelWasLoaded(int level)
	{
		if (level == Tags.ACTIONSCENE || level == Tags.BREAKAREA)
		{
			Reset();
		}

		if (level == Tags.BREAKAREA)
		{
			m_Active = false;
			m_LegsAnimator.SetBool("Walking", false);
			m_LegsAnimator.SetBool("Running", false);
		}
		else
		{
			m_Active = true;
			m_Waiting = false;
		}
	}
	 
	void OnGUI()
	{
		// Draw the unfilled background
		GUI.BeginGroup(new Rect(m_HealthBarPos.x, m_HealthBarPos.y, m_HealthBarSize.x, m_HealthBarSize.y));
		GUI.Box(new Rect(0,0, m_HealthBarSize.x, m_HealthBarSize.y), m_EmptyTex, m_BarStyle);
		 
		// Draw the filled-in health
		GUI.BeginGroup(new Rect(0,0, m_HealthBarSize.x * m_BarPercentage, m_HealthBarSize.y));
		GUI.Box(new Rect(0,0, m_HealthBarSize.x, m_HealthBarSize.y), m_FullTex, m_BarStyle);
		GUI.EndGroup();
		GUI.EndGroup();
	}
	 
	void FixedUpdate()
	{
		if (m_Invincible)
		{
			m_InvincibleTimer -= Time.deltaTime;
			m_FlashTimer -= Time.deltaTime;

			if (m_InvincibleTimer <= 0.0f)
			{
				m_Invincible = false;
				//m_BodyRenderer.enabled = true;
				m_LegsRenderer.enabled = true;
			}
			else if (m_FlashTimer <= 0.0f)
			{
				//m_BodyRenderer.enabled = !m_BodyRenderer.enabled;
				m_LegsRenderer.enabled = !m_LegsRenderer.enabled;
				m_FlashTimer = FLASH_TIME;
			}
		}

		// Fill the health bar to the relevant percentage
		m_TargetBarPercent = m_Health / m_MaxHealth;
		float easeValue = m_TargetBarPercent - m_BarPercentage;
		easeValue *= (0.6f * Time.deltaTime) * 8.0f;
		m_BarPercentage += easeValue;

		if (m_Active)
		{
			m_ActionTimer -= Time.deltaTime;

			// If there's no weapon equipped simple check if there are any enemies in front of the player
			Vector2 rayPos = (Vector2)transform.position;
			rayPos.x += 0.8f;
			rayPos.y += 1.6f;
			if (m_EquippedWeapon == null)
			{
				bool enemyPresent = false;
				int numHits = Physics2D.RaycastNonAlloc(rayPos, new Vector2(1.0f, 0.0f), m_RaycastHitList, 1.6f);
				for (int i = 0; i < numHits; i++)
				{
					if (m_RaycastHitList[i].collider.tag == Tags.ENEMY)
					{
						enemyPresent = true;
					}
				}

				// If no enemies present, move forward at half speed
				if (!enemyPresent && !m_Waiting)
				{
					Vector3 pos = transform.position;
					pos.x += (m_Speed * 0.5f) * Time.deltaTime;
					transform.position = pos;

					m_LegsAnimator.SetBool("Walking", true);
					m_LegsAnimator.SetBool("Running", false);
				}
				else
				{
					m_LegsAnimator.SetBool("Walking", false);
					m_LegsAnimator.SetBool("Running", false);
				}
			}
			// If there is a weapon equipped get all enemies in the effective range of the equipped weapon
			else
			{
				bool enemyPresent = false;
				bool enemyClose = false;
				int numHits = Physics2D.RaycastNonAlloc(rayPos, new Vector2(1.0f, 0.0f), m_RaycastHitList, m_EquippedWeapon.EffectiveRange);
				for (int i = 0; i < numHits; i++)
				{
					if (m_RaycastHitList[i].collider.tag == Tags.ENEMY)
					{
						enemyPresent = true;

						if (m_RaycastHitList[i].fraction * m_EquippedWeapon.EffectiveRange <= 1.6f)
						{
							enemyClose = true;
						}
					}
				}

				// (If there is an enemy right up close stop moving completely)
				if (!enemyClose && !m_Waiting)
				{
					// If there is an enemy in effective range, or if performing an action
					// (Like firing or reloading) move forwards at half speed
					if (enemyPresent || m_ActionTimer > 0.0f)
					{
						Vector3 pos = transform.position;
						pos.x += (m_Speed * 0.5f) * Time.deltaTime;
						transform.position = pos;

						m_LegsAnimator.SetBool("Walking", true);
						m_LegsAnimator.SetBool("Running", false);
					}
					// Otherwise move at full speed
					else if (!enemyClose)
					{
						Vector3 pos = transform.position;
						pos.x += m_Speed * Time.deltaTime;
						transform.position = pos;

						m_LegsAnimator.SetBool("Walking", false);
						m_LegsAnimator.SetBool("Running", true);
					}
				}
				else
				{
					m_LegsAnimator.SetBool("Walking", false);
					m_LegsAnimator.SetBool("Running", false);
				}

				// If the weapon can fire (Cooldown ended, has ammo)
				if (m_ActionTimer <= 0.0f)
				{
					m_Reloading = false;

					if (m_EquippedWeapon.CanFire())
					{
						if (m_EquippedWeapon.WeaponType == WeaponType.Grenade)
						{
							// Spawn and throw the relevant grenade projectile
							InventoryGrenade grenade = m_EquippedWeapon.GetComponent<InventoryGrenade>();
							if (grenade.GrenadeType == GrenadeType.Frag)
							{
								Transform trans = Instantiate(m_GrenadePrefab, transform.position + new Vector3(0.3f, 1.0f, 0.0f), Quaternion.identity) as Transform;
								ProjectileGrenade projectile = trans.GetComponent<ProjectileGrenade>();
								if (projectile != null)
								{
									projectile.DamageUpgradeLevel = m_FragGrenade.DamageLevel;
									projectile.RadiusUpgradeLevel = m_FragGrenade.RadiusLevel;
									projectile.UpdateStats();
								}
							}
							else if (grenade.GrenadeType == GrenadeType.Flash)
							{
								Transform trans = Instantiate(m_FlashGrenadePrefab, transform.position + new Vector3(0.3f, 1.0f, 0.0f), Quaternion.identity) as Transform;
								ProjectileFlashGrenade projectile = trans.GetComponent<ProjectileFlashGrenade>();
								if (projectile != null)
								{
									projectile.DamageUpgradeLevel = m_FlashGrenade.DamageLevel;
									projectile.RadiusUpgradeLevel = m_FlashGrenade.RadiusLevel;
									projectile.UpdateStats();
								}
							}
							else if (grenade.GrenadeType == GrenadeType.Incendiary)
							{
								Transform trans = Instantiate(m_FireGrenadePrefab, transform.position + new Vector3(0.3f, 1.0f, 0.0f), Quaternion.identity) as Transform;
								ProjectileIncendiaryGrenade projectile = trans.GetComponent<ProjectileIncendiaryGrenade>();
								if (projectile != null)
								{
									projectile.DamageUpgradeLevel = m_IncendiaryGrenade.DamageLevel;
									projectile.RadiusUpgradeLevel = m_IncendiaryGrenade.RadiusLevel;
									projectile.UpdateStats();
								}
							}

							//m_BodyAnimator.SetTrigger("Throw");

							// Destroy grenade in inventory
							m_EquippedWeapon.BaseItem.FirstSpace.inventory.DestroyItem(m_EquippedWeapon.BaseItem);
							m_ActionTimer = grenade.FiringSpeed;

							// Re-equip the old weapon after throwing the grenade
							m_EquippedWeapon = m_PreviousWeapon;
							if (m_EquippedWeapon != null)
							{
								m_EquippedWeapon.transform.FindChild("EquippedHighlight").GetComponent<MeshRenderer>().enabled = true;
								m_EquippedWeapon.SetEquipped(true);
							}
						}
						else
						{
							// Get all enemies in the effective range of the currently equipped weapon
							//Vector2 rayPos = (Vector2)transform.position;
							//rayPos.x += 0.25f;
							//rayPos.y += 0.5f;
							/*RaycastHit2D[] hitArray = Physics2D.RaycastAll(rayPos, new Vector2(1.0f, 0.0f), m_EquippedWeapon.EffectiveRange).
								OrderBy(h => (h.fraction * m_EquippedWeapon.EffectiveRange)).ToArray();

							bool enemyPresent = false;
							foreach(RaycastHit2D hitInfo in hitArray)
							{
								if (hitInfo.collider.tag == Tags.ENEMY)
								{
									enemyPresent = true;
								}
							}*/

							int hitsLeft = m_EquippedWeapon.Penetration + 1;
							if (enemyPresent)
							{
								// Order the list to have closer enemies at the beginning
								m_RaycastHitList.OrderBy(h => (h.fraction * m_EquippedWeapon.EffectiveRange)).ToArray();

								// Fire the weapon and see which enemies were hit, based on weapon penetration
								if (m_EquippedWeapon.Fire(ref m_ActionTimer))
								{
									//m_BodyAnimator.SetTrigger("Fire");
									m_LegsAnimator.SetTrigger("Fire");

									//foreach(RaycastHit2D hitInfo in hitArray)
									//foreach(RaycastHit2D hitInfo in m_RaycastHitList)
									for (int i = 0; i < numHits; i++)
									{
										//EnemyAI enemy = hitInfo.collider.GetComponent<EnemyAI>();
										EnemyAI enemy = m_RaycastHitList[i].collider.GetComponent<EnemyAI>();
										if (enemy != null && hitsLeft > 0)
										{
											// Then get and deal the necessary amount of damage
											float damage = m_EquippedWeapon.GetDamage(m_RaycastHitList[i].fraction * m_EquippedWeapon.EffectiveRange);
											if (damage > 0.0f)
											{
												enemy.DealDamage(damage);
											}
											hitsLeft--;
										}
									}
								}
								else
								{
									if (m_ActionTimer > 0.0f)
									{
										//m_BodyAnimator.SetTrigger("Reload");
										m_LegsAnimator.SetTrigger("Reload");
										m_Reloading = true;
									}
								}
							}
							else
							{
								if (m_EquippedWeapon.NeedsReloading())
								{
									m_ActionTimer = m_EquippedWeapon.BeginReloading();
									//m_BodyAnimator.SetTrigger("Reload");
									m_LegsAnimator.SetTrigger("Reload");
									m_Reloading = true;
								}
								else
								{
									/*Vector3 pos = transform.position;
									pos.x += m_Speed * Time.deltaTime;
									transform.position = pos;*/
								}
							}
						}
					}
				}
			}
		}
	}
	
	public void AlterHealth(float amount)
	{
		if ((!m_Invincible && m_Active) || amount > 0.0f)
		{
			if (amount < 0.0f)
			{
				amount *= m_ArmourModifier;
			}
			
			m_Health += amount;
			m_Health = Mathf.Max(0.0f, Mathf.Min(m_Health, m_MaxHealth));

			if (amount < 0.0f)
			{
				if (m_Health <= 0.0f)
				{
					//m_BodyRenderer.enabled = false;
					m_LegsAnimator.SetTrigger("Death");
					
					m_Active = false;
				}
				else
				{
					m_Invincible = true;
					m_InvincibleTimer = INVINCIBLE_TIME;
				}
			}
		}
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == Tags.ITEMDROP)
		{
			ItemDrop itemDrop = other.GetComponent<ItemDrop>();

			// Handle creation of inventory item based on drop
			if (itemDrop.PickupDrop())
			{
				// Destroy the drop if there's space in holding area
				Destroy(other.gameObject);
			}
		}
		else if (other.tag == Tags.DISTANCEMARKER)
		{
			GameController controller = GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>();
			controller.AddScore();
			controller.ScaleHealth();
			controller.ScaleDamage();

			EnemySpawner spawner = GameObject.FindGameObjectWithTag(Tags.ENEMYSPAWNER).GetComponent<EnemySpawner>();
			spawner.ScaleHealth();
			spawner.ScaleDamage();
		}
		else if (other.tag == Tags.LEVELENDMARKER)
		{
			GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>().LevelEnd();
			m_Waiting = true;

			GameController controller = GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>();
			controller.ScaleHealth();
			controller.ScaleDamage();
			
			EnemySpawner spawner = GameObject.FindGameObjectWithTag(Tags.ENEMYSPAWNER).GetComponent<EnemySpawner>();
			spawner.ScaleHealth();
			spawner.ScaleDamage();
		}
	}

	public void Reset()
	{
		transform.position = GameObject.FindGameObjectWithTag(Tags.PLAYERSPAWN).transform.position;
		//Vector3 pos = transform.position;
		//pos.x = -11.0f;
		//transform.position = pos;
	}

	public void ModifyArmour(float modifier)
	{
		m_ArmourModifier *= modifier;
	}

	public void ThrowGrenade(GrenadeType grenadeType)
	{
		// Spawn and throw the relevant grenade projectile
		if (grenadeType == GrenadeType.Frag)
		{
			Transform trans = Instantiate(m_GrenadePrefab, transform.position + new Vector3(1.0f, 3.2f, 0.0f), Quaternion.identity) as Transform;
			ProjectileGrenade projectile = trans.GetComponent<ProjectileGrenade>();
			if (projectile != null)
			{
				projectile.DamageUpgradeLevel = m_FragGrenade.DamageLevel;
				projectile.RadiusUpgradeLevel = m_FragGrenade.RadiusLevel;
				projectile.UpdateStats();
			}
		}
		else if (grenadeType == GrenadeType.Flash)
		{
			Transform trans = Instantiate(m_FlashGrenadePrefab, transform.position + new Vector3(1.0f, 3.2f, 0.0f), Quaternion.identity) as Transform;
			ProjectileFlashGrenade projectile = trans.GetComponent<ProjectileFlashGrenade>();
			if (projectile != null)
			{
				projectile.DamageUpgradeLevel = m_FlashGrenade.DamageLevel;
				projectile.RadiusUpgradeLevel = m_FlashGrenade.RadiusLevel;
				projectile.UpdateStats();
			}
		}
		else if (grenadeType == GrenadeType.Incendiary)
		{
			Transform trans = Instantiate(m_FireGrenadePrefab, transform.position + new Vector3(1.0f, 3.2f, 0.0f), Quaternion.identity) as Transform;
			ProjectileIncendiaryGrenade projectile = trans.GetComponent<ProjectileIncendiaryGrenade>();
			if (projectile != null)
			{
				projectile.DamageUpgradeLevel = m_IncendiaryGrenade.DamageLevel;
				projectile.RadiusUpgradeLevel = m_IncendiaryGrenade.RadiusLevel;
				projectile.UpdateStats();
			}
		}

		if (!m_Reloading)
		{
			//m_BodyAnimator.SetTrigger("Throw");
		}
	}
}