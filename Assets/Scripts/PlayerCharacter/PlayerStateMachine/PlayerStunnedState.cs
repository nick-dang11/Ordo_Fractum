using UnityEngine;

public class PlayerStunnedState : BaseState
{
    private const float stunDuration = 3f;
    private float stunEndTime;

    public PlayerStunnedState()
    {
        prioritylevel = 3;
    }

    public override void EnterState(StateManager context)
    {
        stunEndTime = Time.time + stunDuration;

        Debug.Log("Player entered StunnedState.");
    }

    public override void UpdateState(StateManager context)
    {
        if (Time.time < stunEndTime)
            return;

        context.GetComponent<PlayerPosture>()?
            .RecoverFromPostureBreak(0.8f);

        context.SwitchState(context.idleState, true);
    }

    public override void ExitState(StateManager context)
    {
        Debug.Log("Player exited StunnedState.");
    }

}