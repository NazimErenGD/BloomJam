using UnityEngine;

public abstract class ActionStateSO : PlayerStateSO
{
    protected PlayerActionStateMachine _actionMachine;

    public void InitAction(PlayerActionStateMachine machine)
    {
        _actionMachine = machine;
    }

    // Action Logic usually doesn't need Physics, but you can add it if needed
}