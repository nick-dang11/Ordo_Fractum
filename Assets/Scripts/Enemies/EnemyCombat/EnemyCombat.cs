using UnityEngine;

public class EnemyCombat : MonoBehaviour, IAttackDamageSource
{
    [SerializeField] public WeaponHitbox weaponHitbox;
    [SerializeField] private Animator animate;
    [SerializeField] private float enemyDamage = 1f;

    public bool wasAttacking = false;
    public bool isAttacking = false;
    public bool IsAttacking => isAttacking;

    public void StartAttack()
    {
        isAttacking = true;

        Debug.Log(
            $"[EnemyCombat] StartAttack called on {name}. " +
            $"isAttacking = {isAttacking}"
        );
    }

    public void EndAttack()
    {
        isAttacking = false;

        Debug.Log(
            $"[EnemyCombat] EndAttack called on {name}. " +
            $"isAttacking = {isAttacking}, Time = {Time.time}"
        );

        animate.ResetTrigger("Attack");
        //Debug.Log($"[Combat] EndAttack fired at {Time.time}");
    }


    public void EnableWeaponHitbox()
    {
        Debug.Log(
           $"[EnemyCombat] Legacy EnableWeaponHitbox called. " +
           $"Damage = {enemyDamage}"
        );
        weaponHitbox.EnableHitbox(enemyDamage);
    }

    public void DisableWeaponHitbox()
    {
        Debug.Log("[EnemyCombat] Legacy DisableWeaponHitbox called.");
        weaponHitbox.DisableHitbox();
    }

    public void SetEnemyDamage(float damage)
    {
        enemyDamage = damage;
        Debug.Log(
            $"[EnemyCombat] Enemy damage changed to {enemyDamage}."
        );
    }

    public float GetAttackDamage(AttackData attackData)
    {
        Debug.Log(
            $"[EnemyCombat] GetAttackDamage called. " +
            $"AttackData = {(attackData != null ? attackData.name : "NULL")}, " +
            $"Damage returned = {enemyDamage}"
        );

        return enemyDamage;
    }
}