using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState()
    {
        priorityLevel = 0;
    }

    public override void EnterState(EnemyStateManager context)
    {
        Debug.Log("[Enemy FSM] Entered Chase");
    }

    public override void UpdateState(EnemyStateManager context)
    {
        // If the player leaves detection range, return to Idle.
        if (!context.CanDetectPlayer())
        {
            context.SwitchState(context.idleState);
            return;
        }

        // If the player is in attack range, attempt to attack.
        if (context.IsPlayerInAttackRange())
        {
            context.StopMoving();
            context.FacePlayer();

            if (context.CanAttack())
            {
                context.SwitchState(context.attackState);
            }

            return;
        }

        // Otherwise continue chasing the player.
        context.MoveToPlayer();
    }

    public override void ExitState(EnemyStateManager context)
    {
        context.StopMoving();
    }
}