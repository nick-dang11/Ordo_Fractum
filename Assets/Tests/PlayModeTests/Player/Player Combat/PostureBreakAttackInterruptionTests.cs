using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/*
 * PostureBreakAttackInterruptionTests
 *
 * Purpose:
 * Provides automated Play Mode integration coverage for interruption of an
 * active enemy attack when the enemy's posture is broken.
 *
 * The fixture establishes the minimum runtime chain required to observe:
 *
 * EnemyCombat
 * -> AttackTimelineDriver
 * -> EnemyWeaponHitbox
 * -> EnemyPosture
 * -> EnemyStateManager
 * -> EnemyStunnedState
 *
 * The enemy attack is placed inside an active hitbox window before posture
 * damage reaches the configured break threshold.
 *
 * These tests intentionally do NOT retest:
 * - AttackData frame validation or normalized timing calculations
 * - Generic WeaponHitbox enable / disable lifecycle behavior
 * - Basic EnemyCombat attack eligibility
 *
 * Those behaviors are already covered by:
 * - AttackDataTests
 * - HitboxLifecycleTests
 * - EnemyCombatTests
 *
 *
 * Testing Methodologies:
 *
 * Play Mode Integration Testing
 * - PostureBreak_DuringActiveAttack_InterruptsEnemyAttack
 * - PostureBreak_DuringActiveAttack_DisablesActiveHitbox
 * - PostureBreak_ClearsAttackLifecycleAndRejectsStaleTimeline
 * - PostureBreak_InterruptedAttack_CannotContinueDamagingPlayer
 *
 * State Transition Testing
 * - PostureBreak_DuringActiveAttack_InterruptsEnemyAttack
 *   verifies the transition from an active enemy attack into
 *   EnemyStunnedState while terminating the previous attack lifecycle.
 *
 * Negative / Error-Path Testing
 * - PostureBreak_ClearsAttackLifecycleAndRejectsStaleTimeline
 *   verifies that stale timeline evaluation from the interrupted attack
 *   cannot reactivate the hitbox.
 *
 * - PostureBreak_InterruptedAttack_CannotContinueDamagingPlayer
 *   verifies that an interrupted attack cannot continue producing damage.
 *
 * Regression Testing
 * - All tests protect the shared posture-break attack-interruption contract
 *   from future changes to EnemyCombat, EnemyPosture, EnemyStateManager,
 *   AttackTimelineDriver, and WeaponHitbox.
 *
 *
 * Scope:
 * This fixture directly applies posture damage after establishing an active
 * enemy attack.
 *
 * Player deflect recognition and the decision to apply posture damage after
 * a successful deflect remain outside the scope of this test fixture.
 */

public class PostureBreakAttackInterruptionTests
{
    private const float MaxPosture = 100f;

    private GameObject enemyObject;
    private GameObject enemyWeaponObject;

    private readonly List<GameObject> playerObjects =
        new List<GameObject>();

    private Component enemyCombat;
    private Component enemyPosture;
    private Component enemyStateManager;
    private Component attackTimelineDriver;
    private Component enemyWeaponHitbox;

    private Collider enemyWeaponCollider;

    private ScriptableObject attackData;

    // =========================================================
    // TYPES
    // =========================================================

    private Type enemyCombatType;
    private Type enemyPostureType;
    private Type enemyStateManagerType;

    private Type attackTimelineDriverType;
    private Type attackDataType;

    private Type weaponHitboxType;
    private Type enemyWeaponHitboxType;

    private Type playerCombatType;
    private Type playerHealthType;

    // =========================================================
    // METHODS
    // =========================================================

    private MethodInfo applyPostureDamageMethod;

    private MethodInfo beginAttackMethod;
    private MethodInfo evaluateAttackMethod;

    private MethodInfo onTriggerEnterMethod;

    // =========================================================
    // PROPERTIES
    // =========================================================

