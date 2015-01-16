using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;

public class MainMenu : MonoBehaviour
{
	enum MenuState
	{
		Main = 0,
		ModeSelect,
		ChallengeSelect,
		HowToPlay1,
		HowToPlay2,
		HowToPlay3,
		HowToPlay4,
	};

	[SerializeField] private SpriteRenderer m_HowToPlay1;
	[SerializeField] private SpriteRenderer m_HowToPlay2;
	[SerializeField] private SpriteRenderer m_HowToPlay3;
	[SerializeField] private SpriteRenderer m_HowToPlay4;
	[SerializeField] private Transform m_GameInfoPrefab;

	[SerializeField] private Texture m_ButtonOrderActive;
	[SerializeField] private Texture m_ButtonOrderInactive;
	[SerializeField] private Texture m_ButtonChaosActive;
	[SerializeField] private Texture m_ButtonChaosInactive;

	[SerializeField] private Texture m_ButtonTimedActive;
	[SerializeField] private Texture m_ButtonTimedInactive;
	[SerializeField] private Texture m_ButtonMovesActive;
	[SerializeField] private Texture m_ButtonMovesInactive;

	[SerializeField] private Texture m_ButtonTimedOption1Active;
	[SerializeField] private Texture m_ButtonTimedOption3Active;
	[SerializeField] private Texture m_ButtonTimedOption5Active;
	[SerializeField] private Texture m_ButtonTimedOption1Inactive;
	[SerializeField] private Texture m_ButtonTimedOption3Inactive;
	[SerializeField] private Texture m_ButtonTimedOption5Inactive;

	[SerializeField] private Texture m_ButtonMovesOption50Active;
	[SerializeField] private Texture m_ButtonMovesOption100Active;
	[SerializeField] private Texture m_ButtonMovesOption150Active;
	[SerializeField] private Texture m_ButtonMovesOption50Inactive;
	[SerializeField] private Texture m_ButtonMovesOption100Inactive;
	[SerializeField] private Texture m_ButtonMovesOption150Inactive;

