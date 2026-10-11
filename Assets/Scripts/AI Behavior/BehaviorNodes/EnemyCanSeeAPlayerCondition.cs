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
//        Debug.Log("Test: IsTrue is running in EnemyCanSeeAPlayerCondition");
        
        if(Enemy == null || Enemy.Value == null)
        {
            Debug.LogWarning("Enemy is null or Enemy.Value is null in EnemyCanSeeAPlayerCondition");
            return false;
        }

        EnemyDetection detection = Enemy.Value.GetComponent<EnemyDetection>();

        if(detection == null)
        {
            Debug.LogWarning("EnemyDetection component not found on Enemy in EnemyCanSeeAPlayerCondition");
            return false;
        }

        bool shouldChase =  detection.ShouldChasePlayer(); // Call the ShouldChasePlayer method from EnemyDetection

        Debug.Log($"SHOULD CHASE PLAYER: {shouldChase}");
        return shouldChase;

    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
