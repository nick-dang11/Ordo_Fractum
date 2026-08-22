using UnityEngine;

public class EnemyCombat : MonoBehaviour
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
    }

    public void EndAttack()
    {
        isAttacking = false;
        animate.ResetTrigger("Attack");
        Debug.Log($"[Combat] EndAttack fired at {Time.time}");
    }


    public void EnableWeaponHitbox()
    {
        weaponHitbox.EnableHitbox(enemyDamage);
    }

    public void DisableWeaponHitbox()
    {
        weaponHitbox.DisableHitbox();
    }

    public void SetEnemyDamage(float damage)
    {
        enemyDamage = damage;
    }
}