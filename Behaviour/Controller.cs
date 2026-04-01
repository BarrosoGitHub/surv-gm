using System;
using UnityEngine;

public abstract class Controller : MonoBehaviour
{
    public Action<Controller, InteractionContext> OnInteract;
    public Action<Controller, InteractionContext> OnReceivedInteraction;

    public Action<State> OnStateChanged;
    public Action<Controller> OnTargetChanged;

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
        //ScriptableObject or something
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

    public void Interact(Controller other, InteractionContext ctx)
    {
        InteractionContext result = ResolveInteraction(other, ctx);

        OnInteract?.Invoke(other, result);
        other.OnReceivedInteraction?.Invoke(this, result);
    }

    private InteractionContext ResolveInteraction(Controller other, InteractionContext ctx)
    {
        switch (ctx.Type)
        {
            case InteractionType.Attack:
                DamageResult dmg = entity.Attack(other.entity);
                return new InteractionContext { Type = InteractionType.Attack, Value = dmg.Damage, IsCritical = dmg.IsCritical };
            case InteractionType.Heal:
                other.entity.Heal(ctx.Value);
                return ctx;
            default:
                return ctx;
        }
    }

    public virtual void OnControllerDied()
    {
        
    }

    private void OnEnable()
    {
        if (entity != null)
        {
            entity.OnEntityDied += OnControllerDied;
        }
    }

    private void OnDisable()
    {
        if (entity != null)
        {
            entity.OnEntityDied -= OnControllerDied;
        }
    }
}