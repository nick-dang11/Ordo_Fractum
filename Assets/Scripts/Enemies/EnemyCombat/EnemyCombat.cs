using UnityEngine;

public class EnemyCombat : MonoBehaviour, IAttackDamageSource, IAttackLifecycle
{
    [SerializeField] public WeaponHitbox weaponHitbox;
    [SerializeField] private Animator animate;
    [SerializeField] private EnemyCombatDetection enemyCombatDetection;

    [SerializeField] private float enemyDamage = 1f;
    [SerializeField] private float enemyAttackCooldown = 1.5f;

    private float nextAttackTime;

    public bool wasAttacking = false;
    public bool isAttacking = false;

    public bool IsAttacking => isAttacking;

    public bool CanAttack =>
        !isAttacking && Time.time >= nextAttackTime;

    private void Update()
    {
        if (enemyCombatDetection == null)
            return;

        if (enemyCombatDetection.isPlayerInAttackRange && CanAttack)
        {
            TryAttack();
        }
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

        FacePlayerTarget();

        if (animate != null)
        {
            animate.SetTrigger("Attack");
        }

        return true;
    }

    public void StartAttack()
    {
        // Kept for Animator Events / state machine compatibility.
        // Actual attack is currently started through TryAttack().
    }

    public void EndAttack()
    {
        isAttacking = false;

        if (animate != null)
        {
            animate.ResetTrigger("Attack");
        }

        Debug.Log(
            $"[EnemyCombat] EndAttack called on {name}. " +
            $"isAttacking={isAttacking}, Time={Time.time}"
        );
    }

    public void OnAttackEnded()
    {
        EndAttack();
    }

    public void EnableWeaponHitbox()
    {
        if (weaponHitbox != null)
        {
            weaponHitbox.EnableHitbox(enemyDamage);
        }
    }

    public void DisableWeaponHitbox()
    {
        if (weaponHitbox != null)
        {
            weaponHitbox.DisableHitbox();
        }
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
            $"AttackData={(attackData != null ? attackData.name : "NULL")}, " +
            $"Damage returned={enemyDamage}"
        );

        return enemyDamage;
    }

    public void FacePlayerTarget()
    {
        if (enemyCombatDetection == null ||
            enemyCombatDetection.detectedPlayer == null)
        {
            return;
        }

        Vector3 direction =
            enemyCombatDetection.detectedPlayer.position
            - transform.position;

        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }

    [ContextMenu("DEBUG Trigger Attack")]
    private void DebugTriggerAttack()
    {
        TryAttack();
    }
}