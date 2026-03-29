using System;

public class State
{
    public Action OnEnterState, OnExitState, OnUpdate, OnFixedUpdate;
    public Action<bool> OnStateVariationChanged;
    public StateType Type { get; set; }

    public State()
    {
        Type = StateType.CONTINUOUS;
    }
    public State(StateType stateType)
    {
        Type = stateType;
    }
}

public enum StateType
{
    CONTINUOUS,
    PREPARING,
    EXECUTING
}
