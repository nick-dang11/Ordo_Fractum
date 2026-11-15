
using UnityEngine;

public class EnemyAttackingState : EnemyBaseState
{
    public override int PriorityLevel => 1;

    public override void EnterState(EnemyStateManager context)
    {
        Debug.Log("Enemy entered Attacking State.");
    }

    public override void UpdateState(EnemyStateManager context)
    {
        if (context.Combat == null)
        {
            context.SwitchState(context.NeutralState, true);
            return;
        }

        if (!context.Combat.IsAttacking)
        {
            context.SwitchState(context.NeutralState, true);
        }
    }

    public override void ExitState(EnemyStateManager context)
    {
        Debug.Log("Enemy exited Attacking State.");
    }
}
