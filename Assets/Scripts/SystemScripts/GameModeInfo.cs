using UnityEngine;
using System.Collections;

public class GameModeInfo : MonoBehaviour
{
	public bool m_TimeMode = true;
	public bool m_ChaosMode = false;
	public int m_ChallengeSelect = 0;
	public int m_ModeOptionSelect = 1;

	// Use this for initialization
	void Awake()
	{
		//gameObject.tag = "GameModeInfo";
		DontDestroyOnLoad(gameObject);
	}
}