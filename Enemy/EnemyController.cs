using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : Controller
{
    public NavMeshAgent navMeshAgent;
    public List<State> states;
    public State inactiveState, idlingState, pursuingState, bracingState, stunnedState, deadState;

    private float moveTimer = 0f;
    private Vector3 currentDestination;

    protected override void Awake()
    {
        base.Awake();
        SetStates();

        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public override void Start()
    {
        base.Start();
    }

    private void SetStates()
    {
        states = new List<State>
        {
            (inactiveState = new State("Inactive")),
            (idlingState = new State("Idling")),
            (pursuingState = new State("Pursuing")),
            (bracingState = new State("Bracing")),
            (stunnedState = new State("Stunned")),
            (deadState = new State("Dead"))
        };
    }

    public override void SetOnEnterState()
    {
        idlingState.OnEnterState += () =>
        {
            navMeshAgent.enabled = true;
        };
        pursuingState.OnEnterState += () =>
        {
            navMeshAgent.speed = 5;
        };

        deadState.OnEnterState += () =>
        {
            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = false;
            }
            Target = null;
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
               // Check if the current destination is not set or if the move timer has reached 5 seconds
               if (currentDestination == Vector3.zero || moveTimer >= 5f)
               {
                   // Define a random range for the enemy to move within
                   float randomRange = 10f; // Adjust this value as needed

                   // Calculate a random position within the range
                   currentDestination = transform.position + new Vector3(
                       UnityEngine.Random.Range(-randomRange, randomRange),
                       0,
                       UnityEngine.Random.Range(-randomRange, randomRange)
                   );

                   // Reset the move timer
                   moveTimer = 0f;
               }

               // Set the destination to the current destination
               navMeshAgent.SetDestination(currentDestination);

               // Increment the move timer
               moveTimer += Time.fixedDeltaTime;
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

    public override void SetActions()
    {
        ActionInstances = new List<ActionInstance>
        {
            new AttackWithRange(this, 1, 0.5f)
        };
    }

    public void Attack()
    {
        PerformAction(ActionInstances[0]);
    }

    private void FixedUpdate()
    {
        State.OnFixedUpdate?.Invoke();
    }


    public override void OnDied()
    {
        base.OnDied();

        State = deadState;
    }
}
