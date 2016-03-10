using UnityEngine;
using System.Collections;

public class BackgroundAnimated : MonoBehaviour
{
	[SerializeField] private Transform m_BackgroundHorizontal;
	[SerializeField] private Transform m_BackgroundVertical;

	private float m_PreviousTime = 0.0f;
	static private float s_RealTime = 0.0f;

	// Use this for initialization
	void Start()
	{
		m_PreviousTime = Time.realtimeSinceStartup;
	}

	private void Update()
	{
		float realDelta = Time.realtimeSinceStartup - m_PreviousTime;
		m_PreviousTime = Time.realtimeSinceStartup;
		s_RealTime += realDelta;

		//float currentTime = Time.realtimeSinceStartup;
		//float currentTime = Time.fixedTime;
		Vector3 tempPos = m_BackgroundHorizontal.position;
		//tempPos.x = transform.position.x + ((Mathf.Sin(s_RealTime * 0.12f) * 3.0f) + (Mathf.Sin(s_RealTime * 0.3f) * 2.0f));
		//tempPos.y = transform.position.y + (Mathf.Sin(s_RealTime * 0.19f) * 2.5f);
		tempPos.y = transform.position.y + (Mathf.Cos(s_RealTime * 0.087f) * 5.0f);
		tempPos.x = transform.position.x + (Mathf.Sin(s_RealTime * 0.087f) * 5.0f);
		m_BackgroundHorizontal.position = tempPos;

		tempPos = m_BackgroundVertical.position;
		tempPos.y = transform.position.y + (Mathf.Sin(s_RealTime * 0.097f) * 5.0f);
		tempPos.x = transform.position.x + (Mathf.Cos(s_RealTime * 0.097f) * 4.0f) + (Mathf.Sin(s_RealTime * 0.32f) * 1.0f);
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