using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.InputSystem;


public class WeaponDamageSystem : MonoBehaviour
{
    public event Action OnWeaponLevelChanged;
    [Header("Inventory Reference")]
    [SerializeField] private Inventory inventory;
    [System.Serializable]
    public class WeaponLevel
    {
        public float lightDamage;
        public float heavyDamage;
        public float counterDamage;
    }

    // Damage table with only 3 weapon levels
    [Header("Weapon Damage Table")]
    [SerializeField]

    private WeaponLevel[] damageTable =
    {
        new WeaponLevel
        {
            lightDamage = 10f,
            heavyDamage = 40f,
            counterDamage = 25f
        },
        new WeaponLevel
        {
            lightDamage = 15f,
            heavyDamage = 45f,
            counterDamage = 30f
        },
        new WeaponLevel
        {
            lightDamage = 20f,
            heavyDamage = 50f,
            counterDamage = 35f
        }
    };

    [Header("Upgrade Costs")]
    [SerializeField, Min(1)]
    private int level2cost = 1;

    [SerializeField, Min(1)]
    private int level3cost = 29;

    [Header("Current Weapon Level")]
    [SerializeField, Range(1, 3)]
    private int currentLevel = 1;
    public int CurrentLevel => currentLevel;
    private WeaponLevel GetCurrentWeaponLevel()
    {
        if (damageTable == null || damageTable.Length == 0)
        {
            Debug.LogError("Weapon damage table is empty.");
            return null;
        }
        int index = Mathf.Clamp(
            currentLevel - 1,
            0,
            damageTable.Length - 1
        );
        return damageTable[index];
    }
    public float GetLightDamage()
    {
        WeaponLevel level = GetCurrentWeaponLevel();


        if (level == null)
            return 0f;
        return level.lightDamage;
    }
    public float GetHeavyDamage()
    {
        WeaponLevel level = GetCurrentWeaponLevel();


        if (level == null)
            return 0f;
        return level.heavyDamage;
    }
    public float GetCounterDamage()
    {
        WeaponLevel level = GetCurrentWeaponLevel();
        if (level == null)
            return 0f;
        return level.counterDamage;
    }
    public void UpgradeWeapon()
    {
        if(inventory == null)
        {
            Debug.LogError("Inventory reference is missing.");
            return;
        }

        if(damageTable == null || damageTable.Length < 3 || currentLevel < 1 || currentLevel >= 3)
        {
            Debug.Log("Weapon cannot be upgrades further.");
            return;
        }

        int cost = GetUpgradeCost();
        Debug.Log($"Attempting to upgrade weapon. Current Level: {currentLevel}, Upgrade Cost: {cost}, Whetstone Available: {inventory.Whetstone}");

        if(!inventory.SpendWhetstone(cost))
        {
            Debug.Log("Not enough Whetstones to upgrade weapon.");
            return;
        }

        currentLevel++;

        Debug.Log("Weapon upgraded to level " + currentLevel);
        OnWeaponLevelChanged?.Invoke();
    }

    public int GetUpgradeCost()
    {
        if(currentLevel == 1)
        {
            return level2cost;
        }
        if(currentLevel == 2)
        {
            return level3cost;
        }
        else
        {
            return 0; // No further upgrades available
        }
    }
    private void Update()
    {
        if (Keyboard.current !=null && Keyboard.current.uKey.wasPressedThisFrame)
        {
            UpgradeWeapon();
           
        }
    }
}