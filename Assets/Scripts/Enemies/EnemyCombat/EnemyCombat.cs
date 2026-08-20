using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour
{
    public float enemyAttackCooldown = 2.0f;
    public float enemyAttackRange = 3.0f;
    private bool canEnemyAttack = true;
    private bool isStunned = false;
    private bool isBlocking = false;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] public WeaponHitbox weaponHitbox;

    [Header("Damage")]
    [SerializeField] public float enemyDamage = 1f;

    void Update()
    {
        if (isStunned) return;

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

    public void ForceSturn()
    {
        if (isStunned) return;
        Debug.Log("Enemy stunned from posture break.");
        isStunned = true;
        canEnemyAttack = false;
        StartCoroutine(StunRecoveryRoutine());
    }

    private IEnumerator StunRecoveryRoutine()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("Enemy recovered from stun.");
        isStunned = false;
        canEnemyAttack = true;
    }

    public bool IsBlocking()
    {
        return isBlocking;
    }

    public void SetBlocking(bool blocking) //If we want to add a blocking feature to the enemy AI - Jacob
    {
        isBlocking = blocking;
    }
}

