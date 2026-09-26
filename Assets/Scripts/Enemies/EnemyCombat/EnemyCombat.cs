using UnityEngine;

public class EnemyCombat : MonoBehaviour, IAttackDamageSource
{
    [SerializeField] public WeaponHitbox weaponHitbox;
    [SerializeField] private Animator animate;
    [SerializeField] private float enemyDamage = 1f;
    [SerializeField] private float enemyAttackCooldown = 1.5f;

    private float nextAttackTime;
    public bool wasAttacking = false;
    public bool isAttacking = false;
    public bool IsAttacking => isAttacking;
    public bool CanAttack => !isAttacking && Time.time >= nextAttackTime;

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

    public bool TryAttack()
    {
        Debug.Log(
            $"[EnemyCombat] TryAttack called. " +
            $"CanAttack={CanAttack}, " +
            $"isAttacking={isAttacking}, " +
            $"Time={Time.time}, " +
            $"nextAttackTime={nextAttackTime}"
        );

        if (!CanAttack)
        {
            Debug.Log("[EnemyCombat] TryAttack blocked.");
            return false;
        }

        isAttacking = true;
        nextAttackTime = Time.time + enemyAttackCooldown;

        Debug.Log(
            $"[EnemyCombat] Attack accepted. " +
            $"Next attack allowed at {nextAttackTime}"
        );

        animate.SetTrigger("Attack");
        return true;
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

    [ContextMenu("DEBUG Trigger Attack")]
    private void DebugTriggerAttack()
    {
        TryAttack();
    }

}