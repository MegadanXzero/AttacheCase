using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

// The SaveManager class contains SaveData, which consists of a dictionary of string keys and int values
// Systems which need to save/load data should save values with a name (key) and then load using the same name
public sealed class SaveManager
{
	private static readonly SaveManager m_Instance = new SaveManager();

	private BinaryFormatter m_Formatter = new BinaryFormatter();
	private SaveData m_Data = new SaveData();

	private SaveManager()
	{
	}

	public static SaveManager Instance
	{
		get
		{
			return m_Instance;
		}
	}

	public int GetInt(string key, int defaultValue = 0)
	{
		#if UNITY_WEBPLAYER
		return PlayerPrefs.GetInt(key, defaultValue);
		#else
		int value;
		return m_Data.m_IntData.TryGetValue(key, out value) ? value : defaultValue;
		#endif
	}

	public void SetInt(string key, int value)
	{
		#if UNITY_WEBPLAYER
		PlayerPrefs.SetInt(key, value);
		#else
		if (m_Data.m_IntData.ContainsKey(key))
		{
			m_Data.m_IntData[key] = value;
		}
		else
		{
			m_Data.m_IntData.Add(key, value);
		}
		#endif
	}

	public float GetFloat(string key, float defaultValue = 0.0f)
	{
		#if UNITY_WEBPLAYER
		return PlayerPrefs.GetFloat(key, defaultValue);
		#else
		float value;
		return m_Data.m_FloatData.TryGetValue(key, out value) ? value : defaultValue;
		#endif
	}
	
	public void SetFloat(string key, float value)
	{
		#if UNITY_WEBPLAYER
		PlayerPrefs.SetFloat(key, value);
		#else
		if (m_Data.m_FloatData.ContainsKey(key))
		{
			m_Data.m_FloatData[key] = value;
		}
		else
		{
			m_Data.m_FloatData.Add(key, value);
		}
		#endif
	}

	public void DeleteKey(string key)
	{
		#if UNITY_WEBPLAYER
		PlayerPrefs.DeleteKey(key);
		#else
		if (m_Data.m_IntData.ContainsKey(key))
		{
			m_Data.m_IntData.Remove(key);
		}
		else if (m_Data.m_FloatData.ContainsKey(key))
		{
			m_Data.m_FloatData.Remove(key);
		}
		#endif
	}

	public void Save()
	{
		#if UNITY_WEBPLAYER
		PlayerPrefs.Save();
		#else
		// Create file, serialize data, then close file
		FileStream file = File.Create(Application.persistentDataPath + "/AttacheCaseData.sav");
		m_Formatter.Serialize(file, m_Data);
		file.Close();
		#endif
	}

	public void Load()
	{
		#if !UNITY_WEBPLAYER
		if (File.Exists(Application.persistentDataPath + "/AttacheCaseData.sav"))
		{
			// Open file, deserialize data, then close file
			FileStream file = File.Open(Application.persistentDataPath + "/AttacheCaseData.sav", FileMode.Open);
			m_Data = (SaveData)m_Formatter.Deserialize(file);
			file.Close();
		}
		#endif
	}

	public void Draw()
	{
		int i = 0;
		foreach(KeyValuePair<string, int> entry in m_Data.m_IntData)
		{
			GUI.Label(new Rect(10, 10 + (i * 20), 150, 20), entry.Key + ": " + entry.Value.ToString());
			i++;
		}
		foreach(KeyValuePair<string, float> entry in m_Data.m_FloatData)
		{
			GUI.Label(new Rect(10, 10 + (i * 20), 150, 20), entry.Key + ": " + entry.Value.ToString());
			i++;
		}
	}
}

[Serializable]
class SaveData
{
	public Dictionary<string, int> m_IntData = new Dictionary<string, int>();
	public Dictionary<string, float> m_FloatData = new Dictionary<string, float>();
}