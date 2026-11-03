using UnityEngine;

public class AttackingState : BaseState
{
    private bool sawAttackAnimation;
    private float enteredAt;
    public AttackingState()
    {
        prioritylevel = 1;
    }

    public override void EnterState(StateManager context)
    {
        Debug.Log("Entered atttacking State");
        context.combat.wasAttacking = false;
        enteredAt = Time.time;

        sawAttackAnimation = context.combat.IsAttackAnimationPlaying();
    }

    public override void UpdateState(StateManager context)
    {
        if (context.inputManager.IsEvadeRequested && context.movement.IsGrounded)
        {
            context.SwitchState(context.EvadingState);
            return;
        }

        if (context.combat.BlockPressedThisFrame)
        {
            context.SwitchState(context.blockState);

            if (context.currentState == context.blockState)
            {
                return;
            }
        }

        context.combat.LightAndHeavy();

        bool attackAnimationPlaying = context.combat.IsAttackAnimationPlaying();
        if (attackAnimationPlaying)
        {
            sawAttackAnimation = true;
        }
        bool attackInputReleased = !context.combat.input.attack && !context.combat.input.heavyAttack;

        bool attackFinished = !context.combat.IsAttacking && !attackAnimationPlaying;

        bool canReturnToIdle = sawAttackAnimation || Time.time - enteredAt > 0.4f;

        if (attackFinished && attackInputReleased && canReturnToIdle)
        {
            context.SwitchState(context.idleState, true);
            return;
        }
    }

    public override void ExitState(StateManager context)
    {
        context.combat.EndAttack();
        sawAttackAnimation = false;
    }
}