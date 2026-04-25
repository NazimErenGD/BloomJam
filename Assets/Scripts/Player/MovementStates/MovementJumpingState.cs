using UnityEngine;

[CreateAssetMenu(fileName = "New Jumping State", menuName = "Player/States/Movement/Jumping")]
public class MovementJumpingState : MovementStateSO
{
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Entering Jumping State");
    }
    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exiting Jumping State");
    }
}
