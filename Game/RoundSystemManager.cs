using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundSystemManager : MonoBehaviour
{
    [Header("Spawn Setup")]
    [SerializeField] private EnemyController enemyPrefab;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Round Scaling")]
    [SerializeField] private int initialEnemiesPerRound = 4;
    [SerializeField] private int enemiesAddedPerRound = 2;
    [SerializeField] private int maxAliveEnemiesAtOnce = 8;

    [Header("Timing")]
    [SerializeField] private float initialSpawnInterval = 1.2f;
    [SerializeField] private float spawnIntervalReductionPerRound = 0.08f;
    [SerializeField] private float minimumSpawnInterval = 0.3f;
    [SerializeField] private float timeBetweenRounds = 4f;

    [Header("Control")]
    [SerializeField] private bool startOnAwake = true;

    private readonly List<EnemyController> aliveEnemies = new List<EnemyController>();
    private Coroutine roundLoopRoutine;

    public int CurrentRound { get; private set; }
    public bool IsRoundActive { get; private set; }

    public event Action<int> OnRoundStarted;
    public event Action<int> OnRoundCompleted;

    private void Start()
    {
        if (startOnAwake)
        {
            StartRounds();
        }
    }

    public void StartRounds()
    {
        if (roundLoopRoutine != null)
        {
            return;
        }

        roundLoopRoutine = StartCoroutine(RoundLoop());
    }

    public void StopRounds()
    {
        if (roundLoopRoutine == null)
        {
            return;
        }

        StopCoroutine(roundLoopRoutine);
        roundLoopRoutine = null;
        IsRoundActive = false;
    }

    private IEnumerator RoundLoop()
    {
        while (true)
        {
            CurrentRound++;
            IsRoundActive = true;
            OnRoundStarted?.Invoke(CurrentRound);

            int enemiesToSpawn = initialEnemiesPerRound + (CurrentRound - 1) * enemiesAddedPerRound;
            int spawnedThisRound = 0;
            float spawnInterval = Mathf.Max(
                minimumSpawnInterval,
                initialSpawnInterval - (CurrentRound - 1) * spawnIntervalReductionPerRound
            );

            while (spawnedThisRound < enemiesToSpawn || aliveEnemies.Count > 0)
            {
                while (spawnedThisRound < enemiesToSpawn && aliveEnemies.Count < maxAliveEnemiesAtOnce)
                {
                    SpawnEnemy();
                    spawnedThisRound++;
                    yield return new WaitForSeconds(spawnInterval);
                }

                aliveEnemies.RemoveAll(enemy => enemy == null);

                if (spawnedThisRound >= enemiesToSpawn && aliveEnemies.Count == 0)
                {
                    break;
                }

                yield return null;
            }

            IsRoundActive = false;
            OnRoundCompleted?.Invoke(CurrentRound);
            yield return new WaitForSeconds(timeBetweenRounds);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("RoundSystemManager is missing an enemy prefab reference.");
            return;
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("RoundSystemManager has no spawn points assigned.");
            return;
        }

        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
        EnemyController enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemy.Initialize();
        aliveEnemies.Add(enemy);
        // enemy.OnControllerDied += HandleEnemyDied;
    }

    // private void HandleEnemyDied(Controller enemy)
    // {
    //     if (enemy != null)
    //     {
    //         enemy.OnDied -= HandleEnemyDied;
    //     }

    //     aliveEnemies.Remove((EnemyController) enemy);
    // }

    private void OnDestroy()
    {
        foreach (EnemyController enemy in aliveEnemies)
        {
            if (enemy != null)
            {
                enemy.OnDied -= HandleEnemyDied;
            }
        }

        aliveEnemies.Clear();
    }
}