    private PropertyInfo enemyIsAttackingProperty;
    private PropertyInfo currentAttackInvalidatedProperty;

    private PropertyInfo isPostureBrokenProperty;

    private PropertyInfo currentStateProperty;
    private PropertyInfo stunnedStateProperty;

    // =========================================================
    // ENEMY COMBAT FIELDS
    // =========================================================

    private FieldInfo enemyIsAttackingField;
    private FieldInfo enemyWeaponHitboxField;
    private FieldInfo enemyAttackTimelineDriverField;

    // =========================================================
    // POSTURE FIELDS
    // =========================================================

    private FieldInfo maxPostureField;
    private FieldInfo postureStateManagerField;

    // =========================================================
    // TIMELINE FIELDS
    // =========================================================

    private FieldInfo timelineWeaponHitboxField;
    private FieldInfo timelineCurrentAttackDataField;
    private FieldInfo timelineHitboxActiveField;
    private FieldInfo timelineCurrentDamageField;

    // =========================================================
    // HITBOX FIELDS
    // =========================================================

    private FieldInfo hitboxActiveField;

    // =========================================================
    // ATTACK DATA FIELDS
    // =========================================================

    private FieldInfo attackStartFrameField;
    private FieldInfo attackEndFrameField;
    private FieldInfo attackTotalFramesField;

    // =========================================================
    // PLAYER HEALTH FIELDS
    // =========================================================

    private FieldInfo playerHealthField;
    private FieldInfo playerMaxHealthField;

    // =========================================================
    // SETUP / TEARDOWN
    // =========================================================

    [SetUp]
    public void SetUp()
    {
        ResolveTypes();
        ResolveMembers();

        CreateEnemyFixture();
        CreateAttackData();
    }

    [TearDown]
    public void TearDown()
    {
        if (attackData != null)
        {
            UnityEngine.Object.DestroyImmediate(
                attackData
            );
        }

        foreach (GameObject playerObject in playerObjects)
        {
            if (playerObject != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    playerObject
                );
            }
        }

        playerObjects.Clear();

