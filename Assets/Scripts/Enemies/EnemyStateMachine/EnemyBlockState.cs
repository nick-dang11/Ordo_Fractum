using UnityEngine;

public class EnemyBlockState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager context)
    {
        Debug.Log("Enemy entered Block State");

        if (context.Combat == null)
        {
            Debug.Log("EnemyCombat is missing");

            return;
        }

        context.Combat.StartBlock();
    }

    public override void UpdateState(EnemyStateManager context)
    {
        if (context.Combat == null) return;

        context.Combat.FacePlayerTarget();
    }

    public override void ExitState(EnemyStateManager context)
    {
        if(context.Combat == null) return;
        context.Combat.EndBlock();
    }
}
