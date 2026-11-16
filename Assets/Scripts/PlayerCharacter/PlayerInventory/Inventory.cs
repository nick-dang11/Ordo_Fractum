using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    [Header("Upgrade Materials")]
    [SerializeField, Min(0)]
    private int whetstone = 0; //matierial name

    public int Whetstone => whetstone;

    public void AddWhetstone(int amount)
    {
        if(amount <= 0)
        {
            Debug.LogWarning("Cannot add a negative amount of Whetstone.");
            return;
        }

        whetstone += amount;

        Debug.Log($"Added {amount} Whetstone. Total: {whetstone}");

    

    }

    private bool HasEnoughWhetstone(int requiredAmount)
    {
        if(requiredAmount < 0)
        {
            Debug.LogWarning("Required amount cannot be negative.");
            return false;
        }

        return whetstone >= requiredAmount;
    }

    public bool SpendWhetstone(int amount)
    {
        if(amount < 0)
        {
            return false;
        }

        if (!HasEnoughWhetstone(amount))
        {
            Debug.LogWarning($"Not enough Whetstone)!");
            return false;
        }

        whetstone -= amount;
        Debug.Log("Whetstone spent. Reamining: " + whetstone);
        return true;
    }

    public void Update()
    {
        if(Keyboard.current == null)
        {
            return;
        }

        if(Keyboard.current.iKey.wasPressedThisFrame)
        {
            AddWhetstone(1);
        }

        if(Keyboard.current.jKey.wasPressedThisFrame)
        {
            SpendWhetstone(1);
        }
    }


}
