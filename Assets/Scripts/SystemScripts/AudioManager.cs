using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public sealed class AudioManager
{
	private static readonly AudioManager m_Instance = new AudioManager();

	// Volume controls
	private float m_MasterVolume = 0.0f;
	private float m_MusicVolume = 0.0f;
	private float m_SFXVolume = 0.0f;

	private float m_MuzakVolume = 0.0f;
	private float m_FireVolume = 0.0f;
	private float m_RiverVolume = 0.0f;
	private float m_WavesVolume = 0.0f;

	// Audio Sources
	private GameObject m_AudioSources = null;
	private AudioSource m_SFXSource = null;
	private AudioSource m_MuzakSource = null;
	private AudioSource m_FireSource = null;
	private AudioSource m_RiverSource = null;
	private AudioSource m_WavesSource = null;

	private AudioMixer m_AudioMixer = null;

	// Music Files
	private AudioClip[] m_MuzakArray;
	private int m_MuzakPlaying = 0;

	// Access Functions
	// Volume setter for volume groups sets the volume in the AudioMixer
	public float MasterVolume
	{
		get{return m_MasterVolume;}
		set
		{
			m_MasterVolume = value;
			m_AudioMixer.SetFloat("MasterVolume", -Mathf.Pow(-value, 2.72270623229f));
		}
	}

	public float MusicVolume
	{
		get{return m_MusicVolume;}
		set
		{
			m_MusicVolume = value;
			m_AudioMixer.SetFloat("MusicVolume", -Mathf.Pow(-value, 2.72270623229f));
		}
	}

	public float SFXVolume
	{
		get{return m_SFXVolume;}
		set
		{
			m_SFXVolume = value;
			m_AudioMixer.SetFloat("SFXVolume", -Mathf.Pow(-value, 2.72270623229f));
		}
	}

	// Volume setter for ambiance audio pauses/plays the audio depending on the volume set
	public float MuzakVolume
	{
		get{return m_MuzakVolume;}
		set
		{
			m_MuzakVolume = value;
			m_MuzakSource.volume = value;
			
			if (value > 0.0f && !m_MuzakSource.isPlaying)
			{
				m_MuzakSource.Play();
			}
			else if (value == 0.0f && m_MuzakSource.isPlaying)
			{
				m_MuzakSource.Pause();
			}
		}
	}

	public float FireVolume
	{
		get{return m_FireVolume;}
		set
		{
			m_FireVolume = value;
			m_FireSource.volume = value;

			if (value > 0.0f && !m_FireSource.isPlaying)
			{
				m_FireSource.Play();
			}
			else if (value == 0.0f && m_FireSource.isPlaying)
			{
				m_FireSource.Pause();
			}
		}
	}

	public float RiverVolume
	{
		get{return m_RiverVolume;}
		set
		{
			m_RiverVolume = value;
			m_RiverSource.volume = value;
			
			if (value > 0.0f && !m_RiverSource.isPlaying)
			{
				m_RiverSource.Play();
			}
			else if (value == 0.0f && m_RiverSource.isPlaying)
			{
				m_RiverSource.Pause();
			}
		}
	}

	public float WavesVolume
	{
		get{return m_WavesVolume;}
		set
		{
			m_WavesVolume = value;
			m_WavesSource.volume = value;
			
			if (value > 0.0f && !m_WavesSource.isPlaying)
			{
				m_WavesSource.Play();
			}
			else if (value == 0.0f && m_WavesSource.isPlaying)
			{
				m_WavesSource.Pause();
			}
		}
	}
	
	public static AudioManager Instance
	{
		get
		{
			return m_Instance;
		}
	}

	private AudioManager()
	{

	}

	public void Update()
	{
		if (m_MuzakVolume != 0)
		{
			if (!m_MuzakSource.isPlaying)
			{
				NextMusicTrack();
			}
		}
	}

	public void SetupAudio(AudioMixer mixer, GameObject prefab, AudioClip[] musicArray)
	{
		if (m_AudioSources == null)
		{
			// Create the game object containing the Audio Sources and set to never destroy
			m_AudioSources = GameObject.Instantiate(prefab);
			GameObject.DontDestroyOnLoad(m_AudioSources);
			
			// Check all the sources in the game object and keep a reference to each 
			// using the priority as a temporary index to the correct one
			AudioSource[] sourceList = m_AudioSources.GetComponents<AudioSource>();
			foreach (AudioSource source in sourceList)
			{
				switch (source.priority)
				{
				case 0:
					m_SFXSource = source;
					break;
				case 1:
					m_FireSource = source;
					break;
				case 2:
					m_RiverSource = source;
					break;
				case 3:
					m_WavesSource = source;
					break;
				case 4:
					m_MuzakSource = source;
					break;
				}
				source.priority = 128;
			}

			m_AudioMixer = mixer;

			// Copy the list of Muzak and set a random starting track
			m_MuzakArray = musicArray;
			m_MuzakPlaying = Random.Range(0, 4);
			m_MuzakSource.clip = m_MuzakArray[m_MuzakPlaying];

			// Load necessary values from the Save Manager
			SaveManager saveManager = SaveManager.Instance;
			MasterVolume = saveManager.GetFloat(Tags.PREF_AUDIO_MASTERVOLUME);
			MusicVolume = saveManager.GetFloat(Tags.PREF_AUDIO_MUSICVOLUME);
			SFXVolume = saveManager.GetFloat(Tags.PREF_AUDIO_SFXVOLUME);

			// If ambiance stuff ever gets added back in just uncomment these
			//MuzakVolume = saveManager.GetFloat(Tags.PREF_AUDIO_MUZAKVOLUME, 0.25f);
			//FireVolume = saveManager.GetFloat(Tags.PREF_AUDIO_FIREVOLUME);
			//RiverVolume = saveManager.GetFloat(Tags.PREF_AUDIO_RIVERVOLUME);
			//WavesVolume = saveManager.GetFloat(Tags.PREF_AUDIO_WAVESVOLUME);
			MuzakVolume = 1.0f;
			FireVolume = 0.0f;
			RiverVolume = 0.0f;
			WavesVolume = 0.0f;
		}
	}

	public void PlaySound(AudioClip sound)
	{
		m_SFXSource.PlayOneShot(sound);
	}

	public void SaveSettings()
	{
		// Save necessary values to the Save Manager
		SaveManager saveManager = SaveManager.Instance;
		saveManager.SetFloat(Tags.PREF_AUDIO_MASTERVOLUME, m_MasterVolume);
		saveManager.SetFloat(Tags.PREF_AUDIO_MUSICVOLUME, m_MusicVolume);
		saveManager.SetFloat(Tags.PREF_AUDIO_SFXVOLUME, m_SFXVolume);

		saveManager.SetFloat(Tags.PREF_AUDIO_MUZAKVOLUME, m_MuzakVolume);
		saveManager.SetFloat(Tags.PREF_AUDIO_FIREVOLUME, m_FireVolume);
		saveManager.SetFloat(Tags.PREF_AUDIO_RIVERVOLUME, m_RiverVolume);
		saveManager.SetFloat(Tags.PREF_AUDIO_WAVESVOLUME, m_WavesVolume);

		saveManager.Save();
	}

	private void NextMusicTrack()
	{
		m_MuzakPlaying = ++m_MuzakPlaying < m_MuzakArray.Length ? m_MuzakPlaying : 0;
		m_MuzakSource.clip = m_MuzakArray[m_MuzakPlaying];
		m_MuzakSource.Play();
	}
}