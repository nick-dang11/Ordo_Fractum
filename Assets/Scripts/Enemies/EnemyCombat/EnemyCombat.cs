using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour, IAttackDamageSource, IAttackLifecycle
{
    [Header("References")]
    [SerializeField] private WeaponHitbox weaponHitbox;
    [SerializeField] private Animator animate;
    [SerializeField] private EnemyCombatDetection enemyCombatDetection;
    [SerializeField] private Transform executionAnchor;
    [SerializeField] private AttackTimelineDriver attackTimelineDriver;

    [SerializeField] private GameObject healthUI;
    [SerializeField] private GameObject postureUI;
    [SerializeField] private Collider enemyCollider;

    [SerializeField, Min(0f)] private float enemyDamage = 1f;
    [SerializeField, Min(0f)] private float enemyAttackCooldown = 1.5f;
    [SerializeField, Min(0f)] private float faceTargetSpeed = 8f;

    private int deadLayer;
    private float nextAttackTime;
    private bool isAttacking = false;
    private bool useExecutionDeathPose;

    public Transform ExecutionAnchor => executionAnchor;
    public bool IsAttacking => isAttacking;
    private bool isBlocking = false;
    public bool IsBlocking => isBlocking;

    private bool isStunned;
    public bool IsStunned => isStunned;

    private bool isBeingExecuted;
    public bool IsBeingExecuted => isBeingExecuted;
    public bool UseExecutionDeathPose => useExecutionDeathPose;

    private bool isDead;
    public bool IsDead => isDead;

    private bool currentAttackInvalidated = false;
    public bool CurrentAttackInvalidated => currentAttackInvalidated;

    private bool isDeflectReacting = false;
    public bool IsDeflectReacting => isDeflectReacting;

    [Header("Deflect Reaction")]
    [SerializeField, Min(0f)]
    private float deflectReactionDuration = 0.6f;

    public bool CanAttack => !isDead && !isBeingExecuted && !isAttacking && !isBlocking && !isStunned && !isDeflectReacting && Time.time >= nextAttackTime;
    public bool CanBlock => !isDead && !isBeingExecuted && !isAttacking && !isBlocking && !isStunned;

    private void Awake()
    {
        deadLayer = LayerMask.NameToLayer("DeadEnemy");

        if (deadLayer == -1) Debug.LogError("DeadEnemy layer does not exist.");
    }
    private void Update()
    {
        if (isDead || isBeingExecuted) return;

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

        if (enemyCombatDetection == null)
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
        currentAttackInvalidated = false;
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

        if (animate != null)
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

    public bool StartBlock()
    {
        if (!CanBlock)
        {
            Debug.Log("EnemyCombat rejected block.");
            return false;
        }

        isBlocking = true;
        if (animate != null) animate.SetBool("Blocking", true);
        Debug.Log($"EnemyCombat started block on {name}.");

        return true;
    }

    public void EndBlock()
    {
        if (!isBlocking) return;

        isBlocking = false;
        if (animate != null) animate.SetBool("Blocking", false);

        Debug.Log($"EnemyCombat ended block on {name}.");
    }

    public void StartStun()
    {
        EndBlock();

        if (isAttacking)
        {
            InterruptCurrentAttack();
        }

        isStunned = true;

        if (animate != null)
        {
            animate.SetBool("Stunned", true);
        }
    }

    public void EndStun()
    {
        isStunned = false;

        if (animate != null)
        {
            animate.SetBool("Stunned", false);
        }
    }

    public void StartExecution()
    {
        if (isDead || isBeingExecuted) return;

        useExecutionDeathPose = true;
        isBeingExecuted = true;

        EndBlock();
        isStunned = false;
        if(animate != null)
        {
            animate.SetBool("Stunned", false);
            animate.SetTrigger("GetExecuted");
        }
        EndAttack();

        if (weaponHitbox != null) weaponHitbox.DisableHitbox();
        Debug.Log($"Enemy {name} is being executed.");
    }

    public void StartDeath()
    {
        if (isDead) return;

        isBeingExecuted = false; // not all deaths caused by execution!
        isDead = true;

        if (deadLayer != -1) SetLayerRecursively(gameObject, deadLayer); // fixed CombatDetection flagging corpse as active enemy

        EndBlock();
        EndStun();
        EndAttack();
        HideUI();

        if (weaponHitbox != null) weaponHitbox.DisableHitbox();
        if (enemyCollider != null) enemyCollider.enabled = false;

        if (animate != null && !useExecutionDeathPose) animate.SetTrigger("Death");
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
        if (isDead || isBeingExecuted || isStunned) return;

        if (enemyCombatDetection == null)
        {
            Debug.LogWarning("EnemyCombatDetection is not assigned.");
            return;
        }

        Transform target = enemyCombatDetection.detectedPlayer;

        if (target == null)
        {
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        //transform.rotation = Quaternion.Slerp(
        //    transform.rotation,
        //    targetRotation,
        //    faceTargetSpeed * Time.deltaTime
        //); 
        // lerp version

        transform.rotation = targetRotation;
    }

    public void PlayBlockFeedback()
    {
        if (animate == null) return;

        animate.SetTrigger("BlockHit");
    }

    public void HideUI()
    {
        if (healthUI != null)
            healthUI.SetActive(false);

        if (postureUI != null)
            postureUI.SetActive(false);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void HandleDeflected()
    {
        if (isDead || isBeingExecuted) { return; }
        if (currentAttackInvalidated) { return; }
        Debug.Log($"[EnemyCombat] {name}'s current attack was DEFLECTED.");
        currentAttackInvalidated = true;
        isAttacking = false;
        if (weaponHitbox != null) { weaponHitbox.DisableHitbox(); }
        isDeflectReacting = true;
        if (animate != null)
        {
            animate.ResetTrigger("Attack");
            animate.SetTrigger("DeflectedReaction");
        }
        StartCoroutine(DeflectReactionRoutine());
    }

    private IEnumerator DeflectReactionRoutine()
    {
        yield return new WaitForSeconds(deflectReactionDuration);
        if (isDead || isBeingExecuted) { yield break; }
        isDeflectReacting = false;
        Debug.Log($"[EnemyCombat] {name} finished its deflect reaction.");
    }

    private void InterruptCurrentAttack()
    {
        currentAttackInvalidated = true;
        isAttacking = false;

        if (weaponHitbox != null)
        {
            weaponHitbox.DisableHitbox();
        }

        if (attackTimelineDriver != null)
        {
            attackTimelineDriver.CancelCurrentAttack();
        }
    }
}
