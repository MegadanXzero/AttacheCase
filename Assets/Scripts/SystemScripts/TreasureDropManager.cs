using UnityEngine;
using System.Collections;
using System;

public class TreasureDropManager : MonoBehaviour
{
	[Serializable]
	public class TreasureDropInfo
	{
		public int m_PrefabID;
		public int m_DropRange;
	}

	private const int MAX_OFFSET = 30;
	private const int MAX_RANGE = 45;
	private const int MAX_TOTAL = 60;

	[SerializeField] private TreasureDropInfo[] m_TreasureList;
	[SerializeField] private int m_Offset = 0;
	[SerializeField] private int m_Range = 10;

	public int Offset { get {return m_Offset;} set {m_Offset = value <= MAX_OFFSET ? value : MAX_OFFSET;}}
	public int Range { get {return m_Range;} set {m_Range = value <= MAX_RANGE ? value : MAX_RANGE;}}

	void Awake()
	{
		//DontDestroyOnLoad(gameObject);
	}

	public int GetTreasureDrop()
	{
		int dropValue = UnityEngine.Random.Range(0, m_Range);
		dropValue += m_Offset;

		if (dropValue > MAX_TOTAL)
		{
			dropValue = MAX_TOTAL;
		}

		int dropThreshold = 0;
		foreach (TreasureDropInfo dropInfo in m_TreasureList)
		{
			dropThreshold += dropInfo.m_DropRange;
			if (dropValue <= dropThreshold)
			{
				return dropInfo.m_PrefabID;
			}
		}

		return 0;
	}

	public void Increase()
	{
		if (m_Range < MAX_RANGE)
		{
			m_Range++;
		}
		else
		{
			m_Offset++;
		}
	}
}