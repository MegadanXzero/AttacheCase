using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ArcadeTutorial : MonoBehaviour
{
	[SerializeField] private GameObject[] m_MessageList;
	[SerializeField] private GameObject m_ButtonCashIn;
	[SerializeField] private GameObject m_LabelScore;
	[SerializeField] private Text m_LabelLimit;
	[SerializeField] private ItemDropManager m_ItemDropManager;

	[SerializeField] private InventoryScript m_MainInventory;
	[SerializeField] private MousePicker m_MousePicker;

	private int m_CurrentMessage = 0;
	private const int NUM_MESSAGES = 11;

	void Awake()
	{
		m_MousePicker.Enabled = false;
		m_LabelLimit.text = "Time: 3:00";
	}
	
	// Update is called once per frame
	void Update()
	{
		if (m_CurrentMessage == 2)
		{
			if (m_MainInventory.GetNumberOfItems() < 16)
			{
				Button_NextMessage();
			}
		}
	}

	public void Button_NextMessage()
	{
		if (!m_MousePicker.IsCarrying || m_CurrentMessage == 2)
		{
			if (m_CurrentMessage < NUM_MESSAGES)
			{
				m_MessageList[m_CurrentMessage++].SetActive(false);
				m_MessageList[m_CurrentMessage].SetActive(true);

				if (m_CurrentMessage == 2)
				{
					m_MousePicker.Enabled = true;
				}
				else if (m_CurrentMessage == 4)
				{
					m_ButtonCashIn.SetActive(true);
				}
				else if (m_CurrentMessage == 5)
				{
					m_ButtonCashIn.SetActive(false);
					m_LabelScore.SetActive(true);
				}
				else if (m_CurrentMessage == 6)
				{
					m_MousePicker.Enabled = false;
					m_ItemDropManager.SpawnItems();
				}
				else if (m_CurrentMessage == 8)
				{
					m_LabelLimit.gameObject.SetActive(true);
				}
				else if (m_CurrentMessage == 9)
				{
					m_LabelLimit.text = "Moves: 100";
				}
			}
			else
			{
				//Application.LoadLevel(0);
				MainMenu.LoadLevel(0);
			}
		}
	}
}