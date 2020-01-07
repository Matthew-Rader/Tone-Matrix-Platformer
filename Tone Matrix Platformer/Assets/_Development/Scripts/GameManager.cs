using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public Transform currentReSpawnPoint;

	// GAMEMANAGER SINGLETON --------------------------------------------------
	private static GameManager _gameMangerInstance;
	public static GameManager Instance { get { return _gameMangerInstance; } }

	// PRIVATE VARIABLES ------------------------------------------------------
	private const string _FreePlayLevelName = "ToneMatrix Freeplay";
	private const string _MainMenu = "Main Menu";
	private const string _Level = "Level ";
	[SerializeField] private int startingLevel = 1;
	private int currentLevel;

	// Start is called before the first frame update
	void Awake()
    {
		if (_gameMangerInstance != null && _gameMangerInstance != this) {
			Destroy(this.gameObject);
		}
		else {
			_gameMangerInstance = this;
		}

		DontDestroyOnLoad(this.gameObject);

	}

	public void LoadFirstLevel () {
		currentLevel = startingLevel;
		SceneManager.LoadScene(_Level + startingLevel);
	}

	public void LoadNextLevel () {
		currentLevel += 1;
		SceneManager.LoadScene(_Level + (currentLevel));
	}

	public void LoadLevelFreePlay () {
		SceneManager.LoadScene(_FreePlayLevelName);
	}

	public void LoadMainMenu () {
		SceneManager.LoadScene(_MainMenu);
	}

	public void ExitGame () {
		Application.Quit();
	}
}
