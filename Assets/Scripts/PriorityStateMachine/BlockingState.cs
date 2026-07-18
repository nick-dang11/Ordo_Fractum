using UnityEngine;

public class BlockingState : BaseState
{
    public BlockingState()
    {
        prioritylevel = 2;
    }

    public override void EnterState(StateManager context)
    {
        Debug.Log("Entered Blocking State");
    }

    public override void UpdateState(StateManager context)
    {
        //needs to be implemented
    }
}
