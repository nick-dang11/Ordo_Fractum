public abstract class EnemyBaseState
{
    public abstract void EnterState(EnemyStateManager context);

    public abstract void UpdateState(EnemyStateManager context);

    public virtual void ExitState(EnemyStateManager context)
    {
        // virtual requires some degree of change, leaving empty is acceptable
    }
}
