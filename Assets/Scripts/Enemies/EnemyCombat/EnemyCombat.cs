using UnityEngine;

public class EnemyCombat : MonoBehaviour, IAttackDamageSource, IAttackLifecycle
{
    [SerializeField] private WeaponHitbox weaponHitbox;
    [SerializeField] private Animator animate;
    [SerializeField] private EnemyCombatDetection enemyCombatDetection;

    [SerializeField, Min(0f)] private float enemyDamage = 1f;
    [SerializeField, Min(0f)] private float enemyAttackCooldown = 1.5f;
    [SerializeField, Min(0f)] private float faceTargetSpeed = 8f;

    private float nextAttackTime;
    private bool isAttacking = false;
    public bool IsAttacking => isAttacking;
    public bool CanAttack => !isAttacking && Time.time >= nextAttackTime;

    private void Update()
    {
        if (enemyCombatDetection == null) return;

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

        if(enemyCombatDetection == null)
        {
            Debug.LogWarning($"EnemyCombatDetection is not assigned.");
            return false;
        }

        if (enemyCombatDetection.detectedPlayer == null)
        {
            Debug.LogWarning("No detected player target.");
            return false;
        }

        if (animate == null)
        {
            Debug.LogWarning("Animator is not assigned.");
            return false;
        }

        isAttacking = true;
        nextAttackTime = Time.time + enemyAttackCooldown;

        Debug.Log(
            $"[EnemyCombat] Attack accepted. " +
            $"Next attack allowed at {nextAttackTime}"
        );

        FacePlayerTarget();
        animate.SetTrigger("Attack");
        return true;
    }
    public void EndAttack()
    {
        isAttacking = false;

        Debug.Log(
            $"[EnemyCombat] EndAttack called on {name}. " +
            $"isAttacking = {isAttacking}, Time = {Time.time}"
        );

        if(animate != null)
        {
            animate.ResetTrigger("Attack");
        }
        //Debug.Log($"[Combat] EndAttack fired at {Time.time}");
    }

    public void OnAttackEnded()
    {
        EndAttack();
    }

    public void SetEnemyDamage(float damage)
    {
        enemyDamage = Mathf.Max(0f, damage);
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

    public void FacePlayerTarget()
    {
        if(enemyCombatDetection == null)
        {
            Debug.LogWarning("EnemyCombatDetection is not assigned.");
            return;
        }

        Transform target = enemyCombatDetection.detectedPlayer;
        if(target == null)
        {
            Debug.LogWarning("No detected player.");
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            faceTargetSpeed * Time.deltaTime
        );
    }
}
