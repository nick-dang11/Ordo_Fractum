using UnityEngine;

public class EnemyDeathState : EnemyBaseState
{
    public override int PriorityLevel => 5;

    public override void EnterState(EnemyStateManager context)
    {
        Debug.Log("Enemy entered Dead State");

        if (context.Combat == null)
        {
            Debug.Log("EnemyCombat is missing");

            return;
        }

        context.Combat.StartDeath();
    }

    public override void UpdateState(EnemyStateManager context)
    {
        // no update necessary
    }

    public override void ExitState(EnemyStateManager context)
    {
        // no exit necessary
    }
}