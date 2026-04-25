using UnityEngine;
using TMPro;
public class PlayerActionStateMachine : StateMachineBase
{
    [SerializeField] private TextMeshProUGUI _stateText;
    public override void Initialize(Player player)
    {
        base.Initialize(player);

        // Action-Specific Initialization
        foreach (var state in _stateInstances.Values)
        {
            if (state is ActionStateSO actionState)
                actionState.InitAction(this);

            state.Initialize(player, this);
        }

        // Set Default State
        if (_possibleStates.Length > 0)
            ChangeState(_stateInstances[_possibleStates[0]]);

        _stateText.text = "Current Action State: " + _stateInstances[_possibleStates[0]].name.ToString();
    }
    protected override void ChangeState(PlayerStateSO newState)
    {
        base.ChangeState(newState);
        _stateText.text = "Current Action State: " + CurrentState.name.ToString();
    }
    
}