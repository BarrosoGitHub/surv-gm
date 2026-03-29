using System;
using UnityEngine;

public class Controller : MonoBehaviour
{
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

    public virtual void Start()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        SetOnEnterState();
        SetOnExitState();
        SetOnUpdateState();
        SetOnFixedUpdateState();

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

    private virtual void SetOnEnterState()
    {
        
    }

    public virtual void SetOnExitState()
    {

    }

    private virtual void SetOnUpdateState()
    {

    }

    public virtual void SetOnFixedUpdateState()
    {

    }
}