using UnityEngine;

public class IdleState : BaseState
{
    public IdleState()
    {
        prioritylevel = 0;
    }

    public override void EnterState(StateManager context)
    {
        Debug.Log("Entered Idle State");
    }

    public override void UpdateState(StateManager context)
    {
        //needs to be implemented
    }
}
