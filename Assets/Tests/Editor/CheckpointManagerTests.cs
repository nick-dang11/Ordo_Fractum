using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CheckpointManagerTests
{
    private GameObject managerObject;
    private GameObject defaultSpawnObject;

    private GameObject checkpointAObject;
    private GameObject checkpointBObject;

    private Component checkpointManager;
    private Component checkpointA;
    private Component checkpointB;

    private Type managerType;
    private Type checkpointType;

    private FieldInfo defaultSpawnField;
    private FieldInfo checkpointIdField;
    private FieldInfo respawnPointField;

    private MethodInfo setCheckpointMethod;
    private MethodInfo getRespawnPositionMethod;
    private MethodInfo getRespawnRotationMethod;
    private MethodInfo restoreCheckpointMethod;

    private PropertyInfo currentCheckpointProperty;
    private PropertyInfo currentCheckpointIdProperty;
    private PropertyInfo isActivatedProperty;

    [SetUp]
    public void SetUp()
    {
        // ---------------------------------------------------------
        // Find runtime checkpoint types.
        // ---------------------------------------------------------

        managerType =
            Type.GetType(
                "CheckpointManager, Assembly-CSharp"
            );

        checkpointType =
            Type.GetType(
                "Checkpoint, Assembly-CSharp"
            );

        Assert.IsNotNull(
            managerType,
            "Could not find CheckpointManager in Assembly-CSharp."
        );

        Assert.IsNotNull(
            checkpointType,
            "Could not find Checkpoint in Assembly-CSharp."
        );

        // ---------------------------------------------------------
        // Find private serialized fields.
        // ---------------------------------------------------------

        defaultSpawnField =
            managerType.GetField(
                "defaultSpawnPoint",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        checkpointIdField =
            checkpointType.GetField(
                "checkpointId",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        respawnPointField =
            checkpointType.GetField(
                "respawnPoint",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        Assert.IsNotNull(defaultSpawnField);
        Assert.IsNotNull(checkpointIdField);
        Assert.IsNotNull(respawnPointField);

        // ---------------------------------------------------------
        // Find methods.
        // ---------------------------------------------------------

        setCheckpointMethod =
            managerType.GetMethod(
                "SetCheckpoint"
            );

        getRespawnPositionMethod =
            managerType.GetMethod(
                "GetRespawnPosition"
            );

        getRespawnRotationMethod =
            managerType.GetMethod(
                "GetRespawnRotation"
            );

        restoreCheckpointMethod =
            managerType.GetMethod(
                "RestoreCheckpointById"
            );

        Assert.IsNotNull(setCheckpointMethod);
        Assert.IsNotNull(getRespawnPositionMethod);
        Assert.IsNotNull(getRespawnRotationMethod);
        Assert.IsNotNull(restoreCheckpointMethod);

        // ---------------------------------------------------------
        // Find properties.
        // ---------------------------------------------------------

        currentCheckpointProperty =
            managerType.GetProperty(
                "CurrentCheckpoint"
            );

        currentCheckpointIdProperty =
            managerType.GetProperty(
                "CurrentCheckpointId"
            );

        isActivatedProperty =
            checkpointType.GetProperty(
                "IsActivated"
            );

        Assert.IsNotNull(currentCheckpointProperty);
        Assert.IsNotNull(currentCheckpointIdProperty);
        Assert.IsNotNull(isActivatedProperty);

        // ---------------------------------------------------------
        // Create CheckpointManager.
        // ---------------------------------------------------------

        managerObject =
            new GameObject(
                "CheckpointManager_Test"
            );

        checkpointManager =
            managerObject.AddComponent(
                managerType
            );

        // ---------------------------------------------------------
        // Create default spawn.
        // ---------------------------------------------------------

        defaultSpawnObject =
            new GameObject(
                "DefaultSpawn_Test"
            );

        defaultSpawnObject.transform.position =
            new Vector3(
                10f,
                1f,
                5f
            );

        defaultSpawnObject.transform.rotation =
            Quaternion.Euler(
                0f,
                90f,
                0f
            );

        defaultSpawnField.SetValue(
            checkpointManager,
            defaultSpawnObject.transform
        );

        // ---------------------------------------------------------
        // Create checkpoints.
        // ---------------------------------------------------------

        checkpointA =
            CreateCheckpoint(
                "Checkpoint_A_Test",
                "test_checkpoint_01",
                new Vector3(
                    2f,
                    0f,
                    2f
                ),
                out checkpointAObject
            );

        checkpointB =
            CreateCheckpoint(
                "Checkpoint_B_Test",
                "test_checkpoint_02",
                new Vector3(
                    8f,
                    0f,
                    4f
                ),
                out checkpointBObject
            );
    }

    [TearDown]
    public void TearDown()
    {
        // Destroy manager first so its static
        // Instance is cleared by OnDestroy().
        if (managerObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                managerObject
            );
        }

        if (checkpointAObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                checkpointAObject
            );
        }

        if (checkpointBObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                checkpointBObject
            );
        }

        if (defaultSpawnObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                defaultSpawnObject
            );
        }
    }

    // =========================================================
    // DEFAULT SPAWN
    // =========================================================

    [Test]
    public void NoCheckpoint_UsesDefaultSpawnPosition()
    {
        Vector3 actual =
            (Vector3)
            getRespawnPositionMethod.Invoke(
                checkpointManager,
                null
            );

        Vector3 expected =
            defaultSpawnObject.transform.position;

        Assert.That(
            Vector3.Distance(
                expected,
                actual
            ),
            Is.LessThan(0.001f),
            "Manager should use the default spawn when no checkpoint is active."
        );
    }

    [Test]
    public void NoCheckpoint_UsesDefaultSpawnRotation()
    {
        Quaternion actual =
            (Quaternion)
            getRespawnRotationMethod.Invoke(
                checkpointManager,
                null
            );

        Quaternion expected =
            defaultSpawnObject.transform.rotation;

        Assert.That(
            Quaternion.Angle(
                expected,
                actual
            ),
            Is.LessThan(0.1f),
            "Manager should use the default spawn rotation when no checkpoint is active."
        );
    }

    // =========================================================
    // CHECKPOINT SELECTION
    // =========================================================

    [Test]
    public void SetCheckpoint_SetsCurrentCheckpoint()
    {
        setCheckpointMethod.Invoke(
            checkpointManager,
            new object[]
            {
                checkpointA
            }
        );

        object current =
            currentCheckpointProperty.GetValue(
                checkpointManager
            );

        Assert.AreEqual(
            checkpointA,
            current
        );
    }

    [Test]
    public void SwitchingCheckpoints_UsesMostRecentCheckpoint()
    {
        setCheckpointMethod.Invoke(
            checkpointManager,
            new object[]
            {
                checkpointA
            }
        );

        setCheckpointMethod.Invoke(
            checkpointManager,
            new object[]
            {
                checkpointB
            }
        );

        object current =
            currentCheckpointProperty.GetValue(
                checkpointManager
            );

        Assert.AreEqual(
            checkpointB,
            current,
            "Checkpoint B should replace Checkpoint A as the current checkpoint."
        );

        string currentId =
            (string)
            currentCheckpointIdProperty.GetValue(
                checkpointManager
            );

        Assert.AreEqual(
            "test_checkpoint_02",
            currentId
        );
    }

    [Test]
    public void CurrentCheckpoint_UsesCheckpointRespawnPosition()
    {
        setCheckpointMethod.Invoke(
            checkpointManager,
            new object[]
            {
                checkpointB
            }
        );

        Vector3 actual =
            (Vector3)
            getRespawnPositionMethod.Invoke(
                checkpointManager,
                null
            );

        Vector3 expected =
            new Vector3(
                8f,
                0f,
                4f
            );

        Assert.That(
            Vector3.Distance(
                expected,
                actual
            ),
            Is.LessThan(0.001f)
        );
    }

    // =========================================================
    // INVALID CHECKPOINTS
    // =========================================================

    [Test]
    public void NullCheckpoint_DoesNotReplaceCurrentCheckpoint()
    {
        setCheckpointMethod.Invoke(
            checkpointManager,
            new object[]
            {
                checkpointA
            }
        );

        setCheckpointMethod.Invoke(
            checkpointManager,
            new object[]
            {
                null
            }
        );

        object current =
            currentCheckpointProperty.GetValue(
                checkpointManager
            );

        Assert.AreEqual(
            checkpointA,
            current,
            "Passing null should not remove the current checkpoint."
        );
    }

    [Test]
    public void InvalidCheckpointId_ReturnsFalseAndPreservesCurrentCheckpoint()
    {
        setCheckpointMethod.Invoke(
            checkpointManager,
            new object[]
            {
                checkpointA
            }
        );

        bool result =
            (bool)
            restoreCheckpointMethod.Invoke(
                checkpointManager,
                new object[]
                {
                    "checkpoint_that_does_not_exist"
                }
            );

        Assert.IsFalse(
            result,
            "Restoring an invalid ID should return false."
        );

        object current =
            currentCheckpointProperty.GetValue(
                checkpointManager
            );

        Assert.AreEqual(
            checkpointA,
            current,
            "An invalid restore should not replace a valid current checkpoint."
        );
    }

    // =========================================================
    // SAVE / LOAD RESTORE
    // =========================================================

    [Test]
    public void RestoreCheckpointById_RestoresCorrectCheckpoint()
    {
        bool result =
            (bool)
            restoreCheckpointMethod.Invoke(
                checkpointManager,
                new object[]
                {
                    "test_checkpoint_02"
                }
            );

        Assert.IsTrue(result);

        object current =
            currentCheckpointProperty.GetValue(
                checkpointManager
            );

        Assert.AreEqual(
            checkpointB,
            current
        );

        string currentId =
            (string)
            currentCheckpointIdProperty.GetValue(
                checkpointManager
            );

        Assert.AreEqual(
            "test_checkpoint_02",
            currentId
        );
    }

    [Test]
    public void RestoredCheckpoint_IsMarkedActivated()
    {
        restoreCheckpointMethod.Invoke(
            checkpointManager,
            new object[]
            {
                "test_checkpoint_02"
            }
        );

        bool activated =
            (bool)
            isActivatedProperty.GetValue(
                checkpointB
            );

        Assert.IsTrue(
            activated,
            "A restored checkpoint should be marked as activated."
        );
    }

    // =========================================================
    // HELPER
    // =========================================================

    private Component CreateCheckpoint(
        string objectName,
        string checkpointId,
        Vector3 respawnPosition,
        out GameObject checkpointObject)
    {
        checkpointObject =
            new GameObject(
                objectName
            );

        GameObject respawnObject =
            new GameObject(
                objectName +
                "_RespawnPoint"
            );

        respawnObject.transform.SetParent(
            checkpointObject.transform
        );

        respawnObject.transform.position =
            respawnPosition;

        respawnObject.transform.rotation =
            Quaternion.Euler(
                0f,
                90f,
                0f
            );

        Component checkpoint =
            checkpointObject.AddComponent(
                checkpointType
            );

        checkpointIdField.SetValue(
            checkpoint,
            checkpointId
        );

        respawnPointField.SetValue(
            checkpoint,
            respawnObject.transform
        );

        return checkpoint;
    }
}
