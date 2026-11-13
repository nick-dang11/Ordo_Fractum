
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
        // Check whether the player requested healing.
        if (context.inputManager.self_heal)
        {
            // Consume the healing input.
            context.inputManager.self_heal = false;

            if (context.healing != null &&
                context.healing.CanStartHealing())
            {
                context.SwitchState(context.healingState);
            }

            return;
        }

        if (context.inputManager.IsEvadeRequested &&
            context.movement.IsGrounded)
        {
            context.SwitchState(context.EvadingState);
            return;
        }

        if (context.combat.BlockPressedThisFrame)
        {
            context.SwitchState(context.blockState);
        }
        else if (context.combat.input.attack ||
                 context.combat.input.heavyAttack)
        {
            context.SwitchState(context.attackingState);
        }
    }
}
