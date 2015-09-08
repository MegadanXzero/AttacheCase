using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour
{
	private const int SCREEN_SECTOR = 8;
	private const int PIXELS_TO_UNITS = 10;
	private int PIXEL_SCALING = 3;

	[SerializeField] private float m_BaseHeight;
	[SerializeField] private GameObject m_PlayerCharacter;
	[SerializeField] private float m_DistanceFromCharacter;

	private float m_HorizontalSize;

	void Awake()
	{
		float cameraHeight = (float)Screen.height / (float)(SCREEN_SECTOR * PIXELS_TO_UNITS * PIXEL_SCALING);
		GetComponent<Camera>().orthographicSize = cameraHeight;
		m_HorizontalSize = cameraHeight * GetComponent<Camera>().aspect;
		m_PlayerCharacter = GameObject.FindGameObjectWithTag(Tags.PLAYER);
	}
	
	void FixedUpdate()
	{
		/*if (Screen.height < 700)
		{
			PIXEL_SCALING = 2;
		}
		else
		{
			PIXEL_SCALING = 3;
		}

		float cameraHeight = (float)Screen.height / (float)(SCREEN_SECTOR * PIXELS_TO_UNITS * PIXEL_SCALING);
		camera.orthographicSize = cameraHeight;
		m_HorizontalSize = cameraHeight * camera.aspect;*/

		if (m_PlayerCharacter != null)
		{
			Vector3 playerPos = m_PlayerCharacter.transform.position;
			Vector3 pos = transform.position;
			pos.x = playerPos.x + m_HorizontalSize + m_DistanceFromCharacter;
			pos.y = m_BaseHeight + GetComponent<Camera>().orthographicSize;
			transform.position = pos;
		}
	}
}