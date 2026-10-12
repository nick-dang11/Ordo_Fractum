
using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DetectionGizmos
{
    private GameObject enemy;
    private GameObject player;

    private MonoBehaviour enemyGizmos;
    private MonoBehaviour playerGizmos;

    private const BindingFlags Flags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    private Type FindType(string className)
    {
        Type type = Type.GetType(
            className + ", Assembly-CSharp"
        );

        Assert.IsNotNull(
            type,
            "Could not find " + className +
            " in Assembly-CSharp."
        );

        return type;
    }

    private T GetField<T>(
        object target,
        string fieldName)
    {
        FieldInfo field =
            target.GetType().GetField(
                fieldName,
                Flags
            );

        Assert.IsNotNull(
            field,
            "Field not found: " + fieldName
        );

        return (T)field.GetValue(target);
    }

    private void SetField(
        object target,
        string fieldName,
        object value)
    {
        FieldInfo field =
            target.GetType().GetField(
                fieldName,
                Flags
            );

        Assert.IsNotNull(
            field,
            "Field not found: " + fieldName
        );

        field.SetValue(target, value);
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        enemy = new GameObject("TestEnemy");
        player = new GameObject("TestPlayer");

        enemyGizmos = (MonoBehaviour)
            enemy.AddComponent(
                FindType("EnemyDetectionGizmos")
            );

        playerGizmos = (MonoBehaviour)
            player.AddComponent(
                FindType("PlayerDetectionGizmos")
            );

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (enemy != null)
            UnityEngine.Object.Destroy(enemy);

        if (player != null)
            UnityEngine.Object.Destroy(player);

        yield return null;
    }

    // TEST 1
    [UnityTest]
    public IEnumerator EnemyGizmos_ComponentExists()
    {
        Assert.IsNotNull(enemyGizmos);
        yield return null;
    }

    // TEST 2
    [UnityTest]
    public IEnumerator PlayerGizmos_ComponentExists()
    {
        Assert.IsNotNull(playerGizmos);
        yield return null;
    }

    // TEST 3
    [UnityTest]
    public IEnumerator EnemyGizmos_DefaultRadiusIsTen()
    {
        float radius = GetField<float>(
            enemyGizmos,
            "detectionRadius"
        );

        Assert.AreEqual(10f, radius, 0.001f);
        yield return null;
    }

    // TEST 4
    [UnityTest]
    public IEnumerator PlayerGizmos_DefaultRadiusIsTen()
    {
        float radius = GetField<float>(
            playerGizmos,
            "detectionRadius"
        );

        Assert.AreEqual(10f, radius, 0.001f);
        yield return null;
    }

    // TEST 5
    [UnityTest]
    public IEnumerator EnemyGizmos_RadiusCanBeAdjusted()
    {
        SetField(
            enemyGizmos,
            "detectionRadius",
            15f
        );

        Assert.AreEqual(
            15f,
            GetField<float>(
                enemyGizmos,
                "detectionRadius"
            ),
            0.001f
        );

        yield return null;
    }

    // TEST 6
    [UnityTest]
    public IEnumerator PlayerGizmos_RadiusCanBeAdjusted()
    {
        SetField(
            playerGizmos,
            "detectionRadius",
            5f
        );

        Assert.AreEqual(
            5f,
            GetField<float>(
                playerGizmos,
                "detectionRadius"
            ),
            0.001f
        );

        yield return null;
    }

    // TEST 7
    [UnityTest]
    public IEnumerator EnemyGizmos_DefaultFloorOffset()
    {
        float offset = GetField<float>(
            enemyGizmos,
            "floorOffset"
        );

        Assert.AreEqual(
            0.05f,
            offset,
            0.001f
        );

        yield return null;
    }

    // TEST 8
    [UnityTest]
    public IEnumerator PlayerGizmos_DefaultFloorOffset()
    {
        float offset = GetField<float>(
            playerGizmos,
            "floorOffset"
        );

        Assert.AreEqual(
            0.05f,
            offset,
            0.001f
        );

        yield return null;
    }

    // TEST 9
    [UnityTest]
    public IEnumerator EnemyGizmos_DefaultColorIsGreen()
    {
        Color color = GetField<Color>(
            enemyGizmos,
            "radiusColor"
        );

        Assert.AreEqual(0f, color.r, 0.001f);
        Assert.AreEqual(1f, color.g, 0.001f);
        Assert.AreEqual(0f, color.b, 0.001f);
        Assert.AreEqual(0.2f, color.a, 0.001f);

        yield return null;
    }

    // TEST 10
    [UnityTest]
    public IEnumerator PlayerGizmos_DefaultColorIsBlue()
    {
        Color color = GetField<Color>(
            playerGizmos,
            "radiusColor"
        );

        Assert.AreEqual(0f, color.r, 0.001f);
        Assert.AreEqual(0f, color.g, 0.001f);
        Assert.AreEqual(1f, color.b, 0.001f);
        Assert.AreEqual(0.2f, color.a, 0.001f);

        yield return null;
    }

    // TEST 11
    [UnityTest]
    public IEnumerator EnemyAndPlayerRadii_AreIndependent()
    {
        SetField(
            enemyGizmos,
            "detectionRadius",
            20f
        );

        SetField(
            playerGizmos,
            "detectionRadius",
            6f
        );

        Assert.AreEqual(
            20f,
            GetField<float>(
                enemyGizmos,
                "detectionRadius"
            ),
            0.001f
        );

        Assert.AreEqual(
            6f,
            GetField<float>(
                playerGizmos,
                "detectionRadius"
            ),
            0.001f
        );

        yield return null;
    }
}
