public abstract class BaseState
{
    public int prioritylevel;

    public abstract void EnterState(StateManager context);
    public abstract void UpdateState(StateManager context);
}
