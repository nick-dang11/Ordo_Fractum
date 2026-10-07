public abstract class EnemyBaseState
{
    public int priorityLevel;

    public abstract void EnterState(EnemyStateManager context);

    public abstract void UpdateState(EnemyStateManager context);

    public virtual void ExitState(EnemyStateManager context)
    {
    }
}