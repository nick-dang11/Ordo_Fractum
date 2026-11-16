using UnityEngine;

public class WhetstonePickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField,Min(1)]
    private int _amount = 1;

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entered by: {other.name}");
        Inventory inventory = other.GetComponentInParent<Inventory>();

        if(inventory == null)
        {
            Debug.LogWarning("Inventory component not found on the object that entered the trigger.");
            return;
        }

        if(collected)
        {
            return;
        }

        collected = true;
        inventory.AddWhetstone(_amount);

        Debug.Log($"Whetstone picked up! Amount: {_amount}");

        Destroy(transform.root.gameObject);
    }
}
