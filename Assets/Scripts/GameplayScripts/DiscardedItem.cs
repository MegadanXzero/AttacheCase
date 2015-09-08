using UnityEngine;
using System.Collections;

public class DiscardedItem : MonoBehaviour
{
	
	// Use this for initialization
	void Awake()
	{
		transform.GetComponent<Rigidbody2D>().velocity = new Vector3(-3.0f, 5.0f, 0.0f);
	}
	
	void OnCollisionEnter2D(Collision2D coll)
	{
		if (coll.gameObject.CompareTag(Tags.GROUND))
		{
			GameObject.Destroy(gameObject);
		}
	}
}