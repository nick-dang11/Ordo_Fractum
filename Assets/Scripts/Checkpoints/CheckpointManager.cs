using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }
    [Header("Current Checkpoint")]
    [SerializeField] private Checkpoint currentCheckpoint;
    [Header("Default Spawn")]
    [SerializeField] private Transform defaultSpawnPoint;
    [Header("Debug Restore")]
    [SerializeField] private string debugCheckpointId;
    public Checkpoint CurrentCheckpoint => currentCheckpoint;
    // Gets most recent checkpoint
    public string CurrentCheckpointId
    {
        get
        {
            if (currentCheckpoint == null)
                return string.Empty;
            return currentCheckpoint.CheckpointID;
        }
    }
    // Prevents multiple Checkpoint managers
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("More than one CheckpointManager exists", this);
            enabled = false;
            return;
        }
        Instance = this;
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    public void SetCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null) return;
        // Doesn't reassigns the checkpoint if current
        if (currentCheckpoint == checkpoint) return;
        Checkpoint previousCheckpoint = currentCheckpoint;
        if (previousCheckpoint != null)
        {
            previousCheckpoint.SetCurrent(false);
        }
        currentCheckpoint = checkpoint;
        currentCheckpoint.SetCurrent(true);
        if (previousCheckpoint == null)
        {
            Debug.Log($"[CheckpointManager] Current checkpoint " + $"{currentCheckpoint.CheckpointID}");
        } else
        {
            Debug.Log($"[CheckpointManager] Current checkpoint changed " + $"from '{previousCheckpoint.CheckpointID}'" + $"to '{currentCheckpoint.CheckpointID}'.");
        }
    }
    public Vector3 GetRespawnPosition()
    {
        if (currentCheckpoint != null)
        {
            return currentCheckpoint.RespawnPosition;
        }
        if (defaultSpawnPoint != null)
        {
            return defaultSpawnPoint.position;
        }
        Debug.LogWarning("[CheckpointManager] No checkpoint or default spawn spoint exists.");
        return Vector3.zero;
    }
    public Quaternion GetRespawnRotation()
    {
        if (currentCheckpoint != null)
        {
            return currentCheckpoint.RespawnRotation;
        }
        if (defaultSpawnPoint != null)
        {
            return defaultSpawnPoint.rotation;
        }
        return Quaternion.identity;
    }
    public Checkpoint FindCheckpointById(string checkpointId)
    {
        if (string.IsNullOrEmpty(checkpointId))
        {
            return null;
        }
        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>();
        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (checkpoint.CheckpointID == checkpointId)
            {
                return checkpoint;
            }
        }
        Debug.LogWarning($"[CheckpointManager] No Checkpoint found " + $"with ID '{checkpointId}'.");
        return null;
    }
    public bool RestoreCheckpointById(string checkpointId)
    {
        Checkpoint checkpoint = FindCheckpointById(checkpointId);
        if (checkpoint == null)
        {
            return false;
        }
        checkpoint.RestoreAsActivated();
        SetCheckpoint(checkpoint);
        Debug.Log($"[CheckpointManager] Restored checkpoint " + $"'{checkpointId}'.");
        return true;
    }
    [ContextMenu("Debug - Print Current Checkpoint")]
    private void PrintCurrentCheckpoint()
    {
        if (currentCheckpoint == null)
        {
            Debug.Log("[CheckpointManager] No checkpoint is active. " + $"Respawn would use default position: " + $"{GetRespawnPosition()}");
            return;
        }
        Debug.Log($"[CheckpointManager] Current checpoint is " + $"'{currentCheckpoint.CheckpointID}', " + $"RespawnPostion={GetRespawnPosition()}");
    }
    [ContextMenu("Debug - Print Save Checkpoint ID")]
    private void PrintSaveCheckpointId()
    {
        if (currentCheckpoint == null)
        {
            Debug.Log("[CheckpointManager] Save ID: None");
            return;
        }
        Debug.Log($"[CheckpointManager] Save ID: " + $"{CurrentCheckpointId}");
    }
    [ContextMenu("Debug - Restore Checkpoint by ID")]
    private void DebugRestore()
    {
        bool success = RestoreCheckpointById(debugCheckpointId);
        if (!success)
        {
            Debug.LogWarning($"CheckpointManager] Debug restore failed " + $"for '{debugCheckpointId}'.");
        }
    }
    [ContextMenu("Debug - Validate Checkpoint IDs")]
    private void ValidateCheckpointIds()
    {
        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>();
        HashSet<string> ids = new HashSet<string>();
        foreach (Checkpoint checkpoint in checkpoints)
        {
            string id = checkpoint.CheckpointID;
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"Checkpoint '{checkpoint.name}' " + $"has no ID.", checkpoint);
                continue;
            }
            if (!ids.Add(id))
            {
                Debug.LogError($"Duplicate checkpoint ID found: " + $"'{id}'.", checkpoint);
            }
        }
        Debug.Log("[CheckpointManager] Checkpoint ID validation complete.");
    }
}
