
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimatorDiagnostics : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    private EnemyCombatDetection detection;
    private EnemyCombat combat;

    private int lastStateHash;
    private bool lastInCombat;
    private bool lastTransitioning;
    private int lastNextStateHash;

    private float nextSnapshotTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        detection = GetComponent<EnemyCombatDetection>();
        combat = GetComponent<EnemyCombat>();

        if (animator == null)
        {
            Debug.LogError(
                "[EnemyDiag] Animator component missing.",
                this
            );
        }
    }

    private void Start()
    {
        if (animator == null)
            return;

        lastStateHash = 0;
        lastInCombat = animator.GetBool("InCombat");
        lastTransitioning = false;
        lastNextStateHash = 0;

        LogSnapshot("INITIAL");
    }

    private void LateUpdate()
    {
        if (animator == null)
            return;

        AnimatorStateInfo current =
            animator.GetCurrentAnimatorStateInfo(0);

        bool inCombat = animator.GetBool("InCombat");
        bool transitioning = animator.IsInTransition(0);

        int nextHash = transitioning
            ? animator.GetNextAnimatorStateInfo(0).fullPathHash
            : 0;

        if (current.fullPathHash != lastStateHash)
        {
            lastStateHash = current.fullPathHash;
            LogSnapshot("STATE CHANGED");
        }

        if (inCombat != lastInCombat)
        {
            lastInCombat = inCombat;
            LogSnapshot("INCOMBAT CHANGED");
        }

        if (transitioning != lastTransitioning ||
            (transitioning && nextHash != lastNextStateHash))
        {
            lastTransitioning = transitioning;
            lastNextStateHash = nextHash;

            LogSnapshot(
                transitioning
                    ? "TRANSITION START"
                    : "TRANSITION END"
            );
        }

        if (Time.time >= nextSnapshotTime)
        {
            nextSnapshotTime = Time.time + 1f;
            LogSnapshot("PERIODIC");
        }
    }

    private void LogSnapshot(string reason)
    {
        if (animator == null)
            return;

        AnimatorStateInfo current =
            animator.GetCurrentAnimatorStateInfo(0);

        bool transitioning = animator.IsInTransition(0);

        string nextState = transitioning
            ? GetStateName(animator.GetNextAnimatorStateInfo(0))
            : "None";

        string navigation = "No NavMeshAgent";

        if (agent != null && agent.isOnNavMesh)
        {
            navigation =
                $"Stopped={agent.isStopped}, " +
                $"HasPath={agent.hasPath}, " +
                $"DesiredSpeed={agent.desiredVelocity.magnitude:F2}, " +
                $"ActualSpeed={agent.velocity.magnitude:F2}, " +
                $"RemainingDistance={agent.remainingDistance:F2}";
        }

        string combatStatus = combat != null
            ? $"Attacking={combat.IsAttacking}, " +
              $"CanAttack={combat.CanAttack}"
            : "No EnemyCombat";

        string detectionStatus = detection != null
            ? $"Nearby={detection.isPlayerNearby}, " +
              $"AttackRange={detection.isPlayerInAttackRange}"
            : "No EnemyCombatDetection";

        Debug.Log(
            $"[EnemyDiag][{reason}] " +
            $"Time={Time.time:F3}, " +
            $"Frame={Time.frameCount}, " +
            $"State={GetStateName(current)}, " +
            $"Next={nextState}, " +
            $"Transitioning={transitioning}, " +
            $"InCombat={animator.GetBool("InCombat")}, " +
            $"Speed={animator.GetFloat("Speed"):F2}, " +
            $"{detectionStatus}, " +
            $"{combatStatus}, " +
            $"{navigation}",
            this
        );
    }

    private string GetStateName(AnimatorStateInfo state)
    {
        return $"Hash:{state.fullPathHash}";
    }
}
