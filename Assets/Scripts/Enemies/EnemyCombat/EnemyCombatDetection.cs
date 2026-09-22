using UnityEngine;

public class EnemyCombatDetection : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float attackRange = 2f;

    [SerializeField] public LayerMask playerLayerMask;
    [SerializeField] private Animator animator;

    private Transform detectedPlayer;

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