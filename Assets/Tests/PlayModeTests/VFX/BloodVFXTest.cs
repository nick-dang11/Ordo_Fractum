using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BloodVFXTests
{
    private GameObject weaponObject;
    private GameObject targetObject;
    private GameObject bloodPrefab;

    private Collider weaponCollider;
    private Collider targetCollider;

    private Component weaponHitbox;

    private Type playerWeaponHitboxType;
    private Type weaponHitboxBaseType;

    private FieldInfo bloodPrefabField;
    private FieldInfo bloodLifetimeField;

    private MethodInfo spawnBloodMethod;
    private MethodInfo enableHitboxMethod;

    [SetUp]
    public void SetUp()
    {
        // ---------------------------------------------------------
        // Create a basic weapon.
        // ---------------------------------------------------------

        weaponObject = new GameObject("BloodVFX_TestWeapon");

        weaponCollider = weaponObject.AddComponent<BoxCollider>();

        weaponObject.transform.position = Vector3.zero;

        // Get PlayerWeaponHitbox without requiring a compile-time
        // assembly reference from the test assembly.
        playerWeaponHitboxType =
            Type.GetType("PlayerWeaponHitbox, Assembly-CSharp");

        Assert.IsNotNull(
            playerWeaponHitboxType,
            "Could not find PlayerWeaponHitbox in Assembly-CSharp."
        );

        weaponHitboxBaseType = playerWeaponHitboxType.BaseType;

        Assert.IsNotNull(
            weaponHitboxBaseType,
            "Could not find WeaponHitbox base class."
        );

        weaponHitbox =
            weaponObject.AddComponent(playerWeaponHitboxType);

        // ---------------------------------------------------------
        // Create a target collider.
        // ---------------------------------------------------------

        targetObject = new GameObject("BloodVFX_TestTarget");

        targetObject.transform.position =
            new Vector3(2f, 0f, 0f);

        targetCollider =
            targetObject.AddComponent<BoxCollider>();

        // ---------------------------------------------------------
        // Create a fake Blood VFX prefab.
        //
        // We do not need a real ParticleSystem for this test.
        // We only need to know whether SpawnBloodVFX instantiated
        // the configured prefab correctly.
        // ---------------------------------------------------------

        bloodPrefab =
            new GameObject("BloodVFX_TestPrefab");

        // ---------------------------------------------------------
        // Obtain the private serialized configuration fields.
        // ---------------------------------------------------------

        bloodPrefabField =
            weaponHitboxBaseType.GetField(
                "bloodVFXPrefab",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        bloodLifetimeField =
            weaponHitboxBaseType.GetField(
                "bloodVFXLifetime",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        Assert.IsNotNull(bloodPrefabField);
        Assert.IsNotNull(bloodLifetimeField);

        bloodPrefabField.SetValue(
            weaponHitbox,
            bloodPrefab
        );

        // Short lifetime keeps the cleanup test fast.
        bloodLifetimeField.SetValue(
            weaponHitbox,
            0.05f
        );

        // ---------------------------------------------------------
        // Obtain SpawnBloodVFX().
        // ---------------------------------------------------------

        spawnBloodMethod =
            weaponHitboxBaseType.GetMethod(
                "SpawnBloodVFX",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        Assert.IsNotNull(
            spawnBloodMethod,
            "Could not find SpawnBloodVFX."
        );

        // ---------------------------------------------------------
        // Enable the weapon collider.
        //
        // SpawnBloodVFX uses weaponCollider.bounds.center when
        // calculating the impact position.
        // ---------------------------------------------------------

        enableHitboxMethod =
            weaponHitboxBaseType.GetMethod(
                "EnableHitbox",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        Assert.IsNotNull(enableHitboxMethod);

        enableHitboxMethod.Invoke(
            weaponHitbox,
            new object[] { 1f }
        );
    }

    [TearDown]
    public void TearDown()
    {
        GameObject spawnedBlood =
            GameObject.Find(
                "BloodVFX_TestPrefab(Clone)"
            );

        if (spawnedBlood != null)
        {
            UnityEngine.Object.DestroyImmediate(
                spawnedBlood
            );
        }

        if (weaponObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                weaponObject
            );
        }

        if (targetObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                targetObject
            );
        }

        if (bloodPrefab != null)
        {
            UnityEngine.Object.DestroyImmediate(
                bloodPrefab
            );
        }
    }


    [Test]
    public void SpawnBloodVFX_CreatesBloodObject()
    {
        spawnBloodMethod.Invoke(
            weaponHitbox,
            new object[] { targetCollider }
        );

        GameObject spawnedBlood =
            GameObject.Find("BloodVFX_TestPrefab(Clone)");

        Assert.IsNotNull(
            spawnedBlood,
            "SpawnBloodVFX should create an instance of the configured prefab."
        );
    }

    [Test]
    public void SpawnBloodVFX_SpawnsAtImpactPoint()
    {
        Physics.SyncTransforms();

        Vector3 expectedHitPoint =
            targetCollider.ClosestPoint(
                weaponCollider.bounds.center
            );

        spawnBloodMethod.Invoke(
            weaponHitbox,
            new object[] { targetCollider }
        );

        GameObject spawnedBlood =
            GameObject.Find("BloodVFX_TestPrefab(Clone)");

        Assert.IsNotNull(
            spawnedBlood,
            "Blood VFX should exist immediately after spawning."
        );

        float distance = Vector3.Distance(
            spawnedBlood.transform.position,
            expectedHitPoint
        );

        Assert.That(
            distance,
            Is.LessThan(0.001f),
            $"Blood VFX position mismatch. " +
            $"Expected={expectedHitPoint}, " +
            $"Actual={spawnedBlood.transform.position}, " +
            $"Distance={distance:F6}"
        );
    }



    [UnityTest]
    public IEnumerator SpawnBloodVFX_IsDestroyedAfterLifetime()
    {
        spawnBloodMethod.Invoke(
            weaponHitbox,
            new object[] { targetCollider }
        );

        yield return null;

        GameObject spawnedBlood =
            GameObject.Find(
                "BloodVFX_TestPrefab(Clone)"
            );

        Assert.IsNotNull(
            spawnedBlood,
            "Blood VFX should exist immediately after spawning."
        );

        // Lifetime configured above is 0.05 seconds.
        yield return new WaitForSeconds(0.10f);
        yield return null;

        spawnedBlood =
            GameObject.Find(
                "BloodVFX_TestPrefab(Clone)"
            );

        Assert.IsNull(
            spawnedBlood,
            "Blood VFX should be destroyed after bloodVFXLifetime."
        );
    }

    [UnityTest]
    public IEnumerator SpawnBloodVFX_WithNoPrefab_DoesNothing()
    {
        bloodPrefabField.SetValue(
            weaponHitbox,
            null
        );

        spawnBloodMethod.Invoke(
            weaponHitbox,
            new object[] { targetCollider }
        );

        yield return null;

        GameObject spawnedBlood =
            GameObject.Find(
                "BloodVFX_TestPrefab(Clone)"
            );

        Assert.IsNull(
            spawnedBlood,
            "No Blood VFX should spawn when no prefab is configured."
        );
    }
}