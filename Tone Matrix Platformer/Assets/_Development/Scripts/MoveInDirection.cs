using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInDirection : MonoBehaviour
{
	// Just moves an object in a certain direction
	public Vector2 direction;
	public float movementSpeed;
	public bool setLifeSpan = false;
	public float lifeSpawn;
	private float currentTimeAlive = 0.0f;


    void Update() {
		Vector2 objectPosition = transform.position;
		objectPosition += direction * movementSpeed * Time.deltaTime;
		transform.position = objectPosition;

		if (setLifeSpan) {
			currentTimeAlive += Time.deltaTime;

			if (currentTimeAlive >= lifeSpawn) {
				Destroy(this.gameObject);
			}
		}
	}

	void OnCollision2D (Collision2D col) {
		
	}

	void OnBecameInvisible () {
		Destroy(this);
	}
}
