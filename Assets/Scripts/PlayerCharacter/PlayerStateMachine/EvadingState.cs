using UnityEngine;

public class EvadingState : BaseState
{
    public EvadingState()
    {
        prioritylevel = 3;
    }

    public override void EnterState(StateManager context)
    {
        Debug.Log("Entered Evading State");

        Vector3 worldDir = context.movement.GetCameraRelativeDirection(context.inputManager.EvadeDirection);
        Vector3 localDir = context.movement.transform.InverseTransformDirection(worldDir);
        Vector2 playerLocalDirection = new Vector2(localDir.x, localDir.z).normalized;

        context.inputManager.ConsumeEvadeRequest();
        context.playerEvade.StartEvade(playerLocalDirection);
        
    }

    public override void UpdateState(StateManager context)
    {
        context.playerEvade.UpdateEvade();
        if (context.playerEvade.IsEvadeComplete)
        {
            context.SwitchState(context.idleState, true);
        }
    }

    public override void ExitState(StateManager context)
    {
        context.playerEvade.CleanupEvade();
    }
}
