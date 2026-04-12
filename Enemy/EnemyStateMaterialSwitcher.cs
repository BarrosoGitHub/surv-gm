using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class EnemyStateMaterialSwitcher : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material idlingMaterial;
    [SerializeField] private Material pursuingMaterial;
    [SerializeField] private Material deadMaterial;

    private EnemyController enemyController;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();

        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }
    }



    private void Start()
    {
        if (enemyController != null)
        {
            enemyController.OnStateChanged += OnEnemyStateChanged;
        }

        ApplyFromCurrentState();
    }

    private void OnDestroy()
    {
        if (enemyController == null)
        {
            return;
        }

        enemyController.OnStateChanged -= OnEnemyStateChanged;
    }



    private void OnEnemyStateChanged(State state)
    {
        if (enemyController == null)
        {
            return;
        }

        if (state == enemyController.idlingState)
        {
            ApplyMaterial(idlingMaterial);
            return;
        }

        if (state == enemyController.pursuingState)
        {
            ApplyMaterial(pursuingMaterial);
            return;
        }

        if (state == enemyController.deadState)
        {
            ApplyMaterial(deadMaterial);
        }
    }


    private void ApplyFromCurrentState()
    {
        if (enemyController == null)
        {
            return;
        }

        OnEnemyStateChanged(enemyController.State);
    }

    private void ApplyMaterial(Material material)
    {
        if (targetRenderer == null || material == null)
        {
            return;
        }

        targetRenderer.sharedMaterial = material;
    }
}
