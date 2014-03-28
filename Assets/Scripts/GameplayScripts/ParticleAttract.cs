using UnityEngine;
using System.Collections;

public class ParticleAttract : MonoBehaviour
{
	private static Color[] m_Colours = new Color[] { new Color(100, 100, 100), new Color(200, 0, 0), new Color(0, 160, 0),
													 new Color(0, 165, 200), new Color(200, 200, 0)};

	private Vector3 m_AttractPosition = new Vector3(0.0f, 0.0f, 0.0f);
	private InventoryItem m_AttractTarget = null;
	private Vector3 m_Velocity;
	private float m_FloatTimer = 1.0f;

	public Vector3 AttractPosition { set {m_AttractPosition = value;}}
	public InventoryItem AttractTarget { set {m_AttractTarget = value;}}
	public WeaponType WeaponType { set {GetComponentInChildren<SpriteRenderer>().color = m_Colours[(int)value];}}

	// Use this for initialization
	void Awake()
	{
		m_Velocity = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0.0f);
	}
	
	// Update is called once per frame
	void Update()
	{
		Vector3 distance;
		if (m_AttractTarget != null)
		{
			distance = m_AttractTarget.CentrePosition - transform.position;
		}
		else
		{
			distance = m_AttractPosition - transform.position;
		}

		if (distance.sqrMagnitude <= 0.5f)
		{
			Destroy(transform.gameObject);
		}

		m_FloatTimer -= Time.deltaTime;
		if (m_FloatTimer <= 0.0f)
		{
			m_Velocity = (distance.normalized * 10.0f) * Mathf.Abs(m_FloatTimer * 2.0f);
		}

		/*float magnitude = distance.sqrMagnitude;
		magnitude = 1.0f / (magnitude * 0.1f);
		magnitude = Mathf.Max(magnitude, 1.0f);
		distance.Normalize();
		distance *= magnitude;
		
		m_Velocity += (distance * Time.deltaTime) * 8.0f;*/
		
		transform.position += m_Velocity * Time.deltaTime;
	}
}