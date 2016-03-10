using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
#if UNITY_ANDROID
using GooglePlayGames;
#endif

public class MainMenu : MonoBehaviour
{
	enum MenuState
	{
		Main = 0,
		ModeSelect,
		ChallengeSelect,
		ChallengeSelect2,
		Tutorials,
		Options,
	};

	// Audio intialisation objects
	[SerializeField] private AudioMixer m_AudioMixer;
	[SerializeField] private GameObject m_AudioSourcePrefab;
	[SerializeField] private AudioClip[] m_MusicArray;

	//[SerializeField] private SpriteRenderer m_HowToPlay1;
	//[SerializeField] private SpriteRenderer m_HowToPlay2;
	//[SerializeField] private SpriteRenderer m_HowToPlay3;
	//[SerializeField] private SpriteRenderer m_HowToPlay4;
	[SerializeField] private Transform m_GameInfoPrefab;

	//[SerializeField] private Texture m_ButtonBegin;
	//[SerializeField] private Texture m_ButtonBack;

	[SerializeField] private Canvas m_CanvasMain;
	[SerializeField] private Canvas m_CanvasArcade;
	[SerializeField] private Canvas m_CanvasChallenge;
	[SerializeField] private Canvas m_CanvasChallenge2;
	[SerializeField] private Canvas m_CanvasOptions;
	[SerializeField] private Canvas m_CanvasTutorials;
	[SerializeField] private Canvas m_CanvasAuthentication;
	[SerializeField] private GameObject m_PlayServicesUI;
	[SerializeField] private GameObject m_StartupMessage;

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

		// Change camera aspect ratio to fit everything in frame
		float aspectRatio = (float)Screen.width / (float)Screen.height;
		aspectRatio = Mathf.Min(aspectRatio, 1.78f);
		Camera.main.orthographicSize = 9.0f / aspectRatio;

		// Get GameModeInfo script to determine the menu state, then destroy
		GameObject infoObject = GameObject.FindGameObjectWithTag(Tags.GAMEMODEINFO);
		if (infoObject != null)
		{
			GameModeInfo gameInfo = infoObject.GetComponent<GameModeInfo>();
			if (gameInfo.m_ChallengeSelect == 1)
			{
				m_MenuState = MenuState.ChallengeSelect;
				m_CanvasChallenge.gameObject.SetActive(true);
				m_CanvasMain.gameObject.SetActive(false);
			}
			else if (gameInfo.m_ChallengeSelect == 2)
			{
				m_MenuState = MenuState.ChallengeSelect2;
				m_CanvasChallenge2.gameObject.SetActive(true);
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

		int messageShown = SaveManager.Instance.GetInt(Tags.PREF_STARTUPMESSAGE);
		if (messageShown == 0)
		{
			m_CanvasMain.gameObject.SetActive(false);
			m_CanvasAuthentication.gameObject.SetActive(false);
			m_StartupMessage.SetActive(true);
		}
		else
		{
			if (Application.internetReachability != NetworkReachability.NotReachable
			&& !Social.localUser.authenticated)
			{
				if (!SocialManager.Instance.CheckLoginStatus())
				{
					m_CanvasAuthentication.gameObject.SetActive(false);
				}
				else
				{
					m_CanvasMain.gameObject.SetActive(false);
				}
			}
			else
			{
				m_CanvasAuthentication.gameObject.SetActive(false);
			}
		}

		CheckPlayServicesUI();
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

		if (m_MenuState == MenuState.Main)
		{
			if (m_CanvasAuthentication.gameObject.activeInHierarchy)
			{
				if (!SocialManager.Instance.WaitingForAuthentication)
				{
					m_CanvasAuthentication.gameObject.SetActive(false);
					m_CanvasMain.gameObject.SetActive(true);
					m_PlayServicesUI.SetActive(true);
				}
			}
		}

		CheckPlayServicesUI();
	}

	void OnGUI()
	{
		/*if (GUI.Button(new Rect(Screen.width - 150, Screen.height - 50, 150, 50), "Challenge Debug"))
		{
			SceneManager.LoadScene(27);
		}*/

		/*if (GUI.Button(new Rect(Screen.width - 150, Screen.height - 50, 150, 50), "Clear Challenges"))
		{
			for (int i = 4; i < 28; i++)
			{
				// Construct string for deleting time/moves scores
				string pref = Tags.PREF_CHALLENGE_MOVES + i.ToString();
				SaveManager.Instance.DeleteKey(pref);
			}
			SaveManager.Instance.DeleteKey(Tags.PREF_CHALLENGES_COMPLETED);
			SaveManager.Instance.DeleteKey(Tags.PREF_CHALLENGES_GOLDED);
			SaveManager.Instance.DeleteKey(Sorted.GPGSIDs.achievement_not_so_challenging);
			SaveManager.Instance.DeleteKey(Sorted.GPGSIDs.achievement_a_new_challenger);
			SaveManager.Instance.DeleteKey(Sorted.GPGSIDs.achievement_golden_god);
			SaveManager.Instance.DeleteKey(Sorted.GPGSIDs.achievement_gold_digger);
			SaveManager.Instance.DeleteKey(Sorted.GPGSIDs.achievement_struck_gold);
			SaveManager.Instance.Save();
		}*/

		/*if (GUI.Button(new Rect(Screen.width - 150, Screen.height - 50, 150, 50), "Clear Startup"))
		{
			SaveManager.Instance.SetInt(Tags.PREF_STARTUPMESSAGE, 0);
			SaveManager.Instance.Save();
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

	public void Button_Back_Challenge2()
	{
		m_MenuState = MenuState.ChallengeSelect;
		m_CanvasChallenge.gameObject.SetActive(true);
		m_CanvasChallenge2.gameObject.SetActive(false);
	}

	public void Button_Next_Challenge()
	{
		m_MenuState = MenuState.ChallengeSelect2;
		m_CanvasChallenge.gameObject.SetActive(false);
		m_CanvasChallenge2.gameObject.SetActive(true);
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

	public void Button_Achievements()
	{
		Social.ShowAchievementsUI();
	}

	public void Button_Leaderboards()
	{
		Social.ShowLeaderboardUI();
	}

	public void Button_SignIn()
	{
		if (m_StartupMessage != null)
		{
			SocialManager.Instance.Authenticate();
			m_CanvasAuthentication.gameObject.SetActive(true);
			m_StartupMessage.SetActive(false);
			SaveManager.Instance.SetInt(Tags.PREF_STARTUPMESSAGE, 1);
			SaveManager.Instance.Save();
		}
	}

	public void Button_Continue()
	{
		if (m_StartupMessage != null)
		{
			m_StartupMessage.SetActive(false);
			m_CanvasMain.gameObject.SetActive(true);
			SaveManager.Instance.SetInt(Tags.PREF_STARTUPMESSAGE, 1);
			SaveManager.Instance.Save();
		}
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
		m_CanvasChallenge2.enabled = false;
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

		//Application.LoadLevel(level);
		SceneManager.LoadScene(level);
	}

	void CheckPlayServicesUI()
	{
		if (Social.localUser.authenticated)
		{
			m_PlayServicesUI.SetActive(true);
		}
		else
		{
			m_PlayServicesUI.SetActive(false);
		}
	}
}