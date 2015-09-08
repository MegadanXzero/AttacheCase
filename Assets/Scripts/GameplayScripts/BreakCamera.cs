using UnityEngine;
using System.Collections;

public class BreakCamera : MonoBehaviour
{
	private const int SCREEN_SECTOR = 8;
	private const int PIXELS_TO_UNITS = 10;
	private int PIXEL_SCALING = 3;
	
	[SerializeField] private float m_BaseHeight;
	
	void Awake()
	{
		if (Screen.height <= 700)
		{
			PIXEL_SCALING = 2;
		}
		else
		{
			PIXEL_SCALING = 3;
		}

		float cameraHeight = (float)Screen.height / (float)(SCREEN_SECTOR * PIXELS_TO_UNITS * PIXEL_SCALING);
		GetComponent<Camera>().orthographicSize = cameraHeight;

		Vector3 pos = transform.position;
		pos.y = m_BaseHeight + GetComponent<Camera>().orthographicSize;
		transform.position = pos;
	}
	
	void FixedUpdate()
	{
		/*if (Screen.height <= 700)
		{
			PIXEL_SCALING = 2;
		}
		else
		{
			PIXEL_SCALING = 3;
		}
		
		float cameraHeight = (float)Screen.height / (float)(SCREEN_SECTOR * PIXELS_TO_UNITS * PIXEL_SCALING);
		camera.orthographicSize = cameraHeight;

		Vector3 pos = transform.position;
		pos.y = m_BaseHeight + camera.orthographicSize;
		transform.position = pos;*/
	}
}