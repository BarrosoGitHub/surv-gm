
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class EnemyManager : MonoBehaviour
{
    private WaveSpecification currentWaveSpecification;
    private WaveSystemManager waveSystemManager;
    private ControllerSpawner controllerSpawner;

    void Awake()
    {
        controllerSpawner = GetComponent<ControllerSpawner>();
        waveSystemManager = GetComponent<WaveSystemManager>();
    }

    private void Start()
    {

    }

    private void OnWaveSpecificationChanged(WaveSpecification waveSpecification)
    {
        Debug.Log($"New wave specification generated: {waveSpecification}");
        currentWaveSpecification = waveSpecification;
    }

    private void OnWavePhaseChanged(WavePhase wavePhase)
    {
        Debug.Log($"Wave phase changed: {wavePhase}");
        if (wavePhase == WavePhase.WaveInProgress)
        {
            foreach (EnemyGroup enemyGroup in currentWaveSpecification.enemyGroups)
            {
                StartCoroutine(SpawnEnemiesCoroutine(enemyGroup));
                // StartCoroutine(PerformEnemyGroupAttackCoroutine(enemyGroup));
                StartCoroutine(RefreshAttackingEnemyGroupCoroutine(enemyGroup));
            }
        }
        if (wavePhase == WavePhase.Stoped)
        {
            StopAllCoroutines();
        }
    }

    private Controller SpawnEnemy(EnemyGroup enemyGroup)
    {
        EnemyController enemyController = controllerSpawner.SpawnController(enemyGroup.EnemyType.ToString(), Vector3.zero, Quaternion.identity) as EnemyController;

        enemyController.OnControllerDied += OnEnemyDied;

        return enemyController;
    }

    private void OnEnemyDied(Controller controller)
    {
        EnemyController enemyController = controller as EnemyController;
        if (enemyController != null)
        {
            foreach (EnemyGroup group in currentWaveSpecification.enemyGroups)
            {
                if (group.EnemyList.Contains(enemyController))
                {
                    enemyController.OnControllerDied -= OnEnemyDied;

                    group.EnemyList.Remove((EnemyController)enemyController);

                    controllerSpawner.DespawnController(group.EnemyType.ToString(), controller);

                    break;
                }
            }
        }
    }

    private IEnumerator SpawnEnemiesCoroutine(EnemyGroup enemyGroup)
    {
        while (true)
        {
            SpawnEnemy(enemyGroup);
            float spawnInterval = currentWaveSpecification.waveDuration / enemyGroup.MaxNumberOfEnemies; // TODO: calculate based on wave specification
            yield return new WaitForSeconds(spawnInterval); // Wait for 5 seconds before the next spawn
        }
    }

    private IEnumerator PerformEnemyGroupAttackCoroutine(EnemyGroup enemyGroup)
    {
        List<EnemyController> enemiesToPerformAttack = null;
        var targetPosition = enemyGroup.TargetController.transform.position;

        while (enemiesToPerformAttack == null)
        {
            var orderedEnemiesByDistance = enemyGroup.EnemyList
                        .OrderBy(c => (c.transform.position - targetPosition).sqrMagnitude)
                        .ToList();

            if (orderedEnemiesByDistance.Count > 0 &&
                    Vector3.Distance(targetPosition,
                        orderedEnemiesByDistance[0].transform.position) > enemyGroup.MaxEngagementDistance)
            {
                yield return null; // Wait for the next frame before re-evaluating
                continue; // Skip the rest of the loop and start over
            }

            for (int i = 0; i < orderedEnemiesByDistance.Count; i++)
            {
                int numberOfSimultaneousAttackersCount = 0;

                if (enemyGroup.MaxNumberOfEnemiesThatCanEngageSimultaneously > numberOfSimultaneousAttackersCount)
                {
                    EnemyController enemyController = orderedEnemiesByDistance[i];
                    if (enemyController.State == enemyController.pursuingState)
                    {
                        enemiesToPerformAttack.Add(enemyController);
                        numberOfSimultaneousAttackersCount++;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        foreach (EnemyController enemyController in enemiesToPerformAttack)
        {
            // enemyController.PerformAttack();
        }

        yield return new WaitForSeconds(enemyGroup.AttackInterval);
    }

    private IEnumerator RefreshAttackingEnemyGroupCoroutine(EnemyGroup enemyGroup)
    {
        Debug.Log($"Started refreshing attacking enemy group coroutine for group: {enemyGroup.EnemyType}");
        Debug.Log($"Current enemies in group: {enemyGroup.EnemyList.Count}");
        Debug.Log($"Target controller: {enemyGroup.TargetController}");
        
        List<EnemyController> closestEnemyControllers = GetClosestEnemyControllers(enemyGroup.MaxNumberOfEnemies, enemyGroup.TargetController.transform.position, enemyGroup.MaxEngagementDistance);

        List<EnemyController> enemiesToRemove = enemyGroup.EnemyList.Except(closestEnemyControllers).ToList();
        List<EnemyController> enemiesToAdd = closestEnemyControllers.Except(enemyGroup.EnemyList).ToList();

        foreach (EnemyController enemyController in enemiesToRemove)
        {
            RemoveControllerFromGroup(enemyGroup, enemyController);
        }

        foreach (EnemyController enemyController in enemiesToAdd)
        {
            AddEnemyToGroup(enemyGroup, enemyController);
        }

        yield return new WaitForSeconds(1f);
    }

    private List<EnemyController> GetClosestEnemyControllers(int maxNumberOfEnemies, Vector3 position, float radious)
    {
        return Physics.OverlapSphere(position, radious)
            .Select(c => c.GetComponent<EnemyController>())
            .Where(e => e != null && (e.State == e.idlingState || e.State == e.pursuingState))
            .OrderBy(e => (e.transform.position - position).sqrMagnitude)
            .Take(maxNumberOfEnemies)
            .ToList();
    }

    private void RemoveControllerFromGroup(EnemyGroup enemyGroup, EnemyController enemyController)
    {
        if (enemyGroup.EnemyList.Contains(enemyController))
        {
            enemyGroup.EnemyList.Remove(enemyController);
            enemyController.State = enemyController.idlingState;
        }
    }

    private void AddEnemyToGroup(EnemyGroup enemyGroup, EnemyController enemyController)
    {
        if (!enemyGroup.EnemyList.Contains(enemyController))
        {
            enemyGroup.EnemyList.Add(enemyController);
            enemyController.Target = enemyGroup.TargetController;
            enemyController.State = enemyController.pursuingState;
        }
    }

    private void OnEnable()
    {
        waveSystemManager.OnWavePhaseChanged += OnWavePhaseChanged;
        waveSystemManager.OnNewWaveSpecificationGenerated += OnWaveSpecificationChanged;

    }

    private void OnDisable()
    {
        waveSystemManager.OnWavePhaseChanged -= OnWavePhaseChanged;
        waveSystemManager.OnNewWaveSpecificationGenerated -= OnWaveSpecificationChanged;

    }
}