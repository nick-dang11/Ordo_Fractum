using UnityEngine;

public class EnemyStunnedState : EnemyBaseState
{
    public EnemyStunnedState()
    {
        priorityLevel = 2;
    }

    public override void EnterState(EnemyStateManager context)
    {
        Debug.Log("[Enemy FSM] Entered Stunned");

        context.StopMoving();

        if (context.combat != null)
        {
            //context.combat.DisableWeaponHitbox();
        }

        if (context.animator != null)
        {
            context.animator.SetTrigger("Stun");
        }
    }

    public override void UpdateState(EnemyStateManager context)
    {
        if (Time.time < context.stunEndTime)
            return;

        if (context.CanDetectPlayer())
        {
            context.SwitchState(
                context.chaseState,
                true);
        }
        else
        {
            context.SwitchState(
                context.idleState,
                true);
        }
    }

    public override void ExitState(EnemyStateManager context)
    {
        if (context.animator != null)
        {
            context.animator.ResetTrigger("Stun");
        }
    }
}