
using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "EnemyBlocksAction",
    story: "[Enemy] blocks an incoming attack",
    category: "Action",
    id: "f3c4f4d8-57bb-4b47-947e-20cb5f5d832a"
)]
public partial class EnemyBlocksAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Enemy;

    private EnemyCombat enemyCombat;
    private EnemyStateManager stateManager;
    private EnemyDefenseDecision defenseDecision;

    private float blockEndTime;
    private bool blockStarted;


    protected override Status OnStart()
    {
        blockStarted = false;

        if (Enemy == null || Enemy.Value == null) return Status.Failure;

        GameObject enemy = Enemy.Value;

        enemyCombat = enemy.GetComponent<EnemyCombat>();
        stateManager = enemy.GetComponent<EnemyStateManager>();
        defenseDecision = enemy.GetComponent<EnemyDefenseDecision>();

        if (enemyCombat == null || stateManager == null || defenseDecision == null)
        {
            Debug.LogWarning("[EnemyBlocksAction] Required component missing.");
            return Status.Failure;
        }

        if (!defenseDecision.ShouldBlock) return Status.Failure;
        if (!enemyCombat.CanBlock) return Status.Failure;
        if (stateManager.CurrentState != stateManager.NeutralState) return Status.Failure;

        float duration = defenseDecision.BlockDuration;

        if (!defenseDecision.TryConsumeBlockDecision()) return Status.Failure;

        if (!stateManager.SwitchState(stateManager.BlockState))
        {
            Debug.LogWarning("[EnemyBlocksAction] Failed to enter BlockState.");
            return Status.Failure;
        }

        if (!enemyCombat.IsBlocking)
        {
            Debug.LogWarning("[EnemyBlocksAction] BlockState entered, but StartBlock failed.");

            if (stateManager.CurrentState == stateManager.BlockState)
            {
                stateManager.SwitchState(stateManager.NeutralState);
            }

            return Status.Failure;
        }

        blockEndTime = Time.time + duration;
        blockStarted = true;

        Debug.Log($"[EnemyBlocksAction] Block started for {duration:F2}s.", enemy);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!blockStarted) return Status.Failure;
        if (enemyCombat == null || stateManager == null) return Status.Failure;

        if (enemyCombat.IsDead || enemyCombat.IsBeingExecuted || enemyCombat.IsStunned)
        {
            return Status.Failure;
        }

        if (stateManager.CurrentState != stateManager.BlockState || !enemyCombat.IsBlocking)
        {
            return Status.Failure;
        }

        if (Time.time < blockEndTime) return Status.Running;

        if (!stateManager.SwitchState(stateManager.NeutralState, true))
        {
            Debug.LogWarning("[EnemyBlocksAction] Failed to finish blocking.");
            return Status.Failure;
        }

        Debug.Log("[EnemyBlocksAction] Block completed.", enemyCombat);

        return Status.Success;
    }



    protected override void OnEnd()
    {
        if (blockStarted &&
            stateManager != null &&
            stateManager.CurrentState == stateManager.BlockState)
        {
            stateManager.SwitchState(stateManager.NeutralState, true);
        }

        blockStarted = false;
    }

}
