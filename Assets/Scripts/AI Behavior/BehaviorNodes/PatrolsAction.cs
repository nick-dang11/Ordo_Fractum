using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
//using Unity.VisualScripting;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrols", story: "[Enemy] Patrols", category: "Enemy Action", id: "98e8807654486d8b1f46367a320362a6")]
public partial class PatrolsAction : Action
{
    [SerializeReference] 
    public BlackboardVariable<GameObject> Enemy;

    private EnemyMovement movement;

    protected override Status OnStart()
    {
        movement = Enemy.Value.GetComponent<EnemyMovement>();

        if(movement == null)
        {
            return Status.Failure;
        }

        movement.Patrol();

        return Status.Success;
    }

    protected override Status OnUpdate()
    {
    

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

