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
        if (Enemy.Value == null)
        {
            return Status.Failure;
        }

        movement = Enemy.Value.GetComponent<EnemyMovement>();

        if (movement == null)
        {
            return Status.Failure;
        }

        movement.Chase();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (movement == null)
        {
            return Status.Failure;
        }

        movement.Chase();

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}