
using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyDetectionTests
{
    private GameObject enemyObject;
    private GameObject playerObject;
    private GameObject visionObject;

    private MonoBehaviour controller;
    private MonoBehaviour detection;

    private Type controllerType;
    private Type detectionType;

    private const float DetectionRange = 8f;

    // Find production scripts in Assembly-CSharp.
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

    // Set private serialized fields.
    private void SetField(
        object target,
        string fieldName,
        object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic
        );

        Assert.IsNotNull(
            field,
            "Field not found: " + fieldName
        );

        field.SetValue(target, value);
    }

    // Call production methods through reflection.
    private T InvokeDetection<T>(string methodName)
    {
        MethodInfo method = detectionType.GetMethod(
            methodName,
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic
        );

        Assert.IsNotNull(
            method,
            "Method not found: " + methodName
        );

        return (T)method.Invoke(detection, null);
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        controllerType = FindType("EnemyAIController");
        detectionType = FindType("EnemyDetection");

        // Create enemy.
        enemyObject = new GameObject("TestEnemy");
        enemyObject.transform.position = Vector3.zero;
        enemyObject.transform.rotation =
            Quaternion.identity;

        controller = (MonoBehaviour)
            enemyObject.AddComponent(controllerType);

        detection = (MonoBehaviour)
            enemyObject.AddComponent(detectionType);

        // Create player.
        playerObject = new GameObject("TestPlayer");

        playerObject.transform.position =
            new Vector3(0f, 0f, 5f);

        // Player collider covers chest-height target.
        BoxCollider playerCollider =
            playerObject.AddComponent<BoxCollider>();

        playerCollider.center =
            new Vector3(0f, 1f, 0f);

        playerCollider.size =
            new Vector3(1f, 2f, 1f);

        // Assign player to EnemyAIController.
        SetField(
            controller,
            "player",
            playerObject.transform
        );

        // Create vision origin at chest height.
        visionObject = new GameObject("VisionOrigin");

        visionObject.transform.SetParent(
            enemyObject.transform
        );

        visionObject.transform.localPosition =
            new Vector3(0f, 1f, 0f);

        visionObject.transform.localRotation =
            Quaternion.identity;

        // Configure detection.
        SetField(
            detection,
            "detectionDistance",
            DetectionRange
        );

        SetField(
            detection,
            "fieldOfViewAngle",
            90f
        );

        SetField(
            detection,
            "visionDirection",
            visionObject.transform
        );

        SetField(
            detection,
            "lostSightDelay",
            5f
        );

        // Raycast against default-layer objects.
        SetLineOfSightMask(
            1 << playerObject.layer
        );

        yield return null;

        Physics.SyncTransforms();
    }

    private void SetLineOfSightMask(int mask)
    {
        SetField(
            detection,
            "lineOfSightMask",
            (LayerMask)mask
        );
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (enemyObject != null)
            UnityEngine.Object.Destroy(enemyObject);

        if (playerObject != null)
            UnityEngine.Object.Destroy(playerObject);

        yield return null;
    }

    // TEST 1
    // Player inside detection distance.
    [UnityTest]
    public IEnumerator PlayerInsideRange_IsDetected()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 5f);

        Physics.SyncTransforms();

        Assert.IsTrue(
            InvokeDetection<bool>("IsPlayerDetected")
        );

        yield return null;
    }

    // TEST 2
    // Player outside detection distance.
    [UnityTest]
    public IEnumerator PlayerOutsideRange_IsNotDetected()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 12f);

        Physics.SyncTransforms();

        Assert.IsFalse(
            InvokeDetection<bool>("IsPlayerDetected")
        );

        yield return null;
    }

    // TEST 3
    // Detection distance boundary.
    [UnityTest]
    public IEnumerator DetectionDistance_BoundaryValues()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 7.9f);

        Assert.IsTrue(
            InvokeDetection<bool>("IsPlayerDetected")
        );

        playerObject.transform.position =
            new Vector3(0f, 0f, 8f);

        Assert.IsTrue(
            InvokeDetection<bool>("IsPlayerDetected")
        );

        playerObject.transform.position =
            new Vector3(0f, 0f, 8.1f);

        Assert.IsFalse(
            InvokeDetection<bool>("IsPlayerDetected")
        );

        yield return null;
    }

    // TEST 4
    // Player inside field of view.
    [UnityTest]
    public IEnumerator PlayerInFront_IsInsideFOV()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 5f);

        Physics.SyncTransforms();

        Assert.IsTrue(
            InvokeDetection<bool>(
                "IsPlayerInFieldOfView"
            )
        );

        yield return null;
    }

    // TEST 5
    // Player behind enemy.
    [UnityTest]
    public IEnumerator PlayerBehind_IsOutsideFOV()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, -5f);

        Physics.SyncTransforms();

        Assert.IsFalse(
            InvokeDetection<bool>(
                "IsPlayerInFieldOfView"
            )
        );

        yield return null;
    }

    // TEST 6
    // Player with clear line of sight.
    // FIXED TEST
    [UnityTest]
    public IEnumerator PlayerWithClearLineOfSight_CanBeSeen()
    {
        // Arrange.
        playerObject.transform.position =
            new Vector3(0f, 0f, 5f);

        // Ensure collider exists and covers
        // the raycast's chest-height target.
        BoxCollider playerCollider =
            playerObject.GetComponent<BoxCollider>();

        Assert.IsNotNull(playerCollider);

        playerCollider.center =
            new Vector3(0f, 1f, 0f);

        playerCollider.size =
            new Vector3(1f, 2f, 1f);

        SetLineOfSightMask(
            1 << playerObject.layer
        );

        Physics.SyncTransforms();

        // Act.
        bool hasLineOfSight =
            InvokeDetection<bool>("HasLineOfSight");

        bool canSeePlayer =
            InvokeDetection<bool>("CanSeePlayer");

        // Assert.
        Assert.IsTrue(
            hasLineOfSight,
            "Enemy should have clear line of sight."
        );

        Assert.IsTrue(
            canSeePlayer,
            "Enemy should see the player when " +
            "inside range, FOV, and unobstructed."
        );

        yield return null;
    }

    // TEST 7
    // Obstacle blocks line of sight.
    [UnityTest]
    public IEnumerator WallBlocksLineOfSight()
    {
        GameObject wall = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        wall.name = "TestWall";

        wall.transform.position =
            new Vector3(0f, 1f, 2.5f);

        wall.transform.localScale =
            new Vector3(2f, 3f, 0.5f);

        // Include both player and wall layers.
        int mask =
            (1 << playerObject.layer) |
            (1 << wall.layer);

        SetLineOfSightMask(mask);

        Physics.SyncTransforms();

        try
        {
            Assert.IsFalse(
                InvokeDetection<bool>(
                    "HasLineOfSight"
                )
            );

            Assert.IsFalse(
                InvokeDetection<bool>(
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

    // TEST 8
    // Player outside range cannot be seen.
    [UnityTest]
    public IEnumerator PlayerOutsideRange_CannotBeSeen()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 12f);

        Physics.SyncTransforms();

        Assert.IsFalse(
            InvokeDetection<bool>("CanSeePlayer")
        );

        yield return null;
    }

    // TEST 9
    // Player behind enemy cannot be seen.
    [UnityTest]
    public IEnumerator PlayerBehind_CannotBeSeen()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, -5f);

        Physics.SyncTransforms();

        Assert.IsFalse(
            InvokeDetection<bool>("CanSeePlayer")
        );

        yield return null;
    }

    // TEST 10
    // Missing player reference is handled safely.
    [UnityTest]
    public IEnumerator MissingPlayer_ReturnsSafeResults()
    {
        SetField(controller, "player", null);

        Assert.IsFalse(
            InvokeDetection<bool>("IsPlayerDetected")
        );

        Assert.IsFalse(
            InvokeDetection<bool>("CanSeePlayer")
        );

        Assert.AreEqual(
            Mathf.Infinity,
            InvokeDetection<float>("DistanceToPlayer")
        );

        yield return null;
    }

    // TEST 11
    // Enemy remembers recently seen player.
    [UnityTest]
    public IEnumerator ShouldChase_RemembersRecentlySeenPlayer()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 5f);

        Physics.SyncTransforms();

        Assert.IsTrue(
            InvokeDetection<bool>(
                "ShouldChasePlayer"
            )
        );

        // Move player out of range.
        playerObject.transform.position =
            new Vector3(0f, 0f, 20f);

        Physics.SyncTransforms();

        Assert.IsTrue(
            InvokeDetection<bool>(
                "ShouldChasePlayer"
            )
        );

        yield return null;
    }

    // TEST 12
    // Enemy does not chase before seeing player.
    [UnityTest]
    public IEnumerator ShouldChase_FalseBeforeFirstDetection()
    {
        playerObject.transform.position =
            new Vector3(0f, 0f, 20f);

        Physics.SyncTransforms();

        Assert.IsFalse(
            InvokeDetection<bool>(
                "ShouldChasePlayer"
            )
        );

        yield return null;
    }
}
