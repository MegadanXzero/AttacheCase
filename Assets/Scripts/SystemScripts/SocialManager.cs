using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public sealed class SocialManager
{
	private static readonly SocialManager m_Instance = new SocialManager();
	private bool m_WaitingForAuthentication = false;

	public bool WaitingForAuthentication { get { return m_WaitingForAuthentication; } }

	private SocialManager()
	{
		#if UNITY_ANDROID
		// Activate Google Play Games platform
		PlayGamesPlatform.InitializeInstance(GooglePlayGames.BasicApi.PlayGamesClientConfiguration.DefaultConfiguration);
		PlayGamesPlatform.DebugLogEnabled = true;
		PlayGamesPlatform.Activate();
		#endif
	}

	public static SocialManager Instance
	{
		get
		{
			return m_Instance;
		}
	}

	public void Authenticate()
	{
		if (Application.internetReachability != NetworkReachability.NotReachable
			&& !Social.localUser.authenticated)
		{
			m_WaitingForAuthentication = true;
			Social.localUser.Authenticate((bool success) =>
			{
				m_WaitingForAuthentication = false;
				if (success)
				{
					SaveManager.Instance.SetInt("LoggedIn", 1);
					SaveManager.Instance.Save();
				}
			});
		}
	}

	public bool CheckLoginStatus()
	{
		int loggedIn = SaveManager.Instance.GetInt("LoggedIn", 0);
		if (loggedIn == 1)
		{
			Authenticate();
		}

		return loggedIn == 0 ? false : true;
	}

	public void SignOut()
	{
		#if UNITY_ANDROID
		((PlayGamesPlatform)Social.Active).SignOut();
		SaveManager.Instance.SetInt("LoggedIn", 0);
		SaveManager.Instance.Save();
		#endif
	}

	public void UnlockAchievement(string achievementID)
	{
		if (SaveManager.Instance.GetInt(achievementID, 0) == 0 && Social.localUser.authenticated)
		{
			Social.ReportProgress(achievementID, 100.0f, (bool success) =>
			{
				if (success)
				{
					SaveManager.Instance.SetInt(achievementID, 1);
					SaveManager.Instance.Save();
				}
			});
		}
	}

	public void UpdateLeaderboard(string leaderboardID, int score, int attempt = 0)
	{
		if (Social.localUser.authenticated)
		{
			Social.ReportScore((long)score, leaderboardID, (bool success) =>
			{
				if (success)
				{
					SaveManager.Instance.SetInt(leaderboardID, score);
					SaveManager.Instance.Save();
				}
				else
				{
					if (attempt < 3)
					{
						UpdateLeaderboard(leaderboardID, score, attempt + 1);
					}
				}
			});
		}
	}

	public void ShowLeaderboard(string leaderboardID)
	{
		if (Social.localUser.authenticated)
		{
			#if UNITY_ANDROID
			((PlayGamesPlatform)Social.Active).ShowLeaderboardUI(leaderboardID);
			#endif
		}
	}
}