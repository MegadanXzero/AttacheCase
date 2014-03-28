using UnityEngine;
using System.Collections;
using System;

public class EnemySpawner : MonoBehaviour
{
	[Serializable]
	public class EnemyInfo
	{
		public Transform m_EnemyPrefab;
		public int m_SpawnChance;
		public float m_MinSpawnTime;
		public float m_MaxSpawnTime;
	}

	[SerializeField] private CharacterScript m_Character;
	[SerializeField] private EnemyInfo[] m_EnemyList;
	//[SerializeField] private float m_SpawnDelay;
	//[SerializeField] private float m_SpawnVariance;

	private float m_SpawnTimer;
	private float m_HealthScaling = 0.9f;
	private float m_DamageScaling = 0.9f;

	// Use this for initialization
	void Awake()
	{
		m_SpawnTimer = 1.0f;//m_SpawnDelay;
		m_Character = GameObject.FindGameObjectWithTag(Tags.PLAYER).GetComponent<CharacterScript>();

		m_HealthScaling = GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>().HealthScaling;
		m_DamageScaling = GameObject.FindGameObjectWithTag(Tags.GAMECONTROLLER).GetComponent<GameController>().DamageScaling;
	}

	void FixedUpdate()
	{
		m_SpawnTimer -= Time.fixedDeltaTime;
		if (m_SpawnTimer <= 0.0f)
		{
			// Create a random enemy and place them far enough from the player that they'll be just offscreen
			int random = UnityEngine.Random.Range(0, 100);
			foreach (EnemyInfo enemy in m_EnemyList)
			{
				if (random < enemy.m_SpawnChance)
				{
					//Vector3 pos = m_Character.transform.position + new Vector3(21.0f, 0f, 0.0f);
					Vector3 pos = m_Character.transform.position + new Vector3(68.0f, 0f, 0.0f);
					Transform enemyTrans = Instantiate(enemy.m_EnemyPrefab, pos, Quaternion.identity) as Transform;
					EnemyAI enemyAI = enemyTrans.GetComponent<EnemyAI>();
					enemyAI.SetScaling(m_HealthScaling, m_DamageScaling);

					m_SpawnTimer = UnityEngine.Random.Range(enemy.m_MinSpawnTime, enemy.m_MaxSpawnTime);
					break;
				}
			}

			//m_SpawnTimer = m_SpawnDelay;
			//float realSpawnVariance = UnityEngine.Random.Range(0.0f, m_SpawnVariance);
			//m_SpawnTimer += UnityEngine.Random.Range(0, 2) == 0 ? realSpawnVariance : -realSpawnVariance;
		}
	}

	public void ScaleHealth()
	{
		m_HealthScaling *= 1.01f;
	}
	
	public void ScaleDamage()
	{
		m_DamageScaling *= 1.01f;
	}
}