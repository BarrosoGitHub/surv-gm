
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class EnemyManager : MonoBehaviour
{
    private List<EnemyGroup> enemyGroups;
    private ControllerSpawner controllerSpawner;

    void Awake()
    {
        controllerSpawner = GetComponent<ControllerSpawner>();
    }

    private void Start()
    {
        enemyGroups = new List<EnemyGroup>
        {
            new EnemyGroup(null, 0, 0f, 0, 0f) { EnemyType = EnemyType.Normal },
            new EnemyGroup(null, 0, 0f, 0, 0f) { EnemyType = EnemyType.Tank },
            new EnemyGroup(null, 0, 0f, 0, 0f) { EnemyType = EnemyType.Boss }
        };
    }

    private void OnGameStateChanged(GameState gameState)
    {
        if (gameState == GameState.Playing)
        {
            foreach (EnemyGroup enemyGroup in enemyGroups)
            {
                StartCoroutine(SpawnEnemiesCoroutine(enemyGroup));
                StartCoroutine(PerformEnemyGroupAttackCoroutine(enemyGroup));
                StartCoroutine(RefreshAttackingEnemyGroupCoroutine(enemyGroup));

            }
        }
        if (gameState == GameState.MainMenu)
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
            foreach (EnemyGroup group in enemyGroups)
            {
                if (group.EnemyList.Contains(enemyController))
                {
                    enemyController.OnControllerDied -= OnEnemyDied;

                    group.EnemyList.Remove((EnemyController) enemyController);

                    controllerSpawner.DespawnController(group.EnemyType.ToString(), controller, 5);

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
            yield return new WaitForSeconds(2f); // Wait for 5 seconds before the next spawn
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
                    if (enemyController.IsAliveAndReady)
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
            .Where(e => e != null && e.IsAliveAndReady)
            .OrderBy(e => (e.transform.position - position).sqrMagnitude)
            .Take(maxNumberOfEnemies)
            .ToList();
    }

    private void RemoveControllerFromGroup(EnemyGroup enemyGroup, EnemyController enemyController)
    {
        if (enemyGroup.EnemyList.Contains(enemyController))
        {
            enemyGroup.EnemyList.Remove(enemyController);

            enemyController.ChangeToIdleStateGracefully();
        }
    }

    private void AddEnemyToGroup(EnemyGroup enemyGroup, EnemyController enemyController)
    {
        if (!enemyGroup.EnemyList.Contains(enemyController))
        {
            enemyGroup.EnemyList.Add(enemyController);
            enemyController.Target = enemyGroup.TargetController;

            //Add aditional logic for adding enemy to group if needed
        }
    }
}