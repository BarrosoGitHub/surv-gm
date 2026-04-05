using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : Controller
{
    public NavMeshAgent navMeshAgent;
    public List<State> states;
    public State inactiveState, idlingState, pursuingState, bracingState, stunnedState, deadState;

    protected override void Awake()
    {
        base.Awake();

        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public override void Start()
    {
        SetStates();
        base.Start();

        Initialize();
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

    public override void SetOnEnterState()
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

    public override void SetOnExitState()
    {

    }

    public override void SetOnUpdateState()
    {

    }

    public override void SetOnFixedUpdateState()
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
}
