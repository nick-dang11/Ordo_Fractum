
using UnityEngine;

public class HealingAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private PlayerHealth healthSystem;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private PlayerWill playerWill;

    [Header("Timing for Heal Animation")]
    [SerializeField] private float healingDuration = 1.8f;

    [Header("Healing Cost")]
    [SerializeField] private float healWillCost = 25f;

    [Header("Partial Healing Settings")]
    [SerializeField, Range(0f, 1f)]
    private float minimumHealProgress = 0.5f;

    // Healing state tracking
    private bool isHealing = false;
    private bool healingSettled = false;
    private float healingTimer = 0f;

    // Animator trigger
    private static readonly int HealTrigger =
        Animator.StringToHash("Heal");

    // Public properties
    public bool IsHealing => isHealing;

    public float NormalizedProgress =>
        healingDuration > 0f
            ? Mathf.Clamp01(healingTimer / healingDuration)
            : 0f;

    private void Awake()
    {
        // Automatically locate missing references
        if (playerInputManager == null)
            playerInputManager = GetComponent<PlayerInputManager>();

        if (healthSystem == null)
            healthSystem = GetComponent<PlayerHealth>();

        if (playerWill == null)
            playerWill = GetComponent<PlayerWill>();

        if (playerAnimator == null)
            playerAnimator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // Healing is started by HealingState.
        // This script only manages healing progress.

        if (!isHealing)
            return;

        healingTimer += Time.deltaTime;

        // Complete healing once the duration is reached
        if (healingTimer >= healingDuration)
        {
            FinishHealing();
        }
    }

    // Check whether the player is allowed to heal
    public bool CanStartHealing()
    {
        StateManager manager = GetComponent<StateManager>();

        // Healing is only allowed from IdleState
        // or while entering HealingState.
        bool validState =
            manager == null ||
            manager.currentState == manager.idleState ||
            manager.currentState == manager.healingState;

        if (!validState)
            return false;

        if (isHealing)
            return false;

        if (healthSystem == null || playerWill == null ||
            playerAnimator == null)
        {
            return false;
        }

        // Prevent healing if player is dead
        if (healthSystem.health <= 0)
            return false;

        // Prevent healing at full health
        if (healthSystem.health >= healthSystem.maxHealth)
            return false;

        // Require enough Will for the full healing cost
        if (!playerWill.HasWill(healWillCost))
            return false;

        return true;
    }

    // Start a new healing attempt
    public bool TryStartHealing()
    {
        if (!CanStartHealing())
        {
            Debug.Log(
                "[Healing] Cannot start: Requirements not met."
            );

            return false;
        }

        // Initialize healing
        isHealing = true;
        healingSettled = false;
        healingTimer = 0f;

        // Clear healing input
        if (playerInputManager != null)
        {
            playerInputManager.self_heal = false;
        }

        // Start healing animation
        playerAnimator.ResetTrigger(HealTrigger);
        playerAnimator.SetTrigger(HealTrigger);

        Debug.Log("[Healing] Healing started.");

        return true;
    }

    // Apply healing once based on completed progress
    private void SettleHealing(float progress)
    {
        // Prevent applying healing more than once
        if (!isHealing || healingSettled)
            return;

        healingSettled = true;

        progress = Mathf.Clamp01(progress);

        // Check whether enough of the animation completed
        if (progress >= minimumHealProgress)
        {
            // Calculate how much health can be restored
            int healthToRestore = Mathf.Min(
                healthSystem.heal_Amount,
                healthSystem.maxHealth - healthSystem.health
            );

            // Deduct Will proportionally to progress
            float willCost = healWillCost * progress;

            if (healthToRestore > 0)
            {
                // Only apply healing if Will deduction succeeds
                if (playerWill.SpendWill(willCost))
                {
                    healthSystem.Heal(healthToRestore);

                    Debug.Log(
                        $"[Healing] Restored {healthToRestore} HP. " +
                        $"Will spent: {willCost:F2}"
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "[Healing] Not enough Will to settle healing."
                    );
                }
            }
        }
        else
        {
            Debug.Log(
                "[Healing] Interrupted before minimum progress. " +
                "No healing applied."
            );
        }

        Debug.Log(
            $"[Healing] Settled at {progress:P0}"
        );

        isHealing = false;
    }

    // Called when the healing process completes
    public void FinishHealing()
    {
        if (!isHealing)
            return;

        // Apply full healing and full Will cost
        SettleHealing(1f);

        healingTimer = 0f;

        Debug.Log("[Healing] Healing completed.");
    }

    // Called when healing is interrupted by another action
    public void InterruptHealing()
    {
        if (!isHealing)
            return;

        // Capture progress BEFORE resetting the timer
        float progress = NormalizedProgress;

        Debug.Log(
            $"[Healing] Interrupted at {progress:P0}"
        );

        // Apply partial healing if threshold was reached
        SettleHealing(progress);

        // Clear pending healing trigger
        if (playerAnimator != null)
        {
            playerAnimator.ResetTrigger(HealTrigger);
        }

        healingTimer = 0f;

        // Prevent stale healing input
        if (playerInputManager != null)
        {
            playerInputManager.self_heal = false;
        }
    }

    // Reset healing when the player respawns
    public void ResetForRespawn()
    {
        // Cancel without awarding partial healing
        isHealing = false;
        healingSettled = false;
        healingTimer = 0f;

        if (playerInputManager != null)
        {
            playerInputManager.self_heal = false;
        }

        if (playerAnimator != null)
        {
            playerAnimator.ResetTrigger(HealTrigger);
        }

        Debug.Log(
            "[HealingAnimation] Reset for respawn."
        );
    }
}
