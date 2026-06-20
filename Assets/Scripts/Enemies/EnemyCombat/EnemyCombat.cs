using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour
{
    public float enemyAttackCooldown = 2.0f;
    public float enemyAttackRange = 3.0f;
    private bool canEnemyAttack = true;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Hitbox weaponHitbox;

    [Header("Damage")]
    [SerializeField] private float enemyDamage = 10f;

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position); // distance from parent of component (should be enemy) to player position

        if (distanceToPlayer <= enemyAttackRange && canEnemyAttack)
        {
            StartCoroutine(AttackRoutine(0.3f));
        }
    }

    private IEnumerator AttackRoutine(float duration)
    {
        canEnemyAttack = false;

        weaponHitbox.EnableHitbox(enemyDamage);
        weaponHitbox.GetComponent<Renderer>().material.color = Color.red;

        yield return new WaitForSeconds(duration);
        weaponHitbox.DisableHitbox();
        weaponHitbox.GetComponent<Renderer>().material.color = Color.grey;

        yield return new WaitForSeconds(enemyAttackCooldown);
        canEnemyAttack = true;
    }
}

