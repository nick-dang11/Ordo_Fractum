
using UnityEngine;

// Keep this class name to match the component already used by the Player prefab.
public class PlayerActionController : MonoBehaviour
{
    [Header("Core Scripts")]
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private LockOnController lockOnController;
    [SerializeField] private PlayerInputManager inputManager;

    [Header("Transforms")]
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform mainCamera;

    [Header("Directional Targeting")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Min(0f)] private float targetingRadius = 12f;
    [SerializeField, Range(0f, 180f)] private float maxTargetingAngle = 45f;
    [SerializeField, Min(0f)] private float passiveRotationSpeed = 15f;
    [SerializeField, Min(0f)] private float attackStartupGrace = 0.2f;

    private Transform currentFocusTarget;     // Last engaged enemy, for passive facing.
    private Transform attackTarget;           // Enemy selected for the current attack.
    private Vector3 attackDirection;          // Camera-relative direction for misses.
    private bool hasAttackDirection;
    private float attackFacingUntil;
    private bool wasBlockPressed;

    private bool IsLockedOn => lockOnController != null && lockOnController.IsLockedOn;

    private void Awake()
    {
        if (playerCombat == null)
            playerCombat = GetComponent<PlayerCombat>();

        if (inputManager == null)
            inputManager = playerCombat != null ? playerCombat.input : GetComponent<PlayerInputManager>();

        if (mainCamera == null && Camera.main != null)
            mainCamera = Camera.main.transform;
    }

    private void Update()
    {
        if (inputManager == null || playerModel == null)
            return;

        if (!IsLockedOn)
        {
            // Keep the last focused enemy while alive and in range.
            // When it dies or leaves range, switch to the nearest living enemy.
            if (!IsValidTarget(currentFocusTarget))
                currentFocusTarget = FindNearestEnemy();

            // Blocking without a prior target falls back to the nearest enemy.
            // Successful blocks/deflects should instead call FocusOnEnemy().
            if (inputManager.block && !wasBlockPressed && currentFocusTarget == null)
                currentFocusTarget = FindNearestEnemy();
        }

        wasBlockPressed = inputManager.block;
    }

    private void LateUpdate()
    {
        if (playerModel == null)
            return;

        if (IsLockedOn)
        {
            hasAttackDirection = false;
            attackTarget = null;
            return;
        }

        // Keep attack facing independent from passive focus. This prevents
        // a previous target from turning an intentional attack/miss around.
        bool performingAttack = playerCombat != null && playerCombat.isAttacking;
        bool attackButtonHeld = inputManager != null &&
                                (inputManager.attack || inputManager.heavyAttack);

        if (hasAttackDirection &&
            (performingAttack || attackButtonHeld || Time.time < attackFacingUntil))
        {
            Vector3 direction = attackDirection;
            if (IsValidTarget(attackTarget))
                direction = attackTarget.position - transform.position;

            FaceDirection(direction, instant: true);
            return;
        }

        hasAttackDirection = false;
        attackTarget = null;

        if (IsValidTarget(currentFocusTarget))
        {
            FaceDirection(currentFocusTarget.position - transform.position, instant: false);
        }
        else
        {
            // No enemies: return the visual model to the player's root facing.
            playerModel.localRotation = Quaternion.Slerp(
                playerModel.localRotation,
                Quaternion.identity,
                Time.deltaTime * passiveRotationSpeed
            );
        }
    }

    // Call immediately before PlayerCombat triggers an accepted light/heavy attack.
    public void SnapToAttackTarget()
    {
        if (IsLockedOn || playerModel == null || inputManager == null)
            return;

        if (mainCamera == null && Camera.main != null)
            mainCamera = Camera.main.transform;

        Vector3 inputDirection = GetCameraRelativeInput(inputManager.move);

        // If the player isn't moving, attack toward the direction they face.
        if (inputDirection.sqrMagnitude < 0.01f)
            inputDirection = playerModel.forward;

        inputDirection.y = 0f;
        if (inputDirection.sqrMagnitude < 0.001f)
            return;

        inputDirection.Normalize();

        attackTarget = FindDirectionalEnemy(inputDirection);
        attackDirection = inputDirection;
        hasAttackDirection = true;
        attackFacingUntil = Time.time + attackStartupGrace;

        if (attackTarget != null)
        {
            currentFocusTarget = attackTarget;
            FaceDirection(attackTarget.position - transform.position, instant: true);
        }
        else
        {
            // No valid enemy in this direction: swing toward input, not
            // automatically toward the last remembered/nearest enemy.
            FaceDirection(inputDirection, instant: true);
        }
    }

