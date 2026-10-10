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
       // Debug.Log("Test: PatrolsAction is running");

        if(Enemy == null || Enemy.Value == null)
        {
            Debug.LogWarning("Enemy is null or Enemy.Value is null in PatrolsAction");
            return Status.Failure;
        }

        movement = Enemy.Value.GetComponent<EnemyMovement>();

        if(movement == null)
        {
            Debug.LogError("EnemyMovement component not found on Enemy in PatrolsAction");
            return Status.Failure;
        }

       // Debug.Log("PatrolsAction: Enemy is patrolling.");
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

