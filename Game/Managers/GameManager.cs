using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public Action<Controller> OnPlayerSpawn;
    public Action<Controller> OnPlayerDespawn;
    public Action<GameState> OnGameStateChanged;

    public static GameManager Instance;
    private ControllerSpawner controllerSpawner;

    private GameState gameState;
    public GameState GameState
    {
        get { return gameState; }
        set 
        { 
            if (gameState == value)
                return;

            gameState = value;
            OnGameStateChanged?.Invoke(gameState);
        }
    }

    public Transform playerSpawn;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        GameState = GameState.StartMenu;

        gameObject.AddComponent<EnemyManager>();
        controllerSpawner = gameObject.AddComponent<ControllerSpawner>();

    }

    void Start()
    {
        SpawnPlayer();
    }

    public void StartGame()
    {
        GameState = GameState.InGame;
    }

    void SpawnPlayer()
    {
        Controller controller = controllerSpawner.SpawnController("Player", playerSpawn.position, Quaternion.identity);
        OnPlayerSpawn?.Invoke(controller);

        controller.OnControllerDied += EndGame;
    }
    void DespawnPlayer(Controller playerController)
    {
        OnPlayerDespawn?.Invoke(playerController);
        playerController.OnControllerDied -= EndGame;

        controllerSpawner.DespawnController("Player", playerController);
    }

    private void EndGame(Controller controller)
    {
        
    }
}
