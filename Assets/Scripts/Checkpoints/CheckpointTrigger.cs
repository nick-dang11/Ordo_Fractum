using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private Checkpoint checkpoint;

    private void Awake()
    {
        if (checkpoint == null)
        {
            checkpoint = GetComponentInParent<Checkpoint>();
        }
        if (checkpoint == null)
        {
            Debug.LogError($"CheckpointTrigger on '{gameObject.name}'" + $"could not find a parent Checkpoint.", this);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (checkpoint == null)
            return;
        // Checks if Player is in activation zone.
        if (other.CompareTag("Player"))
        {
            checkpoint.Activate();
            return;
        }
        // Checks the collider is a child of the player.
        if (other.transform.root.CompareTag("Player"))
        {
            checkpoint.Activate();
        }
    }
}
