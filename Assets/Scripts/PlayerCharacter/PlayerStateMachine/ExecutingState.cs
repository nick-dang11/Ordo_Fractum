using UnityEngine;

public class ExecutingState : BaseState
{
    private const float executionTestDuration = 5.5f;
    private float executionEndTime;

    public ExecutingState()
    {
        prioritylevel = 4;
    }

    public override void EnterState(StateManager context)
    {
        Debug.Log("Entered executing state.");

        executionEndTime = Time.time + executionTestDuration;
    }

    public override void UpdateState(StateManager context)
    {
        if (Time.time < executionEndTime)
            return;

        if (context.playerExecution == null)
        {
            Debug.LogError("[ExecutingState] PlayerExecution reference is missing.");

            context.SwitchState(context.idleState, true);
            return;
        }

        Debug.Log("[ExecutingState] Temporary execution completion fired.");

        context.playerExecution.FinishExecution();
    }

    public override void ExitState(StateManager context)
    {
        Debug.Log("Exited executing state.");
    }
}