	[SerializeField] private Texture m_ButtonBegin;
	[SerializeField] private Texture m_ButtonBack;

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
		// Get GameModeInfo script to determine the menu state, then destroy
		GameObject infoObject = GameObject.FindGameObjectWithTag(Tags.GAMEMODEINFO);
		if (infoObject != null)
		{
			GameModeInfo gameInfo = infoObject.GetComponent<GameModeInfo>();
			if (gameInfo.m_ChallengeSelect)
			{
				m_MenuState = MenuState.ChallengeSelect;
			}
			Destroy(gameInfo.gameObject);
		}
	}
	
	// Update is called once per frame
	void Update()
	{

	}

	void OnGUI()
	{
		if (m_MenuState == MenuState.Main)
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

			/*if (!FB.IsLoggedIn)
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
			}*/
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
			// TO DO: Add display of best time/moves per level
			int numChallenges = Application.levelCount - Tags.CHALLENGE_LEVEL_OFFSET;
			int offset = Screen.width / 5;
			for (int i = 0; i < numChallenges; i++)
			{
				int x = (i % 4) + 1;
				int y = i / 4;

				// Construct string for getting time/moves
				string scores = "";
				string timePref = Tags.PREF_CHALLENGE_TIME + (Tags.CHALLENGE_LEVEL_OFFSET + i).ToString();
				float fastestTime = PlayerPrefs.GetFloat(timePref, -1.0f);
				if (fastestTime != -1.0f)
				{
					scores = "\nT: " + fastestTime.ToString("F");
				}

				string movesPref = Tags.PREF_CHALLENGE_MOVES + (Tags.CHALLENGE_LEVEL_OFFSET + i).ToString();
				int leastMoves = PlayerPrefs.GetInt(movesPref, -1);
				if (leastMoves != -1)
				{
					scores += "\nM: " + leastMoves.ToString();
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
				m_MenuState = MenuState.Main;
				m_HowToPlay4.enabled = false;
			}
			
			if (GUI.Button(new Rect(10, Screen.height / 2 - 50, 100, 100), "Previous"))
			{
				m_MenuState = MenuState.HowToPlay3;
				m_HowToPlay3.enabled = true;
				m_HowToPlay4.enabled = false;
			}
		}
	}

	/*private void SetInit()
	{
		FbDebug.Log("SetInit");
		enabled = true;
		if (FB.IsLoggedIn)
		{
			FbDebug.Log("Already logged in");
			OnLoggedIn();
		}
	}

	private void OnHideUnity(bool isGameShown)
	{
		FbDebug.Log("OnHideUnity");
		if (!isGameShown)
		{
			// pause the game - we wil need to hide
			Time.timeScale = 0.0f;
		}
		else
		{
			// start the game back up - getting focus again
			Time.timeScale = 1.0f;
		}
	}

	void LoginCallback(FBResult result)
	{
		FbDebug.Log("LoginCallback");
		if (FB.IsLoggedIn)
		{
			OnLoggedIn();
		}
	}

	void OnLoggedIn()
	{
		FbDebug.Log("Logged in. ID: " + FB.UserId);

		// Request player info and profile picture
		FB.API("/me?fields=id,first_name,friends.limit(100).fields(first_name,id)", Facebook.HttpMethod.GET, APICallback);
		FB.API(FacebookUtil.GetPictureURL("me", 128, 128), Facebook.HttpMethod.GET, MyPictureCallback);
		FB.API("/app/scores?fields=score,user.limit(20)", Facebook.HttpMethod.GET, ScoresCallback);
		//FB.API("/me/scores", Facebook.HttpMethod.GET, ScoreCallback);
	}

	void APICallback(FBResult result)
	{
		FbDebug.Log("APICallback");
		if (result.Error != null)
		{
			FbDebug.Error(result.Error);
			// Let's just try again
			FB.API("/me?fields=id,first_name,friends.limit(100).fields(first_name,id)", Facebook.HttpMethod.GET, APICallback);
			return;
		}

		profile = FacebookUtil.DeserializeJSONProfile(result.Text);
		friends = FacebookUtil.DeserializeJSONFriends(result.Text);
		username = profile["first_name"];
	}

	void MyPictureCallback(FBResult result)
	{
		FbDebug.Log("MyPictureCallback");
		if (result.Error != null)
		{
			FbDebug.Error(result.Error);
			// Let's just try again
			FB.API(FacebookUtil.GetPictureURL("me", 128, 128), Facebook.HttpMethod.GET, MyPictureCallback);
			return;
		}

		userPicture = result.Texture;
	}

	private void OnChallengeClicked()
	{
		FB.AppRequest(message: "Friend Smash is smashing! Check it out.", title: "Play Friend Smash with me!", callback:AppRequestCallback);
	}

	private void AppRequestCallback(FBResult result)
	{
		FbDebug.Log("AppRequestCallback");
		if (result != null)
		{
			var responseObject = Json.Deserialize(result.Text) as Dictionary<string, object>;
			object obj = 0;
			if (responseObject.TryGetValue("cancelled", out obj))
			{
				FbDebug.Log("Request cancelled");
			}
			else if (responseObject.TryGetValue("request", out obj))
			{
				// Record that we sent a request so we can display a message
				m_LastChallengeSentTime = Time.realtimeSinceStartup;
				FbDebug.Log("Request sent");
			}
		}
	}

	private void OnBragClicked()
	{
		FbDebug.Log("OnBragClicked");
		FB.Feed(linkCaption: "I just smashed 9 billion friends! Can you beat it?",
		        picture: "http://www.friendsmash.com/images/logo_large.jpg",
		        linkName: "Checkout my Friend Smash greatness!",
		        link: "https://apps.facebook.com/" + FB.AppId + "/?challenge_brag=" + (FB.IsLoggedIn ? FB.UserId : "guest"));
	}

	private int GetScoreFromEntry(object obj)
	{
		Dictionary<string,object> entry = (Dictionary<string,object>) obj;
		return Convert.ToInt32(entry["score"]);
	}
	
	void ScoresCallback(FBResult result) 
	{
		FbDebug.Log("ScoresCallback");
		if (result.Error != null)
		{
			FbDebug.Error(result.Error);
			return;
		}
		
		scores = new List<object>();
		List<object> scoresList = FacebookUtil.DeserializeScores(result.Text);
		
		foreach(object score in scoresList) 
		{
			var entry = (Dictionary<string,object>) score;
			var user = (Dictionary<string,object>) entry["user"];
			
			string userId = (string)user["id"];
			
			if (string.Equals(userId,FB.UserId))
			{
				// This entry is the current player
				int playerHighScore = GetScoreFromEntry(entry);
				FbDebug.Log("Local players score on server is " + playerHighScore);
				if (playerHighScore < m_Score)
				{
					FbDebug.Log("Locally overriding with just acquired score: " + m_Score);
					playerHighScore = m_Score;
				}
				
				entry["score"] = playerHighScore.ToString();
				m_Score = playerHighScore;
			}
			
			scores.Add(entry);
			FbDebug.Log("Adding player to friend list: " + (string)user["name"]);
			if (!friendImages.ContainsKey(userId))
			{
				// We don't have this players image yet, request it now
				FB.API(FacebookUtil.GetPictureURL(userId, 128, 128), Facebook.HttpMethod.GET, pictureResult =>
				       {
					if (pictureResult.Error != null)
					{
						FbDebug.Error(pictureResult.Error);
					}
					else
					{
						friendImages.Add(userId, pictureResult.Texture);
					}
				});
			}
		}
		
		// Now sort the entries based on score
		scores.Sort(delegate(object firstObj,
		                     object secondObj)
			{
				return -GetScoreFromEntry(firstObj).CompareTo(GetScoreFromEntry(secondObj));
			}
		);
	}*/
}