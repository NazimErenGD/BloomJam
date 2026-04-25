using UnityEngine;

public abstract class MovementStateSO : PlayerStateSO
{
    protected PlayerMovementStateMachine _movementMachine;
    public Rigidbody Rb;

    public void InitMovement(PlayerMovementStateMachine machine)
    {
        _movementMachine = machine;
        Rb = _movementMachine.Rb;
    }

    // Specific to Movement
    public virtual void PhysicsUpdate() { }
}