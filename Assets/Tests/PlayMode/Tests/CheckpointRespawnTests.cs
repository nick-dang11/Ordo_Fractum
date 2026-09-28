using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CheckpointRespawnTests
{
    private GameObject managerObject;
    private GameObject defaultSpawnObject;
    private GameObject checkpointObject;

    private Component checkpointManager;
    private Component checkpoint;

    private Type managerType;
    private Type checkpointType;
    private Type checkpointTriggerType;
    private Type checkpointRespawnType;
    private Type healthSystemType;

    private FieldInfo defaultSpawnField;
    private FieldInfo checkpointIdField;
    private FieldInfo respawnPointField;

    private MethodInfo setCheckpointMethod;
    private MethodInfo respawnMethod;

    private PropertyInfo currentCheckpointProperty;
    private PropertyInfo isActivatedProperty;

    private Transform checkpointRespawnPoint;

    [SetUp]
    public void SetUp()
    {
        // ---------------------------------------------------------
        // Find runtime types.
        // ---------------------------------------------------------

        managerType =
            Type.GetType(
                "CheckpointManager, Assembly-CSharp"
            );

        checkpointType =
            Type.GetType(
                "Checkpoint, Assembly-CSharp"
            );

        checkpointTriggerType =
            Type.GetType(
                "CheckpointTrigger, Assembly-CSharp"
            );

        checkpointRespawnType =
            Type.GetType(
                "CheckpointRespawn, Assembly-CSharp"
            );

        healthSystemType =
            Type.GetType(
                "HealthSystem, Assembly-CSharp"
            );

        Assert.IsNotNull(managerType);
        Assert.IsNotNull(checkpointType);
        Assert.IsNotNull(checkpointTriggerType);
        Assert.IsNotNull(checkpointRespawnType);
        Assert.IsNotNull(healthSystemType);

        // ---------------------------------------------------------
        // Reflection setup.
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

        setCheckpointMethod =
            managerType.GetMethod(
                "SetCheckpoint"
            );

        respawnMethod =
            checkpointRespawnType.GetMethod(
                "RespawnAtCurrentCheckpoint"
            );

        currentCheckpointProperty =
            managerType.GetProperty(
                "CurrentCheckpoint"
            );

        isActivatedProperty =
            checkpointType.GetProperty(
                "IsActivated"
            );

        Assert.IsNotNull(defaultSpawnField);
        Assert.IsNotNull(checkpointIdField);
        Assert.IsNotNull(respawnPointField);
        Assert.IsNotNull(setCheckpointMethod);
        Assert.IsNotNull(respawnMethod);
        Assert.IsNotNull(currentCheckpointProperty);
        Assert.IsNotNull(isActivatedProperty);

        // ---------------------------------------------------------
        // Manager.
        // ---------------------------------------------------------

        managerObject =
            new GameObject(
                "CheckpointManager_PlayModeTest"
            );

        checkpointManager =
            managerObject.AddComponent(
                managerType
            );

        // ---------------------------------------------------------
        // Default spawn.
        // ---------------------------------------------------------

        defaultSpawnObject =
            new GameObject(
                "DefaultSpawn_PlayModeTest"
            );

        defaultSpawnObject.transform.position =
            new Vector3(
                -5f,
                0f,
                0f
            );

        defaultSpawnField.SetValue(
            checkpointManager,
            defaultSpawnObject.transform
        );

        // ---------------------------------------------------------
        // Checkpoint.
        // ---------------------------------------------------------

        checkpointObject =
            new GameObject(
                "Checkpoint_PlayModeTest"
            );

        checkpointObject.transform.position =
            Vector3.zero;

        checkpoint =
            checkpointObject.AddComponent(
                checkpointType
            );

        checkpointIdField.SetValue(
            checkpoint,
            "playmode_checkpoint"
        );

        GameObject respawnPointObject =
            new GameObject(
                "RespawnPoint"
            );

        respawnPointObject.transform.SetParent(
            checkpointObject.transform
        );

        respawnPointObject.transform.position =
            new Vector3(
                4f,
                0f,
                3f
            );

        respawnPointObject.transform.rotation =
            Quaternion.Euler(
                0f,
                90f,
                0f
            );

        checkpointRespawnPoint =
            respawnPointObject.transform;

        respawnPointField.SetValue(
            checkpoint,
            checkpointRespawnPoint
        );
    }

    [TearDown]
    public void TearDown()
    {
        if (managerObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                managerObject
            );
        }

        if (checkpointObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                checkpointObject
            );
        }

        if (defaultSpawnObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                defaultSpawnObject
            );
        }

        GameObject[] testPlayers =
            GameObject.FindGameObjectsWithTag(
                "Player"
            );

        foreach (GameObject player in testPlayers)
        {
            if (player.name.StartsWith(
                "CheckpointTestPlayer"
            ))
            {
                UnityEngine.Object.DestroyImmediate(
                    player
                );
            }
        }
    }

    // =========================================================
    // CHECKPOINT ACTIVATION
    // =========================================================

    [UnityTest]
    public IEnumerator PlayerEnteringTrigger_ActivatesCheckpoint()
    {
        // ---------------------------------------------------------
        // Activation zone.
        // ---------------------------------------------------------

        GameObject activationZone =
            new GameObject(
                "ActivationZone"
            );

        activationZone.transform.SetParent(
            checkpointObject.transform
        );

        activationZone.transform.localPosition =
            Vector3.zero;

        BoxCollider trigger =
            activationZone.AddComponent<BoxCollider>();

        trigger.isTrigger = true;

        trigger.size =
            new Vector3(
                3f,
                3f,
                3f
            );

        activationZone.AddComponent(
            checkpointTriggerType
        );

        // ---------------------------------------------------------
        // Test player.
        // ---------------------------------------------------------

        GameObject player =
            new GameObject(
                "CheckpointTestPlayer_Activation"
            );

        player.tag = "Player";

        player.transform.position =
            new Vector3(
                5f,
                0f,
                0f
            );

        player.AddComponent<BoxCollider>();

        Rigidbody rigidbody =
            player.AddComponent<Rigidbody>();

        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;

        // Allow Awake/startup.
        yield return null;

        // Move player into the checkpoint.
        player.transform.position =
            Vector3.zero;

        Physics.SyncTransforms();

        yield return new WaitForFixedUpdate();
        yield return null;

        bool activated =
            (bool)
            isActivatedProperty.GetValue(
                checkpoint
            );

        Assert.IsTrue(
            activated,
            "Entering the checkpoint trigger should activate the checkpoint."
        );

        object current =
            currentCheckpointProperty.GetValue(
                checkpointManager
            );

        Assert.AreEqual(
            checkpoint,
            current,
            "Activated checkpoint should become the current checkpoint."
        );

        UnityEngine.Object.DestroyImmediate(
            player
        );

        UnityEngine.Object.DestroyImmediate(
            activationZone
        );
    }

    // =========================================================
    // RESPAWN + HEALTH RESET
    // =========================================================

    [UnityTest]
    public IEnumerator Respawn_MovesPlayerAndRestoresHealth()
    {
        // Make our checkpoint current.
        setCheckpointMethod.Invoke(
            checkpointManager,
            new object[]
            {
                checkpoint
            }
        );

        GameObject player =
            new GameObject(
                "CheckpointTestPlayer_Respawn"
            );

        player.transform.position =
            new Vector3(
                20f,
                0f,
                20f
            );

        // Add health first so CheckpointRespawn's
        // Awake can locate it.
        Component healthSystem =
            player.AddComponent(
                healthSystemType
            );

        Component checkpointRespawn =
            player.AddComponent(
                checkpointRespawnType
            );

        // Allow Start() to execute.
        yield return null;

        FieldInfo healthField =
            healthSystemType.GetField(
                "health"
            );

        FieldInfo maxHealthField =
            healthSystemType.GetField(
                "maxHealth"
            );

        Assert.IsNotNull(healthField);
        Assert.IsNotNull(maxHealthField);

        // Simulate a damaged player.
        healthField.SetValue(
            healthSystem,
            1
        );

        // Perform actual checkpoint respawn.
        respawnMethod.Invoke(
            checkpointRespawn,
            null
        );

        yield return null;

        // ---------------------------------------------------------
        // Position.
        // ---------------------------------------------------------

        Assert.That(
            Vector3.Distance(
                player.transform.position,
                checkpointRespawnPoint.position
            ),
            Is.LessThan(0.001f),
            "Player should respawn at the checkpoint's RespawnPoint."
        );

        // ---------------------------------------------------------
        // Rotation.
        // ---------------------------------------------------------

        Assert.That(
            Quaternion.Angle(
                player.transform.rotation,
                checkpointRespawnPoint.rotation
            ),
            Is.LessThan(0.1f),
            "Player should use the checkpoint RespawnPoint rotation."
        );

        // ---------------------------------------------------------
        // Health.
        // ---------------------------------------------------------

        int health =
            (int)
            healthField.GetValue(
                healthSystem
            );

        int maxHealth =
            (int)
            maxHealthField.GetValue(
                healthSystem
            );

        Assert.AreEqual(
            maxHealth,
            health,
            "Respawning should restore the player's health."
        );

        UnityEngine.Object.DestroyImmediate(
            player
        );
    }
}
