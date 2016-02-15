using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
	enum MenuState
	{
		Main = 0,
		ModeSelect,
		ChallengeSelect,
		Tutorials,
		Options,
	};

	// Audio intialisation objects
	[SerializeField] private AudioMixer m_AudioMixer;
	[SerializeField] private GameObject m_AudioSourcePrefab;
	[SerializeField] private AudioClip[] m_MusicArray;

	[SerializeField] private SpriteRenderer m_HowToPlay1;
	[SerializeField] private SpriteRenderer m_HowToPlay2;
	[SerializeField] private SpriteRenderer m_HowToPlay3;
	[SerializeField] private SpriteRenderer m_HowToPlay4;
	[SerializeField] private Transform m_GameInfoPrefab;

	[SerializeField] private Texture m_ButtonBegin;
	[SerializeField] private Texture m_ButtonBack;

	[SerializeField] private Canvas m_CanvasMain;
	[SerializeField] private Canvas m_CanvasArcade;
	[SerializeField] private Canvas m_CanvasChallenge;
	[SerializeField] private Canvas m_CanvasOptions;
	[SerializeField] private Canvas m_CanvasTutorials;

	[SerializeField] private Image[] m_ChallengeButtonList;
	[SerializeField] private Sprite m_ChallengeBronze;
	[SerializeField] private Sprite m_ChallengeSilver;
	[SerializeField] private Sprite m_ChallengeGold;

	private MenuState m_MenuState = MenuState.Main;
	private bool m_OrderSelection = false;
	private bool m_TimeSelection = true;
	private int m_TimeOptionSelection = 1;
	private int m_MovesOptionSelection = 1;

	// FACEBOOK TESTING STUFF
	/*private static List<object>                 friends         = null;
	private static Dictionary<string, string>   profile         = null;
	private static List<object>                 scores          = null;
	private static Dictionary<string, Texture>  friendImages    = new Dictionary<string, Texture>();

	private string username = "";
	private Texture2D userPicture;
	private float m_LastChallengeSentTime;
	private int m_Score;*/
	// FACEBOOK TESTING STUFF
	
	void Awake()
	{
		// Load save data on startup
		SaveManager.Instance.Load();

		// Get GameModeInfo script to determine the menu state, then destroy
		GameObject infoObject = GameObject.FindGameObjectWithTag(Tags.GAMEMODEINFO);
		if (infoObject != null)
		{
			GameModeInfo gameInfo = infoObject.GetComponent<GameModeInfo>();
			if (gameInfo.m_ChallengeSelect)
			{
				m_MenuState = MenuState.ChallengeSelect;
				m_CanvasChallenge.gameObject.SetActive(true);
				m_CanvasMain.gameObject.SetActive(false);
			}
			Destroy(gameInfo.gameObject);
		}
	}

	void Start()
	{
		// Tell the loading canvas to never destroy itself
		GameObject loadingCanvas = GameObject.FindGameObjectWithTag(Tags.LOADINGCANVAS);
		if (loadingCanvas != null)
		{
			DontDestroyOnLoad(loadingCanvas);
			//loadingCanvas.SetActive(false);
			loadingCanvas.GetComponent<Canvas>().enabled = false;
		}

		// Initialise Audio
		AudioManager.Instance.SetupAudio(m_AudioMixer, m_AudioSourcePrefab, m_MusicArray);

		for (int i = 0; i < m_ChallengeButtonList.Length; i++)
		{
			// Get best score for each level and set medal sprite to relevant colour
			int bestMoves = SaveManager.Instance.GetInt(Tags.PREF_CHALLENGE_MOVES + (i + Tags.CHALLENGE_LEVEL_OFFSET).ToString(), 9999);
			if (bestMoves <= ChallengeMedals.MedalRequirements[i].Gold)
			{
				m_ChallengeButtonList[i].sprite = m_ChallengeGold;
			}
			else if (bestMoves <= ChallengeMedals.MedalRequirements[i].Silver)
			{
				m_ChallengeButtonList[i].sprite = m_ChallengeSilver;
			}
			else if (bestMoves <= ChallengeMedals.MedalRequirements[i].Bronze)
			{
				m_ChallengeButtonList[i].sprite = m_ChallengeBronze;
			}
		}
	}

	void OnLevelWasLoaded(int level)
	{
		// Check if there are several loading canvases and if so destroy all but the first one
		GameObject[] loadingCanvasList = GameObject.FindGameObjectsWithTag(Tags.LOADINGCANVAS);
		if (loadingCanvasList.Length > 1)
		{
			for (int i = 1; i < loadingCanvasList.Length; i++)
			{
				Destroy(loadingCanvasList[i]);
			}
		}
		
		// Hide the loading canvas that remains, as the level is loaded
		//loadingCanvasList[0].gameObject.SetActive(false);
		loadingCanvasList[0].GetComponent<Canvas>().enabled = false;

		// Find the animated background and move it to the back
		GameObject bgObject = GameObject.FindGameObjectWithTag(Tags.ANIMATEDBACKGROUND);
		BackgroundAnimated animBG = bgObject.GetComponent<BackgroundAnimated>();
		if (animBG != null)
		{
			animBG.SendToBack();
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		AudioManager.Instance.Update();

		if (m_MenuState == MenuState.ModeSelect)
		{
			if (Input.GetButtonDown("Pause"))
			{
				Button_Back_Arcade();
			}
		}
		else if (m_MenuState == MenuState.ChallengeSelect)
		{
			if (Input.GetButtonDown("Pause"))
			{
				Button_Back_Challenge();
			}
		}
		else if (m_MenuState == MenuState.Tutorials)
		{
			if (Input.GetButtonDown("Pause"))
			{
				Button_Back_Tutorials();
			}
		}
		else if (m_MenuState == MenuState.Options)
		{
			if (m_CanvasOptions.enabled)
			{
				if (Input.GetButtonDown("Pause"))
				{
					m_MenuState = MenuState.Main;
				}
			}
		}
	}

	void OnGUI()
	{
		/*if (m_MenuState == MenuState.Main)
		{
			int centreX = Screen.width / 2;
			int centreY = Screen.height / 2;

			if (GUI.Button(new Rect(centreX - 100, centreY - 160, 200, 100), "ARCADE"))
			{
				m_MenuState = MenuState.ModeSelect;
			}

			if (GUI.Button(new Rect(centreX - 100, centreY - 50, 200, 100), "CHALLENGE"))
			{
				m_MenuState = MenuState.ChallengeSelect;
			}

			if (GUI.Button(new Rect(centreX - 100, centreY + 60, 200, 100), "HOW TO PLAY"))
			{
				m_HowToPlay1.enabled = true;
				m_MenuState = MenuState.HowToPlay1;
			}

			if (GUI.Button(new Rect(centreX - 100, centreY + 170, 200, 100), "QUIT"))
			{
				Application.Quit();
			}

			// Testing Save/Load stuff
			//SaveManager.Instance.Draw();

			if (!FB.IsLoggedIn)
			{
				if (GUI.Button(new Rect(centreX - 100, centreY + 150, 200, 50), "Login to Facebook"))
				{
					FB.Login("email,publish_actions", LoginCallback);
				}
			}
			else
			{
				if (GUI.Button(new Rect(centreX - 100, centreY + 150, 200, 50), "Logout of Facebook"))
				{
					FB.Logout();
				}

				if (GUI.Button(new Rect(centreX - 100, centreY + 220, 200, 50), "Challenge Friends"))
				{
					OnChallengeClicked();
				}

				if (GUI.Button(new Rect(centreX - 100, centreY + 290, 200, 50), "Brag on Facebook"))
				{
					OnBragClicked();
				}

				GUI.DrawTexture(new Rect(10, 10, 128, 128), userPicture);
				GUI.TextField(new Rect(138, 10, 100, 128), username + "\n" + m_Score.ToString());

				int i = 1;
				foreach(object score in scores) 
				{
					var entry = (Dictionary<string,object>) score;
					var user = (Dictionary<string,object>) entry["user"];
					
					string userId = (string)user["id"];
					string firstName = (string)user["name"];

					foreach(object friendObj in friends)
					{

					}
					
					if (string.Equals(userId,FB.UserId))
					{
						// This entry is the current player, do nothing
					}
					else
					{
						GUI.DrawTexture(new Rect(10, 10 + (i * 128), 128, 128), friendImages[userId]);
						GUI.TextField(new Rect(138, 10 + (i * 128), 100, 128), firstName + "\n" + entry["score"]);
					}
				}
			}
		}
		else if (m_MenuState == MenuState.ModeSelect)
		{
			int centreX = Screen.width / 2;
			int centreY = Screen.height / 2;
			
			//if (GUI.Button(new Rect(centreX - 250, centreY - 160, 200, 100), "Order Mode\nTimed")
			if (GUI.Button(new Rect(centreX - 250, centreY - 200, 200, 125), m_OrderSelection ? m_ButtonOrderInactive : m_ButtonOrderActive, GUIStyle.none))
			{
				m_OrderSelection = false;
			}

			//if (GUI.Button(new Rect(centreX + 50, centreY - 160, 200, 100), "Chaos Mode\nTimed"))
			if (GUI.Button(new Rect(centreX + 50, centreY - 200, 200, 125), m_OrderSelection ? m_ButtonChaosActive : m_ButtonChaosInactive, GUIStyle.none))
			{
				m_OrderSelection = true;
			}
			
			//if (GUI.Button(new Rect(centreX - 250, centreY, 200, 100), "Timed"))
			if (GUI.Button(new Rect(centreX - 250, centreY - 40, 200, 125), m_TimeSelection ? m_ButtonTimedActive : m_ButtonTimedInactive, GUIStyle.none))
			{
				m_TimeSelection = true;
			}
			
			//if (GUI.Button(new Rect(centreX + 50, centreY, 200, 100), "Moves"))
			if (GUI.Button(new Rect(centreX + 50, centreY - 40, 200, 125), m_TimeSelection ? m_ButtonMovesInactive : m_ButtonMovesActive, GUIStyle.none))
			{
				m_TimeSelection = false;
			}

			if (m_TimeSelection)
			{
				if (GUI.Button(new Rect(centreX - 250, centreY + 85, 67, 60), 
				               m_TimeOptionSelection == 0 ? m_ButtonTimedOption1Active : m_ButtonTimedOption1Inactive, GUIStyle.none))
				{
					m_TimeOptionSelection = 0;
				}

				if (GUI.Button(new Rect(centreX - 183, centreY + 85, 66, 60), 
				               m_TimeOptionSelection == 1 ? m_ButtonTimedOption3Active : m_ButtonTimedOption3Inactive, GUIStyle.none))
				{
					m_TimeOptionSelection = 1;
				}

				if (GUI.Button(new Rect(centreX - 117, centreY + 85, 67, 60), 
				               m_TimeOptionSelection == 2 ? m_ButtonTimedOption5Active : m_ButtonTimedOption5Inactive, GUIStyle.none))
				{
					m_TimeOptionSelection = 2;
				}
			}
			else
			{
				if (GUI.Button(new Rect(centreX + 50, centreY + 85, 67, 60), 
				               m_MovesOptionSelection == 0 ? m_ButtonMovesOption50Active : m_ButtonMovesOption50Inactive, GUIStyle.none))
				{
					m_MovesOptionSelection = 0;
				}
				
				if (GUI.Button(new Rect(centreX + 117, centreY + 85, 66, 60), 
				               m_MovesOptionSelection == 1 ? m_ButtonMovesOption100Active : m_ButtonMovesOption100Inactive, GUIStyle.none))
				{
					m_MovesOptionSelection = 1;
				}
				
				if (GUI.Button(new Rect(centreX + 183, centreY + 85, 67, 60), 
				               m_MovesOptionSelection == 2 ? m_ButtonMovesOption150Active : m_ButtonMovesOption150Inactive, GUIStyle.none))
				{
					m_MovesOptionSelection = 2;
				}
			}
			
			//if (GUI.Button(new Rect(centreX - 100, centreY + 200, 200, 100), "Begin"))
			if (GUI.Button(new Rect(centreX - 100, centreY + 200, 200, 125), m_ButtonBegin, GUIStyle.none))
			{
				Transform info = Instantiate(m_GameInfoPrefab) as Transform;
				GameModeInfo modeInfo = info.GetComponent<GameModeInfo>();
				modeInfo.m_TimeMode = m_TimeSelection;
				modeInfo.m_ChaosMode = m_OrderSelection;
				modeInfo.m_ModeOptionSelect = m_TimeSelection ? m_TimeOptionSelection : m_MovesOptionSelection;
				
				Application.LoadLevel(Tags.ORDERINVENTORY);
			}

			//if (GUI.Button(new Rect(10, Screen.height - 110, 100, 100), "Back"))
			if (GUI.Button(new Rect(10, Screen.height - 80, 100, 70), m_ButtonBack, GUIStyle.none))
			{
				m_MenuState = MenuState.Main;
			}
		}
		else if (m_MenuState == MenuState.ChallengeSelect)
		{
			// Create a button for each Challenge level which loads the level
			int numChallenges = Application.levelCount - Tags.CHALLENGE_LEVEL_OFFSET;
			int offset = Screen.width / 5;
			for (int i = 0; i < numChallenges; i++)
			{
				int x = (i % 4) + 1;
				int y = i / 4;

				// Construct string for getting moves
				string scores = "";
				string movesPref = Tags.PREF_CHALLENGE_MOVES + (Tags.CHALLENGE_LEVEL_OFFSET + i).ToString();
				//int leastMoves = PlayerPrefs.GetInt(movesPref, -1);
				int leastMoves = SaveManager.Instance.GetInt(movesPref, -1);
				if (leastMoves != -1)
				{
					scores = "\nMOVES: " + leastMoves.ToString();
				}

				if (GUI.Button(new Rect((x * offset) - 75, 250 + (y * (offset / 2)), 150, 100), (i + 1).ToString() + scores))
				{
					Application.LoadLevel(Tags.CHALLENGE_LEVEL_OFFSET + i);
				}
			}

			// Go back to the main menu
			if (GUI.Button(new Rect(10, Screen.height - 80, 100, 70), m_ButtonBack, GUIStyle.none))
			{
				m_MenuState = MenuState.Main;
			}
		}
		else if (m_MenuState == MenuState.HowToPlay1)
		{
			if (GUI.Button(new Rect(10, Screen.height - 80, 100, 70), m_ButtonBack, GUIStyle.none))
			{
				m_CanvasMain.gameObject.SetActive(true);
				m_MenuState = MenuState.Main;
				m_HowToPlay1.enabled = false;
				m_HowToPlay2.enabled = false;
			}

			if (GUI.Button(new Rect(Screen.width - 110, Screen.height / 2 - 50, 100, 100), "Next"))
			{
				m_MenuState = MenuState.HowToPlay2;
				m_HowToPlay1.enabled = false;
				m_HowToPlay2.enabled = true;
			}
		}
		else if (m_MenuState == MenuState.HowToPlay2)
		{
			if (GUI.Button(new Rect(10, Screen.height - 80, 100, 70), m_ButtonBack, GUIStyle.none))
			{
				m_CanvasMain.gameObject.SetActive(true);
				m_MenuState = MenuState.Main;
				m_HowToPlay1.enabled = false;
				m_HowToPlay2.enabled = false;
			}

			if (GUI.Button(new Rect(10, Screen.height / 2 - 50, 100, 100), "Previous"))
			{
				m_MenuState = MenuState.HowToPlay1;
				m_HowToPlay1.enabled = true;
				m_HowToPlay2.enabled = false;
			}
			
			if (GUI.Button(new Rect(Screen.width - 110, Screen.height / 2 - 50, 100, 100), "Next"))
			{
				m_MenuState = MenuState.HowToPlay3;
				m_HowToPlay2.enabled = false;
				m_HowToPlay3.enabled = true;
			}
		}
		else if (m_MenuState == MenuState.HowToPlay3)
		{
			if (GUI.Button(new Rect(10, Screen.height - 80, 100, 70), m_ButtonBack, GUIStyle.none))
			{
				m_CanvasMain.gameObject.SetActive(true);
				m_MenuState = MenuState.Main;
				m_HowToPlay3.enabled = false;
			}
			
			if (GUI.Button(new Rect(10, Screen.height / 2 - 50, 100, 100), "Previous"))
			{
				m_MenuState = MenuState.HowToPlay2;
				m_HowToPlay2.enabled = true;
				m_HowToPlay3.enabled = false;
			}
			
			if (GUI.Button(new Rect(Screen.width - 110, Screen.height / 2 - 50, 100, 100), "Next"))
			{
				m_MenuState = MenuState.HowToPlay4;
				m_HowToPlay3.enabled = false;
				m_HowToPlay4.enabled = true;
			}
		}
		else if (m_MenuState == MenuState.HowToPlay4)
		{
			if (GUI.Button(new Rect(10, Screen.height - 80, 100, 70), m_ButtonBack, GUIStyle.none))
			{
				m_CanvasMain.gameObject.SetActive(true);
				m_MenuState = MenuState.Main;
				m_HowToPlay4.enabled = false;
			}
			
			if (GUI.Button(new Rect(10, Screen.height / 2 - 50, 100, 100), "Previous"))
			{
				m_MenuState = MenuState.HowToPlay3;
				m_HowToPlay3.enabled = true;
				m_HowToPlay4.enabled = false;
			}
		}*/
	}

	public void Button_Arcade()
	{
		m_MenuState = MenuState.ModeSelect;
		m_CanvasMain.gameObject.SetActive(false);
		m_CanvasArcade.gameObject.SetActive(true);
	}

	public void Button_Challenge()
	{
		m_MenuState = MenuState.ChallengeSelect;
		m_CanvasMain.gameObject.SetActive(false);
		m_CanvasChallenge.gameObject.SetActive(true);
	}

	public void Button_HowToPlay()
	{
		/*m_MenuState = MenuState.HowToPlay1;
		m_HowToPlay1.enabled = true;*/

		m_MenuState = MenuState.Tutorials;
		m_CanvasMain.gameObject.SetActive(false);
		m_CanvasTutorials.gameObject.SetActive(true);
	}

	public void Button_Options()
	{
		m_MenuState = MenuState.Options;
		m_CanvasOptions.gameObject.SetActive(true);
		m_CanvasMain.gameObject.SetActive(false);
	}

	public void Button_Back_Arcade()
	{
		m_MenuState = MenuState.Main;
		m_CanvasMain.gameObject.SetActive(true);
		m_CanvasArcade.gameObject.SetActive(false);
	}

	public void Button_Back_Challenge()
	{
		m_MenuState = MenuState.Main;
		m_CanvasMain.gameObject.SetActive(true);
		m_CanvasChallenge.gameObject.SetActive(false);
	}

	public void Button_Back_Tutorials()
	{
		m_MenuState = MenuState.Main;
		m_CanvasMain.gameObject.SetActive(true);
		m_CanvasTutorials.gameObject.SetActive(false);
	}

	public void Button_Begin_Arcade()
	{
		Transform info = Instantiate(m_GameInfoPrefab) as Transform;
		GameModeInfo modeInfo = info.GetComponent<GameModeInfo>();
		modeInfo.m_TimeMode = m_TimeSelection;
		modeInfo.m_ChaosMode = m_OrderSelection;
		modeInfo.m_ModeOptionSelect = m_TimeSelection ? m_TimeOptionSelection : m_MovesOptionSelection;

		//Application.LoadLevel(Tags.ORDERINVENTORY);
		HideUI();
		MainMenu.LoadLevel(1);
	}

	public void Button_Tutorial_Arcade()
	{
		//Application.LoadLevel(2);
		HideUI();
		MainMenu.LoadLevel(2);
	}

	public void Button_Tutorial_Challenge()
	{
		//Application.LoadLevel(3);
		HideUI();
		MainMenu.LoadLevel(3);
	}

	public void Button_Quit()
	{
		Application.Quit();
	}

	public void SetChaosMode(bool chaosMode)
	{
		m_OrderSelection = chaosMode;
	}

	public void SetTimeMode(bool timeMode)
	{
		m_TimeSelection = timeMode;
	}

	public void SetTimeOption(int timeOption)
	{
		m_TimeOptionSelection = timeOption;
	}

	public void SetMoveOption(int moveOption)
	{
		m_MovesOptionSelection = moveOption;
	}

	private void HideUI()
	{
		m_CanvasMain.enabled = false;
		m_CanvasArcade.enabled = false;
		m_CanvasChallenge.enabled = false;
		m_CanvasOptions.enabled = false;
		m_CanvasTutorials.enabled = false;
	}

	public void LoadChallengeLevel(int level)
	{
		//Application.LoadLevel(Tags.CHALLENGE_LEVEL_OFFSET + level);
		HideUI();
		LoadLevel(Tags.CHALLENGE_LEVEL_OFFSET + level);
	}

	public static void LoadLevel(int level)
	{
		// Show the loading canvas as a new level is loading
		GameObject loadingCanvas = GameObject.FindGameObjectWithTag(Tags.LOADINGCANVAS);
		if (loadingCanvas != null)
		{
			//loadingCanvas.gameObject.SetActive(true);
			loadingCanvas.GetComponent<Canvas>().enabled = true;
		}

		// Find the animated background and move it to the front
		GameObject bgObject = GameObject.FindGameObjectWithTag(Tags.ANIMATEDBACKGROUND);
		BackgroundAnimated animBG = bgObject.GetComponent<BackgroundAnimated>();
		if (animBG != null)
		{
			animBG.BringToFront();
		}

		Application.LoadLevel(level);
	}
}