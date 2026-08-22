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
        if (context.combat.input.block)
        {
            context.SwitchState(context.blockState);
        }
        else if (context.combat.input.attack || context.combat.input.heavyAttack)
        {
            context.SwitchState(context.attackingState);
        }
    }
}
