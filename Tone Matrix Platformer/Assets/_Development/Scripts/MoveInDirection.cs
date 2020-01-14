using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInDirection : MonoBehaviour
{
	public Vector2 direction;
	public float movementSpeed;

    void Update() {
		Vector2 objectPosition = transform.position;
		objectPosition += direction * movementSpeed * Time.deltaTime;
		transform.position = objectPosition;
	}
}
