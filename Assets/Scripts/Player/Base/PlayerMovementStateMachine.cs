using UnityEngine;
using TMPro;
public class PlayerMovementStateMachine : StateMachineBase
{
    [SerializeField] private TextMeshProUGUI _stateText;
    public Rigidbody Rb;
    public override void Initialize(Player player)
    {
        base.Initialize(player);
        Rb = player.Rb;
        // Movement-Specific Initialization
        foreach (var state in _stateInstances.Values)
        {
            if (state is MovementStateSO moveState)
                moveState.InitMovement(this);

            state.Initialize(player, this);
        }

        // Set Default State
        if (_possibleStates.Length > 0)
        {
            ChangeState(_stateInstances[_possibleStates[0]]);
        }

        _stateText.text = "Current Movement State: " + _stateInstances[_possibleStates[0]].name.ToString();
    }
    protected override void ChangeState(PlayerStateSO newState)
    {
        base.ChangeState(newState);
        _stateText.text = "Current Movement State: " + CurrentState.name.ToString();
    }
}