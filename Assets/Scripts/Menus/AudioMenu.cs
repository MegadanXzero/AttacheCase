using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;

public class AudioMenu : MonoBehaviour
{
	//[SerializeField] private AudioMixer m_AudioMixer;
	//[SerializeField] private GameObject m_AudioSourcePrefab;

	[SerializeField] private Canvas m_CanvasMain;

	[SerializeField] private Text m_TextMaster;
	[SerializeField] private Text m_TextMusic;
	[SerializeField] private Text m_TextEffects;

	[SerializeField] private Slider m_SliderMaster;
	[SerializeField] private Slider m_SliderMusic;
	[SerializeField] private Slider m_SliderSFX;
	[SerializeField] private Slider m_SliderMuzak;
	[SerializeField] private Slider m_SliderFire;
	[SerializeField] private Slider m_SliderRiver;
	[SerializeField] private Slider m_SliderWaves;

	// Use this for initialization
	void Start()
	{
		AudioManager audioManager = AudioManager.Instance;

		//audioManager.SetupAudio(m_AudioMixer, m_AudioSourcePrefab);
		if (m_SliderMaster != null)
		{
			m_SliderMaster.value = audioManager.MasterVolume;
		}

		if (m_SliderMusic != null)
		{
			m_SliderMusic.value = audioManager.MusicVolume;
		}

		if (m_SliderSFX != null)
		{
			m_SliderSFX.value = audioManager.SFXVolume;
		}

		if (m_SliderMuzak != null)
		{
			m_SliderMuzak.value = audioManager.MuzakVolume;
		}

		if (m_SliderFire != null)
		{
			m_SliderFire.value = audioManager.FireVolume;
		}

		if (m_SliderRiver != null)
		{
			m_SliderRiver.value = audioManager.RiverVolume;
		}

		if (m_SliderMaster != null)
		{
			m_SliderWaves.value = audioManager.WavesVolume;
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		int fontSize = m_TextMaster.cachedTextGenerator.fontSizeUsedForBestFit;
		m_TextMusic.fontSize = fontSize;
		m_TextEffects.fontSize = fontSize;

		if (Input.GetButtonDown("Pause"))
		{
			Button_Back();
		}
	}

	public void AdjustVolume_Master(float volume)
	{
		AudioManager.Instance.MasterVolume = volume;
	}

	public void AdjustVolume_Music(float volume)
	{
		AudioManager.Instance.MusicVolume = volume;
	}

	public void AdjustVolume_SFX(float volume)
	{
		AudioManager.Instance.SFXVolume = volume;
	}

	public void AdjustVolume_Muzak(float volume)
	{
		AudioManager.Instance.MuzakVolume = volume;
	}

	public void AdjustVolume_Fire(float volume)
	{
		AudioManager.Instance.FireVolume = volume;
	}

	public void AdjustVolume_River(float volume)
	{
		AudioManager.Instance.RiverVolume = volume;
	}

	public void AdjustVolume_Waves(float volume)
	{
		AudioManager.Instance.WavesVolume = volume;
	}

	public void Button_Back()
	{
		m_CanvasMain.gameObject.SetActive(true);
		m_CanvasMain.enabled = true;
		gameObject.SetActive(false);
		AudioManager.Instance.SaveSettings();
	}
}