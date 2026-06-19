using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour
{
    public float enemyAttackCooldown = 2.0f;
    public float enemyAttackRange = 3.0f;
    private bool canEnemyAttack = true;

    [SerializeField] public Transform player;
    [SerializeField] public Transform enemy;
    [SerializeField] public GameObject weapon;

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(enemy.position, player.position);

        //if (distanceToPlayer <= enemyAttackRange) => StartCoroutine(AttackRoutine)
     //   if (Player's collider overlaps attackRadius && enemyattackCooldown = 0){ 
     //radius will be more than weapon length so it can trigger while player is out of attack range


     //       Weapon.SetActive(true), Weapon.color Red, 0.3s coroutine, Weapon.color Grey, Weapon.SetActive(false)
     //       TimerStarts


     //       if{
     //       player collider overlaps enemyAttackRange:
     //               player takes damage
     //}
    }
}

