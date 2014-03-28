using UnityEngine;
using System.Collections;

public class LineDrawing : MonoBehaviour
{
	public int m_Width;
	public int m_Height; 
	
	private float m_Spacing = 1.0f;
	
	// Update is called once per frame
	void Update ()
	{
		Vector3 pos = transform.position;
		Debug.DrawLine(pos, new Vector3(pos.x, pos.y - ((float)m_Height * m_Spacing)), Color.white);
		Debug.DrawLine(pos, new Vector3(pos.x + ((float)m_Width * m_Spacing), pos.y), Color.white);
		
		for (int i = 0; i <= m_Width; i++)
		{
			Debug.DrawLine(new Vector3(pos.x + ((float)i * m_Spacing), pos.y), 
						   new Vector3(pos.x + ((float)i * m_Spacing), pos.y - ((float)m_Height * m_Spacing)), Color.white);
		}
		
		for (int i = 0; i <= m_Height; i++)
		{
			Debug.DrawLine(new Vector3(pos.x, pos.y - ((float)i * m_Spacing)), 
						   new Vector3(pos.x + ((float)m_Width * m_Spacing), pos.y - ((float)i * m_Spacing)), Color.white);
		}
	}
}
