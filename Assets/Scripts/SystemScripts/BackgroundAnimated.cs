using UnityEngine;
using System.Collections;

public class BackgroundAnimated : MonoBehaviour
{
	[SerializeField] private Transform m_BackgroundHorizontal;
	[SerializeField] private Transform m_BackgroundVertical;

	// Use this for initialization
	void Start()
	{

	}

	private void Update()
	{
		float currentTime = Time.realtimeSinceStartup;
		//float currentTime = Time.fixedTime;
		Vector3 tempPos = m_BackgroundHorizontal.position;
		tempPos.x = transform.position.x + ((Mathf.Sin(currentTime * 0.12f) * 3.0f) + (Mathf.Sin(currentTime * 0.3f) * 2.0f));
		tempPos.y = transform.position.y + (Mathf.Sin(currentTime * 0.17f) * 1.5f);
		m_BackgroundHorizontal.position = tempPos;

		/*tempPos = m_BackgroundVertical.position;
		tempPos.y = transform.position.y + (Mathf.Sin(currentTime * 0.083f) * 5.0f);
		tempPos.x = transform.position.y + (Mathf.Sin(currentTime * 0.14f) * 1.5f);
		m_BackgroundVertical.position = tempPos;*/

		tempPos = m_BackgroundVertical.position;
		tempPos.y = transform.position.y + (Mathf.Sin(currentTime * 0.087f) * 5.0f);
		tempPos.x = transform.position.x + (Mathf.Sin((currentTime * 0.087f) + 90.0f) * 5.0f);
		m_BackgroundVertical.position = tempPos;
	}

	public void SendToBack()
	{
		Vector3 tempPos = transform.position;
		tempPos.z = 10.0f;
		transform.position = tempPos;
	}

	public void BringToFront()
	{
		Vector3 tempPos = transform.position;
		tempPos.z = -5.0f;
		transform.position = tempPos;
	}
}