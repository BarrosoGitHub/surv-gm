using UnityEngine;

public class AttackWithRange : ActionInstance
{
    private EnemyController controller;
    public AttackWithRange(EnemyController controller, float preparingTime, float executingTime)
    {
        this.controller = controller;
        this.preparingTime = preparingTime;
        this.executingTime = executingTime;

        InitializeStates();
    }

    public override void Preparing()
    {
        // Intentionally left blank: ranged attack preparation feedback/telegraphing
        // is handled by external systems, so this action does not need additional
        // logic during the preparing phase.
    }

    public override void Executing()
    {
        // Intentionally left blank: this action does not perform per-frame execution
        // logic here because the ranged attack behavior is handled elsewhere in the
        // attack lifecycle.
    }

    public override void Complete()
    {
        controller.State = controller.pursuingState;
    }

    public override void Cancel()
    {
        throw new System.NotImplementedException();
    }
}