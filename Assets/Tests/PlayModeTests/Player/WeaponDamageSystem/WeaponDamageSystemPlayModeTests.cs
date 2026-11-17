
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class WeaponDamageSystemPlayModeTests
{
    private Type weaponType;
    private Type inventoryType;

    private MethodInfo upgradeMethod;
    private MethodInfo lightDamageMethod;
    private MethodInfo heavyDamageMethod;
    private MethodInfo counterDamageMethod;
    private MethodInfo addWhetstoneMethod;

    private FieldInfo inventoryField;
    private FieldInfo level2CostField;
    private FieldInfo level3CostField;

    private PropertyInfo currentLevelProperty;
    private PropertyInfo whetstoneProperty;

    private GameObject testPlayer;
    private Component weapon;
    private Component inventory;

    [SetUp]
    public void Setup()
    {
        // Find existing gameplay classes at runtime.
        weaponType = FindType("WeaponDamageSystem");
        inventoryType = FindType("Inventory");

        Assert.That(weaponType, Is.Not.Null,
            "WeaponDamageSystem class was not found.");

        Assert.That(inventoryType, Is.Not.Null,
            "Inventory class was not found.");

        // Find weapon methods.
        upgradeMethod = weaponType.GetMethod("UpgradeWeapon");
        lightDamageMethod = weaponType.GetMethod("GetLightDamage");
        heavyDamageMethod = weaponType.GetMethod("GetHeavyDamage");
        counterDamageMethod = weaponType.GetMethod("GetCounterDamage");

        // Find inventory methods.
        addWhetstoneMethod = inventoryType.GetMethod("AddWhetstone");

        Assert.That(upgradeMethod, Is.Not.Null);
        Assert.That(lightDamageMethod, Is.Not.Null);
        Assert.That(heavyDamageMethod, Is.Not.Null);
        Assert.That(counterDamageMethod, Is.Not.Null);
        Assert.That(addWhetstoneMethod, Is.Not.Null);

        // Find properties.
        currentLevelProperty = weaponType.GetProperty("CurrentLevel");
        whetstoneProperty = inventoryType.GetProperty("Whetstone");

        Assert.That(currentLevelProperty, Is.Not.Null);
        Assert.That(whetstoneProperty, Is.Not.Null);

        // Find private fields.
        inventoryField = weaponType.GetField(
            "inventory",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        level2CostField = weaponType.GetField(
            "level2cost",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        level3CostField = weaponType.GetField(
            "level3cost",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.That(inventoryField, Is.Not.Null);
        Assert.That(level2CostField, Is.Not.Null);
        Assert.That(level3CostField, Is.Not.Null);

        // Create a temporary Player GameObject.
        testPlayer = new GameObject("WeaponUpgradeTestPlayer");

        inventory = testPlayer.AddComponent(inventoryType);
        weapon = testPlayer.AddComponent(weaponType);

        // Connect the inventory.
        inventoryField.SetValue(weapon, inventory);

        // Use known costs for repeatable tests.
        level2CostField.SetValue(weapon, 1);
        level3CostField.SetValue(weapon, 5);
    }

    [TearDown]
    public void Cleanup()
    {
        if (testPlayer != null)
        {
            UnityEngine.Object.DestroyImmediate(testPlayer);
        }
    }

    // Find a class by name across loaded assemblies.
    private Type FindType(string className)
    {
        foreach (Assembly assembly in
                 AppDomain.CurrentDomain.GetAssemblies())
        {
            Type found = assembly.GetType(className);

            if (found != null)
                return found;
        }

        return null;
    }

    // Read the current weapon level.
    private int GetLevel()
    {
        return (int)currentLevelProperty.GetValue(weapon);
    }

    // Read the current Whetstone amount.
    private int GetWhetstones()
    {
        return (int)whetstoneProperty.GetValue(inventory);
    }

    // Add materials through the real Inventory method.
    private void AddWhetstones(int amount)
    {
        addWhetstoneMethod.Invoke(
            inventory,
            new object[] { amount }
        );
    }

    // Call the real UpgradeWeapon method.
    private void Upgrade()
    {
        upgradeMethod.Invoke(weapon, null);
    }

    // Retrieve damage values.
    private float GetDamage(MethodInfo method)
    {
        return (float)method.Invoke(weapon, null);
    }

    [Test]
    public void WT01_WeaponStartsAtLevelOne()
    {
        Assert.That(GetLevel(), Is.EqualTo(1));

        Debug.Log("WT-01 PASS: Weapon starts at level 1.");
    }

    [Test]
    public void WT02_LevelOneDamageIsCorrect()
    {
        Assert.That(GetDamage(lightDamageMethod), Is.EqualTo(10f));
        Assert.That(GetDamage(heavyDamageMethod), Is.EqualTo(40f));
        Assert.That(GetDamage(counterDamageMethod), Is.EqualTo(25f));

        Debug.Log("WT-02 PASS: Level 1 damage is correct.");
    }

    [Test]
    public void WT03_UpgradeFailsWithoutMaterials()
    {
        Upgrade();

        Assert.That(GetLevel(), Is.EqualTo(1));
        Assert.That(GetWhetstones(), Is.EqualTo(0));

        Debug.Log("WT-03 PASS: Upgrade rejected without materials.");
    }

    [Test]
    public void WT04_UpgradeToLevelTwo()
    {
        AddWhetstones(1);

        Upgrade();

        Assert.That(GetLevel(), Is.EqualTo(2));
        Assert.That(GetWhetstones(), Is.EqualTo(0));

        Debug.Log("WT-04 PASS: Weapon upgraded to level 2.");
    }

    [Test]
    public void WT05_LevelTwoDamageIsCorrect()
    {
        AddWhetstones(1);

        Upgrade();

        Assert.That(GetDamage(lightDamageMethod), Is.EqualTo(15f));
        Assert.That(GetDamage(heavyDamageMethod), Is.EqualTo(45f));
        Assert.That(GetDamage(counterDamageMethod), Is.EqualTo(30f));

        Debug.Log("WT-05 PASS: Level 2 damage is correct.");
    }

    [Test]
    public void WT06_UpgradeToLevelThree()
    {
        // Level 2 costs 1, level 3 costs 5.
        AddWhetstones(6);

        Upgrade();
        Upgrade();

        Assert.That(GetLevel(), Is.EqualTo(3));
        Assert.That(GetWhetstones(), Is.EqualTo(0));

        Assert.That(GetDamage(lightDamageMethod), Is.EqualTo(20f));
        Assert.That(GetDamage(heavyDamageMethod), Is.EqualTo(50f));
        Assert.That(GetDamage(counterDamageMethod), Is.EqualTo(35f));

        Debug.Log("WT-06 PASS: Weapon upgraded to level 3.");
    }

    [Test]
    public void WT07_CannotUpgradeBeyondLevelThree()
    {
        AddWhetstones(20);

        Upgrade();
        Upgrade();

        int materialsBefore = GetWhetstones();

        Upgrade();

        Assert.That(GetLevel(), Is.EqualTo(3));
        Assert.That(GetWhetstones(), Is.EqualTo(materialsBefore));

        Debug.Log("WT-07 PASS: Maximum weapon level enforced.");
    }

    [Test]
    public void WT08_InsufficientMaterialsAreNotSpent()
    {
        AddWhetstones(1);

        Upgrade();

        AddWhetstones(2);

        Upgrade();

        Assert.That(GetLevel(), Is.EqualTo(2));
        Assert.That(GetWhetstones(), Is.EqualTo(2));

        Debug.Log("WT-08 PASS: Insufficient materials preserved.");
    }
}
