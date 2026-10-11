
using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyAIPlayModeTests
{
    private GameObject enemy;
    private GameObject player;

    private MonoBehaviour controller;
    private MonoBehaviour detection;
    private MonoBehaviour movement;

    private Type controllerType;
    private Type detectionType;
    private Type movementType;

    // Find a class from Assembly-CSharp without
    // directly referencing it in the test assembly.
    private Type FindType(string className)
    {
        Type type = Type.GetType(
            className + ", Assembly-CSharp"
        );

        Assert.IsNotNull(
            type,
            "Could not find " + className +
            " in Assembly-CSharp. Check its assembly."
        );

        return type;
    }

    // Set a private field using reflection.
    private void SetField(
        object target,
        string fieldName,
        object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance |
            BindingFlags.NonPublic |
            BindingFlags.Public
        );

        Assert.IsNotNull(
            field,
            "Field not found: " + fieldName
        );

        field.SetValue(target, value);
    }

    // Read a private field using reflection.
    private T GetField<T>(
        object target,
        string fieldName)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance |
            BindingFlags.NonPublic |
            BindingFlags.Public
        );

        Assert.IsNotNull(field);

        return (T)field.GetValue(target);
    }

    // Call a public method using reflection.
    private T Call<T>(
        object target,
        string methodName,
        params object[] arguments)
    {
        MethodInfo method = target.GetType().GetMethod(
            methodName,
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic
        );

        Assert.IsNotNull(
            method,
            "Method not found: " + methodName
        );

        return (T)method.Invoke(target, arguments);
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        controllerType = FindType("EnemyAIController");
        detectionType = FindType("EnemyDetection");
        movementType = FindType("EnemyMovement");

        // Create player with collider.
        player = GameObject.CreatePrimitive(
            PrimitiveType.Capsule
        );

        player.name = "TestPlayer";
        player.transform.position =
            new Vector3(0, 0, 4);

        // Create enemy.
        enemy = new GameObject("TestEnemy");
        enemy.transform.position = Vector3.zero;
        enemy.transform.rotation = Quaternion.identity;

        // Add real production components by Type.
        controller = (MonoBehaviour)
            enemy.AddComponent(controllerType);

        detection = (MonoBehaviour)
            enemy.AddComponent(detectionType);

        movement = (MonoBehaviour)
            enemy.AddComponent(movementType);

        // Assign controller's private player field.
        SetField(
            controller,
            "player",
            player.transform
        );

        // Configure detection.
        SetField(detection, "detectionDistance", 8f);
        SetField(detection, "fieldOfViewAngle", 90f);
        SetField(
            detection,
            "lineOfSightMask",
            (LayerMask)Physics.DefaultRaycastLayers
        );

        SetField(detection, "lostSightDelay", 5f);

        // Vision origin at chest height.
        GameObject vision = new GameObject("VisionOrigin");
        vision.transform.SetParent(enemy.transform);

        vision.transform.localPosition =
            new Vector3(0, 1, 0);

        vision.transform.localRotation =
            Quaternion.identity;

        SetField(
            detection,
            "visionDirection",
            vision.transform
        );

        yield return null;
        Physics.SyncTransforms();
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

    // TEST 1 - Controller player reference
    [UnityTest]
    public IEnumerator Controller_PlayerReference_IsCorrect()
    {
        PropertyInfo property = controllerType.GetProperty(
            "Player",
            BindingFlags.Public |
            BindingFlags.Instance
        );

        Assert.IsNotNull(property);

        Transform assignedPlayer =
            (Transform)property.GetValue(controller);

        Assert.AreEqual(
            player.transform,
            assignedPlayer
        );

        yield return null;
    }

    // TEST 2 - Player inside detection range
    [UnityTest]
    public IEnumerator PlayerInsideRange_IsDetected()
    {
        player.transform.position =
            new Vector3(0, 0, 4);

        Physics.SyncTransforms();

        Assert.IsTrue(
            Call<bool>(detection, "IsPlayerDetected")
        );

        yield return null;
    }

    // TEST 3 - Player outside detection range
    [UnityTest]
    public IEnumerator PlayerOutsideRange_IsNotDetected()
    {
        player.transform.position =
            new Vector3(0, 0, 12);

        Physics.SyncTransforms();

        Assert.IsFalse(
            Call<bool>(detection, "IsPlayerDetected")
        );

        yield return null;
    }

    // TEST 4 - Detection boundary
    [UnityTest]
    public IEnumerator DetectionDistance_BoundaryValues()
    {
        player.transform.position =
            new Vector3(0, 0, 7.9f);

        Assert.IsTrue(
            Call<bool>(detection, "IsPlayerDetected")
        );

        player.transform.position =
            new Vector3(0, 0, 8f);

        Assert.IsTrue(
            Call<bool>(detection, "IsPlayerDetected")
        );

        player.transform.position =
            new Vector3(0, 0, 8.1f);

        Assert.IsFalse(
            Call<bool>(detection, "IsPlayerDetected")
        );

        yield return null;
    }

    // TEST 5 - Player in front of enemy
    [UnityTest]
    public IEnumerator PlayerInFront_IsInsideFOV()
    {
        player.transform.position =
            new Vector3(0, 0, 4);

        Physics.SyncTransforms();

        Assert.IsTrue(
            Call<bool>(
                detection,
                "IsPlayerInFieldOfView"
            )
        );

        yield return null;
    }

    // TEST 6 - Player behind enemy
    [UnityTest]
    public IEnumerator PlayerBehind_IsOutsideFOV()
    {
        player.transform.position =
            new Vector3(0, 0, -4);

        Physics.SyncTransforms();

        Assert.IsFalse(
            Call<bool>(
                detection,
                "IsPlayerInFieldOfView"
            )
        );

        yield return null;
    }

    // TEST 7 - Visible player
    [UnityTest]
    public IEnumerator VisiblePlayer_CanBeSeen()
    {
        player.transform.position =
            new Vector3(0, 0, 4);

        Physics.SyncTransforms();

        Assert.IsTrue(
            Call<bool>(detection, "CanSeePlayer")
        );

        yield return null;
    }

    // TEST 8 - Wall blocks line of sight
    [UnityTest]
    public IEnumerator WallBlocksLineOfSight()
    {
        GameObject wall = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        wall.name = "TestWall";
        wall.transform.position =
            new Vector3(0, 1, 2);

        wall.transform.localScale =
            new Vector3(3, 3, 0.3f);

        Physics.SyncTransforms();

        try
        {
            Assert.IsFalse(
                Call<bool>(
                    detection,
                    "HasLineOfSight"
                )
            );

            Assert.IsFalse(
                Call<bool>(
                    detection,
                    "CanSeePlayer"
                )
            );
        }
        finally
        {
            UnityEngine.Object.Destroy(wall);
        }

        yield return null;
    }

    // TEST 9 - Missing player reference
    [UnityTest]
    public IEnumerator MissingPlayer_ReturnsSafeResults()
    {
        SetField(controller, "player", null);

        Assert.IsFalse(
            Call<bool>(detection, "IsPlayerDetected")
        );

        Assert.IsFalse(
            Call<bool>(detection, "CanSeePlayer")
        );

        Assert.AreEqual(
            Mathf.Infinity,
            Call<float>(detection, "DistanceToPlayer")
        );

        yield return null;
    }

    // TEST 10 - Patrol waypoint index wraps
    [UnityTest]
    public IEnumerator PatrolIndex_WrapsToZero()
    {
        GameObject pointA =
            new GameObject("PatrolPointA");

        GameObject pointB =
            new GameObject("PatrolPointB");

        try
        {
            Transform[] points =
            {
                pointA.transform,
                pointB.transform
            };

            SetField(
                movement,
                "patrolPoints",
                points
            );

            Call<object>(
                movement,
                "NextPatrolPoint"
            );

            Call<object>(
                movement,
                "NextPatrolPoint"
            );

            int index = GetField<int>(
                movement,
                "currentPatrolIndex"
            );

            Assert.AreEqual(0, index);
        }
        finally
        {
            UnityEngine.Object.Destroy(pointA);
            UnityEngine.Object.Destroy(pointB);
        }

        yield return null;
    }

    // TEST 11 - MoveTo without NavMeshAgent
    [UnityTest]
    public IEnumerator MoveTo_WithoutNavMesh_DoesNotCrash()
    {
        Assert.DoesNotThrow(() =>
        {
            Call<object>(
                movement,
                "MoveTo",
                new Vector3(5, 0, 5)
            );
        });

        yield return null;
    }

    // TEST 12 - Chase memory when player disappears
    [UnityTest]
    public IEnumerator ShouldChase_RemembersRecentlySeenPlayer()
    {
        player.transform.position =
            new Vector3(0, 0, 4);

        Physics.SyncTransforms();

        Assert.IsTrue(
            Call<bool>(
                detection,
                "ShouldChasePlayer"
            )
        );

        player.transform.position =
            new Vector3(0, 0, 20);

        Physics.SyncTransforms();

        Assert.IsTrue(
            Call<bool>(
                detection,
                "ShouldChasePlayer"
            )
        );

        yield return null;
    }
}
