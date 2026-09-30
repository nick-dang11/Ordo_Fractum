using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class EnemyDetectionTests
{
    private GameObject enemyObject;
    private GameObject playerObject;

    private Component controller;
    private Component detection;

    private Type controllerType;
    private Type detectionType;

    private FieldInfo detectionDistanceField;
    private FieldInfo fieldOfViewAngleField;
    private FieldInfo lineOfSightMaskField;

    [SetUp]
    public void SetUp()
    {
        // Resolve runtime types from Assembly-CSharp without creating
        // a compile-time dependency from PlayModeTests -> Assembly-CSharp.
        controllerType =
            Type.GetType("EnemyAIController, Assembly-CSharp");

        detectionType =
            Type.GetType("EnemyDetection, Assembly-CSharp");

        Assert.IsNotNull(
            controllerType,
            "EnemyAIController could not be found in Assembly-CSharp."
        );

        Assert.IsNotNull(
            detectionType,
            "EnemyDetection could not be found in Assembly-CSharp."
        );

        enemyObject = new GameObject("Test Enemy");
        playerObject = new GameObject("Test Player");

        controller =
            enemyObject.AddComponent(controllerType);

        detection =
            enemyObject.AddComponent(detectionType);

        Assert.IsNotNull(controller);
        Assert.IsNotNull(detection);

        SetPlayerReference(playerObject.transform);

        detectionDistanceField =
            detectionType.GetField(
                "detectionDistance",
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

        fieldOfViewAngleField =
            detectionType.GetField(
                "fieldOfViewAngle",
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

        lineOfSightMaskField =
            detectionType.GetField(
                "lineOfSightMask",
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

        Assert.IsNotNull(
            detectionDistanceField,
            "EnemyDetection.detectionDistance could not be found."
        );

        Assert.IsNotNull(
            fieldOfViewAngleField,
            "EnemyDetection.fieldOfViewAngle could not be found."
        );

        Assert.IsNotNull(
            lineOfSightMaskField,
            "EnemyDetection.lineOfSightMask could not be found."
        );

        SetDetectionDistance(8f);
        SetFieldOfViewAngle(90f);

        enemyObject.transform.position = Vector3.zero;
        enemyObject.transform.forward = Vector3.forward;
    }

    [TearDown]
    public void TearDown()
    {
        if (enemyObject != null)
        {
            UnityEngine.Object.DestroyImmediate(enemyObject);
        }

        if (playerObject != null)
        {
            UnityEngine.Object.DestroyImmediate(playerObject);
        }
    }

    // ---------------------------------------------------------
    // Distance tests
    // ---------------------------------------------------------

    [Test]
    public void DistanceToPlayer_ReturnsCorrectDistance()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 5f);

        float actual =
            InvokeDetection<float>("DistanceToPlayer");

        Assert.AreEqual(
            5f,
            actual,
            0.001f
        );
    }

    [Test]
    public void PlayerInsideDetectionDistance_IsDetected()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 5f);

        Assert.IsTrue(
            InvokeDetection<bool>("IsPlayerDetected")
        );
    }

    [Test]
    public void PlayerOutsideDetectionDistance_IsNotDetected()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 10f);

        Assert.IsFalse(
            InvokeDetection<bool>("IsPlayerDetected")
        );
    }

    [Test]
    public void PlayerExactlyAtDetectionDistance_IsDetected()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 8f);

        Assert.IsTrue(
            InvokeDetection<bool>("IsPlayerDetected")
        );
    }

    [Test]
    public void PlayerJustOutsideDetectionDistance_IsNotDetected()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 8.01f);

        Assert.IsFalse(
            InvokeDetection<bool>("IsPlayerDetected")
        );
    }

    // ---------------------------------------------------------
    // Direction tests
    // ---------------------------------------------------------

    [Test]
    public void DirectionToPlayer_IsNormalized()
    {
        playerObject.transform.position =
            new Vector3(3f, 0f, 4f);

        Vector3 direction =
            InvokeDetection<Vector3>("DirectionToPlayer");

        Assert.AreEqual(
            1f,
            direction.magnitude,
            0.001f
        );
    }

    [Test]
    public void DirectionToPlayer_PointsTowardPlayer()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 5f);

        Vector3 direction =
            InvokeDetection<Vector3>("DirectionToPlayer");

        Assert.AreEqual(
            Vector3.forward.x,
            direction.x,
            0.001f
        );

        Assert.AreEqual(
            Vector3.forward.z,
            direction.z,
            0.001f
        );
    }

    // ---------------------------------------------------------
    // Field of view tests
    // ---------------------------------------------------------

    [Test]
    public void PlayerDirectlyInFront_IsInsideFieldOfView()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 5f);

        Assert.IsTrue(
            InvokeDetection<bool>("IsPlayerInFieldOfView")
        );
    }

    [Test]
    public void PlayerDirectlyBehind_IsOutsideFieldOfView()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, -5f);

        Assert.IsFalse(
            InvokeDetection<bool>("IsPlayerInFieldOfView")
        );
    }

    [Test]
    public void PlayerToRightInsideFov_IsInsideFieldOfView()
    {
        playerObject.transform.position =
            new Vector3(2f, 0f, 5f);

        Assert.IsTrue(
            InvokeDetection<bool>("IsPlayerInFieldOfView")
        );
    }

    [Test]
    public void PlayerToLeftInsideFov_IsInsideFieldOfView()
    {
        playerObject.transform.position =
            new Vector3(-2f, 0f, 5f);

        Assert.IsTrue(
            InvokeDetection<bool>("IsPlayerInFieldOfView")
        );
    }

    [Test]
    public void PlayerOutsideFov_IsOutsideFieldOfView()
    {
        playerObject.transform.position =
            new Vector3(5f, 0f, 2f);

        Assert.IsFalse(
            InvokeDetection<bool>("IsPlayerInFieldOfView")
        );
    }

    [Test]
    public void NarrowerFov_RejectsPlayerPreviouslyVisible()
    {
        SetFieldOfViewAngle(30f);

        playerObject.transform.position =
            new Vector3(2f, 0f, 5f);

        Assert.IsFalse(
            InvokeDetection<bool>("IsPlayerInFieldOfView")
        );
    }

    // ---------------------------------------------------------
    // Combined detection tests
    // ---------------------------------------------------------

    [Test]
    public void PlayerInRangeButBehindEnemy_CannotBeSeen()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, -5f);

        Assert.IsTrue(
            InvokeDetection<bool>("IsPlayerDetected")
        );

        Assert.IsFalse(
            InvokeDetection<bool>("IsPlayerInFieldOfView")
        );

        Assert.IsFalse(
            InvokeDetection<bool>("CanSeePlayer")
        );
    }

    [Test]
    public void PlayerOutsideRangeButInFront_CannotBeSeen()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 10f);

        Assert.IsFalse(
            InvokeDetection<bool>("IsPlayerDetected")
        );

        Assert.IsTrue(
            InvokeDetection<bool>("IsPlayerInFieldOfView")
        );

        Assert.IsFalse(
            InvokeDetection<bool>("CanSeePlayer")
        );
    }

    // ---------------------------------------------------------
    // Line of sight tests
    // ---------------------------------------------------------

    [Test]
    public void PlayerWithClearLineOfSight_CanBeSeen()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 5f);

        playerObject.AddComponent<BoxCollider>();

        SetLineOfSightMask(
            1 << playerObject.layer
        );

        Physics.SyncTransforms();

        Assert.IsTrue(
            InvokeDetection<bool>("HasLineOfSight")
        );
    }

    [Test]
    public void ObstacleBlocksLineOfSight_PlayerCannotBeSeen()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 6f);

        playerObject.AddComponent<BoxCollider>();

        GameObject obstacle =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

        obstacle.name = "Test Obstacle";

        obstacle.transform.position =
            new Vector3(0f, 0f, 3f);

        SetLineOfSightMask(
            Physics.DefaultRaycastLayers
        );

        Physics.SyncTransforms();

        bool hasLineOfSight =
            InvokeDetection<bool>("HasLineOfSight");

        UnityEngine.Object.DestroyImmediate(obstacle);

        Assert.IsFalse(
            hasLineOfSight,
            "An obstacle between the enemy and player should block line of sight."
        );
    }

    [Test]
    public void PlayerInRangeAndFovButBlocked_CannotBeSeen()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 6f);

        playerObject.AddComponent<BoxCollider>();

        GameObject obstacle =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

        obstacle.name = "Test Obstacle";

        obstacle.transform.position =
            new Vector3(0f, 0f, 3f);

        SetLineOfSightMask(
            Physics.DefaultRaycastLayers
        );

        Physics.SyncTransforms();

        bool canSeePlayer =
            InvokeDetection<bool>("CanSeePlayer");

        UnityEngine.Object.DestroyImmediate(obstacle);

        Assert.IsFalse(
            canSeePlayer,
            "Player should not be visible when an obstacle blocks line of sight."
        );
    }

    // ---------------------------------------------------------
    // Configuration tests
    // ---------------------------------------------------------

    [Test]
    public void IncreasingDetectionDistance_AllowsFartherPlayerDetection()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 12f);

        Assert.IsFalse(
            InvokeDetection<bool>("IsPlayerDetected")
        );

        SetDetectionDistance(15f);

        Assert.IsTrue(
            InvokeDetection<bool>("IsPlayerDetected")
        );
    }

    [Test]
    public void ReducingDetectionDistance_PreventsPlayerDetection()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 5f);

        Assert.IsTrue(
            InvokeDetection<bool>("IsPlayerDetected")
        );

        SetDetectionDistance(4f);

        Assert.IsFalse(
            InvokeDetection<bool>("IsPlayerDetected")
        );
    }

    // ---------------------------------------------------------
    // Reflection helpers
    // ---------------------------------------------------------

    private T InvokeDetection<T>(string methodName)
    {
        MethodInfo method =
            detectionType.GetMethod(
                methodName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

        Assert.IsNotNull(
            method,
            $"EnemyDetection.{methodName} could not be found."
        );

        object result =
            method.Invoke(
                detection,
                null
            );

        return (T)result;
    }

    private void SetDetectionDistance(float value)
    {
        detectionDistanceField.SetValue(
            detection,
            value
        );
    }

    private void SetFieldOfViewAngle(float value)
    {
        fieldOfViewAngleField.SetValue(
            detection,
            value
        );
    }

    private void SetLineOfSightMask(int value)
    {
        lineOfSightMaskField.SetValue(
            detection,
            (LayerMask)value
        );
    }

    private void SetPlayerReference(Transform player)
    {
        PropertyInfo playerProperty =
            controllerType.GetProperty(
                "Player",
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

        if (playerProperty != null &&
            playerProperty.CanWrite)
        {
            playerProperty.SetValue(
                controller,
                player
            );

            return;
        }

        FieldInfo playerField =
            controllerType.GetField(
                "player",
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

        Assert.IsNotNull(
            playerField,
            "EnemyAIController must expose a Player property or player field for the test."
        );

        playerField.SetValue(
            controller,
            player
        );
    }
}