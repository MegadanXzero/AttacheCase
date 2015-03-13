using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefabIDList : MonoBehaviour
{
	public List<Transform> m_TempPrefabList;
	
	private static List<Transform> m_PrefabList = new List<Transform>();
	
	void Awake()
	{
		//DontDestroyOnLoad(gameObject);

		foreach (Transform trans in m_TempPrefabList)
		{
			m_PrefabList.Add(trans);
		}
		
		//m_PrefabList = m_TempPrefabList;
		m_TempPrefabList.Clear();
	}
	
	public static Transform GetPrefabWithID(int prefabID)
	{
		if (m_PrefabList.Count > prefabID)
		{
			return m_PrefabList[prefabID];
		}
		return null;
	}
}