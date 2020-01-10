using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	public void StartFreePlay () {
		GameManager.LoadLevelFreePlay();
	}

	public void StartGame () {
		GameManager.LoadFirstLevel();
	}

	public void ExitGame () {
		GameManager.ExitGame();
	}
}
