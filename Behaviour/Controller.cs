using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public abstract class Controller : MonoBehaviour
{
    public Action<ActionInstance> OnActionInstancePerformed;
    public Action<float, ActionInstance> OnPreparingActionInstance;
    public Action<float, ActionInstance> OnExecutingActionInstance;
    public Action<ActionInstance> OnCompleteActionInstance;
    public Action<ActionInstance> OnCancelActionInstance;
    public Action<State> OnStateChanged;
    public Action<Controller> OnTargetChanged;
    public Action<Controller> OnControllerDied;

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
    public List<ActionInstance> ActionInstances { get; set; }
    public ActionInstance CurrentActionInstance { get; private set; }
    public Sequence ActionSequence;

    protected virtual void Awake()
    {
        ActionInstances = new List<ActionInstance>();

        //This needs a better way to start
        //ScriptableObject or something
        if (entity == null)
        {
            entity = new Entity(
                new Stats(
                    maxHealth: 1,
                    maxSpeed: 3,
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

    public void AttackController(Controller target)
    {
        Interact(target, new InteractionContext { Type = InteractionType.Attack });
    }

    private void Interact(Controller other, InteractionContext ctx)
    {
        if (!InteractionManager.Instance.TryProcessInteraction(this, other, ctx, out _))
        {
            return;
        }
    }

    public virtual void OnDied()
    {
        OnControllerDied?.Invoke(this);
    }

    protected void PerformAction(ActionInstance actionInstance)
    {
        ActionSequence.Kill();
        DOTween.Kill("ActionTween");
        CurrentActionInstance = actionInstance;
        OnActionInstancePerformed?.Invoke(actionInstance);
        State = actionInstance.preparing;
        actionInstance.Preparing();
        OnPreparingActionInstance?.Invoke(actionInstance.preparingTime, actionInstance);

        ActionSequence = DOTween.Sequence();

        ActionSequence.AppendInterval(actionInstance.preparingTime).AppendCallback(() =>
        {
            State = actionInstance.executing;
            actionInstance.Executing();
            OnExecutingActionInstance?.Invoke(actionInstance.executingTime, actionInstance);
        });
        ActionSequence.AppendInterval(actionInstance.executingTime).OnComplete(() =>
        {
            actionInstance.Complete();
            OnCompleteActionInstance?.Invoke(actionInstance);
            CurrentActionInstance = null;
        });
    }

    public void CancelAction()
    {
        if (CurrentActionInstance == null) return;

        ActionSequence.Kill();
        DOTween.Kill("ActionTween");
        CurrentActionInstance.Cancel();
        OnCancelActionInstance?.Invoke(CurrentActionInstance);
        CurrentActionInstance = null;
    }

    private void OnEnable()
    {
        if (entity != null)
        {
            entity.OnEntityDied += OnDied;
        }
    }

    private void OnDisable()
    {
        if (entity != null)
        {
            entity.OnEntityDied -= OnDied;
        }
    }
}