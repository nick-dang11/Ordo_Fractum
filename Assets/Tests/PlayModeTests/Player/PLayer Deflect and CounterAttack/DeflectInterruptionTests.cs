using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DeflectInterruptionTests
{
    private readonly List<GameObject> createdObjects =
        new List<GameObject>();

    private EnemyFixture primaryEnemy;

    private class EnemyFixture
    {
        public GameObject Root;
        public Component Combat;

        public GameObject WeaponObject;
        public Component Hitbox;
        public Collider WeaponCollider;
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        primaryEnemy =
            CreateEnemy("Sprint3_DeflectTest_EnemyA");

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (GameObject obj in createdObjects)
        {
            if (obj != null)
            {
                UnityEngine.Object.Destroy(obj);
            }
        }

        createdObjects.Clear();

        yield return null;
    }

    // ---------------------------------------------------------
    // 3.1 - ATTACKER IDENTIFICATION
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator DeflectContext_IdentifiesCorrectAttackingEnemy()
    {
        EnemyFixture enemyB =
            CreateEnemy("Sprint3_DeflectTest_EnemyB");

        GameObject player =
            new GameObject("Sprint3_DeflectTest_Player");

        createdObjects.Add(player);

        yield return null;

        Type contextType =
            FindType("CombatHitContext");

        Assert.IsNotNull(
            contextType,
            "CombatHitContext type could not be found."
        );

        object context =
            Activator.CreateInstance(
                contextType,
                primaryEnemy.Root,
                player,
                primaryEnemy.WeaponCollider,
                null,
                10f,
                0f,
                Vector3.zero
            );

        GameObject attackerRoot =
            GetProperty<GameObject>(
                context,
                "AttackerRoot"
            );

        Collider weaponCollider =
            GetProperty<Collider>(
                context,
                "WeaponCollider"
            );

        Assert.AreSame(
            primaryEnemy.Root,
            attackerRoot,
            "CombatHitContext should identify Enemy A as the attacker."
        );

        Assert.AreNotSame(
            enemyB.Root,
            attackerRoot,
            "CombatHitContext must not identify Enemy B as the attacker."
        );

        Assert.AreSame(
            primaryEnemy.WeaponCollider,
            weaponCollider,
            "CombatHitContext should preserve the attacking weapon collider."
        );

        Component attackingCombat =
            attackerRoot.GetComponent(
                FindType("EnemyCombat")
            );

        Assert.IsNotNull(
            attackingCombat,
            "The attacker stored in CombatHitContext should have EnemyCombat."
        );

        InvokeMethod(
            attackingCombat,
            "HandleDeflected"
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                primaryEnemy.Combat,
                "CurrentAttackInvalidated"
            ),
            "The enemy identified by the context should be invalidated."
        );

        Assert.IsFalse(
            GetProperty<bool>(
                enemyB.Combat,
                "CurrentAttackInvalidated"
            ),
            "Deflecting Enemy A must not invalidate Enemy B."
        );
    }

    // ---------------------------------------------------------
    // 3.2 - ATTACK INTERRUPTION
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator Deflect_InvalidatesCurrentEnemyAttack()
    {
        Assert.IsFalse(
            GetProperty<bool>(
                primaryEnemy.Combat,
                "CurrentAttackInvalidated"
            ),
            "A newly created enemy should not start invalidated."
        );

        InvokeMethod(
            primaryEnemy.Combat,
            "HandleDeflected"
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                primaryEnemy.Combat,
                "CurrentAttackInvalidated"
            ),
            "A successful deflect should invalidate the current attack."
        );
    }

    [UnityTest]
    public IEnumerator Deflect_InterruptsCurrentEnemyAttack()
    {
        SetPrivateField(
            primaryEnemy.Combat,
            "isAttacking",
            true
        );

        Assert.IsTrue(
            GetProperty<bool>(
                primaryEnemy.Combat,
                "IsAttacking"
            ),
            "Test setup failed to place the enemy in an attacking state."
        );

        InvokeMethod(
            primaryEnemy.Combat,
            "HandleDeflected"
        );

        yield return null;

        Assert.IsFalse(
            GetProperty<bool>(
                primaryEnemy.Combat,
                "IsAttacking"
            ),
            "A deflect should interrupt the current enemy attack."
        );

        Assert.IsTrue(
            GetProperty<bool>(
                primaryEnemy.Combat,
                "CurrentAttackInvalidated"
            ),
            "The interrupted attack should remain invalidated."
        );
    }

    [UnityTest]
    public IEnumerator Deflect_DisablesEnemyWeaponHitbox()
    {
        GameObject ownerRoot =
            GetProperty<GameObject>(
                primaryEnemy.Hitbox,
                "OwnerRoot"
            );

        Assert.AreSame(
            primaryEnemy.Root,
            ownerRoot,
            "Enemy weapon should resolve EnemyCombat root as its owner."
        );

        InvokeMethod(
            primaryEnemy.Hitbox,
            "EnableHitbox",
            10f
        );

        Assert.IsTrue(
            primaryEnemy.WeaponCollider.enabled,
            "Test setup failed to enable the enemy weapon collider."
        );

        InvokeMethod(
            primaryEnemy.Combat,
            "HandleDeflected"
        );

        yield return null;

        Assert.IsFalse(
            primaryEnemy.WeaponCollider.enabled,
            "A deflect should immediately disable the enemy weapon collider."
        );
    }

    [UnityTest]
    public IEnumerator Deflect_InvalidatedAttackCannotReactivateWeaponHitbox()
    {
        GameObject ownerRoot =
            GetProperty<GameObject>(
                primaryEnemy.Hitbox,
                "OwnerRoot"
            );

        Assert.AreSame(
            primaryEnemy.Root,
            ownerRoot,
            "Enemy weapon should resolve the correct owner."
        );

        // Initial attack activation.
        InvokeMethod(
            primaryEnemy.Hitbox,
            "EnableHitbox",
            10f
        );

        Assert.IsTrue(
            primaryEnemy.WeaponCollider.enabled,
            "Weapon collider should initially become active."
        );

        // Deflect the active attack.
        InvokeMethod(
            primaryEnemy.Combat,
            "HandleDeflected"
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                primaryEnemy.Combat,
                "CurrentAttackInvalidated"
            ),
            "Deflected attack should be marked invalid."
        );

        Assert.IsFalse(
            primaryEnemy.WeaponCollider.enabled,
            "Deflect should disable the active weapon."
        );

        // Simulate a later animation event from the same swing
        // attempting to enable the weapon again.
        InvokeMethod(
            primaryEnemy.Hitbox,
            "EnableHitbox",
            10f
        );

        yield return null;

        Assert.IsFalse(
            primaryEnemy.WeaponCollider.enabled,
            "An invalidated attack must not reactivate its weapon hitbox."
        );
    }

    [UnityTest]
    public IEnumerator Deflect_DoesNotInvalidateDifferentEnemy()
    {
        EnemyFixture enemyB =
            CreateEnemy("Sprint3_DeflectTest_EnemyB");

        yield return null;

        SetPrivateField(
            primaryEnemy.Combat,
            "isAttacking",
            true
        );

        SetPrivateField(
            enemyB.Combat,
            "isAttacking",
            true
        );

        InvokeMethod(
            primaryEnemy.Combat,
            "HandleDeflected"
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                primaryEnemy.Combat,
                "CurrentAttackInvalidated"
            ),
            "Enemy A's attack should be invalidated."
        );

        Assert.IsFalse(
            GetProperty<bool>(
                enemyB.Combat,
                "CurrentAttackInvalidated"
            ),
            "Deflecting Enemy A must not invalidate Enemy B."
        );

        Assert.IsFalse(
            GetProperty<bool>(
                primaryEnemy.Combat,
                "IsAttacking"
            ),
            "Enemy A should no longer be attacking."
        );

        Assert.IsTrue(
            GetProperty<bool>(
                enemyB.Combat,
                "IsAttacking"
            ),
            "Enemy B should remain in its existing attack state."
        );
    }

    // ---------------------------------------------------------
    // FIXTURE CREATION
    // ---------------------------------------------------------

    private EnemyFixture CreateEnemy(string enemyName)
    {
        Type enemyCombatType =
            FindType("EnemyCombat");

        Type enemyWeaponHitboxType =
            FindType("EnemyWeaponHitbox");

        Assert.IsNotNull(
            enemyCombatType,
            "EnemyCombat type could not be found."
        );

        Assert.IsNotNull(
            enemyWeaponHitboxType,
            "EnemyWeaponHitbox type could not be found."
        );

        GameObject enemyRoot =
            new GameObject(enemyName);

        createdObjects.Add(enemyRoot);

        Component enemyCombat =
            enemyRoot.AddComponent(
                enemyCombatType
            );

        GameObject weaponObject =
            new GameObject(
                $"{enemyName}_Weapon"
            );

        weaponObject.transform.SetParent(
            enemyRoot.transform
        );

        BoxCollider weaponCollider =
            weaponObject.AddComponent<BoxCollider>();

        weaponCollider.isTrigger = true;

        Component weaponHitbox =
            weaponObject.AddComponent(
                enemyWeaponHitboxType
            );

        SetPrivateField(
            enemyCombat,
            "weaponHitbox",
            weaponHitbox
        );

        return new EnemyFixture
        {
            Root = enemyRoot,
            Combat = enemyCombat,
            WeaponObject = weaponObject,
            Hitbox = weaponHitbox,
            WeaponCollider = weaponCollider
        };
    }

    // ---------------------------------------------------------
    // REFLECTION HELPERS
    // ---------------------------------------------------------

    private static Type FindType(string typeName)
    {
        foreach (
            Assembly assembly
            in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type =
                assembly.GetType(typeName);

            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    private static void SetPrivateField(
        object target,
        string fieldName,
        object value)
    {
        Assert.IsNotNull(
            target,
            $"Cannot set '{fieldName}' because target is null."
        );

        FieldInfo field =
            target.GetType().GetField(
                fieldName,
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        Assert.IsNotNull(
            field,
            $"Could not find private field '{fieldName}' " +
            $"on {target.GetType().Name}."
        );

        field.SetValue(
            target,
            value
        );
    }

    private static T GetProperty<T>(
        object target,
        string propertyName)
    {
        Assert.IsNotNull(
            target,
            $"Cannot read '{propertyName}' because target is null."
        );

        PropertyInfo property =
            target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        Assert.IsNotNull(
            property,
            $"Could not find property '{propertyName}' " +
            $"on {target.GetType().Name}."
        );

        object value =
            property.GetValue(target);

        return (T)value;
    }

    private static object InvokeMethod(
        object target,
        string methodName,
        params object[] args)
    {
        Assert.IsNotNull(
            target,
            $"Cannot invoke '{methodName}' because target is null."
        );

        MethodInfo method =
            FindMatchingMethod(
                target.GetType(),
                methodName,
                args
            );

        Assert.IsNotNull(
            method,
            $"Could not find method '{methodName}' " +
            $"with {args.Length} parameter(s) on " +
            $"{target.GetType().Name}."
        );

        return method.Invoke(
            target,
            args
        );
    }

    private static MethodInfo FindMatchingMethod(
        Type type,
        string methodName,
        object[] args)
    {
        MethodInfo[] methods =
            type.GetMethods(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        foreach (MethodInfo method in methods)
        {
            if (method.Name != methodName)
            {
                continue;
            }

            ParameterInfo[] parameters =
                method.GetParameters();

            if (parameters.Length != args.Length)
            {
                continue;
            }

            bool matches = true;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (args[i] == null)
                {
                    continue;
                }

                if (!parameters[i]
                    .ParameterType
                    .IsAssignableFrom(
                        args[i].GetType()))
                {
                    // Allow numeric arguments such as float.
                    if (!IsNumericCompatible(
                        parameters[i].ParameterType,
                        args[i].GetType()))
                    {
                        matches = false;
                        break;
                    }
                }
            }

            if (matches)
            {
                return method;
            }
        }

        return null;
    }

    private static bool IsNumericCompatible(
        Type parameterType,
        Type argumentType)
    {
        return
            parameterType == typeof(float) &&
            argumentType == typeof(float);
    }
}