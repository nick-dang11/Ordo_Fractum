using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "EnemyAttacksAction", story: "[Enemy] attacks target once.", category: "Action", id: "4e37b46991a33cb2c78a68af21c99568")]
public partial class EnemyAttacksAction : Action
{
    [SerializeReference]
    public BlackboardVariable<GameObject> Enemy;
    private EnemyCombat enemyCombat;


    protected override Status OnStart()
    {
        if(Enemy == null || Enemy.Value == null)
        {
            Debug.LogWarning("Enemy reference is missing in EnemyAttacksAction");
            return Status.Failure;
        }

        if(enemyCombat == null || enemyCombat.gameObject != Enemy.Value)
        {
            enemyCombat = Enemy.Value.GetComponent<EnemyCombat>();
        }

        if(enemyCombat == null)
        {
            Debug.LogWarning("EnemyCombat reference is missing in EnemyAttacksAction");
            return Status.Failure;
        }

        if (!enemyCombat.TryAttack())
        {
            return Status.Failure;
        }

        EnemyMovement movement = Enemy.Value.GetComponent<EnemyMovement>();

        if ( movement != null) movement.Stop();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if(enemyCombat == null || 
            enemyCombat.IsDead || 
            enemyCombat.IsBeingExecuted || 
            enemyCombat.IsStunned || 
            enemyCombat.CurrentAttackInvalidated)
        {
            return Status.Failure;
        }

        return enemyCombat.IsAttacking ? Status.Running : Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

