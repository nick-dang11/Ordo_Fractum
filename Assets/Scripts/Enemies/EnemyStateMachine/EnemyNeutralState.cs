using UnityEngine;

public class EnemyNeutralState : EnemyBaseState
{
    public override int PriorityLevel => 0;

    public override void EnterState(EnemyStateManager context)
    {
    }

    public override void UpdateState(EnemyStateManager context)
    {
    }

    public override void ExitState(EnemyStateManager context)
    {
    }
}
