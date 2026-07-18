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
        context.combat.wasAttacking = false;
    }

    public override void UpdateState(StateManager context)
    {
        if (context.combat.input.block)
        {
            context.SwitchState(context.blockState);
        }

        context.combat.LightAndHeavy();

        if(!context.combat.IsAttacking && !context.combat.input.attack)
        {
            context.SwitchState(context.idleState, true);
        }
    }

    public override void ExitState(StateManager context)
    {
        context.combat.EndAttack();
    }
}
