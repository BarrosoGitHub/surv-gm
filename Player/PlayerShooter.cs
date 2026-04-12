using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Shoot Settings")]
    [SerializeField] private float range = 50f;
    [SerializeField] private float fireRate = 1f;


    public PlayerController playerController;

    private float _nextFireTime;

    private void Update()
    {
        if (Time.time < _nextFireTime)
            return;

        EnemyController target = FindClosestAliveEnemyInRange();
        if (target != null)
        {
            _nextFireTime = Time.time + fireRate;
            Debug.Log($"Shooting at SHOOT!");
            Shoot(target);
        }
    }

    private EnemyController FindClosestAliveEnemyInRange()
    {
        EnemyController[] enemies = FindObjectsByType<EnemyController>();
        EnemyController closest = null;
        float closestDist = range;

        foreach (EnemyController enemy in enemies)
        {
            if (enemy == null || enemy.State == enemy.deadState) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= closestDist)
            {
                closestDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }

    private void Shoot(Controller target)
    {
        playerController.AttackController(target);
    }

}

