using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
	enum GameState
	{
		Gameplay = 0,
		InBreak,
		Paused,
		GameOver,
	};

	public float m_TimeScale = 1.0f;

	[SerializeField] private InventoryScript m_MainInventory;
	[SerializeField] private InventoryScript m_HoldingArea;
	[SerializeField] private Texture2D m_BackgroundTexture;
	[SerializeField] private Texture2D m_HighlightTexture;
	[SerializeField] private MousePicker m_MousePicker;
	[SerializeField] private ShopMenu m_ShopMenu;
	[SerializeField] private Transform m_DeathMessage;
	[SerializeField] private Transform m_ParticlePrefab;

	[SerializeField] private bool m_ChaosMode;
	[SerializeField] private bool m_EnableDifficultyScaling;
	[SerializeField] private bool m_ShowRotationBonus;

	private const int DEFAULT_CHAOS_SCORE = 2000;
	
	private int m_Score = 0;
	private int m_DisplayedScore = 0;
	private int m_ScoreIncrement = 0;
	private int m_Distance = 0;
	private GameState m_GameState = GameState.Gameplay;
	private bool m_WaitForLevelEnd = false;
	private float m_LevelEndTimer = 0.0f;
	private bool m_ShowEffects = true;

	private float m_HealthScaling = 0.9f;
	private float m_DamageScaling = 0.9f;

	private int m_ShowingScoring = 0;
	private int m_ScoreGroupIndex = 0;
	private int m_OrbsToSpawn = 0;
	private float m_ScoreOrbTimer = 0.0f;
	private float m_GroupStartTime = 0.0f;
	private float m_ScoreGroupTimer = 0.0f;
	private float m_ScoreFlashTime = 0.2f;
	private bool m_FadingIn = true;
	private List<HashSet<InventoryItem>> m_ItemScoringList;
	private List<HashSet<InventoryItem>> m_RotationScoringList;

	public float HealthScaling { get {return m_HealthScaling;}}
	public float DamageScaling { get {return m_DamageScaling;}}
	public bool ShowEffects { get {return m_ShowEffects;} set {m_ShowEffects = value;}}

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		DontDestroyOnLoad(Camera.main.gameObject);
		DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Tags.DISCARDAREA).gameObject);
		DontDestroyOnLoad(GameObject.FindObjectOfType<Light>().gameObject);

		Application.LoadLevelAdditive(Tags.ACTIONSCENE);
		guiText.text = "SCORE: 0";
	}

	void OnGUI()
	{
		//if (m_Paused)
		if (m_GameState == GameState.Paused)
		{
			int top = Screen.height / 4;
			int side = Screen.width / 5;
			GUI.DrawTexture(new Rect(0, top , Screen.width, Screen.height), m_BackgroundTexture);

			if (GUI.Button(new Rect(side * 2, top * 2, side, top / 2), "RESUME"))
			{
				if (m_ShowingScoring == 0)
				{
					Time.timeScale = 1.0f;
				}

				//m_Paused = false;
				m_GameState = GameState.Gameplay;
				m_ShowEffects = true;
				m_MousePicker.Enabled = true;
				m_MainInventory.DrawLines = true;
				m_HoldingArea.DrawLines = true;
			}

			if (GUI.Button(new Rect(side * 2, top * 3, side, top / 2), "QUIT"))
			{
				Quit();
			}
		}
		else if (m_GameState == GameState.InBreak)
		{
			if (!m_ShopMenu.ShopShowing)
			{
				int top = Screen.height / 4;
				GUI.DrawTexture(new Rect(0, top , Screen.width, Screen.height), m_BackgroundTexture);

				int screenCentreX = Screen.width / 2;
				int screenCentreY = top + ((top * 3) / 2);
				int screenQuarterX = Screen.width / 4;

				if (GUI.Button(new Rect((screenQuarterX * 3) - 100, screenCentreY - 50, 200, 100), "CONTINUE"))
				{
					Application.LoadLevel(Tags.ACTIONSCENE);
					m_GameState = GameState.Gameplay;
					m_ShowEffects = true;
					m_MousePicker.Enabled = true;

					m_MainInventory.DrawLines = true;
					m_HoldingArea.DrawLines = true;
				}

				if (GUI.Button(new Rect(screenCentreX - 100, screenCentreY - 50, 200, 100), "SHOP"))
				{
					m_ShopMenu.ToggleShop();
				}

				if (GUI.Button(new Rect(screenQuarterX - 100, screenCentreY - 50, 200, 100), "QUIT"))
				{
					Quit();
				}
			}
		}
		else if (m_GameState == GameState.GameOver)
		{
			//GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), m_BackgroundTexture);
			int top = (Screen.height / 4) * 3;
			
			int screenCentreX = Screen.width / 2;
			//int screenCentreY = top + ((top * 3) / 2);
			int screenQuarterX = Screen.width / 4;
			
			if (GUI.Button(new Rect((screenQuarterX * 3) - 100, top, 200, 100), "TRY AGAIN"))
			{
				DestroyAllObjects();

				if (m_ChaosMode)
				{
					Application.LoadLevel(Tags.CHAOSINVENTORY);
				}
				else
				{
					Application.LoadLevel(Tags.ORDERINVENTORY);
				}
			}
			
			if (GUI.Button(new Rect(screenCentreX - 100, top, 200, 100), "LEADERBOARD\n(Coming Soon)"))
			{
				/*if (FB.IsLoggedIn)
				{
					var query = new Dictionary<string, string>();
					query["score"] = m_Score.ToString();
					FB.API("/me/scores", Facebook.HttpMethod.POST, delegate(FBResult r) { FbDebug.Log("Result: " + r.Text); }, query);
				}*/
			}
			
			if (GUI.Button(new Rect(screenQuarterX - 100, top, 200, 100), "QUIT"))
			{
				Quit();
			}
		}
		else
		{
			if (m_LevelEndTimer > 0.0f)
			{
				GUI.TextArea(new Rect((float)Screen.width * 0.73f, (float)Screen.height * 0.25f, (float)Screen.width * 0.192f, Screen.height / 30), m_LevelEndTimer.ToString());
			}

			if (m_ShowingScoring != 0)
			{
				foreach(InventoryItem item in m_ShowingScoring == 1 ? m_ItemScoringList[m_ScoreGroupIndex] : m_RotationScoringList[m_ScoreGroupIndex])
				{
					if (item != null)
					{
						// Get width/height of object based on rotation
						int width = item.Rotation == 0 || item.Rotation == 2 ? item.Width : item.Height;
						int height = item.Rotation == 0 || item.Rotation == 2 ? item.Height : item.Width;
						width = item.Rotation == 1 || item.Rotation == 2 ? -width : width;
						height = item.Rotation == 3 || item.Rotation == 2 ? -height : height;

						// Get the boundaries of the object in screen space based on main camera
						Vector3 topLeft = Camera.main.WorldToScreenPoint(item.transform.position);
						Vector3 bottomRight = new Vector3(item.transform.position.x + (float)width, item.transform.position.y + (float)height);
						bottomRight = Camera.main.WorldToScreenPoint(bottomRight);
						
						// Calculate alpha of texture based on firing/reload timer
						float time = m_ScoreGroupTimer / m_ScoreFlashTime;
						float alpha = 0.0f;
						if (m_FadingIn)
						{
							 alpha = Mathf.Lerp(0.0f, 1.0f, time);
						}
						else
						{
							alpha = Mathf.Lerp(1.0f, 0.0f, time);
						}
						
						// Draw texture with relevant alpha
						Color tempColor = GUI.color;
						GUI.color = new Color(1.0f, 1.0f, 1.0f, alpha);
						GUI.DrawTexture(new Rect(topLeft.x, Screen.height - topLeft.y, bottomRight.x - topLeft.x, bottomRight.y - topLeft.y), m_HighlightTexture);
						GUI.color = tempColor;
					}
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		if (Input.GetButtonDown("Pause"))
		{
			//m_Paused = !m_Paused;
			if (m_GameState == GameState.Gameplay)
			{
				Time.timeScale = 0.0f;
				m_MousePicker.Enabled = false;
				m_GameState = GameState.Paused;
				m_ShowEffects = false;

				m_MainInventory.DrawLines = false;
				m_HoldingArea.DrawLines = false;
			}
			else if (m_GameState == GameState.Paused)
			{
				if (m_ShowingScoring == 0)
				{
					Time.timeScale = 1.0f;
				}

				m_GameState = GameState.Gameplay;
				m_MousePicker.Enabled = true;
				m_ShowEffects = true;

				m_MainInventory.DrawLines = true;
				m_HoldingArea.DrawLines = true;
			}
		}

		if (m_WaitForLevelEnd)
		{
			m_LevelEndTimer -= Time.deltaTime;

			if ((m_HoldingArea.GetNumberOfItems() == 0 && !m_MousePicker.IsCarrying) || m_LevelEndTimer <= 0.0f)
			{
				// Clear all items from the holding area and add to the score
				m_HoldingArea.ClearItems();
				AddScore();

				// Prepare for the next level, and then load the break area
				Application.LoadLevel(Tags.BREAKAREA);
				m_GameState = GameState.InBreak;
				m_ShowEffects = false;
				m_MousePicker.Enabled = false;
				
				m_MainInventory.DrawLines = false;
				m_HoldingArea.DrawLines = false;

				m_WaitForLevelEnd = false;
				m_LevelEndTimer = 0.0f;
				m_Distance += 100;
			}
		}

		// Timer for showing scoring groups
		if (m_GameState == GameState.Gameplay && m_ShowingScoring != 0)
		{
			m_ScoreGroupTimer = Time.realtimeSinceStartup - m_GroupStartTime;
			if (m_ScoreGroupTimer >= m_ScoreFlashTime)
			{
				if (m_FadingIn)
				{
					m_ScoreGroupTimer = 0.0f;
					m_GroupStartTime = Time.realtimeSinceStartup;
					m_FadingIn = false;

					foreach (InventoryItem item in m_ItemScoringList[m_ScoreGroupIndex])
					{
						for (int i = 0; i < m_OrbsToSpawn / m_ItemScoringList[m_ScoreGroupIndex].Count; i++)
						{
							Vector3 pos = item.CentrePosition;
							pos.z = -5.0f;
							Transform trans = Instantiate(m_ParticlePrefab, pos, Quaternion.identity) as Transform;
							ParticleAttract particle = trans.GetComponent<ParticleAttract>();
							particle.AttractPosition = new Vector3(3.5f, 0.0f, -5.0f);
						}
					}
				}
				else
				{
					m_ScoreGroupIndex++;
					m_ScoreGroupTimer = 0.0f;
					m_GroupStartTime = Time.realtimeSinceStartup;
					m_FadingIn = true;
					if (m_ScoreGroupIndex >= (m_ShowingScoring == 1 ? m_ItemScoringList.Count : m_RotationScoringList.Count))
					{
						if (m_ShowingScoring == 1)
						{
							if (m_ShowRotationBonus && m_RotationScoringList.Count > 0)
							{
								m_ShowingScoring = 2;
								m_ScoreGroupIndex = 0;
							}
							else
							{
								m_ShowingScoring = 0;
								Time.timeScale = 1.0f;
								m_ScoreGroupIndex = 0;
								m_ScoreOrbTimer = 1.5f;
							}
						}
						else if (m_ShowingScoring == 2)
						{
							m_ShowingScoring = 0;
							Time.timeScale = 1.0f;
							m_ScoreGroupIndex = 0;
							m_ScoreOrbTimer = 1.5f;
						}
					}
				}
			}
		}

		if (Input.GetMouseButtonDown(3))
		{
			LevelEnd();
		}

		/*m_Timer -= Time.deltaTime;
		
		if (m_Timer <= 0.0f)
		{
			m_Timer = 5.0f;
			int currentScore = m_MainInventory.GetCurrentScore();
			m_Score += currentScore;
			guiText.text = "SCORE: " + m_Score.ToString() + " (+" + currentScore.ToString() + ")  MONEY: " + m_MainInventory.TotalMoney.ToString();
		}*/
	}

	private void FixedUpdate()
	{
		m_ScoreOrbTimer -= Time.deltaTime;
		if (m_ScoreOrbTimer <= 0.0f)
		{
			if (m_DisplayedScore <= m_Score - 10)
			{
				m_DisplayedScore += (m_Score - m_DisplayedScore) / 6;
			}
			else
			{
				m_DisplayedScore = m_Score;
			}
		}
		guiText.text = "SCORE: " + m_DisplayedScore.ToString() + " (+" + m_ScoreIncrement.ToString() + ")  MONEY: " + m_MainInventory.TotalMoney.ToString();
	}

	private void DestroyAllObjects()
	{
		// Manually get and destroy all objects in the scene
		// (Because everything on the bottom is set to not destroy on load)
		Transform[] objectList = GameObject.FindObjectsOfType<Transform>();
		foreach (Transform trans in objectList)
		{
			Destroy(trans.gameObject);
		}
	}

	private void Quit()
	{
		DestroyAllObjects();
		
		Time.timeScale = 1.0f;
		Application.LoadLevelAdditive(Tags.MAINMENU);
	}

	public void GameOver()
	{
		m_Distance += (int)GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<CharacterScript>().transform.position.x;

		// Manually get and destroy all objects in the scene
		// (Because everything on the bottom is set to not destroy on load)
		Transform[] objectList = GameObject.FindObjectsOfType<Transform>();
		foreach (Transform trans in objectList)
		{
			if (trans.gameObject != gameObject && trans.gameObject != Camera.main.gameObject)
			{
				Destroy(trans.gameObject);
			}
		}
		
		Time.timeScale = 1.0f;
		m_GameState = GameState.GameOver;
		m_ShowEffects = false;
		Camera.main.rect = new Rect(0, 0, 1, 1);

		Instantiate(m_DeathMessage, new Vector3(11.0f, -0.5f, 0.0f), Quaternion.identity);

		//guiText.enabled = false;
		transform.position = new Vector3(0.5f, 0.5f, 0.0f);
		guiText.anchor = TextAnchor.MiddleCenter;
		guiText.alignment = TextAlignment.Center;
		guiText.text = "FINAL SCORE:\n" + m_Score.ToString();

		int highScore = m_ChaosMode ? PlayerPrefs.GetInt(Tags.PREF_CHAOSSCORE) : PlayerPrefs.GetInt(Tags.PREF_ORDERSCORE);
		if (m_Score > highScore)
		{
			PlayerPrefs.SetInt(m_ChaosMode ? Tags.PREF_CHAOSSCORE : Tags.PREF_ORDERSCORE, m_Score);
			guiText.text += "\n(NEW HIGH SCORE!)";
		}

		guiText.text += "\n\nTOTAL DISTANCE:\n" + m_Distance.ToString();
		int highDistance = PlayerPrefs.GetInt(Tags.PREF_DISTANCE);
		if (m_Distance > highDistance)
		{
			PlayerPrefs.SetInt(Tags.PREF_DISTANCE, m_Distance);
			guiText.text += "\n(NEW HIGH SCORE!)";
		}

		PlayerPrefs.Save();
	}

	public void AddScore()
	{
		// If in chaos mode score is subtracted from a default score, then added to total score
		// otherwise simply add current score to the total score
		m_ItemScoringList = new List<HashSet<InventoryItem>>();
		m_RotationScoringList = new List<HashSet<InventoryItem>>();
		int currentScore = 0;

		if (m_ChaosMode)
		{
			currentScore = m_MainInventory.GetCurrentScore(m_ItemScoringList, m_RotationScoringList, true);
			m_ScoreIncrement = DEFAULT_CHAOS_SCORE - currentScore;
		}
		else
		{
			currentScore = m_MainInventory.GetCurrentScore(m_ItemScoringList, m_RotationScoringList, false);
			m_ScoreIncrement = currentScore;
		}

		m_Score += m_ScoreIncrement;

		if (!m_WaitForLevelEnd)
		{
			m_OrbsToSpawn = (currentScore / m_ItemScoringList.Count) / 20;
			if (m_ItemScoringList.Count > 0)
			{
				m_ShowingScoring = 1;
				m_GroupStartTime = Time.realtimeSinceStartup;
				Time.timeScale = 0.0f;
			}
			else if (m_RotationScoringList.Count > 0)
			{
				m_ShowingScoring = 2;
				m_GroupStartTime = Time.realtimeSinceStartup;
				Time.timeScale = 0.0f;
			}
		}
	}

	public void LevelEnd()
	{
		m_WaitForLevelEnd = true;
		m_LevelEndTimer = 5.0f;
	}

	public void ScaleHealth()
	{
		if (m_EnableDifficultyScaling)
		{
			m_HealthScaling *= 1.01f;
		}
	}

	public void ScaleDamage()
	{
		if (m_EnableDifficultyScaling)
		{
			m_DamageScaling *= 1.01f;
		}
	}
}