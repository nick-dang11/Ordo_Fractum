using UnityEngine;

public class EnemyStunnedState : EnemyBaseState
{
    public override int PriorityLevel => 2;

    private const float stunDuration = 3f;
    private float stunEndTime;

    public override void EnterState(EnemyStateManager context)
    {
        Debug.Log("Enemy entered Stunned state.");

        stunEndTime = Time.time + stunDuration;

        if (context.Combat != null)
        {
            context.Combat.StartStun();
        }
    }

    public override void UpdateState(EnemyStateManager context)
    {
        if (Time.time < stunEndTime) return;

        context.RecoverFromStun();
        context.SwitchState(context.BlockState, true);
    }

    public override void ExitState(EnemyStateManager context)
    {
        if (context.Combat != null)
        {
            context.Combat.EndStun();
        }
    }
}
