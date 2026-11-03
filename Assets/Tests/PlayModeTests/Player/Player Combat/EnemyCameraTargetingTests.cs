
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyCameraTargetingTests
{
    private const BindingFlags Flags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    private readonly List<GameObject> createdObjects =
        new List<GameObject>();

    private Component targeting;
    private Component inputManager;
    private Type enemyCombatType;

    private Transform playerModel;
    private Transform cameraTransform;

    // ----------------------------------------------------
    // TEST SETUP
    // ----------------------------------------------------

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        Type targetingType = FindType("PlayerActionController");
        Type inputType = FindType("PlayerInputManager");
        enemyCombatType = FindType("EnemyCombat");

        Assert.IsNotNull(targetingType);
        Assert.IsNotNull(inputType);
        Assert.IsNotNull(enemyCombatType);

        int enemyLayer = LayerMask.NameToLayer("Enemy");

        Assert.AreNotEqual(
            -1,
            enemyLayer,
            "The Enemy layer must exist in Unity."
        );

        // Create a temporary player.
        GameObject player = CreateObject("TestPlayer");

        // Create the input component first so the
        // targeting controller can find it.
        inputManager = player.AddComponent(inputType);

        // Create a visual player model.
        GameObject model = CreateObject("TestPlayerModel");
        model.transform.SetParent(player.transform, false);

        playerModel = model.transform;

        // Create a camera facing world forward (+Z).
        GameObject camera = CreateObject("TestCamera");
        cameraTransform = camera.transform;
        cameraTransform.rotation = Quaternion.identity;

        targeting = player.AddComponent(targetingType);

        // Connect the same private fields used by
        // the actual PlayerActionController.
        SetField(targeting, "inputManager", inputManager);
        SetField(targeting, "playerModel", playerModel);
        SetField(targeting, "mainCamera", cameraTransform);

        LayerMask targetMask = 1 << enemyLayer;

        SetField(targeting, "enemyLayer", targetMask);
        SetField(targeting, "targetingRadius", 12f);
        SetField(targeting, "maxTargetingAngle", 45f);

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (GameObject obj in createdObjects)
        {
            if (obj != null)
                UnityEngine.Object.Destroy(obj);
        }

        createdObjects.Clear();

        yield return null;
    }

    // ----------------------------------------------------
    // TC-01: SMALLEST ANGLE PRIORITY
    // ----------------------------------------------------

    [UnityTest]
    public IEnumerator ForwardInput_SelectsSmallestAngle()
    {
        GameObject enemyFront = CreateEnemy(
            "EnemyFront",
            new Vector3(0f, 0f, 8f)
        );

        CreateEnemy(
            "CloserAngledEnemy",
            new Vector3(2f, 0f, 3f)
        );

        SetMovementInput(new Vector2(0f, 1f));

        SnapToTarget();

        Assert.AreSame(
            enemyFront.transform,
            GetTarget("attackTarget"),
            "W should target the enemy with the " +
            "smallest angle, not the closest enemy."
        );

        yield return null;
    }

    // ----------------------------------------------------
    // TC-02: CAMERA-RELATIVE INPUT
    // ----------------------------------------------------

    [UnityTest]
    public IEnumerator CameraRotation_ChangesTargetDirection()
    {
        GameObject enemyEast = CreateEnemy(
            "EnemyEast",
            new Vector3(5f, 0f, 0f)
        );

        CreateEnemy(
            "EnemyNorth",
            new Vector3(0f, 0f, 5f)
        );

        // Rotate camera 90 degrees to the right.
        cameraTransform.rotation =
            Quaternion.Euler(0f, 90f, 0f);

        SetMovementInput(new Vector2(0f, 1f));

        SnapToTarget();

        Assert.AreSame(
            enemyEast.transform,
            GetTarget("attackTarget"),
            "W should target camera-forward, " +
            "which is world-right after rotation."
        );

        yield return null;
    }

    // ----------------------------------------------------
    // TC-03: SINGLE ENEMY DIRECTION FILTERING
    // ----------------------------------------------------

    [UnityTest]
    public IEnumerator EnemyBehind_DoesNotForceTargeting()
    {
        CreateEnemy(
            "EnemyBehind",
            new Vector3(0f, 0f, -5f)
        );

        // Player attacks north, enemy is south.
        SetMovementInput(new Vector2(0f, 1f));

        SnapToTarget();

        Assert.IsNull(
            GetTarget("attackTarget"),
            "An enemy behind the intended attack " +
            "direction should not be selected."
        );

        Assert.Less(
            Vector3.Angle(playerModel.forward, Vector3.forward),
            1f,
            "Player should still attack north."
        );

        yield return null;
    }

    // ----------------------------------------------------
    // TC-04: DIAGONAL INPUT
    // ----------------------------------------------------

    [UnityTest]
    public IEnumerator DiagonalInput_SelectsDiagonalEnemy()
    {
        GameObject diagonalEnemy = CreateEnemy(
            "EnemyNorthEast",
            new Vector3(4f, 0f, 4f)
        );

        CreateEnemy(
            "EnemyNorth",
            new Vector3(0f, 0f, 5f)
        );

        // W + D input.
        SetMovementInput(new Vector2(1f, 1f));

        SnapToTarget();

        Assert.AreSame(
            diagonalEnemy.transform,
            GetTarget("attackTarget"),
            "W+D should select the northeast enemy."
        );

        yield return null;
    }

    // ----------------------------------------------------
    // TC-05: REMEMBER LAST FOCUSED ENEMY
    // ----------------------------------------------------

    [UnityTest]
    public IEnumerator FocusOnEnemy_RemembersLastEnemy()
    {
        GameObject enemyA = CreateEnemy(
            "EnemyA",
            new Vector3(0f, 0f, 4f)
        );

        GameObject enemyB = CreateEnemy(
            "EnemyB",
            new Vector3(4f, 0f, 0f)
        );

        FocusOnEnemy(enemyA.transform);

        Assert.AreSame(
            enemyA.transform,
            GetTarget("currentFocusTarget")
        );

        // Simulate engaging a different enemy.
        FocusOnEnemy(enemyB.transform);

        Assert.AreSame(
            enemyB.transform,
            GetTarget("currentFocusTarget"),
            "The last engaged enemy should " +
            "replace the previous focus."
        );

        yield return null;
    }

    // ----------------------------------------------------
    // TC-06: DEAD ENEMY TARGET SWITCHING
    // ----------------------------------------------------

    [UnityTest]
    public IEnumerator DeadEnemy_SwitchesToLivingEnemy()
    {
        GameObject enemyA = CreateEnemy(
            "EnemyA",
            new Vector3(0f, 0f, 3f)
        );

        GameObject enemyB = CreateEnemy(
            "EnemyB",
            new Vector3(3f, 0f, 4f)
        );

        FocusOnEnemy(enemyA.transform);

        Assert.AreSame(
            enemyA.transform,
            GetTarget("currentFocusTarget")
        );

        // Use the actual EnemyCombat death method.
        Component enemyCombat =
            enemyA.GetComponent(enemyCombatType);

        MethodInfo deathMethod =
            enemyCombatType.GetMethod("StartDeath", Flags);

        Assert.IsNotNull(deathMethod);

        deathMethod.Invoke(enemyCombat, null);

        // Allow PlayerActionController.Update to run.
        yield return null;

        Assert.AreSame(
            enemyB.transform,
            GetTarget("currentFocusTarget"),
            "The controller should switch to " +
            "a living enemy when its focus dies."
        );
    }

    // ----------------------------------------------------
    // TC-07: DISTANCE TIEBREAKER
    // ----------------------------------------------------

    [UnityTest]
    public IEnumerator EqualAngles_SelectsCloserEnemy()
    {
        GameObject closerEnemy = CreateEnemy(
            "CloserEnemy",
            new Vector3(1f, 0f, 2f)
        );

        CreateEnemy(
            "FartherEnemy",
            new Vector3(2f, 0f, 4f)
        );

        SetMovementInput(new Vector2(0f, 1f));

        SnapToTarget();

        Assert.AreSame(
            closerEnemy.transform,
            GetTarget("attackTarget"),
            "When angles are equal, " +
            "the closer enemy should be selected."
        );

        yield return null;
    }

    // ----------------------------------------------------
    // TC-08: TARGETING RANGE
    // ----------------------------------------------------

    [UnityTest]
    public IEnumerator EnemyOutsideRange_IsNotTargeted()
    {
        CreateEnemy(
            "FarEnemy",
            new Vector3(0f, 0f, 20f)
        );

        SetMovementInput(new Vector2(0f, 1f));

        SnapToTarget();

        Assert.IsNull(
            GetTarget("attackTarget"),
            "An enemy beyond the targeting radius " +
            "should not be selected."
        );

        yield return null;
    }

    // ----------------------------------------------------
    // TEST HELPERS
    // ----------------------------------------------------

    private GameObject CreateObject(string objectName)
    {
        GameObject obj = new GameObject(objectName);
        createdObjects.Add(obj);
        return obj;
    }

    private GameObject CreateEnemy(
        string enemyName,
        Vector3 position)
    {
        GameObject enemy = CreateObject(enemyName);

        enemy.transform.position = position;
        enemy.layer = LayerMask.NameToLayer("Enemy");

        // EnemyCombat is required by GetEnemyRoot()
        // and IsValidTarget() in PlayerActionController.
        enemy.AddComponent(enemyCombatType);
        enemy.AddComponent<BoxCollider>();

        // Update physics positions before querying.
        Physics.SyncTransforms();

        return enemy;
    }

    private void SetMovementInput(Vector2 movement)
    {
        SetField(inputManager, "move", movement);
    }

    private void SnapToTarget()
    {
        Physics.SyncTransforms();

        MethodInfo method =
            targeting.GetType().GetMethod(
                "SnapToAttackTarget",
                Flags
            );

        Assert.IsNotNull(method);

        method.Invoke(targeting, null);
    }

    private void FocusOnEnemy(Transform enemy)
    {
        MethodInfo method =
            targeting.GetType().GetMethod(
                "FocusOnEnemy",
                Flags
            );

        Assert.IsNotNull(method);

        method.Invoke(targeting, new object[] { enemy });
    }

    private Transform GetTarget(string fieldName)
    {
        FieldInfo field =
            targeting.GetType().GetField(fieldName, Flags);

        Assert.IsNotNull(
            field,
            "Target field not found: " + fieldName
        );

        return (Transform)field.GetValue(targeting);
    }

    private static void SetField(
        Component component,
        string fieldName,
        object value)
    {
        FieldInfo field =
            component.GetType().GetField(fieldName, Flags);

        Assert.IsNotNull(
            field,
            "Required field not found: " + fieldName
        );

        field.SetValue(component, value);
    }

    private static Type FindType(string typeName)
    {
        foreach (System.Reflection.Assembly assembly
                 in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(typeName, false);

            if (type != null)
                return type;
        }

        return null;
    }
}
