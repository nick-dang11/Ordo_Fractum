
using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[Condition(
    name: "EnemyShouldBlock",
    story: "[Enemy] should block",
    category: "Conditions",
    id: "e87b4a65-4d37-4f6e-8cb1-08eae314b962"
)]
public partial class EnemyShouldBlockCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Enemy;

    private EnemyDefenseDecision defenseDecision;

    public override bool IsTrue()
    {
        if (Enemy == null || Enemy.Value == null) return false;

        if (defenseDecision == null || defenseDecision.gameObject != Enemy.Value)
        {
            defenseDecision = Enemy.Value.GetComponent<EnemyDefenseDecision>();
        }

        if (defenseDecision == null) return false;

        return defenseDecision.ShouldBlock;
    }
}
