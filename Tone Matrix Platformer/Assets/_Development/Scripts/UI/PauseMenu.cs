using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
	[SerializeField] private GameObject pauseMenuUI;

	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			if (GameManager.gamePaused) {
				Resume();
			}
			else {
				Pause();
			}
		}
	}

	public void Resume () {
		GameManager.gamePaused = false;
		pauseMenuUI.SetActive(false);
		Time.timeScale = 1.0f;
	}

	void Pause () {
		GameManager.gamePaused = true;
		pauseMenuUI.SetActive(true);
		Time.timeScale = 0.0f;
	}

	public void ExitToMainMenu () {
		GameManager.LoadMainMenu();
	}

	public void ExitGame () {
		GameManager.ExitGame();
	}
}
