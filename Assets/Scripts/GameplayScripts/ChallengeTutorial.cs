using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChallengeTutorial : MonoBehaviour 
{
	[SerializeField] private GameObject[] m_MessageList;
	[SerializeField] private GameObject m_MessyItems;
	[SerializeField] private GameObject m_LabelScore;
	[SerializeField] private Text m_LabelLimit;
	[SerializeField] private Image m_MedalImage;
	[SerializeField] private Sprite m_SilverSprite;
	[SerializeField] private Sprite m_BronzeSprite;
	
	[SerializeField] private InventoryScript m_MainInventory;
	[SerializeField] private MousePicker m_MousePicker;
	[SerializeField] private GameObject m_PauseCanvas;
	[SerializeField] private GameObject m_MainCanvas;
	
	private int m_CurrentMessage = 0;
	private const int NUM_MESSAGES = 8;
	
	void Awake()
	{
		m_MousePicker.Enabled = false;
		m_LabelLimit.text = "1";
	}
	
	// Update is called once per frame
	void Update()
	{
		if (m_CurrentMessage == 6)
		{
			if (m_MainInventory.GetNumberOfItems() > 0)
			{
				Button_NextMessage();
			}
		}
	
		if (Input.GetButtonDown("Pause"))
		{
			if (m_PauseCanvas.gameObject.activeInHierarchy)
			{
				Button_Resume();
			}
			else
			{
				Canvas canvas = GetComponent<Canvas>();
				if (canvas != null)
				{
					canvas.enabled = false;
				}
			}
		}
	}
	
	public void Button_Resume()
	{
		Canvas canvas = GetComponent<Canvas>();
		if (canvas != null)
		{
			canvas.enabled = true;
		}
	}
	
	public void Button_NextMessage()
	{
		if (!m_MousePicker.IsCarrying || m_CurrentMessage == 6)
		{
			if (m_CurrentMessage < NUM_MESSAGES)
			{
				m_MessageList[m_CurrentMessage++].SetActive(false);
				m_MessageList[m_CurrentMessage].SetActive(true);
				
				if (m_CurrentMessage == 1)
				{
					m_MessyItems.SetActive(true);
				}
				else if (m_CurrentMessage == 2)
				{
					m_MessyItems.SetActive(false);
					m_MainInventory.ClearItems(true);
					m_LabelScore.SetActive(true);
				}
				else if (m_CurrentMessage == 3)
				{
					m_LabelLimit.gameObject.SetActive(true);
					m_MedalImage.gameObject.SetActive(true);
				}
				else if (m_CurrentMessage == 4)
				{
					m_MedalImage.sprite = m_SilverSprite;
					m_LabelLimit.text = "2";
				}
				else if (m_CurrentMessage == 5)
				{
					m_MedalImage.sprite = m_BronzeSprite;
					m_LabelLimit.text = "99";
				}
				else if (m_CurrentMessage == 6)
				{
					m_MousePicker.Enabled = true;
				}
				else if (m_CurrentMessage == 7)
				{
					m_MousePicker.Enabled = false;
				}
			}
			else
			{
				//Application.LoadLevel(0);
				MainMenu.LoadLevel(0);
				gameObject.SetActive(false);
				m_MainCanvas.SetActive(false);
			}
		}
	}
}