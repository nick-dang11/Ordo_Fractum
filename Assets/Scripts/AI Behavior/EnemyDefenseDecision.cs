
using UnityEngine;

public class EnemyDefenseDecision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCombat enemyCombat;
    [SerializeField] private EnemyCombatDetection combatDetection;
    [SerializeField] private EnemyStateManager stateManager;

    [Header("Defensive Probabilities")]
    [SerializeField, Range(0f, 1f)] private float blockChance = 0.30f;

    [Header("Reaction Timing")]
    [SerializeField, Min(0f)] private float minimumReactionDelay = 0.15f;
    [SerializeField, Min(0f)] private float maximumReactionDelay = 0.35f;

    [Header("Blocking")]
    [SerializeField, Min(0.05f)] private float blockDuration = 0.65f;
    [SerializeField, Min(0f)] private float defensiveCooldown = 1.5f;

    private PlayerCombat observedPlayer;
    private int lastObservedAttackSequence;

    private bool reactionPending;
    private bool blockReady;

    private float reactionReadyTime;
    private float nextDefenseAllowedTime;

    public bool ShouldBlock => blockReady;
    public float BlockDuration => blockDuration;

    private void Awake()
    {
        if (enemyCombat == null) enemyCombat = GetComponent<EnemyCombat>();
        if (combatDetection == null) combatDetection = GetComponent<EnemyCombatDetection>();
        if (stateManager == null) stateManager = GetComponent<EnemyStateManager>();
    }

    private void Update()
    {
        if (combatDetection == null || enemyCombat == null) return;

        Transform player = combatDetection.detectedPlayer;

        if (player == null || !combatDetection.isPlayerInAttackRange)
        {
            CancelPendingDecision();
            observedPlayer = null;
            return;
        }

        PlayerCombat playerCombat = player.GetComponentInParent<PlayerCombat>();

        if (playerCombat == null)
        {
            CancelPendingDecision();
            observedPlayer = null;
            return;
        }

        if (observedPlayer != playerCombat)
        {
            observedPlayer = playerCombat;
            lastObservedAttackSequence = playerCombat.ConfirmedAttackSequence;

            CancelPendingDecision();
            return;
        }

        ObserveAttack(playerCombat);
        UpdateReaction(playerCombat);
    }

    private void ObserveAttack(PlayerCombat playerCombat)
    {
        int sequence = playerCombat.ConfirmedAttackSequence;

        if (sequence == lastObservedAttackSequence) return;

        lastObservedAttackSequence = sequence;
        CancelPendingDecision();

        if (!CanConsiderDefense()) return;
        if (Random.value >= blockChance) return;

        float minimumDelay = Mathf.Min(minimumReactionDelay, maximumReactionDelay);
        float maximumDelay = Mathf.Max(minimumReactionDelay, maximumReactionDelay);
        float delay = Random.Range(minimumDelay, maximumDelay);

        reactionReadyTime = Time.time + delay;
        reactionPending = true;

        Debug.Log(
            $"[EnemyDefense] Block selected. " +
            $"Reaction delay={delay:F2}s, " +
            $"AttackSequence={sequence}",
            this
        );
    }

    private void UpdateReaction(PlayerCombat playerCombat)
    {
        if (blockReady)
        {
            if (!playerCombat.IsAttackAnimationPlaying() || !CanConsiderDefense())
            {
                CancelPendingDecision();
            }

            return;
        }

        if (!reactionPending) return;

        if (!playerCombat.IsAttackAnimationPlaying() || !CanConsiderDefense())
        {
            CancelPendingDecision();
            return;
        }

        if (Time.time < reactionReadyTime) return;

        reactionPending = false;
        blockReady = true;

        Debug.Log("[EnemyDefense] Block decision ready.", this);
    }

    private bool CanConsiderDefense()
    {
        if (Time.time < nextDefenseAllowedTime) return false;
        if (enemyCombat == null || !enemyCombat.CanBlock) return false;
        if (stateManager == null) return false;

        return stateManager.CurrentState == stateManager.NeutralState;
    }

    public bool TryConsumeBlockDecision()
    {
        if (!blockReady) return false;

        blockReady = false;

        if (!CanConsiderDefense()) return false;

        nextDefenseAllowedTime = Time.time + defensiveCooldown;

        return true;
    }

    public void CancelPendingDecision()
    {
        reactionPending = false;
        blockReady = false;
    }

    private void OnDisable()
    {
        CancelPendingDecision();
    }
}
