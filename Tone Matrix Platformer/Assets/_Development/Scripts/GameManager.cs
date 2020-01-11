using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static Transform _CurrentSpawnPoint;
	public static int currentLevel;
	public static bool gamePaused = false;

	// GAMEMANAGER SINGLETON ------------------------------------------------------------
	private static GameManager _gameMangerInstance;
	public static GameManager Instance { get { return _gameMangerInstance; } }

	// STRING CONST VARIABLES -----------------------------------------------------------
	public const string _FreePlayLevelName = "ToneMatrix Freeplay";
	public const string _MainMenu = "Main Menu";
	public const string _Level = "Level ";
	public const string _PlayerTag = "Player";

	// PRIVATE VARIABLES ----------------------------------------------------------------
	[SerializeField] private static int startingLevel = 1;

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

	public static void LoadFirstLevel () {
		currentLevel = startingLevel;
		SceneManager.LoadScene(_Level + startingLevel);
	}

	public static void LoadNextLevel () {
		currentLevel += 1;
		SceneManager.LoadScene(_Level + (currentLevel));
	}

	public static void LoadLevelFreePlay () {
		SceneManager.LoadScene(_FreePlayLevelName);
	}

	public static void LoadMainMenu () {
		SceneManager.LoadScene(_MainMenu);
	}

	public static void ExitGame () {
		Application.Quit();
	}

	public static Vector3 GetCurrentSpawnPointPosition () {
		return _CurrentSpawnPoint.position;
	}

	public static void SetSpawnPoint (Transform spawnPoint) {
		_CurrentSpawnPoint = spawnPoint;
	}

	public static void SetPlayerToCurrentSpawnPoint () {
		GameObject.FindGameObjectWithTag(_PlayerTag).transform.position = _CurrentSpawnPoint.position;
	}
}
