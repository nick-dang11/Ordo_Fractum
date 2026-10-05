using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class HitboxLifecycleTests
{
    private const float FirstAttackDamage = 10f;
    private const float SecondAttackDamage = 25f;

    private Type weaponHitboxType;
    private Type playerWeaponHitboxType;
    private Type enemyWeaponHitboxType;

    private MethodInfo enableHitboxMethod;
    private MethodInfo disableHitboxMethod;
    private MethodInfo onTriggerEnterMethod;

    private FieldInfo isHitboxActiveField;
    private FieldInfo currentDamageField;
    private FieldInfo hitTargetsField;

    private GameObject playerWeaponObject;
    private GameObject enemyWeaponObject;

    private Component playerWeaponHitbox;
    private Component enemyWeaponHitbox;

    private Collider playerWeaponCollider;
    private Collider enemyWeaponCollider;

    [SetUp]
    public void SetUp()
    {
        weaponHitboxType =
            Type.GetType("WeaponHitbox, Assembly-CSharp");

        playerWeaponHitboxType =
            Type.GetType("PlayerWeaponHitbox, Assembly-CSharp");

        enemyWeaponHitboxType =
            Type.GetType("EnemyWeaponHitbox, Assembly-CSharp");

        Assert.IsNotNull(
            weaponHitboxType,
            "Could not find WeaponHitbox in Assembly-CSharp."
        );

        Assert.IsNotNull(
            playerWeaponHitboxType,
            "Could not find PlayerWeaponHitbox in Assembly-CSharp."
        );

        Assert.IsNotNull(
            enemyWeaponHitboxType,
            "Could not find EnemyWeaponHitbox in Assembly-CSharp."
        );

        enableHitboxMethod =
            weaponHitboxType.GetMethod(
                "EnableHitbox",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        disableHitboxMethod =
            weaponHitboxType.GetMethod(
                "DisableHitbox",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        onTriggerEnterMethod =
            weaponHitboxType.GetMethod(
                "OnTriggerEnter",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        isHitboxActiveField =
            weaponHitboxType.GetField(
                "isHitboxActive",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        currentDamageField =
            weaponHitboxType.GetField(
                "currentDamage",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        hitTargetsField =
            weaponHitboxType.GetField(
                "hitTargets",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        Assert.IsNotNull(enableHitboxMethod);
        Assert.IsNotNull(disableHitboxMethod);
        Assert.IsNotNull(onTriggerEnterMethod);

        Assert.IsNotNull(isHitboxActiveField);
        Assert.IsNotNull(currentDamageField);
        Assert.IsNotNull(hitTargetsField);

        CreateHitbox(
            "HitboxLifecycle_PlayerWeapon",
            playerWeaponHitboxType,
            out playerWeaponObject,
            out playerWeaponCollider,
            out playerWeaponHitbox
        );

        CreateHitbox(
            "HitboxLifecycle_EnemyWeapon",
            enemyWeaponHitboxType,
            out enemyWeaponObject,
            out enemyWeaponCollider,
            out enemyWeaponHitbox
        );
    }

    [TearDown]
    public void TearDown()
    {
        if (playerWeaponObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                playerWeaponObject
            );
        }

        if (enemyWeaponObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                enemyWeaponObject
            );
        }
    }

    // =========================================================
    // PLAYER WEAPON HITBOX
    // =========================================================

    [UnityTest]
    public IEnumerator PlayerHitbox_Activation_EnablesColliderAndStoresDamage()
    {
        yield return null;

        EnableHitbox(
            playerWeaponHitbox,
            FirstAttackDamage
        );

        Assert.IsTrue(
            playerWeaponCollider.enabled,
            "Player hitbox collider should be enabled during an active attack."
        );

        Assert.IsTrue(
            IsHitboxActive(playerWeaponHitbox),
            "Player hitbox should report itself as active."
        );

        Assert.AreEqual(
            FirstAttackDamage,
            GetCurrentDamage(playerWeaponHitbox),
            "Player hitbox should store the damage supplied for the current attack."
        );
    }

    [UnityTest]
    public IEnumerator PlayerHitbox_Shutdown_DisablesColliderAndClearsActiveState()
    {
        yield return null;

        EnableHitbox(
            playerWeaponHitbox,
            FirstAttackDamage
        );

        DisableHitbox(
            playerWeaponHitbox
        );

        Assert.IsFalse(
            playerWeaponCollider.enabled,
            "Player hitbox collider should be disabled after shutdown."
        );

        Assert.IsFalse(
            IsHitboxActive(playerWeaponHitbox),
            "Player hitbox should no longer be active after shutdown."
        );

        Assert.AreEqual(
            0,
            GetTrackedTargetCount(playerWeaponHitbox),
            "Player hitbox should clear its tracked targets when closed."
        );
    }

    [UnityTest]
    public IEnumerator PlayerHitbox_Interruption_DisablingComponentForceClosesHitbox()
    {
        yield return null;

        EnableHitbox(
            playerWeaponHitbox,
            FirstAttackDamage
        );

        Assert.IsTrue(
            playerWeaponCollider.enabled
        );

        ((Behaviour)playerWeaponHitbox).enabled = false;

        yield return null;

        Assert.IsFalse(
            playerWeaponCollider.enabled,
            "Disabling PlayerWeaponHitbox should force the collider closed."
        );

        Assert.IsFalse(
            IsHitboxActive(playerWeaponHitbox),
            "Disabling PlayerWeaponHitbox should clear the active state."
        );

        Assert.AreEqual(
            0,
            GetTrackedTargetCount(playerWeaponHitbox),
            "Interrupted player attacks should not retain targets from the cancelled window."
        );
    }

    [UnityTest]
    public IEnumerator PlayerHitbox_RepeatedAttacks_ReopensWithNewDamage()
    {
        yield return null;

        EnableHitbox(
            playerWeaponHitbox,
            FirstAttackDamage
        );

        DisableHitbox(
            playerWeaponHitbox
        );

        EnableHitbox(
            playerWeaponHitbox,
            SecondAttackDamage
        );

        Assert.IsTrue(
            playerWeaponCollider.enabled,
            "A later player attack should be able to reopen the hitbox."
        );

        Assert.IsTrue(
            IsHitboxActive(playerWeaponHitbox)
        );

        Assert.AreEqual(
            SecondAttackDamage,
            GetCurrentDamage(playerWeaponHitbox),
            "A later player attack should replace the previous damage snapshot."
        );

        Assert.AreEqual(
            0,
            GetTrackedTargetCount(playerWeaponHitbox),
            "A new player attack should begin with no targets already consumed."
        );
    }

    [UnityTest]
    public IEnumerator PlayerHitbox_DuplicateTarget_IsIgnoredWithinSameWindow()
    {
        yield return null;

        GameObject targetRoot =
            new GameObject(
                "HitboxLifecycle_PlayerDuplicateTarget"
            );

        GameObject targetColliderObject =
            new GameObject(
                "BodyCollider"
            );

        targetColliderObject.transform.SetParent(
            targetRoot.transform
        );

        Collider targetCollider =
            targetColliderObject.AddComponent<BoxCollider>();

        try
        {
            EnableHitbox(
                playerWeaponHitbox,
                FirstAttackDamage
            );

            AddTrackedTarget(
                playerWeaponHitbox,
                targetRoot
            );

            Assert.AreEqual(
                1,
                GetTrackedTargetCount(playerWeaponHitbox)
            );

            InvokeTriggerEnter(
                playerWeaponHitbox,
                targetCollider
            );

            Assert.AreEqual(
                1,
                GetTrackedTargetCount(playerWeaponHitbox),
                "Player hitbox should reject a target root that was already hit during the same window."
            );
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                targetRoot
            );
        }
    }

    // =========================================================
    // ENEMY WEAPON HITBOX
    // =========================================================

    [UnityTest]
    public IEnumerator EnemyHitbox_Activation_EnablesColliderAndStoresDamage()
    {
        yield return null;

        EnableHitbox(
            enemyWeaponHitbox,
            FirstAttackDamage
        );

        Assert.IsTrue(
            enemyWeaponCollider.enabled,
            "Enemy hitbox collider should be enabled during an active attack."
        );

        Assert.IsTrue(
            IsHitboxActive(enemyWeaponHitbox),
            "Enemy hitbox should report itself as active."
        );

        Assert.AreEqual(
            FirstAttackDamage,
            GetCurrentDamage(enemyWeaponHitbox),
            "Enemy hitbox should store the damage supplied for the current attack."
        );
    }

    [UnityTest]
    public IEnumerator EnemyHitbox_Shutdown_DisablesColliderAndClearsActiveState()
    {
        yield return null;

        EnableHitbox(
            enemyWeaponHitbox,
            FirstAttackDamage
        );

        DisableHitbox(
            enemyWeaponHitbox
        );

        Assert.IsFalse(
            enemyWeaponCollider.enabled,
            "Enemy hitbox collider should be disabled after shutdown."
        );

        Assert.IsFalse(
            IsHitboxActive(enemyWeaponHitbox),
            "Enemy hitbox should no longer be active after shutdown."
        );

        Assert.AreEqual(
            0,
            GetTrackedTargetCount(enemyWeaponHitbox),
            "Enemy hitbox should clear its tracked targets when closed."
        );
    }

    [UnityTest]
    public IEnumerator EnemyHitbox_Interruption_DisablingComponentForceClosesHitbox()
    {
        yield return null;

        EnableHitbox(
            enemyWeaponHitbox,
            FirstAttackDamage
        );

        Assert.IsTrue(
            enemyWeaponCollider.enabled
        );

        ((Behaviour)enemyWeaponHitbox).enabled = false;

        yield return null;

        Assert.IsFalse(
            enemyWeaponCollider.enabled,
            "Disabling EnemyWeaponHitbox should force the collider closed."
        );

        Assert.IsFalse(
            IsHitboxActive(enemyWeaponHitbox),
            "Disabling EnemyWeaponHitbox should clear the active state."
        );

        Assert.AreEqual(
            0,
            GetTrackedTargetCount(enemyWeaponHitbox),
            "Interrupted enemy attacks should not retain targets from the cancelled window."
        );
    }

    [UnityTest]
    public IEnumerator EnemyHitbox_RepeatedAttacks_ReopensWithNewDamage()
    {
        yield return null;

        EnableHitbox(
            enemyWeaponHitbox,
            FirstAttackDamage
        );

        DisableHitbox(
            enemyWeaponHitbox
        );

        EnableHitbox(
            enemyWeaponHitbox,
            SecondAttackDamage
        );

        Assert.IsTrue(
            enemyWeaponCollider.enabled,
            "A later enemy attack should be able to reopen the hitbox."
        );

        Assert.IsTrue(
            IsHitboxActive(enemyWeaponHitbox)
        );

        Assert.AreEqual(
            SecondAttackDamage,
            GetCurrentDamage(enemyWeaponHitbox),
            "A later enemy attack should replace the previous damage snapshot."
        );

        Assert.AreEqual(
            0,
            GetTrackedTargetCount(enemyWeaponHitbox),
            "A new enemy attack should begin with no targets already consumed."
        );
    }

    [UnityTest]
    public IEnumerator EnemyHitbox_DuplicateTarget_IsIgnoredWithinSameWindow()
    {
        yield return null;

        GameObject targetRoot =
            new GameObject(
                "HitboxLifecycle_EnemyDuplicateTarget"
            );

        GameObject targetColliderObject =
            new GameObject(
                "BodyCollider"
            );

        targetColliderObject.transform.SetParent(
            targetRoot.transform
        );

        Collider targetCollider =
            targetColliderObject.AddComponent<BoxCollider>();

        try
        {
            EnableHitbox(
                enemyWeaponHitbox,
                FirstAttackDamage
            );

            AddTrackedTarget(
                enemyWeaponHitbox,
                targetRoot
            );

            Assert.AreEqual(
                1,
                GetTrackedTargetCount(enemyWeaponHitbox)
            );

            InvokeTriggerEnter(
                enemyWeaponHitbox,
                targetCollider
            );

            Assert.AreEqual(
                1,
                GetTrackedTargetCount(enemyWeaponHitbox),
                "Enemy hitbox should reject a target root that was already hit during the same window."
            );
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                targetRoot
            );
        }
    }

    // =========================================================
    // SHARED HELPERS
    // =========================================================

    private static void CreateHitbox(
        string objectName,
        Type concreteHitboxType,
        out GameObject hitboxObject,
        out Collider hitboxCollider,
        out Component hitboxComponent)
    {
        hitboxObject =
            new GameObject(
                objectName
            );

        hitboxCollider =
            hitboxObject.AddComponent<BoxCollider>();

        hitboxCollider.isTrigger = true;

        hitboxComponent =
            hitboxObject.AddComponent(
                concreteHitboxType
            );

        Assert.IsNotNull(
            hitboxComponent
        );
    }

    private void EnableHitbox(
        Component hitbox,
        float damage)
    {
        enableHitboxMethod.Invoke(
            hitbox,
            new object[]
            {
                damage
            }
        );
    }

    private void DisableHitbox(
        Component hitbox)
    {
        disableHitboxMethod.Invoke(
            hitbox,
            null
        );
    }

    private void InvokeTriggerEnter(
        Component hitbox,
        Collider targetCollider)
    {
        onTriggerEnterMethod.Invoke(
            hitbox,
            new object[]
            {
                targetCollider
            }
        );
    }

    private bool IsHitboxActive(
        Component hitbox)
    {
        return
            (bool)isHitboxActiveField.GetValue(
                hitbox
            );
    }

    private float GetCurrentDamage(
        Component hitbox)
    {
        return
            (float)currentDamageField.GetValue(
                hitbox
            );
    }

    private int GetTrackedTargetCount(
        Component hitbox)
    {
        object collection =
            hitTargetsField.GetValue(
                hitbox
            );

        PropertyInfo countProperty =
            collection.GetType().GetProperty(
                "Count"
            );

        Assert.IsNotNull(
            countProperty
        );

        return
            (int)countProperty.GetValue(
                collection
            );
    }

    private void AddTrackedTarget(
        Component hitbox,
        GameObject target)
    {
        object collection =
            hitTargetsField.GetValue(
                hitbox
            );

        MethodInfo addMethod =
            collection.GetType().GetMethod(
                "Add"
            );

        Assert.IsNotNull(
            addMethod
        );

        addMethod.Invoke(
            collection,
            new object[]
            {
                target
            }
        );
    }
}
