using UnityEngine;
[CreateAssetMenu(fileName = "New Walking State", menuName = "Player/States/Movement/Walking")]
public class MovementWalkingState : MovementStateSO
{
    private int _speed = 5;
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Entering Walking State");
        _movementMachine.PlayerInputHandler.OnSprintEvent += OnSprint;
        _movementMachine.PlayerInputHandler.OnSprintCanceledEvent += OnSprintCanceled;
    }
    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exiting Walking State");
        _movementMachine.PlayerInputHandler.OnSprintEvent -= OnSprint;
        _movementMachine.PlayerInputHandler.OnSprintCanceledEvent -= OnSprintCanceled;
    }
    public override void LogicUpdate()
    {
        if (_movementMachine.PlayerInputHandler.MovementInput == Vector2.zero)
        {
            _movementMachine.ChangeToIdleState();
        }
    }
    public override void PhysicsUpdate()
    {
        // 1. Get Input (X = Horizontal, Y = Vertical)
        Vector2 input = _movementMachine.PlayerInputHandler.MovementInput;

        // 2. Calculate Direction in 3D Space
        // transform.right is "Global East/West", transform.forward is "Global North/South" relative to rotation
        Vector3 moveDirection = (Rb.transform.right * input.x) + (Rb.transform.forward * input.y);

        // 3. Flatten it (Optional but good for slopes) to ensure we don't walk into the ground
        moveDirection.y = 0;
        moveDirection.Normalize();

        // 4. Apply Velocity
        // We keep the Rigidbody's current Y velocity (Gravity) intact
        Rb.linearVelocity = new Vector3(moveDirection.x * _speed, Rb.linearVelocity.y, moveDirection.z * _speed);
    }

    private void OnSprint(InputSignalSO signal)
    {
        //Debug.Log("Sprinting");
        _speed = 10;
    }
    private void OnSprintCanceled(InputSignalSO signal)
    {
        //Debug.Log("Sprinting Canceled");
        _speed = 5;
    }
}
