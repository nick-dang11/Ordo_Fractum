using UnityEngine;

public class EnemyExecutedState : EnemyBaseState
{
    public override int PriorityLevel => 4;

    public override void EnterState(EnemyStateManager context)
    {
        Debug.Log("Enemy entered executed state.");

        if(context.Combat == null)
        {
            Debug.LogWarning($"EnemyCombat is missing.");
            return;
        }

        context.Combat.StartExecution();
    }

    public override void UpdateState(EnemyStateManager context)
    {
        // wait til execution completes
    }

    public override void ExitState(EnemyStateManager context)
    {
    }
}
