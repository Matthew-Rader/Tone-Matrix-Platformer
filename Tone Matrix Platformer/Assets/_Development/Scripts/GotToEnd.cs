using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GotToEnd : MonoBehaviour
{
	void OnTriggerEnter2D (Collider2D col) {
		if (col.tag == GameManager._PlayerTag) {
			GameManager.LoadNextLevel();
		}
	}
}
