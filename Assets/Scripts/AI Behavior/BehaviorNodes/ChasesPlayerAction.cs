using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Chases Player",
    story: "[Enemy] Chases the Player",
    category: "Enemy Action",
    id: "94003fbed547af91ecb1a3233bdfffaf"
)]
public partial class ChasesPlayerAction : Action
{
    [SerializeReference]
    public BlackboardVariable<GameObject> Enemy;

    private EnemyMovement movement;

    protected override Status OnStart()
    {
       // Debug.Log("Test: ChasesPlayerAction is running");

        if (Enemy == null || Enemy.Value == null)
        {
            Debug.LogWarning("Enemy is null or Enemy.Value is null in ChasesPlayerAction");
            return Status.Failure;
        }

        movement = Enemy.Value.GetComponent<EnemyMovement>();

        if (movement == null)
        {
            Debug.LogError("EnemyMovement component not found on Enemy in ChasesPlayerAction");
            return Status.Failure;
        }

       // Debug.Log("ChasesPlayerAction: Enemy is chasing the player.");
        movement.Chase();

        return Status.Success;
    }

    protected override Status OnUpdate()
    {
       

        return Status.Success; // Action is complete
    }

    protected override void OnEnd()
    {
    }
}