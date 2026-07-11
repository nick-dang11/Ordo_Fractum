using UnityEngine;

public class AttackingState : BaseState
{
    public AttackingState()
    {
        prioritylevel = 1;
    }

    public override void EnterState(StateManager context)
    {
        Debug.Log("Entered atttacking State");
    }

    public override void UpdateState(StateManager context)
    {
        //needs to be implemented
    }
}
