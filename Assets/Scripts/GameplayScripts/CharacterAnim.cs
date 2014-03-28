using UnityEngine;
using System.Collections;

public class CharacterAnim : MonoBehaviour
{
	//private CharacterScript m_MainScript;

	// Use this for initialization
	void Awake()
	{
		//m_MainScript = transform.parent.GetComponent<CharacterScript>();
	}
	
	public void Death()
	{
		GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>().GameOver();
	}
}