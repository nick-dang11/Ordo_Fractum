using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Data")]
    [SerializeField] private string checkpointId;
    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [Header("Feedback")]
    [SerializeField] private CheckpointFeedback checkpointFeedback;
    [Header("Runtime State")]
    [SerializeField] private bool isActivated = false;
    public string CheckpointID => checkpointId;
    public bool IsActivated => isActivated;
    private void Awake()
    {
        if (checkpointFeedback == null)
        {
            checkpointFeedback = GetComponent<CheckpointFeedback>();
        }
    }
    // Puts the player into the respawn position
    public Vector3 RespawnPosition
    {
        get
        {
            if (respawnPoint != null)
                return respawnPoint.position;
            return transform.position;
        }
    }
    // Rotates the player into the correct posiiton after respawn.
    public Quaternion RespawnRotation
    {
        get
        {
            if (respawnPoint != null)
                return respawnPoint.rotation;
            return transform.rotation;
        }
    }
    // When checkpoint is activated
    public void Activate()
    {
        bool firstActivation = !isActivated;
        // Activated first time
        if (firstActivation)
        {
            isActivated = true;
            Debug.Log($"Checkpoint activated: {checkpointId}");
            if (checkpointFeedback != null)
            {
                checkpointFeedback.PlayFirstActivation();
            }
        }
        // Tells the manager that this checkpoint should now be the respawn
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SetCheckpoint(this);
        } else
        {
            Debug.LogWarning($"Checkpoint '{checkpointId}' activated, " + $"but no Checkpoint manager exists.", this);
        }
    }
    // Validation to see if the checkpoint works
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(checkpointId))
        {
            Debug.LogWarning($"Checkpoint '{gameObject.name}' does not have an ID.", this);
        }
        if (respawnPoint == null)
        {
            Debug.LogWarning($"Checkpoint '{gameObject.name}' does not have a Respawn point addigned.", this);
        }
    }
    private void OnDrawGizmos()
    {
        if (respawnPoint == null)
            return;
        Gizmos.DrawSphere(respawnPoint.position, 0.2f);
        Gizmos.DrawLine(respawnPoint.position, respawnPoint.position + respawnPoint.forward * 1.5f);
    }
    public void SetCurrent(bool isCurrent)
    {
        if (checkpointFeedback != null)
        {
            checkpointFeedback.SetCurrent(isCurrent);
        }
    }
    public void PlayRespawnFeedback()
    {
        if (checkpointFeedback != null)
        {
            checkpointFeedback.PlayRespawn();
        }
    }
    public void RestoreAsActivated()
    {
        isActivated = true;
        if (checkpointFeedback != null)
        {
            checkpointFeedback.SetActivated(true);
        }
    }
}
