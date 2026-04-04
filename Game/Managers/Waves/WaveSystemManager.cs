using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSystemManager : MonoBehaviour
{
    public Action<WavePhase> OnWavePhaseChanged;
    public Action<WaveSpecification> OnNewWaveStarted;


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

    public int CurrentWave { get; private set; }
    public bool IsRoundActive { get; private set; }

    private void Start()
    {
        if (startOnAwake)
        {
            StartRounds();
        }
    }

    public void StartWave()
    {
        WaveSpecification waveSpec = new WaveSpecification
        {
            waveNumber = CurrentWave,
            enemyGroups = GenerateEnemyGroupsForWave(CurrentWave),
            totalEnemiesPerType = CalculateTotalEnemiesPerTypeForWave(CurrentWave), // TODO: generate based on round config
            waveDuration = 30f // TODO: calculate based on spawn intervals and enemy count
        };

        OnNewWaveStarted?.Invoke(waveSpec);
        OnWavePhaseChanged?.Invoke(WavePhase.WaveInProgress);
    }

    private List<EnemyGroup> GenerateEnemyGroupsForWave(int waveNumber)
    {

        Controller targetController = null;
        int maxNumberOfEnemies = 3;
        float maxEngagementDistance = 5f;
        int maxNumberOfEnemiesThatCanEngageSimultaneously = 1 + waveNumber / 20;
        float attackInterval = Mathf.Max(0.2f, 1f / (1f + 0.3f * Mathf.Log(waveNumber))); // Round 10 0.59s, Round 20 0.53s, 

        List<EnemyGroup> enemyGroups = new List<EnemyGroup>
        {
            new EnemyGroup(EnemyType.Normal, targetController, maxNumberOfEnemies, maxEngagementDistance, maxNumberOfEnemiesThatCanEngageSimultaneously, attackInterval),
            // new EnemyGroup(EnemyType.Tank, null, 0, 0f, 0, 0f),
            // new EnemyGroup(EnemyType.Boss, null, 0, 0f, 0, 0f)
        };

        return enemyGroups;
    }

    private Dictionary<EnemyType, int> CalculateTotalEnemiesPerTypeForWave(int waveNumber)
    {
        Dictionary<EnemyType, int> totals = new Dictionary<EnemyType, int>
        {
            { EnemyType.Normal, initialEnemiesPerRound + (waveNumber - 1) * enemiesAddedPerRound },
            { EnemyType.Tank, Mathf.Max(0, (waveNumber - 5) / 10) }, // Start spawning tanks at round 5, then add 1 every 10 rounds
            { EnemyType.Boss, waveNumber >= 5 ? 1 : 0 } // Spawn 1 boss every round starting at round 5
        };

        return totals;
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
            CurrentWave++;
            IsRoundActive = true;
            OnRoundStarted?.Invoke(CurrentWave);

            int enemiesToSpawn = initialEnemiesPerRound + (CurrentWave - 1) * enemiesAddedPerRound;
            int spawnedThisRound = 0;
            float spawnInterval = Mathf.Max(
                minimumSpawnInterval,
                initialSpawnInterval - (CurrentWave - 1) * spawnIntervalReductionPerRound
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
            OnRoundCompleted?.Invoke(CurrentWave);
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
                // enemy.OnDied -= HandleEnemyDied;
            }
        }

        aliveEnemies.Clear();
    }
}
