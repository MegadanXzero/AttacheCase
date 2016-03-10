using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

	// Gameplay objects
	[SerializeField] private InventoryScript m_MainInventory;
	[SerializeField] private InventoryScript m_HoldingArea;
	[SerializeField] private Texture2D m_BackgroundTexture;
	[SerializeField] private Texture2D m_HighlightTexture;
	[SerializeField] private MousePicker m_MousePicker;
	[SerializeField] private ShopMenu m_ShopMenu;
	[SerializeField] private Transform m_DeathMessage;
	[SerializeField] private Transform m_ParticlePrefab;
	[SerializeField] private Transform m_GameInfoPrefab;

	// Mode information
	[SerializeField] private bool m_ChaosMode;
	[SerializeField] private bool m_TimeMode = true;
	[SerializeField] private bool m_ChallengeMode = false;
	[SerializeField] private bool m_Tutorial = false;
	[SerializeField] private bool m_EnableDifficultyScaling;
	[SerializeField] private bool m_ShowRotationBonus;
	[SerializeField] private int m_MoveLimit;
	
	[SerializeField] private ItemDropManager m_ItemDropManager;

	// GUI objects
	[SerializeField] private Canvas m_MainCanvas;
	[SerializeField] private Canvas m_PauseCanvas;
	[SerializeField] private Canvas m_OptionsCanvas;
	[SerializeField] private Canvas m_OverCanvas;
	[SerializeField] private Canvas m_CountdownCanvas;
	[SerializeField] private Text m_ScoreText;
	[SerializeField] private Text m_LimitText;
	[SerializeField] private Text m_GameOverText;
	[SerializeField] private Text m_CountdownText;
	[SerializeField] private GameObject m_HighScoreText;
	[SerializeField] private GameObject m_AuthenticationCanvas;
	[SerializeField] private Button m_ButtonLeaderboard;
	[SerializeField] private Button m_ButtonSignIn;
	[SerializeField] private Image m_MedalSmall;
	[SerializeField] private Image m_MedalScore;

	// GUI Medal Images
	[SerializeField] private Sprite m_MedalImage_Gold;
	[SerializeField] private Sprite m_MedalImage_Silver;
	[SerializeField] private Sprite m_MedalImage_Bronze;
	[SerializeField] private Sprite m_MedalImage_Gold_S;
	[SerializeField] private Sprite m_MedalImage_Silver_S;
	[SerializeField] private Sprite m_MedalImage_Bronze_S;

	// Sound Effects
	[SerializeField] private AudioClip m_SoundCountdownBoop;
	[SerializeField] private AudioClip m_SoundAlertBeep;
	[SerializeField] private AudioClip m_SoundGameOver;
	[SerializeField] private AudioClip m_SoundFlashBoop;
	[SerializeField] private AudioClip m_SoundScoreCount;

	private const int DEFAULT_CHAOS_SCORE = 2000;
	private const int PER_ITEM_CHAOS_SCORE = 30;
	private const int LEFTOVER_PENALTY = 50;
	
	private int m_Score = 0;
	private int m_DisplayedScore = 0;
	private int m_ScoreIncrement = 0;
	private int m_ModeOptionSelect = 1;
	//private int m_Distance = 0;
	private int m_MovesUsed = 0;
	private int m_MovesUsedThisInventory = 0;
	private int m_InventoriesSorted = 0;
	private GameState m_GameState = GameState.Gameplay;
	private bool m_WaitForLevelEnd = false;
	//private float m_LevelEndTimer = 0.0f;
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
	private float m_AlertTimer = 0.5f;
	private float m_CountdownTimer = 3.5f;
	private bool m_FadingIn = true;
	private List<HashSet<InventoryItem>> m_ItemScoringList;
	private List<HashSet<InventoryItem>> m_RotationScoringList;

	public float HealthScaling { get {return m_HealthScaling;}}
	public float DamageScaling { get {return m_DamageScaling;}}
	public int MovesUsed { get { return m_MovesUsed; } }
	public bool ShowEffects { get {return m_ShowEffects;} set {m_ShowEffects = value;}}

	// DEBUG VARIABLES // - Can be removed
	//private int DEBUG_ItemID = 0;

	void Awake()
	{
		//DontDestroyOnLoad(gameObject);
		//DontDestroyOnLoad(Camera.main.gameObject);
		//DontDestroyOnLoad(GameObject.FindGameObjectWithTag(Tags.DISCARDAREA).gameObject);
		//DontDestroyOnLoad(GameObject.FindObjectOfType<Light>().gameObject);

		// Change camera aspect ratio to fit everything in frame
		float aspectRatio = (float)Screen.width / (float)Screen.height;
		aspectRatio = Mathf.Min(aspectRatio, 1.78f);
		Camera.main.orthographicSize = 9.0f / aspectRatio;

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

		if (m_TimeMode && !m_ChallengeMode)
		{
			m_MousePicker.Enabled = false;
		}

		if (!m_TimeMode)
		{
			m_CountdownText.gameObject.SetActive(false);
			m_CountdownCanvas.gameObject.SetActive(false);
			m_MainCanvas.gameObject.SetActive(true);

			// Find the animated background and move it to the back
			GameObject bgObject = GameObject.FindGameObjectWithTag(Tags.ANIMATEDBACKGROUND);
			BackgroundAnimated animBG = bgObject.GetComponent<BackgroundAnimated>();
			if (animBG != null)
			{
				animBG.SendToBack();
			}
		}
	}
	
	void Start()
	{
		if (!m_ChallengeMode && !m_Tutorial)
		{
			GetComponent<GUIText>().text = "SCORE: 0";
			SpawnItems();
		}
		else
		{
			m_RoundTimer = 0.0f;
		}

		if (m_ChallengeMode && !m_Tutorial)
		{
			//m_LimitText.text = ChallengeMedals.MedalRequirements[Application.loadedLevel - Tags.CHALLENGE_LEVEL_OFFSET].Gold.ToString();
			m_LimitText.text = ChallengeMedals.MedalRequirements[SceneManager.GetActiveScene().buildIndex - Tags.CHALLENGE_LEVEL_OFFSET].Gold.ToString();
		}
	}

	void OnLevelWasLoaded(int level)
	{
		// Hide the loading canvas that remains, as the level is loaded
		GameObject loadingCanvas = GameObject.FindGameObjectWithTag(Tags.LOADINGCANVAS);
		if (loadingCanvas != null)
		{
			//loadingCanvas.gameObject.SetActive(false);
			loadingCanvas.GetComponent<Canvas>().enabled = false;
		}
	}

	void OnGUI()
	{
		if (m_GameState == GameState.GameOver)
		{
			if (m_ChallengeMode)
			{
				/*if (GUI.Button(new Rect(Screen.width - 150, Screen.height / 2, 150, 100), "-DEBUG-\nCLEAR SCORES\n(CAN'T UNDO!)"))
				{
					// Construct string for deleting time/moves scores
					string pref = Tags.PREF_CHALLENGE_MOVES + SceneManager.GetActiveScene().buildIndex.ToString();
					SaveManager.Instance.DeleteKey(pref);
					SaveManager.Instance.DeleteKey(Tags.PREF_CHALLENGES_COMPLETED);
					SaveManager.Instance.DeleteKey(Tags.PREF_CHALLENGES_GOLDED);
					SaveManager.Instance.Save();
				}*/
			}
		}
		else if (m_GameState == GameState.Gameplay)
		{
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

			// Debug mode for creating Challenge levels // - DEBUG
			/*if (GUI.Button(new Rect(Screen.width - 250, Screen.height - 50, 50, 50), "<"))
			{
				DEBUG_ItemID--;
				if (DEBUG_ItemID < 0)
				{
					DEBUG_ItemID = 43;
				}
			}
			if (GUI.Button(new Rect(Screen.width - 50, Screen.height - 50, 50, 50), ">"))
			{
				DEBUG_ItemID++;
				if (DEBUG_ItemID > 43)
				{
					DEBUG_ItemID = 0;
				}
			}
			if (GUI.Button(new Rect(Screen.width - 200, Screen.height - 50, 150, 50), "Spawn Item\n" + DEBUG_ItemID.ToString()))
			{
				Transform item = Instantiate(PrefabIDList.GetPrefabWithID(DEBUG_ItemID)) as Transform;
				InventoryItem itemComponent = item.GetComponent<InventoryItem>();
				if (itemComponent != null)
				{
					if (!GameObject.FindGameObjectWithTag(Tags.HOLDINGAREA).GetComponent<InventoryScript>().FindAvailableSpace(itemComponent))
					{
						Destroy(itemComponent.gameObject);
					}
				}
			}*/
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		AudioManager.Instance.Update();

		if (Input.GetButtonDown("Pause"))
		{
			//m_Paused = !m_Paused;
			if (m_GameState == GameState.Gameplay)
			{
				Button_Pause();
			}
			else if (m_GameState == GameState.Paused)
			{
				if (m_PauseCanvas.gameObject.activeInHierarchy)
				{
					Button_Resume();

					/*if (m_ShowingScoring == 0)
					{
						Time.timeScale = 1.0f;
						m_MousePicker.Enabled = true;
					}

					m_GameState = GameState.Gameplay;
					m_ShowEffects = true;

					m_PauseCanvas.gameObject.SetActive(false);
					if (m_CountdownTimer <= 0.0f || m_ChallengeMode)
					{
						m_MainCanvas.gameObject.SetActive(true);
					}

					// Find the animated background and move it to the back
					GameObject bgObject = GameObject.FindGameObjectWithTag(Tags.ANIMATEDBACKGROUND);
					BackgroundAnimated animBG = bgObject.GetComponent<BackgroundAnimated>();
					if (animBG != null)
					{
						animBG.SendToBack();
					}*/
				}
			}
		}

		if (m_WaitForLevelEnd)
		{
			/*m_LevelEndTimer -= Time.deltaTime;

			//if ((m_HoldingArea.GetNumberOfItems() == 0 && !m_MousePicker.IsCarrying) || m_LevelEndTimer <= 0.0f)
			if (m_LevelEndTimer <= 0.0f)
			{
				m_WaitForLevelEnd = false;
				m_LevelEndTimer = 0.0f;

				GameOver();
			}*/

			if (Time.timeScale != 0.0f)
			{
				m_WaitForLevelEnd = false;
				GameOver();
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
					if (!m_WaitForLevelEnd)
					{
						foreach (InventoryItem item in m_ShowingScoring == 1 ? m_ItemScoringList[m_ScoreGroupIndex] : m_RotationScoringList[m_ScoreGroupIndex])
						{
							if (item != null)
							{
								//for (int i = 0; i < m_OrbsToSpawn / m_ItemScoringList[m_ScoreGroupIndex].Count; i++)
								for (int i = 0; i < m_OrbsToSpawn / (m_ShowingScoring == 1 ? m_ItemScoringList[m_ScoreGroupIndex].Count : m_RotationScoringList[m_ScoreGroupIndex].Count); i++)
								{
									Vector3 pos = item.CentrePosition;
									pos.z = -1.5f;
									Transform trans = Instantiate(m_ParticlePrefab, pos, Quaternion.identity) as Transform;
									ParticleAttract particle = trans.GetComponent<ParticleAttract>();
									//particle.AttractPosition = new Vector3(3.5f, 0.5f, -5.0f);
									particle.AttractPosition = gameObject.transform.position;
								}
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
								m_ScoreOrbTimer = 1.0f;
								m_MousePicker.Enabled = true;

								// Clear objects in inventory and spawn new items
								// (Score is now added when moving to next inventory)
								if (!m_WaitForLevelEnd)
								{
									SpawnItems();
								}
							}
						}
						else if (m_ShowingScoring == 2)
						{
							m_ShowingScoring = 0;
							Time.timeScale = 1.0f;
							m_ScoreGroupIndex = 0;
							m_ScoreOrbTimer = 1.0f;
							m_MousePicker.Enabled = true;

							// Clear objects in inventory and spawn new items
							// (Score is now added when moving to next inventory)
							if (!m_WaitForLevelEnd)
							{
								SpawnItems();
							}
						}
					}
					else
					{
						if (m_SoundFlashBoop != null)
						{
							AudioManager.Instance.PlaySound(m_SoundFlashBoop);
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
				if (m_HoldingArea.GetNumberOfItems() == 0 && !m_MousePicker.IsCarrying && !m_Tutorial)
				{
					GameOver();
				}
			}
			else if (!m_Tutorial)
			{
				if (!m_WaitForLevelEnd)
				{
					if (m_TimeMode)
					{
						if (m_CountdownTimer > 0.0f)
						{
							// Timed mode has a countdown before starting, with beeps every second
							m_CountdownTimer -= Time.deltaTime;
							m_AlertTimer -= Time.deltaTime;
							if (m_AlertTimer <= 0.0f)
							{
								if (m_SoundCountdownBoop != null)
								{
									AudioManager.Instance.PlaySound(m_SoundCountdownBoop);
								}
								m_AlertTimer += 1.0f;

								if (m_CountdownTimer > 2.0f)
								{
									m_CountdownText.text = "3";
								}
								else if (m_CountdownTimer > 1.0f)
								{
									m_CountdownText.text = "2";
								}
								else
								{
									m_CountdownText.text = "1";
								}
							}

							if (m_CountdownTimer <= 0.0f)
							{
								if (m_SoundAlertBeep != null)
								{
									AudioManager.Instance.PlaySound(m_SoundAlertBeep);
								}

								// Reset alert timer with times for gameplay time alerts
								if (m_ModeOptionSelect == 0)
								{
									m_AlertTimer = 30.0f;
								}
								else if (m_ModeOptionSelect == 1)
								{
									m_AlertTimer = 90.0f;
								}
								else if (m_ModeOptionSelect == 2)
								{
									m_AlertTimer = 210.0f;
								}

								// Also enable the mouse picker to start actual gameplay
								m_MousePicker.Enabled = true;
								m_CountdownText.gameObject.SetActive(false);
								m_CountdownCanvas.gameObject.SetActive(false);
								m_MainCanvas.gameObject.SetActive(true);

								// Find the animated background and send it to the back
								GameObject bgObject = GameObject.FindGameObjectWithTag(Tags.ANIMATEDBACKGROUND);
								BackgroundAnimated animBG = bgObject.GetComponent<BackgroundAnimated>();
								if (animBG != null)
								{
									animBG.SendToBack();
								}
							}
						}
						else
						{
							// In time mode, game over when time runs out
							m_RoundTimer -= Time.deltaTime;
							if (m_RoundTimer <= 0.0f)
							{
								AddScore();
								LevelEnd();

								// Set GUI Timer to 0:00 to prevent looking weird
								if (m_LimitText != null)
								{
									m_LimitText.text = "Time: 0:00";
								}

								//GameOver();
							}

							// Decrease timer for time-related 'alert' beep sound
							// Sound plays at 1:30, :30, and every second below :10
							m_AlertTimer -= Time.deltaTime;
							if (m_AlertTimer <= 0.0f)
							{
								if (m_SoundAlertBeep != null)
								{
									AudioManager.Instance.PlaySound(m_SoundAlertBeep);
								}

								if (m_RoundTimer > 30.0f)
								{
									m_AlertTimer += 60.0f;
								}
								else if (m_RoundTimer > 10.0f)
								{
									m_AlertTimer += 20.0f;
								}
								else
								{
									m_AlertTimer += 1.0f;
								}
							}
						}
					}
					else
					{
						// In moves mode, game over when all moves used
						if (m_MovesUsed >= m_MoveLimit)
						{
							AddScore();
							LevelEnd();

							// Create strings for showing Score/Time and set GUI text elements
							/*string time = string.Format("{0}:{1:00}", ((int)m_RoundTimer + 1) / 60, ((int)m_RoundTimer + 1) % 60);
							string mode = m_TimeMode ? "Time: " + time : "Moves: " + (m_MoveLimit - m_MovesUsed).ToString();
							if (m_LimitText != null)
							{
								m_LimitText.text = mode;
							}*/

							//GameOver();
						}
					}
				}
			}
		}
		else if (m_GameState == GameState.GameOver)
		{
			if (m_AuthenticationCanvas != null)
			{
				if (m_AuthenticationCanvas.activeInHierarchy)
				{
					if (!SocialManager.Instance.WaitingForAuthentication)
					{
						m_AuthenticationCanvas.SetActive(false);
						m_OverCanvas.gameObject.SetActive(true);

						if (Social.localUser.authenticated)
						{
							if (m_ButtonLeaderboard != null)
							{
								m_ButtonLeaderboard.gameObject.SetActive(true);
							}
							if (m_ButtonSignIn != null)
							{
								m_ButtonSignIn.gameObject.SetActive(false);
							}


							string leaderboardPref;
							if (m_TimeMode)
							{
								if (m_ChaosMode)
								{
									leaderboardPref = Sorted.GPGSIDs.leaderboard_chaos__timed;
								}
								else
								{
									leaderboardPref = Sorted.GPGSIDs.leaderboard_order__timed;
								}
							}
							else
							{
								if (m_ChaosMode)
								{
									leaderboardPref = Sorted.GPGSIDs.leaderboard_chaos__moves;
								}
								else
								{
									leaderboardPref = Sorted.GPGSIDs.leaderboard_order__moves;
								}
							}

							int leaderboardScore = SaveManager.Instance.GetInt(leaderboardPref);
							if (m_Score > leaderboardScore)
							{
								// Send score to GPGS leaderboard
								SocialManager.Instance.UpdateLeaderboard(leaderboardPref, m_Score);
							}
						}
					}
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if (m_GameState == GameState.Gameplay)
		{
			if (!m_ChallengeMode)
			{
				// Handle timer for scoring orbs & increment displayed score
				if (m_ScoreOrbTimer > 0.0f)
				{
					m_ScoreOrbTimer -= Time.deltaTime;
					if (m_ScoreOrbTimer <= 0.0f && m_ScoreIncrement > 0)
					{
						if (m_SoundScoreCount != null)
						{
							AudioManager.Instance.PlaySound(m_SoundScoreCount);
						}
					}
				}
				else
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

				if (m_TimeMode && !m_Tutorial)
				{
					if (m_LimitText.color == Color.white)
					{
						if (m_RoundTimer <= 10.0f || (m_RoundTimer <= 90.0f && m_RoundTimer > 89.0f) || (m_RoundTimer <= 30.0f && m_RoundTimer > 29.0f))
						{
							m_LimitText.color = Color.red;
						}
					}
					else
					{
						if ((m_RoundTimer <= 89.0f && m_RoundTimer > 30.0f) || (m_RoundTimer <= 29.0f && m_RoundTimer > 10.0f))
						{
							m_LimitText.color = Color.white;
						}
					}
				}

				// Create strings for showing Score/Time and set GUI text elements
				string time = string.Format("{0}:{1:00}", ((int)m_RoundTimer + 1) / 60, ((int)m_RoundTimer + 1) % 60);
				string mode = m_TimeMode ? "Time: " + time : "Moves: " + (m_MoveLimit - m_MovesUsed).ToString();
				if (m_ScoreText != null)
				{
					m_ScoreText.text = "Score: " + m_DisplayedScore.ToString() + (m_ScoreIncrement < 0 ? " (" : " (+") + m_ScoreIncrement.ToString() + ") ";
				}
				if (m_LimitText != null && !m_Tutorial)
				{
					m_LimitText.text = mode;
				}
				//guiText.text = "SCORE: " + m_DisplayedScore.ToString() + (m_ScoreIncrement < 0 ? " (" : " (+") + 
				//	m_ScoreIncrement.ToString() + ") " + mode;// time + " MOVES: " + m_MovesUsed.ToString();//  MONEY: " + m_MainInventory.TotalMoney.ToString();
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
			if (trans.tag != Tags.AUDIOSOURCE && trans.tag != Tags.ANIMATEDBACKGROUND)
			{
				Destroy(trans.gameObject);
			}
		}
	}

	private void Quit()
	{
		//DestroyAllObjects();
		
		Time.timeScale = 1.0f;
		HideUI();
		MainMenu.LoadLevel(Tags.MAINMENU);
	}

	public void GameOver()
	{

		if (m_ChallengeMode)
		{
			// Manually get and destroy all objects in the scene
			// (Because everything on the bottom is set to not destroy on load)
			/*Transform[] objectList = GameObject.FindObjectsOfType<Transform>();
			foreach (Transform trans in objectList)
			{
				if (trans.gameObject != gameObject && trans.gameObject != Camera.main.gameObject 
				    && trans.gameObject.tag != Tags.GUIOBJECT && trans.tag != Tags.AUDIOSOURCE
				    && trans.tag != Tags.ANIMATEDBACKGROUND && trans.tag != Tags.LOADINGCANVAS)
				{
					if (trans.parent != null)
					{
						if (trans.parent.tag != Tags.ANIMATEDBACKGROUND && trans.parent.tag != Tags.LOADINGCANVAS)
						{
							Destroy(trans.gameObject);
						}
					}
					else
					{
						Destroy(trans.gameObject);
					}
				}
			}*/

			// Find the animated background and bring it to the front
			GameObject bgObject = GameObject.FindGameObjectWithTag(Tags.ANIMATEDBACKGROUND);
			BackgroundAnimated animBG = bgObject.GetComponent<BackgroundAnimated>();
			if (animBG != null)
			{
				animBG.BringToFront();
			}

			Time.timeScale = 1.0f;
			m_ShowEffects = false;
			m_GameState = GameState.GameOver;
			m_MainCanvas.gameObject.SetActive(false);
			m_OverCanvas.gameObject.SetActive(true);
			Camera.main.rect = new Rect(0, 0, 1, 1);

			transform.position = new Vector3(0.5f, 0.5f, 0.0f);
			GetComponent<GUIText>().anchor = TextAnchor.MiddleCenter;
			GetComponent<GUIText>().alignment = TextAlignment.Center;

			// Construct string for saving best time/moves
			m_GameOverText.text += m_MovesUsed.ToString() + " moves";

			string movesPref = Tags.PREF_CHALLENGE_MOVES + SceneManager.GetActiveScene().buildIndex.ToString();
			int leastMoves = SaveManager.Instance.GetInt(movesPref, 9999);

			MedalInfo info = ChallengeMedals.MedalRequirements[SceneManager.GetActiveScene().buildIndex - Tags.CHALLENGE_LEVEL_OFFSET];
			if (m_MovesUsed <= info.Gold)
			{
				m_MedalScore.sprite = m_MedalImage_Gold;
			}
			else if (m_MovesUsed <= info.Silver)
			{
				m_MedalScore.sprite = m_MedalImage_Silver;
			}
			else
			{
				m_MedalScore.sprite = m_MedalImage_Bronze;
			}

			int challengesCompleted = SaveManager.Instance.GetInt(Tags.PREF_CHALLENGES_COMPLETED);
			int challengesGolded = SaveManager.Instance.GetInt(Tags.PREF_CHALLENGES_GOLDED);
			if (m_MovesUsed < leastMoves)
			{
				if (leastMoves == 9999)
				{
					// This is the first time this challenge has been completed, increment the achievement counter
					challengesCompleted++;
					SaveManager.Instance.SetInt(Tags.PREF_CHALLENGES_COMPLETED, challengesCompleted);
				}

				if (m_MovesUsed <= info.Gold)
				{
					// Player got a gold medal for the first time, increment the achievement counter
					challengesGolded++;
					SaveManager.Instance.SetInt(Tags.PREF_CHALLENGES_GOLDED, challengesGolded);
				}

				SaveManager.Instance.SetInt(movesPref, m_MovesUsed);
				SaveManager.Instance.Save();
				m_HighScoreText.SetActive(true);
			}

			// Unlock the challenge achievements if enough have been completed
			if (challengesCompleted >= 12)
			{
				SocialManager.Instance.UnlockAchievement(Sorted.GPGSIDs.achievement_a_new_challenger);
			}
			if (challengesCompleted >= 24)
			{
				SocialManager.Instance.UnlockAchievement(Sorted.GPGSIDs.achievement_not_so_challenging);
			}

			// Unlock the gold achievements if enough have been golded
			if (challengesGolded >= 8)
			{
				SocialManager.Instance.UnlockAchievement(Sorted.GPGSIDs.achievement_struck_gold);
			}
			if (challengesGolded >= 16)
			{
				SocialManager.Instance.UnlockAchievement(Sorted.GPGSIDs.achievement_gold_digger);
			}
			if (challengesGolded >= 24)
			{
				SocialManager.Instance.UnlockAchievement(Sorted.GPGSIDs.achievement_golden_god);
			}
		}
		else
		{
			if (m_SoundGameOver != null)
			{
				AudioManager.Instance.PlaySound(m_SoundGameOver);
			}

			//AddScore();

			// Manually get and destroy all objects in the scene
			// (Because everything on the bottom is set to not destroy on load)
			/*Transform[] objectList = GameObject.FindObjectsOfType<Transform>();
			foreach (Transform trans in objectList)
			{
				if (trans.gameObject != gameObject && trans.gameObject != Camera.main.gameObject 
				    && trans.gameObject.tag != Tags.GUIOBJECT && trans.tag != Tags.AUDIOSOURCE
				    && trans.tag != Tags.ANIMATEDBACKGROUND && trans.tag != Tags.LOADINGCANVAS)
				{
					if (trans.parent != null)
					{
						if (trans.parent.tag != Tags.ANIMATEDBACKGROUND && trans.parent.tag != Tags.LOADINGCANVAS)
						{
							Destroy(trans.gameObject);
						}
					}
					else
					{
						Destroy(trans.gameObject);
					}
				}
			}*/

			// Find the animated background and bring it to the front
			GameObject bgObject = GameObject.FindGameObjectWithTag(Tags.ANIMATEDBACKGROUND);
			BackgroundAnimated animBG = bgObject.GetComponent<BackgroundAnimated>();
			if (animBG != null)
			{
				animBG.BringToFront();
			}

			Time.timeScale = 1.0f;
			m_ShowEffects = false;
			m_GameState = GameState.GameOver;
			m_MainCanvas.gameObject.SetActive(false);
			m_OverCanvas.gameObject.SetActive(true);
			Camera.main.rect = new Rect(0, 0, 1, 1);

			if (Social.localUser.authenticated)
			{
				if (m_ButtonLeaderboard != null)
				{
					m_ButtonLeaderboard.gameObject.SetActive(true);
				}
			}
			else
			{
				if (m_ButtonSignIn != null)
				{
					m_ButtonSignIn.gameObject.SetActive(true);
				}
			}

			// Use new GUI system
			m_GameOverText.text += m_Score.ToString();

			// Get string for saving high score
			string scorePref;
			string leaderboardPref;
			if (m_TimeMode)
			{
				if (m_ChaosMode)
				{
					scorePref = Tags.PREF_CHAOS_TIME_SCORE + m_ModeOptionSelect.ToString();
					leaderboardPref = Sorted.GPGSIDs.leaderboard_chaos__timed;
				}
				else
				{
					scorePref = Tags.PREF_ORDER_TIME_SCORE + m_ModeOptionSelect.ToString();
					leaderboardPref = Sorted.GPGSIDs.leaderboard_order__timed;
				}
			}
			else
			{
				if (m_ChaosMode)
				{
					scorePref = Tags.PREF_CHAOS_MOVE_SCORE + m_ModeOptionSelect.ToString();
					leaderboardPref = Sorted.GPGSIDs.leaderboard_chaos__moves;
				}
				else
				{
					scorePref = Tags.PREF_ORDER_MOVE_SCORE + m_ModeOptionSelect.ToString();
					leaderboardPref = Sorted.GPGSIDs.leaderboard_order__moves;
				}
			}

			//int highScore = PlayerPrefs.GetInt(scorePref);
			int highScore = SaveManager.Instance.GetInt(scorePref);
			if (m_Score > highScore)
			{
				//PlayerPrefs.SetInt(scorePref, m_Score);
				SaveManager.Instance.SetInt(scorePref, m_Score);
				SaveManager.Instance.Save();
				//guiText.text += "\n(NEW HIGH SCORE!)";
				//m_GameOverText.text += "\nNEW BEST!";
				m_HighScoreText.SetActive(true);
			}

			int leaderboardScore = SaveManager.Instance.GetInt(leaderboardPref);
			if (m_Score > leaderboardScore)
			{
				// Send score to GPGS leaderboard
				if (Social.localUser.authenticated)
				{
					SocialManager.Instance.UpdateLeaderboard(leaderboardPref, m_Score);
				}
			}

			//PlayerPrefs.Save();
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
			m_ScoreIncrement = (PER_ITEM_CHAOS_SCORE * (m_MainInventory.Size - m_MainInventory.NumFreeCells())) - currentScore;
			//m_ScoreIncrement = (PER_ITEM_CHAOS_SCORE * m_MainInventory.GetNumberOfItems()) - currentScore;
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
			m_ScoreIncrement -= m_HoldingArea.GetNumberOfItems() * (m_ChaosMode ? LEFTOVER_PENALTY * 3 : LEFTOVER_PENALTY);

			// Extra protection from Cash In spamming
			if (m_HoldingArea.GetNumberOfItems() > 5)
			{
				m_ScoreIncrement -= 100;
			}
		}
		m_Score += m_ScoreIncrement;
		m_Score = m_Score < 0 ? 0 : m_Score;

		// Unlock single cash-in score achievements
		if (m_ChaosMode)
		{
			if (m_ScoreIncrement > 1500)
			{
				SocialManager.Instance.UnlockAchievement(Sorted.GPGSIDs.achievement_a_case_of_chaos);
			}
		}
		else
		{
			if (m_ScoreIncrement > 2000)
			{
				SocialManager.Instance.UnlockAchievement(Sorted.GPGSIDs.achievement_order_in_the_case);
			}
		}
		// Cash in a positive inventory using 5 or less moves
		if (!m_TimeMode)
		{
			if (m_MovesUsedThisInventory <= 5 && m_Score > 0)
			{
				SocialManager.Instance.UnlockAchievement(Sorted.GPGSIDs.achievement_bust_a_move);
			}
		}

		// Check if 10+ positive inventories have been cashed in and unlock achievement
		if (m_Score > 0)
		{
			m_InventoriesSorted++;

			if (m_InventoriesSorted > 10)
			{
				if (m_TimeMode)
				{
					SocialManager.Instance.UnlockAchievement(Sorted.GPGSIDs.achievement_quick_sort);
				}
				else
				{
					SocialManager.Instance.UnlockAchievement(Sorted.GPGSIDs.achievement_efficient_sort);
				}
			}
		}

		if (!m_WaitForLevelEnd)
		{
			if (m_ItemScoringList.Count > 0)
			{
				m_OrbsToSpawn = m_ScoreIncrement > 0 ? (currentScore / m_ItemScoringList.Count) / 20 : 0;
				m_ShowingScoring = 1;
				m_GroupStartTime = Time.realtimeSinceStartup;
				Time.timeScale = 0.0f;
				m_MousePicker.Enabled = false;
			}
			else if (m_RotationScoringList.Count > 0)
			{
				m_OrbsToSpawn = m_ScoreIncrement > 0 ? (currentScore / m_RotationScoringList.Count) / 20 : 0;
				m_ShowingScoring = 2;
				m_GroupStartTime = Time.realtimeSinceStartup;
				Time.timeScale = 0.0f;
				m_MousePicker.Enabled = false;
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

		/*if (m_ShowingScoring == 1)
		{
			m_LevelEndTimer = m_ScoreFlashTime * (float)m_ItemScoringList.Count;
		}
		else if (m_ShowingScoring == 2)
		{
			m_LevelEndTimer = m_ScoreFlashTime * (float)m_RotationScoringList.Count;
		}
		else
		{
			m_LevelEndTimer = 0.0f;
		}*/
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
		if (!m_Tutorial)
		{
			m_ItemDropManager.SpawnItems();
		}
	}

	public void CashIn()
	{
		if (!m_MousePicker.IsCarrying && m_ShowingScoring == 0)
		{
			AddScore();
			m_MovesUsedThisInventory = 0;
			
			if (m_ItemScoringList.Count == 0 && m_RotationScoringList.Count == 0)
			{
				// Clear objects in inventory and spawn new items
				// (Score is now added when moving to next inventory)
				SpawnItems();
			}
			else
			{
				if (m_SoundFlashBoop != null)
				{
					AudioManager.Instance.PlaySound(m_SoundFlashBoop);
				}
			}
		}
	}

	public void MoveUsed()
	{
		m_MovesUsed++;
		m_MovesUsedThisInventory++;
		if (m_ChallengeMode)
		{
			if (m_MovesUsed > 99)
			{
				m_MovesUsed = 99;
			}
			m_ScoreText.text = "Moves: " + m_MovesUsed.ToString();

			if (!m_Tutorial)
			{
				//MedalInfo info = ChallengeMedals.MedalRequirements[Application.loadedLevel - Tags.CHALLENGE_LEVEL_OFFSET];
				MedalInfo info = ChallengeMedals.MedalRequirements[SceneManager.GetActiveScene().buildIndex - Tags.CHALLENGE_LEVEL_OFFSET];
				if (m_MovesUsed + 1 <= info.Gold)
				{
					m_MedalSmall.sprite = m_MedalImage_Gold_S;
					m_LimitText.text = info.Gold.ToString();
				}
				else if (m_MovesUsed + 1 <= info.Silver)
				{
					m_MedalSmall.sprite = m_MedalImage_Silver_S;
					m_LimitText.text = info.Silver.ToString();
				}
				else
				{
					m_MedalSmall.sprite = m_MedalImage_Bronze_S;
					m_LimitText.text = info.Bronze.ToString();
				}
			}
		}
		else
		{
			if (!m_TimeMode)
			{
				if (m_MoveLimit - m_MovesUsed <= 10)
				{
					m_LimitText.color = Color.red;
				}
			}
		}
	}

	private void HideUI()
	{
		m_MainCanvas.enabled = false;
		m_OverCanvas.enabled = false;
		m_PauseCanvas.enabled = false;
		m_OptionsCanvas.enabled = false;
		if (m_CountdownCanvas != null)
		{
			m_CountdownCanvas.enabled = false;
		}
	}

	public void Button_Pause()
	{
		Time.timeScale = 0.0f;
		m_MousePicker.Enabled = false;
		m_GameState = GameState.Paused;
		m_ShowEffects = false;

		m_MainCanvas.gameObject.SetActive(false);
		m_PauseCanvas.gameObject.SetActive(true);
		if (m_CountdownCanvas != null)
		{
			m_CountdownCanvas.gameObject.SetActive(false);
		}

		//m_MainInventory.DrawLines = false;
		//m_HoldingArea.DrawLines = false;

		// Find the animated background and bring it to the front
		GameObject bgObject = GameObject.FindGameObjectWithTag(Tags.ANIMATEDBACKGROUND);
		BackgroundAnimated animBG = bgObject.GetComponent<BackgroundAnimated>();
		if (animBG != null)
		{
			animBG.BringToFront();
		}
	}

	public void Button_Resume()
	{
		if (m_ShowingScoring == 0 && !m_Tutorial)
		{
			Time.timeScale = 1.0f;
			m_MousePicker.Enabled = true;
		}

		m_GameState = GameState.Gameplay;
		m_ShowEffects = true;
		m_PauseCanvas.gameObject.SetActive(false);
		if (m_CountdownTimer <= 0.0f || m_ChallengeMode || m_Tutorial || !m_TimeMode)
		{
			m_MainCanvas.gameObject.SetActive(true);

			// Find the animated background and move it to the back
			GameObject bgObject = GameObject.FindGameObjectWithTag(Tags.ANIMATEDBACKGROUND);
			BackgroundAnimated animBG = bgObject.GetComponent<BackgroundAnimated>();
			if (animBG != null)
			{
				animBG.SendToBack();
			}
		}
		else
		{
			if (m_CountdownCanvas != null)
			{
				m_CountdownCanvas.gameObject.SetActive(true);
			}
		}


	}
	
	public void Button_Restart()
	{
		Time.timeScale = 1.0f;
		m_MousePicker.Enabled = true;
		
		// Create new GameModeInfo based on this game mode and reload level
		Transform info = Instantiate(m_GameInfoPrefab) as Transform;
		GameModeInfo modeInfo = info.GetComponent<GameModeInfo>();
		modeInfo.m_TimeMode = m_TimeMode;
		modeInfo.m_ChaosMode = m_ChaosMode;
		modeInfo.m_ModeOptionSelect = m_ModeOptionSelect;
		
		HideUI();
		//Application.LoadLevel(Application.loadedLevel);
		//MainMenu.LoadLevel(Application.loadedLevel);
		MainMenu.LoadLevel(SceneManager.GetActiveScene().buildIndex);
	}

	public void Button_Options()
	{
		if (m_OptionsCanvas != null)
		{
			m_OptionsCanvas.gameObject.SetActive(true);
			m_PauseCanvas.gameObject.SetActive(false);
		}
	}
	
	public void Button_Quit()
	{
		Quit();
	}

	public void Button_Leaderboard()
	{
		if (m_ChaosMode)
		{
			if (m_TimeMode)
			{
				SocialManager.Instance.ShowLeaderboard(Sorted.GPGSIDs.leaderboard_chaos__timed);
			}
			else
			{
				SocialManager.Instance.ShowLeaderboard(Sorted.GPGSIDs.leaderboard_chaos__moves);
			}
		}
		else
		{
			if (m_TimeMode)
			{
				SocialManager.Instance.ShowLeaderboard(Sorted.GPGSIDs.leaderboard_order__timed);
			}
			else
			{
				SocialManager.Instance.ShowLeaderboard(Sorted.GPGSIDs.leaderboard_order__moves);
			}
		}
	}

	public void Button_LevelSelect()
	{
		// Create new GameModeInfo to show correct menu state
		Transform info = Instantiate(m_GameInfoPrefab) as Transform;
		GameModeInfo modeInfo = info.GetComponent<GameModeInfo>();
		int levelIndex = SceneManager.GetActiveScene().buildIndex;
		if (levelIndex > 15)
		{
			modeInfo.m_ChallengeSelect = 2;
		}
		else
		{
			modeInfo.m_ChallengeSelect = 1;
		}
		
		Quit();
	}

	public void Button_NextLevel()
	{
		HideUI();
		//Application.LoadLevel(Application.loadedLevel + 1);
		//MainMenu.LoadLevel(Application.loadedLevel + 1);
		MainMenu.LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
	}

	public void Button_SignIn()
	{
		SocialManager.Instance.Authenticate();
		if (m_AuthenticationCanvas != null)
		{
			m_AuthenticationCanvas.SetActive(true);
			m_OverCanvas.gameObject.SetActive(false);
		}
	}
}