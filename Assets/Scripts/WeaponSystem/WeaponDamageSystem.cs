using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.InputSystem;


public class WeaponDamageSystem : MonoBehaviour
{
    public event Action OnWeaponLevelChanged;
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
        if (damageTable == null || damageTable.Length == 0)
            return;
        if (currentLevel < 3 &&
            currentLevel < damageTable.Length)
        {
            currentLevel++;
            Debug.Log("Weapon upgraded to level " + currentLevel);
            OnWeaponLevelChanged?.Invoke();
        }
    }
    private void Update()
    {
        if (Keyboard.current !=null && Keyboard.current.uKey.wasPressedThisFrame)
        {
            UpgradeWeapon();
            Debug.Log("Weapon upgraded to level " + currentLevel);
        }
    }
}