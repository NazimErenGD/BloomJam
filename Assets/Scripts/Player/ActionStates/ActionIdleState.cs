using UnityEngine;

[CreateAssetMenu(fileName = "New Idle Action State", menuName = "Player/States/Action/Idle")]
public class ActionIdleState : ActionStateSO, IIdle
{
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Entering IdleAction State");
    }
    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exiting IdleAction State");
    }
}
