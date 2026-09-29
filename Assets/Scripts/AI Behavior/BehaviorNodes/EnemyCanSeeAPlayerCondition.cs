using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Enemy Can see a player", story: "[Enemy] can see Player", category: "Enemy AI V2", id: "254805ffd6f7ccb7f5d40510d6b68678")]
public partial class EnemyCanSeeAPlayerCondition : Condition
{
    [SerializeReference]
    public BlackboardVariable<GameObject> Enemy;

    public override bool IsTrue()
    {
        //Find the existing enemydetection component on the enemy gameobject and store its refrence in detection
        EnemyDetection detection = Enemy.Value.GetComponent<EnemyDetection>();

        return detection.CanSeePlayer(); //return true of false it the enemy can see the player
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
