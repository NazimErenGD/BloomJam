using UnityEngine;
using System.Collections.Generic;

public abstract class PlayerStateSO : ScriptableObject
{
    [Header("Transitions")]
    [SerializeField] private List<StateTransition> _transitions;

    // The fast lookup map for Runtime
    protected Dictionary<InputSignalSO, PlayerStateSO> _transitionMap;
    protected Player _player;
    protected StateMachineBase _stateMachine;

    public virtual void Initialize(Player player, StateMachineBase machine)
    {
        _player = player;
        _stateMachine = machine;

        // Convert List -> Dictionary
        _transitionMap = new Dictionary<InputSignalSO, PlayerStateSO>();
        foreach (var transition in _transitions)
        {
            if (!_transitionMap.ContainsKey(transition.InputSignal))
            {
                _transitionMap.Add(transition.InputSignal, transition.TargetState);
            }
        }
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void LogicUpdate() { }
    
    // The Guard Clause: "Can I enter this state?"
    public virtual bool CanEnter() { return true; }

    // The Lookup: "Do I care about this input?"
    public PlayerStateSO GetTransitionFor(InputSignalSO input)
    {
        if (_transitionMap.ContainsKey(input))
            return _transitionMap[input];
        return null;
    }
}