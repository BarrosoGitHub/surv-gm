using UnityEngine;

[DisallowMultipleComponent]
public class EnemyRangeIndicatorController : MonoBehaviour
{
    [Header("Indicator")]
    [SerializeField] private Renderer indicatorRenderer;
    [SerializeField] private bool hideOnStart = true;

    [Header("Shader Settings")]
    [SerializeField] private Color indicatorColor = new Color(1f, 0.2f, 0.2f, 0.45f);
    [SerializeField] private Color fillColor = new Color(1f, 1f, 1f, 0.45f);
    [SerializeField, Min(0f)] private float range = 5f;
    [SerializeField, Range(1f, 89f)] private float halfAngleDeg = 30f;
    [SerializeField, Min(0f)] private float edgeSoftness = 0.35f;
    [SerializeField, Range(0f, 1f)] private float fill;

    [Header("Fill Animation")]
    [SerializeField, Min(0f)] private float fillSpeed = 1f;
    [SerializeField] private bool autoFill;

    [Header("Optional State Sync")]
    [SerializeField] private bool syncWithEnemyState;
    [SerializeField] private EnemyController enemyController;

    private MaterialPropertyBlock materialPropertyBlock;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int FillColorId = Shader.PropertyToID("_FillColor");
    private static readonly int RangeId = Shader.PropertyToID("_Range");
    private static readonly int HalfAngleDegId = Shader.PropertyToID("_HalfAngleDeg");
    private static readonly int EdgeSoftnessId = Shader.PropertyToID("_EdgeSoftness");
    private static readonly int FillId = Shader.PropertyToID("_Fill");

    private void Awake()
    {
        if (indicatorRenderer == null)
        {
            indicatorRenderer = GetComponentInChildren<Renderer>(true);
        }

        if (syncWithEnemyState && enemyController == null)
        {
            enemyController = GetComponent<EnemyController>();
        }

        materialPropertyBlock = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        if (syncWithEnemyState && enemyController != null)
        {
            enemyController.OnStateChanged += OnEnemyStateChanged;
        }
    }

    private void Start()
    {
        ApplyToMaterial();

        if (syncWithEnemyState && enemyController != null)
        {
            OnEnemyStateChanged(enemyController.State);
            return;
        }

        if (hideOnStart)
        {
            Hide();
        }
    }

    private void OnDisable()
    {
        if (enemyController == null)
        {
            return;
        }

        enemyController.OnStateChanged -= OnEnemyStateChanged;
    }

    private void OnValidate()
    {
        range = Mathf.Max(0f, range);
        edgeSoftness = Mathf.Max(0f, edgeSoftness);

        if (!Application.isPlaying)
        {
            if (indicatorRenderer == null)
            {
                indicatorRenderer = GetComponentInChildren<Renderer>(true);
            }

            if (materialPropertyBlock == null)
            {
                materialPropertyBlock = new MaterialPropertyBlock();
            }

            ApplyToMaterial();
        }
    }

    public void SetRange(float newRange)
    {
        range = Mathf.Max(0f, newRange);
        ApplyToMaterial();
    }

    public void SetHalfAngle(float newHalfAngleDeg)
    {
        halfAngleDeg = Mathf.Clamp(newHalfAngleDeg, 1f, 89f);
        ApplyToMaterial();
    }

    public void SetEdgeSoftness(float newEdgeSoftness)
    {
        edgeSoftness = Mathf.Max(0f, newEdgeSoftness);
        ApplyToMaterial();
    }

    public void SetColor(Color newColor)
    {
        indicatorColor = newColor;
        ApplyToMaterial();
    }

    public void SetFillColor(Color newFillColor)
    {
        fillColor = newFillColor;
        ApplyToMaterial();
    }

    public void SetFill(float newFill)
    {
        fill = Mathf.Clamp01(newFill);
        ApplyToMaterial();
    }

    public void ResetFill()
    {
        fill = 0f;
        ApplyToMaterial();
    }

    public void Configure(float newRange, float newHalfAngleDeg, float newEdgeSoftness, Color newColor)
    {
        range = Mathf.Max(0f, newRange);
        halfAngleDeg = Mathf.Clamp(newHalfAngleDeg, 1f, 89f);
        edgeSoftness = Mathf.Max(0f, newEdgeSoftness);
        indicatorColor = newColor;
        ApplyToMaterial();
    }

    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (indicatorRenderer == null)
        {
            return;
        }

        if (indicatorRenderer.gameObject.activeSelf != visible)
        {
            indicatorRenderer.gameObject.SetActive(visible);
        }
    }

    private void Update()
    {
        if (!autoFill || !indicatorRenderer || !indicatorRenderer.gameObject.activeSelf)
        {
            return;
        }

        if (fill < 1f)
        {
            fill = Mathf.Clamp01(fill + fillSpeed * Time.deltaTime);
            ApplyToMaterial();
        }
    }

    private void OnEnemyStateChanged(State state)
    {
        if (enemyController == null)
        {
            return;
        }

        bool shouldShow = state == enemyController.pursuingState;

        if (shouldShow)
        {
            ResetFill();
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void ApplyToMaterial()
    {
        if (indicatorRenderer == null)
        {
            return;
        }

        indicatorRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetColor(ColorId, indicatorColor);
        materialPropertyBlock.SetColor(FillColorId, fillColor);
        materialPropertyBlock.SetFloat(RangeId, range);
        materialPropertyBlock.SetFloat(HalfAngleDegId, halfAngleDeg);
        materialPropertyBlock.SetFloat(EdgeSoftnessId, edgeSoftness);
        materialPropertyBlock.SetFloat(FillId, fill);
        indicatorRenderer.SetPropertyBlock(materialPropertyBlock);
    }
}
