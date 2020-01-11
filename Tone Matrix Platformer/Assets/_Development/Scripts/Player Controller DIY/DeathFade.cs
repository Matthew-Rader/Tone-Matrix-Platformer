using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathFade : MonoBehaviour
{
	[SerializeField] private Animator fadeAnim;

	public void StartDeathFadeCoroutine () {
		StartCoroutine(ReSpawnPlayer());
	}

	public void StartDeathFadeCoroutineNonDIY () {
		StartCoroutine(ReSpawnPlayerNonDIY());
	}

	// This could eventually trigger a fuller death sequence to play.
	// PlayerDeathAnim -> FadeOut -> Wait -> FadeIn -> PlayerEntraceAnim
	public IEnumerator ReSpawnPlayer()
	{
		fadeAnim.SetTrigger("FadeOut");

		// Reset player movement information
		transform.GetComponent<Player>().ResetPlayer();

		// Freeze the player
		transform.GetComponent<Player>().enabled = false;

		yield return new WaitForSeconds(0.75f);

		GameManager.SetPlayerToCurrentSpawnPoint();

		// Unfreeze the player
		transform.GetComponent<Player>().enabled = true;

		fadeAnim.SetTrigger("FadeIn");
	}

	// This could eventually trigger a fuller death sequence to play.
	// PlayerDeathAnim -> FadeOut -> Wait -> FadeIn -> PlayerEntraceAnim
	public IEnumerator ReSpawnPlayerNonDIY () {
		Debug.Log("fadeOut anim called");
		fadeAnim.SetTrigger("FadeOut");

		// Reset player movement information
		transform.GetComponent<CharacterController2D>().ResetPlayer();

		// Freeze the player
		transform.GetComponent<CharacterController2D>().enabled = false;
		//transform.GetComponent<Collision>().enabled = false;

		yield return new WaitForSeconds(0.75f);

		GameManager.SetPlayerToCurrentSpawnPoint();

		// Unfreeze the player
		transform.GetComponent<CharacterController2D>().enabled = true;
		//transform.GetComponent<Collision>().enabled = true; ;

		fadeAnim.SetTrigger("FadeIn");
		Debug.Log("fadeIn anim called");
	}
}
