using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform muzzle;

    [Header("Shoot Settings")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float range = 50f;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private LayerMask hitMask = Physics.DefaultRaycastLayers;
    [SerializeField] private float hitRadius = 0.08f;

    [Header("Visual")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private float muzzleFlashDuration = 0.05f;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = true;
    [SerializeField] private bool enableVisualLogs = true;
    [SerializeField] private float debugRayDuration = 0.5f;

    private float _nextFireTime;

    private void Update()
    {
        if (Time.time < _nextFireTime)
            return;

        EnemyController target = FindClosestAliveEnemyInRange();
        if (target != null)
        {
            _nextFireTime = Time.time + fireRate;
            Shoot(target.transform);
        }
    }

    private EnemyController FindClosestAliveEnemyInRange()
    {
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        EnemyController closest = null;
        float closestDist = range;

        foreach (EnemyController enemy in enemies)
        {
            if (enemy == null || enemy.IsDead) continue;

            if (enableLogs)
            {
                Debug.Log($"[PlayerShooter] Checking enemy: {enemy.entity.CurrentHealth} HP | Position: {enemy.transform.position}", enemy);
            }
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= closestDist)
            {
                closestDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }

    private void Shoot(Transform target)
    {
        Vector3 origin = transform.position;
        Vector3 aimPoint = target.position;
        Vector3 direction = (aimPoint - origin).normalized;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = transform.forward;
        }

        if (enableVisualLogs)
        {
            Debug.DrawRay(origin, direction * range, Color.cyan, debugRayDuration);
            Debug.DrawLine(origin, aimPoint, Color.magenta, debugRayDuration);
        }

        if (enableLogs)
        {
            Debug.Log($"[PlayerShooter] Fire | Origin: {origin} | Direction: {direction} | Range: {range}", this);
        }

        bool didHit;
        RaycastHit hit;

        if (hitRadius > 0f)
        {
            didHit = Physics.SphereCast(origin, hitRadius, direction, out hit, range, hitMask, QueryTriggerInteraction.Ignore);
        }
        else
        {
            didHit = Physics.Raycast(origin, direction, out hit, range, hitMask, QueryTriggerInteraction.Ignore);
        }

        if (didHit)
        {
            if (enableVisualLogs)
            {
                Debug.DrawLine(origin, hit.point, Color.green, debugRayDuration);
            }

            EnemyController enemy = hit.collider.GetComponentInParent<EnemyController>();
            if (enemy != null)
            {
                enemy.ApplyDamage(damage);

                if (enableLogs)
                {
                    Debug.Log($"[PlayerShooter] Hit enemy: {enemy.name} | Damage: {damage} | Point: {hit.point}", enemy);
                }
            }
            else if (enableLogs)
            {
                Debug.Log($"[PlayerShooter] Hit non-enemy: {hit.collider.name} | Point: {hit.point}", hit.collider);
            }
        }
        else if (enableLogs)
        {
            Debug.Log("[PlayerShooter] Missed shot.", this);
        }

        if (enableVisualLogs)
        {
            Debug.DrawRay(origin + Vector3.up * 0.05f, direction * range, Color.yellow, debugRayDuration);
        }

        if (muzzleFlashPrefab != null && muzzle != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, muzzle.position, muzzle.rotation, muzzle);
            Destroy(flash, muzzleFlashDuration);
        }
    }

}

