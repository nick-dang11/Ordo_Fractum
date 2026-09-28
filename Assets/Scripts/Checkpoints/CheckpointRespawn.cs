using JetBrains.Annotations;
using UnityEngine;

public class CheckpointRespawn : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private HealingAnimation healingAnimation;

    [Header("Debug")]
    [SerializeField] private bool logRespawns = true;

    private void Awake()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        if (playerCombat == null)
        {
            playerCombat = GetComponent<PlayerCombat>();
        }
        if (healthSystem == null)
        {
            healthSystem = GetComponent<HealthSystem>();
        }
        if (healingAnimation == null)
        {
            healingAnimation = GetComponent<HealingAnimation>();
        }
    }
    public void RespawnAtCurrentCheckpoint()
    {
        if (CheckpointManager.Instance == null)
        {
            Debug.LogError("[CheckpointRespawn] No CheckpointManager exists.", this);
            return;
        }
        // Clean up before respawn
        if (healingAnimation != null)
        {
            healingAnimation.ResetForRespawn();
        }
        if (playerCombat != null)
        {
            playerCombat.ResetForRespawn();
        }
        // Respawn location
        Vector3 respawnPosition = CheckpointManager.Instance.GetRespawnPosition();
        Quaternion respawnRotation = CheckpointManager.Instance.GetRespawnRotation();
        // Teleport
        TeleportPlayer(respawnPosition, respawnRotation);
        // Restore health
        if (healthSystem != null)
        {
            healthSystem.RestoreForRespawn();
        }
        // Respawn Feedback
        Checkpoint currentCheckpoint = CheckpointManager.Instance.CurrentCheckpoint;
        if (currentCheckpoint != null)
        {
            currentCheckpoint.PlayRespawnFeedback();
        }
        if (logRespawns)
        {
            Debug.Log($"[CheckpointRespawn] Player respawned at " + $"{respawnPosition}");
        }
    }
    private void TeleportPlayer(Vector3 position, Quaternion rotation)
    {
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        transform.SetPositionAndRotation(position, rotation);
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        if (logRespawns)
        {
            Debug.Log($"[CheckpointRespawn] Player respawnwd at " + $"{position}");
        }
    }
    [ContextMenu("Test - Respawn at Current Checkpoint")]
    private void TestRespawn()
    {
        RespawnAtCurrentCheckpoint();
    }
}
