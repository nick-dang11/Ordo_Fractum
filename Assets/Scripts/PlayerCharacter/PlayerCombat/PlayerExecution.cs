using UnityEngine;

public class PlayerExecution : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StateManager stateManager;
    [SerializeField] private Animator playerAnimator;

    [Header("Detection")]
    [SerializeField] private float executionRange = 2f;
    [SerializeField] private LayerMask enemyLayerMask;

    private EnemyStateManager executionTarget;

    private void Awake()
    {
        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();

        if (stateManager == null)
            stateManager = GetComponent<StateManager>();
    }

    public bool TryExecute()
    {
        if (executionTarget != null) return false;
        if (stateManager == null) return false;

        Collider[] enemies = Physics.OverlapSphere(
            transform.position,
            executionRange,
            enemyLayerMask
        );

        foreach (Collider enemyCollider in enemies)
        {
            EnemyStateManager enemyStateManager =
                enemyCollider.GetComponentInParent<EnemyStateManager>();

            if (enemyStateManager == null)
                continue;

            EnemyCombat enemyCombat =
                enemyStateManager.Combat;

            if (enemyCombat == null)
            {
                Debug.LogWarning(
                    $"{enemyStateManager.name} has no EnemyCombat."
                );

                continue;
            }

            Transform executionAnchor =
                enemyCombat.ExecutionAnchor;

            if (executionAnchor == null)
            {
                Debug.LogWarning($"Execution {enemyStateManager.name} has no ExecutionAnchor.");

                continue;
            }

            if (!enemyStateManager.RequestExecution())
                continue;

            executionTarget = enemyStateManager;

            stateManager.movement.Teleport(executionAnchor.position, executionAnchor.rotation);
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("ExecuteEnemy");
            }

            stateManager.SwitchState(
                stateManager.playerExecutingState
            );

            Debug.Log($"Execution Executing {executionTarget.name}.");

            return true;
        }

        return false;
    }

    public void FinishExecution()
    {
        if (executionTarget == null)
        {
            Debug.LogWarning("FinishExecution called with no execution target.");

            return;
        }

        Debug.Log($"Finishing execution on {executionTarget.name}.");

        executionTarget.RequestDeath();

        stateManager.SwitchState(
            stateManager.idleState,
            true
        );

        executionTarget = null;

        Debug.Log("Execution finished. Player returned to Idle.");
    }
}