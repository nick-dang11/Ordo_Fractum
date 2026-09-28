using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }
    [Header("Current Checkpoint")]
    [SerializeField] private Checkpoint currentCheckpoint;
    [Header("Default Spawn")]
    [SerializeField] private Transform defaultSpawnPoint;
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
        currentCheckpoint = checkpoint;
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
}
