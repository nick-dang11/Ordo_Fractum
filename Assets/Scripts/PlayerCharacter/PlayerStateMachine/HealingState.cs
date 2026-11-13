
using UnityEngine;

public class HealingState : BaseState
{
    public HealingState()
    {
        prioritylevel = 2;
    }

    public override void EnterState(StateManager context)
    {
        Debug.Log("[HealingState] Entered healing state.");

        if (context.healing == null)
        {
            Debug.LogError(
                "[HealingState] HealingAnimation reference missing."
            );
            return;
        }

        if (!context.healing.TryStartHealing())
        {
            Debug.Log("[HealingState] Healing could not start.");
        }
    }

    public override void UpdateState(StateManager context)
    {
        if (context.healing == null || !context.healing.IsHealing)
        {
            context.SwitchState(context.idleState, true);
        }
    }

    public override void ExitState(StateManager context)
    {
        if (context.healing != null && context.healing.IsHealing)
        {
            context.healing.InterruptHealing();
        }

        Debug.Log("[HealingState] Exited healing state.");
    }
}
