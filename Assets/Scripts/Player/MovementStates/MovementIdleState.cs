using UnityEngine;

[CreateAssetMenu(fileName = "New Idle State", menuName = "Player/States/Movement/Idle")]
public class MovementIdleState : MovementStateSO, IIdle
{
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Entering Idle State");
    }
    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exiting Idle State");
    }
}
