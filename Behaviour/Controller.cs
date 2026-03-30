using System;
using System.Runtime.InteropServices;
using UnityEngine;

public abstract class Controller : MonoBehaviour
{
    public Action<Controller> OnAttackController;
    public Action<Controller> OnDamaged;

    public Action<State> OnStateChanged;
    public Action<Controller> OnTargetChanged;
    public Action<Controller> OnDied;

    private State state;
    private Controller target;

    public State State
    {
        get
        {
            return state;
        }
        set
        {
            if (state == value)
            {
                return;
            }

            if (state != null && state.OnExitState != null)
            {
                state.OnExitState();
            }

            state = value;

            state.OnEnterState?.Invoke();

            OnStateChanged?.Invoke(state);
        }
    }
    public Controller Target
    {
        get
        {
            return target;
        }
        set
        {
            target = value;

            OnTargetChanged?.Invoke(target);
        }
    }

    public Collider col;
    public Rigidbody rb;

    public Entity entity;

    protected virtual void Awake()
    {
        //This needs a better way to start
        if (entity == null)
        {
            entity = new Entity(
                new Stats(
                    maxHealth: 1,
                    maxSpeed: 1,
                    baseDamage: 1,
                    attackSpeed: 1,
                    defense: 0,
                    criticalChance: 0,
                    criticalDamage: 1
                )
            );
        }
    }

    public virtual void Start()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        SetOnEnterState();
        SetOnExitState();
        SetOnUpdateState();
        SetOnFixedUpdateState();
    }

    public abstract void SetOnEnterState();

    public abstract void SetOnExitState();

    public abstract void SetOnUpdateState();

    public abstract void SetOnFixedUpdateState();

    private void AttackController(Controller other)
    {
        float damage = entity.stats.BaseDamage * entity.stats.BaseDamagePerc;
        bool isCritical = UnityEngine.Random.Range(0f, 100f) <= entity.stats.CriticalChance;
        float finalDamage = isCritical ? damage * entity.stats.CriticalDamage : damage;

        OnAttackController?.Invoke(other, finalDamage);

        other.GetAttackedByController(this, finalDamage);
    }

    public void GetAttackedByController(Controller attackerController, float amount)
    {
        entity.TakeDamage(amount);

        OnDamaged?.Invoke(attackerController);
    }

    public virtual OnControllerDied()
    {

    }

    private void OnEnable()
    {
        if (entity != null)
            entity.OnEntityDied += OnControllerDied;
    }

    private void OnDisable()
    {
        if (entity != null)
            entity.OnEntityDied -= OnControllerDied;
    }
}