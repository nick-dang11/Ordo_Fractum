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
        context.combat.HandleBlock();

        if (!context.combat.input.block)
        {
            context.SwitchState(context.idleState, true);
        }
    }

    public override void ExitState(StateManager context)
    {
        context.combat.ForceStopBlocking();
    }
}
