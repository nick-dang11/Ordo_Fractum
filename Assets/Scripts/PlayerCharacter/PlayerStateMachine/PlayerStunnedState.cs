
using UnityEngine;

public class PlayerStunnedState : BaseState
{
    private const float stunDuration = 3f;
    private float stunEndTime;

    private Animator animator;

    public PlayerStunnedState()
    {
        prioritylevel = 3;
    }

    public override void EnterState(StateManager context)
    {
        stunEndTime = Time.time + stunDuration;

        if (animator == null)
            animator = context.GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.SetBool("IsStunned", true);
        }

        Debug.Log("Player entered StunnedState.");
    }

    public override void UpdateState(StateManager context)
    {
        if (Time.time < stunEndTime)
            return;

        context.GetComponent<PlayerPosture>()?
            .RecoverFromPostureBreak(0f); // player is already penalized 2hp 

        context.SwitchState(context.idleState, true);
    }

    public override void ExitState(StateManager context)
    {
        if (animator != null)
        {
            animator.SetBool("IsStunned", false);
        }

        Debug.Log("Player exited StunnedState.");
    }
}
