using UnityEngine;
using System.Collections;

public class OptionsMenu : MonoBehaviour
{

	[SerializeField] private Canvas m_CanvasPrevious;
	[SerializeField] private Canvas m_CanvasAudio;
	[SerializeField] private GameObject m_CanvasCredits;

	[SerializeField] private GameObject m_ButtonSignIn;
	[SerializeField] private GameObject m_ButtonSignOut;

	[SerializeField] private GameObject m_AuthenticationCanvas;

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

		if (Social.localUser.authenticated)
		{
			m_ButtonSignIn.SetActive(false);
			m_ButtonSignOut.SetActive(true);
		}
		else
		{
			m_ButtonSignIn.SetActive(true);
			m_ButtonSignOut.SetActive(false);
		}

		if (m_AuthenticationCanvas.gameObject.activeInHierarchy)
		{
			if (!SocialManager.Instance.WaitingForAuthentication)
			{
				m_AuthenticationCanvas.gameObject.SetActive(false);
				m_CanvasRenderer.enabled = true;
			}
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

	public void Button_Credits()
	{
		if (m_CanvasCredits != null)
		{
			m_CanvasCredits.SetActive(true);
		}
		if (m_CanvasRenderer != null)
		{
			m_CanvasRenderer.enabled = false;
		}
	}

	public void Button_CreditsBack()
	{
		if (m_CanvasCredits != null)
		{
			m_CanvasCredits.SetActive(false);
		}
		if (m_CanvasRenderer != null)
		{
			m_CanvasRenderer.enabled = true;
		}
	}

	public void Button_SignIn()
	{
		SocialManager.Instance.Authenticate();
		m_AuthenticationCanvas.SetActive(true);
		/*Animator anim = m_AuthenticationCanvas.GetComponent<Animator>();
		if (anim != null)
		{
			anim.enabled = true;
		}*/

		if (m_CanvasRenderer != null)
		{
			m_CanvasRenderer.enabled = false;
		}
	}

	public void Button_SignOut()
	{
		SocialManager.Instance.SignOut();
	}
}