        if (enemyObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                enemyObject
            );
        }
    }

    // =========================================================
    // PLAY MODE INTEGRATION / STATE TRANSITION TESTING
    // =========================================================

    [UnityTest]
    public IEnumerator PostureBreak_DuringActiveAttack_InterruptsEnemyAttack()
    {
        yield return null;

        int attackToken =
            BeginActiveAttack();

        Assert.Greater(
            attackToken,
            0,
            "Fixture should establish a valid attack token."
        );

        Assert.IsTrue(
            GetBoolProperty(
                enemyIsAttackingProperty,
                enemyCombat
            ),
            "Enemy should be attacking before posture is broken."
        );

        Assert.IsTrue(
            enemyWeaponCollider.enabled,
            "Enemy weapon collider should be active before posture break."
        );

        // Act
        BreakPosture();

        yield return null;

        // Assert posture break occurred.
        Assert.IsTrue(
            GetBoolProperty(
                isPostureBrokenProperty,
                enemyPosture
            ),
            "Enemy posture should be broken after reaching the configured threshold."
        );

        object currentState =
            currentStateProperty.GetValue(
                enemyStateManager
            );

        object stunnedState =
            stunnedStateProperty.GetValue(
                enemyStateManager
            );

        Assert.AreSame(
            stunnedState,
            currentState,
            "Posture break should transition the enemy into EnemyStunnedState."
        );

        // Assert active attack was interrupted.
        Assert.IsFalse(
            GetBoolProperty(
                enemyIsAttackingProperty,
                enemyCombat
            ),
            "Posture break should terminate the active enemy attack."
        );

        Assert.IsTrue(
            GetBoolProperty(
                currentAttackInvalidatedProperty,
                enemyCombat
            ),
            "The interrupted attack should be invalidated so it cannot resume."
        );
    }

    // =========================================================
    // PLAY MODE INTEGRATION / HITBOX SHUTDOWN TESTING
    // =========================================================

    [UnityTest]
    public IEnumerator PostureBreak_DuringActiveAttack_DisablesActiveHitbox()
    {
        yield return null;

        BeginActiveAttack();

        Assert.IsTrue(
            enemyWeaponCollider.enabled,
            "Precondition failed: enemy weapon collider should be active before posture break."
        );

        Assert.IsTrue(
            IsHitboxActive(),
            "Precondition failed: WeaponHitbox should report an active hitbox."
        );

        // Act
        BreakPosture();

        yield return null;

        // Assert
        Assert.IsFalse(
            enemyWeaponCollider.enabled,
            "Posture break should disable the active enemy weapon collider."
        );

        Assert.IsFalse(
            IsHitboxActive(),
            "Posture break should clear WeaponHitbox's active-state flag."
        );
    }

    // =========================================================
    // NEGATIVE / ERROR-PATH + REGRESSION TESTING
    // =========================================================

    [UnityTest]
    public IEnumerator PostureBreak_ClearsAttackLifecycleAndRejectsStaleTimeline()
    {
        yield return null;

        int interruptedAttackToken =
            BeginActiveAttack();

        Assert.IsTrue(
            enemyWeaponCollider.enabled,
            "Precondition failed: hitbox should be active before interruption."
        );

        // Act
        BreakPosture();

        yield return null;

        /*
         * The interrupted timeline should no longer contain an active
         * attack lifecycle.
         */

        object currentAttackData =
            timelineCurrentAttackDataField.GetValue(
                attackTimelineDriver
            );

        bool timelineHitboxActive =
            (bool)timelineHitboxActiveField.GetValue(
                attackTimelineDriver
            );

        float timelineDamage =
            (float)timelineCurrentDamageField.GetValue(
                attackTimelineDriver
            );

        Assert.IsNull(
            currentAttackData,
            "Posture break should clear the timeline's active AttackData."
        );

        Assert.IsFalse(
            timelineHitboxActive,
            "Posture break should clear the timeline's active-hitbox state."
        );

        Assert.AreEqual(
            0f,
            timelineDamage,
            0.001f,
            "Posture break should clear the timeline's stored attack damage."
        );

        /*
         * Negative-path regression:
         *
         * Simulate AttackStateBehaviour attempting another update using
         * the token belonging to the interrupted attack.
         */

        evaluateAttackMethod.Invoke(
            attackTimelineDriver,
            new object[]
            {
                interruptedAttackToken,
                0.50f
            }
        );

        yield return null;

        Assert.IsFalse(
            enemyWeaponCollider.enabled,
            "A stale timeline update must not reactivate the interrupted enemy hitbox."
        );

        Assert.IsFalse(
            IsHitboxActive(),
            "A stale timeline update must not restore WeaponHitbox active state."
        );
    }

    // =========================================================
    // NEGATIVE / ERROR-PATH DAMAGE REGRESSION
    // =========================================================

    [UnityTest]
    public IEnumerator PostureBreak_InterruptedAttack_CannotContinueDamagingPlayer()
    {
        yield return null;

        int interruptedAttackToken =
            BeginActiveAttack();

        Assert.IsTrue(
            enemyWeaponCollider.enabled,
            "Precondition failed: enemy hitbox should be active before posture break."
        );

        // -----------------------------------------------------
        // PLAYER A:
        // Prove the active enemy attack is capable of damage.
        // -----------------------------------------------------

        Collider firstPlayerCollider;
        Component firstPlayerHealth;

        CreatePlayerTarget(
            "PostureBreakAttackInterruption_PlayerA",
            out firstPlayerCollider,
            out firstPlayerHealth
        );

        yield return null;

        int firstPlayerMaxHealth =
            (int)playerMaxHealthField.GetValue(
                firstPlayerHealth
            );

        playerHealthField.SetValue(
            firstPlayerHealth,
            firstPlayerMaxHealth
        );

        onTriggerEnterMethod.Invoke(
            enemyWeaponHitbox,
            new object[]
            {
                firstPlayerCollider
            }
        );

        int firstPlayerHealthAfterHit =
            (int)playerHealthField.GetValue(
                firstPlayerHealth
            );

        Assert.Less(
            firstPlayerHealthAfterHit,
            firstPlayerMaxHealth,
            "Fixture validation failed: the active enemy attack should be capable of damaging the player."
        );

        // -----------------------------------------------------
        // INTERRUPT THE ATTACK
        // -----------------------------------------------------

        BreakPosture();

        yield return null;

        /*
         * Simulate stale timeline activity from the attack that was
         * interrupted by posture break.
         */

        evaluateAttackMethod.Invoke(
            attackTimelineDriver,
            new object[]
            {
                interruptedAttackToken,
                0.50f
            }
        );

        yield return null;

        // -----------------------------------------------------
        // PLAYER B:
        //
        // Use a fresh target after interruption.
        //
        // This prevents WeaponHitbox's duplicate-target HashSet from
        // accidentally making the regression test pass.
        // -----------------------------------------------------

        Collider secondPlayerCollider;
        Component secondPlayerHealth;

        CreatePlayerTarget(
            "PostureBreakAttackInterruption_PlayerB",
            out secondPlayerCollider,
            out secondPlayerHealth
        );

        yield return null;

        int secondPlayerMaxHealth =
            (int)playerMaxHealthField.GetValue(
                secondPlayerHealth
            );

        playerHealthField.SetValue(
            secondPlayerHealth,
            secondPlayerMaxHealth
        );

        int healthBeforeInterruptedContact =
            (int)playerHealthField.GetValue(
                secondPlayerHealth
            );

        /*
         * Simulate contact from the interrupted attack against a target
         * that was never previously registered in the attack window.
         */

        onTriggerEnterMethod.Invoke(
            enemyWeaponHitbox,
            new object[]
            {
                secondPlayerCollider
            }
        );

        yield return null;

        int healthAfterInterruptedContact =
            (int)playerHealthField.GetValue(
                secondPlayerHealth
            );

        Assert.AreEqual(
            healthBeforeInterruptedContact,
            healthAfterInterruptedContact,
            "An enemy attack interrupted by posture break must not damage a new valid player target."
        );

        Assert.IsFalse(
            enemyWeaponCollider.enabled,
            "The interrupted attack should remain unable to reopen its weapon collider."
        );

        Assert.IsFalse(
            IsHitboxActive(),
            "The interrupted attack should remain unable to restore WeaponHitbox active state."
        );
    }

    // =========================================================
    // TYPE RESOLUTION
    // =========================================================

    private void ResolveTypes()
    {
        enemyCombatType =
            Type.GetType(
                "EnemyCombat, Assembly-CSharp"
            );

        enemyPostureType =
            Type.GetType(
                "EnemyPosture, Assembly-CSharp"
            );

        enemyStateManagerType =
            Type.GetType(
                "EnemyStateManager, Assembly-CSharp"
            );

        attackTimelineDriverType =
            Type.GetType(
                "AttackTimelineDriver, Assembly-CSharp"
            );

        attackDataType =
            Type.GetType(
                "AttackData, Assembly-CSharp"
            );

        weaponHitboxType =
            Type.GetType(
                "WeaponHitbox, Assembly-CSharp"
            );

        enemyWeaponHitboxType =
            Type.GetType(
                "EnemyWeaponHitbox, Assembly-CSharp"
            );

        playerCombatType =
            Type.GetType(
                "PlayerCombat, Assembly-CSharp"
            );

        playerHealthType =
            Type.GetType(
                "PlayerHealth, Assembly-CSharp"
            );

        Assert.IsNotNull(
            enemyCombatType,
            "Could not find EnemyCombat in Assembly-CSharp."
        );

        Assert.IsNotNull(
            enemyPostureType,
            "Could not find EnemyPosture in Assembly-CSharp."
        );

        Assert.IsNotNull(
            enemyStateManagerType,
            "Could not find EnemyStateManager in Assembly-CSharp."
        );

        Assert.IsNotNull(
            attackTimelineDriverType,
            "Could not find AttackTimelineDriver in Assembly-CSharp."
        );

        Assert.IsNotNull(
            attackDataType,
            "Could not find AttackData in Assembly-CSharp."
        );

        Assert.IsNotNull(
            weaponHitboxType,
            "Could not find WeaponHitbox in Assembly-CSharp."
        );

        Assert.IsNotNull(
            enemyWeaponHitboxType,
            "Could not find EnemyWeaponHitbox in Assembly-CSharp."
        );

        Assert.IsNotNull(
            playerCombatType,
            "Could not find PlayerCombat in Assembly-CSharp."
        );

        Assert.IsNotNull(
            playerHealthType,
            "Could not find PlayerHealth in Assembly-CSharp."
        );
    }

    // =========================================================
    // MEMBER RESOLUTION
    // =========================================================

    private void ResolveMembers()
    {
        applyPostureDamageMethod =
            enemyPostureType.GetMethod(
                "ApplyPostureDamage",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        beginAttackMethod =
            attackTimelineDriverType.GetMethod(
                "BeginAttack",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        evaluateAttackMethod =
            attackTimelineDriverType.GetMethod(
                "EvaluateAttack",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        onTriggerEnterMethod =
            weaponHitboxType.GetMethod(
                "OnTriggerEnter",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        enemyIsAttackingProperty =
            enemyCombatType.GetProperty(
                "IsAttacking",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        currentAttackInvalidatedProperty =
            enemyCombatType.GetProperty(
                "CurrentAttackInvalidated",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        isPostureBrokenProperty =
            enemyPostureType.GetProperty(
                "IsPostureBroken",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        currentStateProperty =
            enemyStateManagerType.GetProperty(
                "CurrentState",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        stunnedStateProperty =
            enemyStateManagerType.GetProperty(
                "StunnedState",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        enemyAttackTimelineDriverField =
            enemyCombatType.GetField(
                "attackTimelineDriver",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        enemyIsAttackingField =
            enemyCombatType.GetField(
                "isAttacking",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        enemyWeaponHitboxField =
            enemyCombatType.GetField(
                "weaponHitbox",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        maxPostureField =
            enemyPostureType.GetField(
                "maxPosture",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        postureStateManagerField =
            enemyPostureType.GetField(
                "stateManager",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        timelineWeaponHitboxField =
            attackTimelineDriverType.GetField(
                "weaponHitbox",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        timelineCurrentAttackDataField =
            attackTimelineDriverType.GetField(
                "currentAttackData",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        timelineHitboxActiveField =
            attackTimelineDriverType.GetField(
                "hitboxActive",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        timelineCurrentDamageField =
            attackTimelineDriverType.GetField(
                "currentDamage",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        hitboxActiveField =
            weaponHitboxType.GetField(
                "isHitboxActive",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        attackStartFrameField =
            attackDataType.GetField(
                "startHitFrame",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        attackEndFrameField =
            attackDataType.GetField(
                "endHitFrame",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        attackTotalFramesField =
            attackDataType.GetField(
                "totalFrames",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        playerHealthField =
            playerHealthType.GetField(
                "health",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        playerMaxHealthField =
            playerHealthType.GetField(
                "maxHealth",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        Assert.IsNotNull(
            enemyAttackTimelineDriverField,
            "Could not find EnemyCombat.attackTimelineDriver."
        );

        Assert.IsNotNull(
            applyPostureDamageMethod,
            "Could not find EnemyPosture.ApplyPostureDamage."
        );

        Assert.IsNotNull(
            beginAttackMethod,
            "Could not find AttackTimelineDriver.BeginAttack."
        );

        Assert.IsNotNull(
            evaluateAttackMethod,
            "Could not find AttackTimelineDriver.EvaluateAttack."
        );

        Assert.IsNotNull(
            onTriggerEnterMethod,
            "Could not find WeaponHitbox.OnTriggerEnter."
        );

        Assert.IsNotNull(
            enemyIsAttackingProperty,
            "Could not find EnemyCombat.IsAttacking."
        );

        Assert.IsNotNull(
            currentAttackInvalidatedProperty,
            "Could not find EnemyCombat.CurrentAttackInvalidated."
        );

        Assert.IsNotNull(
            isPostureBrokenProperty,
            "Could not find EnemyPosture.IsPostureBroken."
        );

        Assert.IsNotNull(
            currentStateProperty,
            "Could not find EnemyStateManager.CurrentState."
        );

        Assert.IsNotNull(
            stunnedStateProperty,
            "Could not find EnemyStateManager.StunnedState."
        );

        Assert.IsNotNull(
            enemyIsAttackingField,
            "Could not find EnemyCombat.isAttacking."
        );

        Assert.IsNotNull(
            enemyWeaponHitboxField,
            "Could not find EnemyCombat.weaponHitbox."
        );

        Assert.IsNotNull(
            maxPostureField,
            "Could not find EnemyPosture.maxPosture."
        );

        Assert.IsNotNull(
            postureStateManagerField,
            "Could not find EnemyPosture.stateManager."
        );

        Assert.IsNotNull(
            timelineWeaponHitboxField,
            "Could not find AttackTimelineDriver.weaponHitbox."
        );

        Assert.IsNotNull(
            timelineCurrentAttackDataField,
            "Could not find AttackTimelineDriver.currentAttackData."
        );

        Assert.IsNotNull(
            timelineHitboxActiveField,
            "Could not find AttackTimelineDriver.hitboxActive."
        );

        Assert.IsNotNull(
            timelineCurrentDamageField,
            "Could not find AttackTimelineDriver.currentDamage."
        );

        Assert.IsNotNull(
            hitboxActiveField,
            "Could not find WeaponHitbox.isHitboxActive."
        );

        Assert.IsNotNull(
            attackStartFrameField,
            "Could not find AttackData.startHitFrame."
        );

        Assert.IsNotNull(
            attackEndFrameField,
            "Could not find AttackData.endHitFrame."
        );

        Assert.IsNotNull(
            attackTotalFramesField,
            "Could not find AttackData.totalFrames."
        );

        Assert.IsNotNull(
            playerHealthField,
            "Could not find PlayerHealth.health."
        );

        Assert.IsNotNull(
            playerMaxHealthField,
            "Could not find PlayerHealth.maxHealth."
        );
    }

    // =========================================================
    // FIXTURE CREATION
    // =========================================================

    private void CreateEnemyFixture()
    {
        enemyObject =
            new GameObject(
                "PostureBreakAttackInterruption_Enemy"
            );

        /*
         * EnemyCombat is created before AttackTimelineDriver so that
         * AttackTimelineDriver.Awake() can resolve IAttackDamageSource.
         */

        enemyCombat =
            enemyObject.AddComponent(
                enemyCombatType
            );

        enemyPosture =
            enemyObject.AddComponent(
                enemyPostureType
            );

        enemyStateManager =
            enemyObject.AddComponent(
                enemyStateManagerType
            );

        attackTimelineDriver =
            enemyObject.AddComponent(
                attackTimelineDriverType
            );

        enemyWeaponObject =
            new GameObject(
                "PostureBreakAttackInterruption_EnemyWeapon"
            );

        enemyWeaponObject.transform.SetParent(
            enemyObject.transform
        );

        enemyWeaponCollider =
            enemyWeaponObject.AddComponent<BoxCollider>();

        enemyWeaponCollider.isTrigger = true;

        enemyWeaponHitbox =
            enemyWeaponObject.AddComponent(
                enemyWeaponHitboxType
            );

        // Deterministic posture configuration.
        maxPostureField.SetValue(
            enemyPosture,
            MaxPosture
        );

        postureStateManagerField.SetValue(
            enemyPosture,
            enemyStateManager
        );

        // Give EnemyCombat direct access to its hitbox.
        enemyWeaponHitboxField.SetValue(
            enemyCombat,
            enemyWeaponHitbox
        );

        // Give AttackTimelineDriver access to the same hitbox.
        timelineWeaponHitboxField.SetValue(
            attackTimelineDriver,
            enemyWeaponHitbox
        );

        enemyAttackTimelineDriverField.SetValue(
            enemyCombat,
            attackTimelineDriver
        );
    }

    private void CreateAttackData()
    {
        attackData =
            ScriptableObject.CreateInstance(
                attackDataType
            );

        attackData.name =
            "PostureBreakAttackInterruption_TestAttack";

        /*
         * These values only establish a broad active window.
         * AttackData conversion behavior is tested elsewhere.
         */

        attackStartFrameField.SetValue(
            attackData,
            25
        );

        attackEndFrameField.SetValue(
            attackData,
            75
        );

        attackTotalFramesField.SetValue(
            attackData,
            100
        );
    }

    private void CreatePlayerTarget(
        string objectName,
        out Collider playerCollider,
        out Component playerHealth)
    {
        GameObject playerObject =
            new GameObject(
                objectName
            );

        playerObjects.Add(
            playerObject
        );

        /*
         * EnemyWeaponHitbox and WeaponHitbox identify the logical player
         * combat target through PlayerCombat, so include the real runtime
         * component without introducing a compile-time test reference.
         */

        playerObject.AddComponent(
            playerCombatType
        );

        playerHealth =
            playerObject.AddComponent(
                playerHealthType
            );

        playerCollider =
            playerObject.AddComponent<BoxCollider>();

        int maxHealth =
            (int)playerMaxHealthField.GetValue(
                playerHealth
            );

        playerHealthField.SetValue(
            playerHealth,
            maxHealth
        );
    }

    // =========================================================
    // ATTACK / POSTURE HELPERS
    // =========================================================

    private int BeginActiveAttack()
    {
        /*
         * TryAttack is intentionally not retested here.
         *
         * EnemyCombatTests already covers attack eligibility.
         * This integration fixture begins after the attack has already
         * been accepted.
         */

        enemyIsAttackingField.SetValue(
            enemyCombat,
            true
        );

        int attackToken =
            (int)beginAttackMethod.Invoke(
                attackTimelineDriver,
                new object[]
                {
                    attackData
                }
            );

        /*
         * 0.50 lies inside the configured 0.25 -> 0.75 hit window.
         */

        evaluateAttackMethod.Invoke(
            attackTimelineDriver,
            new object[]
            {
                attackToken,
                0.50f
            }
        );

        Assert.IsTrue(
            enemyWeaponCollider.enabled,
            "Fixture failed to establish an active enemy weapon collider."
        );

        Assert.IsTrue(
            IsHitboxActive(),
            "Fixture failed to establish WeaponHitbox active state."
        );

        return attackToken;
    }

    private void BreakPosture()
    {
        applyPostureDamageMethod.Invoke(
            enemyPosture,
            new object[]
            {
                MaxPosture
            }
        );
    }

    // =========================================================
    // VALUE HELPERS
    // =========================================================

    private bool IsHitboxActive()
    {
        return
            (bool)hitboxActiveField.GetValue(
                enemyWeaponHitbox
            );
    }

    private static bool GetBoolProperty(
        PropertyInfo property,
        object target)
    {
        return
            (bool)property.GetValue(
                target
            );
    }
}