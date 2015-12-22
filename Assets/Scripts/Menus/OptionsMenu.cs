using UnityEngine;
using System.Collections;

public class OptionsMenu : MonoBehaviour
{

	[SerializeField] private Canvas m_CanvasPrevious;
	[SerializeField] private Canvas m_CanvasAudio;

	private Canvas m_CanvasRenderer;

	void Start()
	{
		m_CanvasRenderer = GetComponent<Canvas>();
	}

	void Update()
	{
		if (Input.GetButtonDown("Pause") && m_CanvasRenderer.enabled)
		{
			Button_Back();
		}
	}

	public void Button_Back()
	{
		if (m_CanvasPrevious != null)
		{
			m_CanvasPrevious.gameObject.SetActive(true);
		}
		gameObject.SetActive(false);
	}

	public void Button_Audio()
	{
		if (m_CanvasAudio != null)
		{
			m_CanvasAudio.gameObject.SetActive(true);
		}
		if (m_CanvasRenderer != null)
		{
			m_CanvasRenderer.enabled = false;
		}
	}
}