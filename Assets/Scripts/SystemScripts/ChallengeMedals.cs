using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public struct MedalInfo
{
	[SerializeField] private int m_Bronze;
	[SerializeField] private int m_Silver;
	[SerializeField] private int m_Gold;

	public int Bronze { get {return m_Bronze;}}
	public int Silver { get {return m_Silver;}}
	public int Gold { get {return m_Gold;}}
}

public class ChallengeMedals : MonoBehaviour
{
	[SerializeField] private MedalInfo[] m_TempMedalRequirements;
	private static MedalInfo[] m_MedalRequirements;

	public static MedalInfo[] MedalRequirements { get {return m_MedalRequirements;}}

	void Awake()
	{
		if (m_MedalRequirements == null)
		{
			m_MedalRequirements = m_TempMedalRequirements;
		}
	}
}