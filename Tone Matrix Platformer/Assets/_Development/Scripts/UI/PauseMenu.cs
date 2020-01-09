using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
	public static bool _GamePaused = false;
	[SerializeField] private GameObject pauseMenuUI;

	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			if (_GamePaused) {
				Resume();
			}
			else {
				Pause();
			}
		}
	}

	public void Resume () {
		_GamePaused = false;
		pauseMenuUI.SetActive(false);
		Time.timeScale = 1.0f;
	}

	void Pause () {
		_GamePaused = true;
		pauseMenuUI.SetActive(true);
		Time.timeScale = 0.0f;
	}
}
