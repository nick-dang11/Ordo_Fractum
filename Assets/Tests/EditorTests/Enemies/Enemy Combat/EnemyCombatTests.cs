using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class EnemyCombatTests
{
    private GameObject enemyObject;

    private Component enemyCombat;
    private Type enemyCombatType;

    private MethodInfo getAttackDamageMethod;
    private MethodInfo setEnemyDamageMethod;
    private MethodInfo tryAttackMethod;

    private PropertyInfo canAttackProperty;

    private FieldInfo isAttackingField;

    [SetUp]
    public void SetUp()
    {
        enemyObject =
            new GameObject("EnemyCombat_TestEnemy");

        enemyCombatType =
            Type.GetType("EnemyCombat, Assembly-CSharp");

        Assert.IsNotNull(
            enemyCombatType,
            "Could not find EnemyCombat in Assembly-CSharp."
        );

        enemyCombat =
            enemyObject.AddComponent(enemyCombatType);

        Assert.IsNotNull(enemyCombat);

        getAttackDamageMethod =
            enemyCombatType.GetMethod(
                "GetAttackDamage",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        setEnemyDamageMethod =
            enemyCombatType.GetMethod(
                "SetEnemyDamage",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        tryAttackMethod =
            enemyCombatType.GetMethod(
                "TryAttack",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        canAttackProperty =
            enemyCombatType.GetProperty(
                "CanAttack",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        isAttackingField =
            enemyCombatType.GetField(
                "isAttacking",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        Assert.IsNotNull(getAttackDamageMethod);
        Assert.IsNotNull(setEnemyDamageMethod);
        Assert.IsNotNull(tryAttackMethod);
        Assert.IsNotNull(canAttackProperty);
        Assert.IsNotNull(isAttackingField);
    }

    [TearDown]
    public void TearDown()
    {
        if (enemyObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                enemyObject
            );
        }
    }

    [Test]
    public void EnemyCombat_ImplementsIAttackDamageSource()
    {
        Type damageSourceType =
            Type.GetType(
                "IAttackDamageSource, Assembly-CSharp"
            );

        Assert.IsNotNull(
            damageSourceType,
            "Could not find IAttackDamageSource."
        );

        Assert.IsTrue(
            damageSourceType.IsAssignableFrom(
                enemyCombatType
            ),
            "EnemyCombat should implement IAttackDamageSource."
        );
    }

    [Test]
    public void GetAttackDamage_ReturnsDefaultDamage()
    {
        float damage =
            (float)getAttackDamageMethod.Invoke(
                enemyCombat,
                new object[] { null }
            );

        Assert.AreEqual(
            1f,
            damage,
            "EnemyCombat should return its default damage value."
        );
    }

    [Test]
    public void SetEnemyDamage_ChangesReturnedDamage()
    {
        setEnemyDamageMethod.Invoke(
            enemyCombat,
            new object[] { 3f }
        );

        float damage =
            (float)getAttackDamageMethod.Invoke(
                enemyCombat,
                new object[] { null }
            );

        Assert.AreEqual(
            3f,
            damage,
            "GetAttackDamage should return the updated enemy damage."
        );
    }

    [Test]
    public void CanAttack_IsTrueForFreshEnemy()
    {
        bool canAttack =
            (bool)canAttackProperty.GetValue(
                enemyCombat
            );

        Assert.IsTrue(
            canAttack,
            "A fresh enemy should initially be able to attack."
        );
    }

    [Test]
    public void CanAttack_IsFalseWhileEnemyIsAttacking()
    {
        isAttackingField.SetValue(
            enemyCombat,
            true
        );

        bool canAttack =
            (bool)canAttackProperty.GetValue(
                enemyCombat
            );

        Assert.IsFalse(
            canAttack,
            "Enemy should not be able to attack while already attacking."
        );
    }

    [Test]
    public void TryAttack_ReturnsFalseWhileEnemyIsAlreadyAttacking()
    {
        isAttackingField.SetValue(
            enemyCombat,
            true
        );

        bool result =
            (bool)tryAttackMethod.Invoke(
                enemyCombat,
                null
            );

        Assert.IsFalse(
            result,
            "TryAttack should reject an attack while one is already active."
        );
    }
}