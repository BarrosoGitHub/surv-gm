using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : Controller
{
    public NavMeshAgent navMeshAgent;
    public List<State> states;
    public State inactiveState, idlingState, pursuingState, bracingState, stunnedState, deadState;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public override void Start()
    {
        base.Start();

        SetStates();
        SetOnEnterState();
        SetOnExitState();
        SetFixedUpdateState();

        Initialize();
    }
    private void SetOnEnterState()
    {
        pursuingState.OnEnterState += () =>
        {
            navMeshAgent.speed = 5;
        };

        deadState.OnEnterState += () =>
        {
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = true;
            }
        };

        
    }
    
    private void SetStates()
    {
        states = new List<State>
        {
            (inactiveState = new State()),
            (idlingState = new State()),
            (pursuingState = new State()),
            (bracingState = new State()),
            (stunnedState = new State()),
            (deadState = new State())
        };
    }

    private void SetOnExitState()
    {

    }

    private void SetFixedUpdateState()
    {
        idlingState.OnFixedUpdate += () =>
        {
            navMeshAgent.SetDestination(transform.position);
        };

        pursuingState.OnFixedUpdate += () =>
        {
            if (Target == null || navMeshAgent == null || !navMeshAgent.enabled)
            {
                return;
            }

            navMeshAgent.SetDestination(Target.transform.position);
        };
    }

    private void FixedUpdate()
    {
        State.OnFixedUpdate?.Invoke();
    }

    public void Initialize()
    {
     

        var playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            Target = playerObject.GetComponent<PlayerController>();
        }

        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = false;
        }

        State = pursuingState;
    }

    public void ApplyDamage(float amount)
    {
        entity.CurrentHealth -= amount;
    }

    private void OnDead()
    {
        State = deadState;
        col.enabled = false;
        //Some death logic here, like playing an animation, dropping loot, etc. delay...
        OnDied?.Invoke(this);
    }

    public bool IsDead => State == deadState;
}