    // Call this from PlayerCombat's block and deflect feedback functions.
    public void FocusOnEnemy(Transform enemy)
    {
        if (IsLockedOn || enemy == null)
            return;

        EnemyCombat combat = enemy.GetComponentInParent<EnemyCombat>();
        Transform target = combat != null ? combat.transform : enemy;

        if (IsValidTarget(target))
            currentFocusTarget = target;
    }

    private Transform FindDirectionalEnemy(Vector3 inputDirection)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, targetingRadius, enemyLayer);
        Transform best = null;
        float bestAngle = Mathf.Infinity;
        float bestDistanceSq = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            Transform candidate = GetEnemyRoot(hit);
            if (!IsValidTarget(candidate))
                continue;

            Vector3 toEnemy = candidate.position - transform.position;
            toEnemy.y = 0f;
            float distanceSq = toEnemy.sqrMagnitude;
            if (distanceSq < 0.001f)
                continue;

            float angle = Vector3.Angle(inputDirection, toEnemy);
            if (angle > maxTargetingAngle)
                continue;

            // Smallest camera-relative angle wins; distance breaks close ties.
            if (angle < bestAngle - 0.01f ||
                (Mathf.Abs(angle - bestAngle) <= 0.01f && distanceSq < bestDistanceSq))
            {
                best = candidate;
                bestAngle = angle;
                bestDistanceSq = distanceSq;
            }
        }

        return best;
    }

    private Transform FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, targetingRadius, enemyLayer);
        Transform nearest = null;
        float bestDistanceSq = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            Transform candidate = GetEnemyRoot(hit);
            if (!IsValidTarget(candidate))
                continue;

            Vector3 offset = candidate.position - transform.position;
            offset.y = 0f;
            float distanceSq = offset.sqrMagnitude;

            if (distanceSq < bestDistanceSq)
            {
                nearest = candidate;
                bestDistanceSq = distanceSq;
            }
        }

        return nearest;
    }

    private static Transform GetEnemyRoot(Collider hit)
    {
        if (hit == null)
            return null;

        EnemyCombat combat = hit.GetComponentInParent<EnemyCombat>();
        if (combat != null)
            return combat.transform;

        EnemyHealth health = hit.GetComponentInParent<EnemyHealth>();
        return health != null ? health.transform : null;
    }

    private bool IsValidTarget(Transform target)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            return false;

        EnemyCombat combat = target.GetComponent<EnemyCombat>();
        EnemyHealth health = target.GetComponent<EnemyHealth>();
        if (combat == null && health == null)
            return false;

        if (combat != null && (combat.IsDead || combat.IsBeingExecuted))
            return false;

        if (health != null && health.deathRequested)
            return false;

        Vector3 offset = target.position - transform.position;
        offset.y = 0f;
        return offset.sqrMagnitude <= targetingRadius * targetingRadius;
    }

    private Vector3 GetCameraRelativeInput(Vector2 moveInput)
    {
        if (mainCamera == null)
            return new Vector3(moveInput.x, 0f, moveInput.y);

        Vector3 forward = mainCamera.forward;
        Vector3 right = mainCamera.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        return forward * moveInput.y + right * moveInput.x;
    }

    private void FaceDirection(Vector3 direction, bool instant)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        playerModel.rotation = instant
            ? targetRotation
            : Quaternion.Slerp(playerModel.rotation, targetRotation,
                               Time.deltaTime * passiveRotationSpeed);
    }
}
