using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public Action<GameState> OnGameStateChanged;

    public static GameManager Instance;

    [SerializeField]
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

    }

    void Start()
    {
        GameState = GameState.Playing;
    }
}
