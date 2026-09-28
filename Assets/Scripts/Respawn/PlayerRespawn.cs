using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private CheckpointRespawn checkpointRespawn;
    private void Awake()
    {
        if (checkpointRespawn == null)
        {
            checkpointRespawn = GetComponent<CheckpointRespawn>();
        }
    }
    public void Respawn()
    {
        if (checkpointRespawn == null)
        {
            Debug.LogError("[PlayerRespawn] CheckpointRespawn could not be found.", this);
            return;
        }
        checkpointRespawn.RespawnAtCurrentCheckpoint();
    }
}
