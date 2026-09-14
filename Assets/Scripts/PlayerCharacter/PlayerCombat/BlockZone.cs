using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class BlockZone : MonoBehaviour
{
    [SerializeField] private PlayerCombat owner;

    private CapsuleCollider zoneCollider;

    public PlayerCombat Owner => owner;

    public Collider ZoneCollider => zoneCollider;

    public bool IsActive =>
        zoneCollider != null && zoneCollider.enabled;

    private void Awake()
    {
        zoneCollider = GetComponent<CapsuleCollider>();

        zoneCollider.isTrigger = true;

        if (owner == null)
        {
            owner = GetComponentInParent<PlayerCombat>();
        }

        if (owner == null)
        {
            Debug.LogError(
                "[BlockZone] Could not find PlayerCombat on a parent object.",
                this
            );
        }

        zoneCollider.enabled = false;
    }

    public void SetActive(bool active)
    {
        if (zoneCollider == null)
            return;

        zoneCollider.enabled = active;
    }
}