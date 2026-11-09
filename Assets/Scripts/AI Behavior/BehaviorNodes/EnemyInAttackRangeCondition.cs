using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[Condition(name: "Enemy In Combat Range", story: "[Enemy] is in combat range.", category: "Enemy AI V2", id: "01f97167e332cd8dc412e12ded98dee7")]
public partial class EnemyInCombatRangeCondition : Condition
{
    [SerializeReference]
    public BlackboardVariable<GameObject> Enemy;

    public override bool IsTrue()
    {
        if (Enemy == null || Enemy.Value == null) return false;

        EnemyCombatDetection detection = Enemy.Value.GetComponent<EnemyCombatDetection>();

        if (detection == null) return false;

        return detection.isPlayerInAttackRange;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
