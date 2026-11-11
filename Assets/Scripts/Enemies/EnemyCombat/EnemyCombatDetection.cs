using UnityEngine;

public class EnemyCombatDetection : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float attackRange = 3f;

    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private Animator animator;

    private bool previousNearby;
    private bool previousAttackRange;

    public Transform detectedPlayer;

    public bool isPlayerNearby { get; private set; }
    public bool isPlayerInAttackRange { get; private set; }
    public Transform DetectedPlayer => detectedPlayer;

    private void Update()
    {
        isPlayerNearby = CheckForPlayer();

        if (isPlayerNearby && detectedPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(
                transform.position,
                detectedPlayer.position
            );

            isPlayerInAttackRange = distanceToPlayer <= attackRange;
        }
        else
        {
            isPlayerInAttackRange = false;
        }

        if (animator != null)
        {
            animator.SetBool("InCombat", isPlayerNearby);
        }

        if (previousNearby != isPlayerNearby ||
          previousAttackRange != isPlayerInAttackRange)
        {
            Debug.Log(
                $"[EnemyDetection] " +
                $"Time={Time.time:F3}, " +
                $"Frame={Time.frameCount}, " +
                $"Nearby={isPlayerNearby}, " +
                $"AttackRange={isPlayerInAttackRange}, " +
                $"Player={(detectedPlayer != null ? detectedPlayer.name : "None")}"
            );

            previousNearby = isPlayerNearby;
            previousAttackRange = isPlayerInAttackRange;
        }
    }

    private bool CheckForPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            playerLayerMask
        );

        if(hits.Length == 0)
        {
            detectedPlayer = null;
            return false;
        }

        detectedPlayer = hits[0].transform.root;
        return true;
    }
}