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
	[SerializeField] private Transform m_GameInfoPrefab;

	[SerializeField] private bool m_ChaosMode;
	[SerializeField] private bool m_TimeMode = true;
	[SerializeField] private bool m_ChallengeMode = false;
	[SerializeField] private bool m_EnableDifficultyScaling;
	[SerializeField] private bool m_ShowRotationBonus;
	[SerializeField] private int m_MoveLimit;

	[SerializeField] private ItemDropManager m_ItemDropManager;

	private const int DEFAULT_CHAOS_SCORE = 2000;
	private const int PER_ITEM_CHAOS_SCORE = 100;
	private const int LEFTOVER_PENALTY = 50;
	
	private int m_Score = 0;
	private int m_DisplayedScore = 0;
	private int m_ScoreIncrement = 0;
	private int m_ModeOptionSelect = 1;
	private int m_Distance = 0;
	private int m_MovesUsed = 0;
	private GameState m_GameState = GameState.Gameplay;
	private bool m_WaitForLevelEnd = false;
	private float m_LevelEndTimer = 0.0f;
	private bool m_ShowEffects = true;

	private float m_HealthScaling = 0.9f;
	private float m_DamageScaling = 0.9f;

	private int m_ShowingScoring = 0;
	private int m_ScoreGroupIndex = 0;
	private int m_OrbsToSpawn = 0;
	private float m_RoundTimer = 180.0f;
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
	public void MoveUsed() {m_MovesUsed++;}

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		DontDestroyOnLoad(Camera.main.gameObject);
		DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Tags.DISCARDAREA).gameObject);
		DontDestroyOnLoad(GameObject.FindObjectOfType<Light>().gameObject);

		// Get GameModeInfo script to determine the game mode for this session, then destroy
		GameObject infoObject = GameObject.FindGameObjectWithTag(Tags.GAMEMODEINFO);
		if (infoObject != null)
		{
			// Set the correct game mode (Timed/Moves and Order/Chaos) based on menu selection
			GameModeInfo gameInfo = infoObject.GetComponent<GameModeInfo>();
			m_TimeMode = gameInfo.m_TimeMode;
			m_ChaosMode = gameInfo.m_ChaosMode;
			m_ModeOptionSelect = gameInfo.m_ModeOptionSelect;

			// Set the correct time or move limit based on the menu selection
			if (gameInfo.m_ModeOptionSelect == 0)
			{
				if (m_TimeMode)
				{
					m_RoundTimer = 60.0f;
				}
				else
				{
					m_MoveLimit = 50;
				}
			}
			else if (gameInfo.m_ModeOptionSelect == 2)
			{
				if (m_TimeMode)
				{
					m_RoundTimer = 300.0f;
				}
				else
				{
					m_MoveLimit = 150;
				}
			}

			Destroy(gameInfo.gameObject);
		}

		//Application.LoadLevelAdditive(Tags.ACTIONSCENE);

		if (!m_ChallengeMode)
		{
			guiText.text = "SCORE: 0";
			SpawnItems();
		}
		else
		{
			m_RoundTimer = 0.0f;
		}
	}

	void OnGUI()
	{
		//if (m_Paused)
		if (m_GameState == GameState.Paused)
		{
			int top = Screen.height / 4;
			int side = Screen.width / 5;
			GUI.DrawTexture(new Rect(0, 0 , Screen.width, Screen.height), m_BackgroundTexture);

			if (GUI.Button(new Rect(side * 2, top, side, top / 2), "RESUME"))
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

			if (GUI.Button(new Rect(side * 2, top * 2, side, top / 2), "RESTART"))
			{
				Time.timeScale = 1.0f;
				DestroyAllObjects();
				Application.LoadLevel(Application.loadedLevel);
			}

			if (m_ChallengeMode)
			{
				if (GUI.Button(new Rect(side * 2, top * 3, side, top / 2), "LEVEL SELECT"))
				{
					Quit();
					
					// Create new GameModeInfo to show correct menu state
					Transform info = Instantiate(m_GameInfoPrefab) as Transform;
					GameModeInfo modeInfo = info.GetComponent<GameModeInfo>();
					modeInfo.m_ChallengeSelect = true;
				}
			}
			else
			{
				if (GUI.Button(new Rect(side * 2, top * 3, side, top / 2), "QUIT"))
				{
					Quit();
				}
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

			if (m_ChallengeMode)
			{
				if (GUI.Button(new Rect(Screen.width - 150, Screen.height / 2, 150, 100), "-DEBUG-\nCLEAR SCORES\n(CAN'T UNDO!)"))
				{
					// Construct string for deleting time/moves scores
					string pref = Tags.PREF_CHALLENGE_TIME + Application.loadedLevel;
					PlayerPrefs.DeleteKey(pref);
					pref = Tags.PREF_CHALLENGE_MOVES + Application.loadedLevel;
					PlayerPrefs.DeleteKey(pref);
				}

				if (Application.loadedLevel != Application.levelCount - 1)
				{
					if (GUI.Button(new Rect((screenQuarterX * 3) - 100, top, 200, 100), "NEXT LEVEL"))
					{
						DestroyAllObjects();
						Application.LoadLevel(Application.loadedLevel + 1);
					}
				}

				if (GUI.Button(new Rect(screenCentreX - 100, top, 200, 100), "TRY AGAIN"))
				{
					DestroyAllObjects();
					Application.LoadLevel(Application.loadedLevel);
				}

				if (GUI.Button(new Rect(screenQuarterX - 100, top, 200, 100), "LEVEL SELECT"))
				{
					Quit();

					// Create new GameModeInfo to show correct menu state
					Transform info = Instantiate(m_GameInfoPrefab) as Transform;
					GameModeInfo modeInfo = info.GetComponent<GameModeInfo>();
					modeInfo.m_ChallengeSelect = true;
				}
			}
			else
			{
				if (GUI.Button(new Rect((screenQuarterX * 3) - 100, top, 200, 100), "TRY AGAIN"))
				{
					DestroyAllObjects();

					// Create new GameModeInfo based on this game mode and reload level
					Transform info = Instantiate(m_GameInfoPrefab) as Transform;
					GameModeInfo modeInfo = info.GetComponent<GameModeInfo>();
					modeInfo.m_TimeMode = m_TimeMode;
					modeInfo.m_ChaosMode = m_ChaosMode;
					
					Application.LoadLevel(Tags.ORDERINVENTORY);
				}
				
				//if (GUI.Button(new Rect(screenCentreX - 100, top, 200, 100), "LEADERBOARD\n(Coming Soon)"))
				{

				}
				
				if (GUI.Button(new Rect(screenQuarterX - 100, top, 200, 100), "QUIT"))
				{
					Quit();
				}
			}
		}
		else
		{
			int screenCentreX = Screen.width / 2;
			if (!m_ChallengeMode)
			{
				if (GUI.Button(new Rect(screenCentreX - 100, 50, 200, 100), "CASH IN"))
				{
					AddScore();

					if (m_ItemScoringList.Count == 0 && m_RotationScoringList.Count == 0)
					{
						// Clear objects in inventory and spawn new items
						// (Score is now added when moving to next inventory)
						SpawnItems();
					}
				}
			}

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
						
						// Calculate alpha of texture based on flash timer
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

					//foreach (InventoryItem item in m_ItemScoringList[m_ScoreGroupIndex])
					foreach (InventoryItem item in m_ShowingScoring == 1 ? m_ItemScoringList[m_ScoreGroupIndex] : m_RotationScoringList[m_ScoreGroupIndex])
					{
						if (item != null)
						{
							//for (int i = 0; i < m_OrbsToSpawn / m_ItemScoringList[m_ScoreGroupIndex].Count; i++)
							for (int i = 0; i < m_OrbsToSpawn / (m_ShowingScoring == 1 ? m_ItemScoringList[m_ScoreGroupIndex].Count : m_RotationScoringList[m_ScoreGroupIndex].Count); i++)
							{
								Vector3 pos = item.CentrePosition;
								pos.z = -5.0f;
								Transform trans = Instantiate(m_ParticlePrefab, pos, Quaternion.identity) as Transform;
								ParticleAttract particle = trans.GetComponent<ParticleAttract>();
								particle.AttractPosition = new Vector3(3.5f, 0.5f, -5.0f);
							}
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
								m_ScoreOrbTimer = 1.4f;

								// Clear objects in inventory and spawn new items
								// (Score is now added when moving to next inventory)
								SpawnItems();
							}
						}
						else if (m_ShowingScoring == 2)
						{
							m_ShowingScoring = 0;
							Time.timeScale = 1.0f;
							m_ScoreGroupIndex = 0;
							m_ScoreOrbTimer = 1.4f;

							// Clear objects in inventory and spawn new items
							// (Score is now added when moving to next inventory)
							SpawnItems();
						}
					}
				}
			}
		}

		//if (Input.GetMouseButtonDown(3))
		//{
		//	LevelEnd();
		//}

		/*m_Timer -= Time.deltaTime;
		
		if (m_Timer <= 0.0f)
		{
			m_Timer = 5.0f;
			int currentScore = m_MainInventory.GetCurrentScore();
			m_Score += currentScore;
			guiText.text = "SCORE: " + m_Score.ToString() + " (+" + currentScore.ToString() + ")  MONEY: " + m_MainInventory.TotalMoney.ToString();
		}*/

		if (m_GameState == GameState.Gameplay)
		{
			if (m_ChallengeMode)
			{
				m_RoundTimer += Time.deltaTime;

				// In challenge mode, if the holding area is empty and no item carried, success!
				if (m_HoldingArea.GetNumberOfItems() == 0 && !m_MousePicker.IsCarrying)
				{
					GameOver();
				}
			}
			else
			{
				if (m_TimeMode)
				{
					// In time mode, game over when time runs out
					m_RoundTimer -= Time.deltaTime;
					if (m_RoundTimer <= 0.0f)
					{
						GameOver();
					}
				}
				else
				{
					// In moves mode, game over when all moves used
					if (m_MovesUsed >= m_MoveLimit)
					{
						GameOver();
					}
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if (m_GameState == GameState.Gameplay)
		{
			if (m_ChallengeMode)
			{
				guiText.text = "TIME: " + m_RoundTimer.ToString("F") + "          MOVES: " + m_MovesUsed.ToString();
			}
			else
			{
				m_ScoreOrbTimer -= Time.deltaTime;
				if (m_ScoreOrbTimer <= 0.0f)
				{
					if (m_DisplayedScore <= m_Score - 10)
					{
						m_DisplayedScore += (m_Score - m_DisplayedScore) / 9;
					}
					else
					{
						m_DisplayedScore = m_Score;
					}
				}

				string time = string.Format("{0}:{1:00}", ((int)m_RoundTimer + 1) / 60, ((int)m_RoundTimer + 1) % 60);
				string mode = m_TimeMode ? "TIME: " + time : "MOVES: " + (m_MoveLimit - m_MovesUsed).ToString();
				guiText.text = "SCORE: " + m_DisplayedScore.ToString() + (m_ScoreIncrement < 0 ? " (" : " (+") + 
					m_ScoreIncrement.ToString() + ") " + mode;// time + " MOVES: " + m_MovesUsed.ToString();//  MONEY: " + m_MainInventory.TotalMoney.ToString();
			}
		}
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
		Application.LoadLevel(Tags.MAINMENU);
	}

	public void GameOver()
	{
		//m_Distance += (int)GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<CharacterScript>().transform.position.x;

		if (m_ChallengeMode)
		{
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

			transform.position = new Vector3(0.5f, 0.6f, 0.0f);
			guiText.anchor = TextAnchor.MiddleCenter;
			guiText.alignment = TextAlignment.Center;
			guiText.text = "TIME:\n" + m_RoundTimer.ToString() + "\n";

			// Construct string for saving time/moves
			string timePref = Tags.PREF_CHALLENGE_TIME + Application.loadedLevel;
			float fastestTime = PlayerPrefs.GetFloat(timePref, 999999.9f);
			if (m_RoundTimer < fastestTime)
			{
				PlayerPrefs.SetFloat(timePref, m_RoundTimer);
				guiText.text += "(NEW BEST!)";
			}

			guiText.text += "\n\nMOVES:\n" + m_MovesUsed.ToString();
			string movesPref = Tags.PREF_CHALLENGE_MOVES + Application.loadedLevel;
			int leastMoves = PlayerPrefs.GetInt(movesPref, 9999);
			if (m_MovesUsed < leastMoves)
			{
				PlayerPrefs.SetInt(movesPref, m_MovesUsed);
				guiText.text += "\n(NEW BEST!)";
			}
		}
		else
		{
			AddScore();

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

			//Instantiate(m_DeathMessage, new Vector3(10.5f, -0.5f, 0.0f), Quaternion.identity);

			//guiText.enabled = false;
			transform.position = new Vector3(0.5f, 0.5f, 0.0f);
			guiText.anchor = TextAnchor.MiddleCenter;
			guiText.alignment = TextAlignment.Center;
			guiText.text = "FINAL SCORE:\n" + m_Score.ToString();

			// Get string for saving high score
			string scorePref;
			if (m_TimeMode)
			{
				if (m_ChaosMode)
				{
					scorePref = Tags.PREF_CHAOS_TIME_SCORE + m_ModeOptionSelect.ToString();
				}
				else
				{
					scorePref = Tags.PREF_ORDER_TIME_SCORE + m_ModeOptionSelect.ToString();
				}
			}
			else
			{
				if (m_ChaosMode)
				{
					scorePref = Tags.PREF_CHAOS_MOVE_SCORE + m_ModeOptionSelect.ToString();
				}
				else
				{
					scorePref = Tags.PREF_ORDER_MOVE_SCORE + m_ModeOptionSelect.ToString();
				}
			}

			int highScore = PlayerPrefs.GetInt(scorePref);
			if (m_Score > highScore)
			{
				PlayerPrefs.SetInt(scorePref, m_Score);
				guiText.text += "\n(NEW HIGH SCORE!)";
			}

			/*guiText.text += "\n\nTOTAL DISTANCE:\n" + m_Distance.ToString();
			int highDistance = PlayerPrefs.GetInt(Tags.PREF_DISTANCE);
			if (m_Distance > highDistance)
			{
				PlayerPrefs.SetInt(Tags.PREF_DISTANCE, m_Distance);
				guiText.text += "\n(NEW HIGH SCORE!)";
			}*/

			PlayerPrefs.Save();
		}
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
			m_ScoreIncrement = (PER_ITEM_CHAOS_SCORE * m_MainInventory.GetNumberOfItems()) - currentScore;
			//m_ScoreIncrement = DEFAULT_CHAOS_SCORE - currentScore;
		}
		else
		{
			currentScore = m_MainInventory.GetCurrentScore(m_ItemScoringList, m_RotationScoringList, false);
			m_ScoreIncrement = currentScore;
		}

		// Decrease score for each item left in the holding area (Unless it's the end of the game), then add to the total score
		if (m_TimeMode ? m_RoundTimer > 0.0f : m_MovesUsed < m_MoveLimit)
		{
			m_ScoreIncrement -= m_HoldingArea.GetNumberOfItems() * (LEFTOVER_PENALTY + (m_ChaosMode ? PER_ITEM_CHAOS_SCORE : 0));
		}
		m_Score += m_ScoreIncrement;
		m_Score = m_Score < 0 ? 0 : m_Score;

		if (!m_WaitForLevelEnd)
		{

			if (m_ItemScoringList.Count > 0)
			{
				m_OrbsToSpawn = (currentScore / m_ItemScoringList.Count) / 20;
				m_ShowingScoring = 1;
				m_GroupStartTime = Time.realtimeSinceStartup;
				Time.timeScale = 0.0f;
			}
			else if (m_RotationScoringList.Count > 0)
			{
				m_OrbsToSpawn = (currentScore / m_RotationScoringList.Count) / 20;
				m_ShowingScoring = 2;
				m_GroupStartTime = Time.realtimeSinceStartup;
				Time.timeScale = 0.0f;
			}
			else
			{
				// Clear objects in inventory and spawn new items
				// (Score is now added when moving to next inventory)
				//SpawnItems();
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

	IEnumerator WaitForRequest(WWW www)
	{
		yield return www;
		
		/* Check for errors */
		if (www.error == null)
		{
			Debug.Log("WWW Ok!: " + www.text);
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}    
	}

	private void SpawnItems()
	{
		m_MainInventory.ClearItems();
		m_HoldingArea.ClearItems();
		m_ItemDropManager.SpawnItems();
	}
}