using System;

// A helper struct to allow us to view the Dictionary in the Inspector.
[Serializable]
public struct StateTransition
{
    public InputSignalSO InputSignal; // The Trigger
    public PlayerStateSO TargetState; // The Result
}