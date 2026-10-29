using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/*
 * ExecutionEligibilityTests
 *
 * Purpose:
 * Provides automated Play Mode regression coverage for enemy execution
 * eligibility and execution-state transitions.
 *
 * Execution eligibility exists only while the enemy is in
 * EnemyStunnedState. The enemy must still be alive, within the player's
 * configured execution range, and not already executing.
 *
 * If the stun timer expires before execution is accepted, the enemy recovers
 * posture, returns to Neutral / normal combat, and is no longer execution
 * eligible.
 *
 * These tests intentionally do NOT retest:
 * - Posture accumulation or posture-break threshold behavior
 * - Enemy attack interruption / hitbox shutdown
 * - Player execution animation quality
 * - Execution-anchor visual alignment
 *
 * Those behaviors are covered by existing automated tests or the later
 * manual execution acceptance test.
 *
 *
 * Testing Methodologies:
 *
 * Decision Table Testing
 * - Execution_NotStunned_IsDenied
 * - Execution_StunnedButOutsideRange_IsDenied
 * - Execution_StunnedAliveAndInRange_IsAccepted
 * - Execution_DeadEnemy_IsDenied
 * - Execution_DuplicateAttempt_IsDenied
 * - Execution_RecoveryExpires_RemovesEligibility
 *
 * State Transition Testing
 * - Execution_StunnedAliveAndInRange_IsAccepted
 *   verifies EnemyStunnedState -> EnemyExecutedState
 *
 * - Execution_RecoveryExpires_RemovesEligibility
 *   verifies EnemyStunnedState -> posture recovery -> EnemyNeutralState
 *
 * - Execution_AcceptedBeforeRecovery_TransitionsToDeathExactlyOnce
 *   verifies EnemyStunnedState -> EnemyExecutedState -> EnemyDeathState
 *
 * Negative-Path Testing
 * - Not-stunned execution is denied
 * - Out-of-range execution is denied
 * - Dead-enemy execution is denied
 * - Duplicate execution is denied
 * - Recovered enemies are no longer execution eligible
 *
 * Regression Testing
 * - All tests protect the intended execution eligibility window and
 *   finisher-state lifecycle from future changes.
 *
 *
 * Scope:
 * Player execution range is tested through PlayerExecution.TryExecute().
 * Enemy execution-state eligibility is tested through the real
 * EnemyStateManager state machine.
 *
 * Reflection is used consistently so the Play Mode test assembly does not
 * require direct compile-time references to production gameplay classes.
 */

public class ExecutionEligibilityTests
{
    private const float ExecutionRange = 2f;

    private GameObject enemyObject;
    private GameObject executionAnchorObject;
    private GameObject playerObject;

    private Component enemyCombat;
    private Component enemyPosture;
    private Component enemyStateManager;

    private Component playerExecution;
    private Component playerStateManager;
    private Component playerMovement;

    private Collider enemyCollider;

    // =========================================================
    // TYPES
    // =========================================================

    private Type enemyCombatType;
    private Type enemyPostureType;
    private Type enemyStateManagerType;
    private Type enemyStunnedStateType;

    private Type playerExecutionType;
    private Type playerStateManagerType;
    private Type playerMovementType;

    // =========================================================
    // ENEMY METHODS
    // =========================================================

    private MethodInfo requestStunMethod;
    private MethodInfo requestExecutionMethod;
    private MethodInfo requestDeathMethod;

    // =========================================================
    // PLAYER EXECUTION METHODS
    // =========================================================

    private MethodInfo tryExecuteMethod;
    private MethodInfo finishExecutionMethod;

    // =========================================================
    // ENEMY PROPERTIES
    // =========================================================

    private PropertyInfo currentEnemyStateProperty;
    private PropertyInfo stunnedStateProperty;
    private PropertyInfo executedStateProperty;
    private PropertyInfo deathStateProperty;
    private PropertyInfo neutralStateProperty;

    private PropertyInfo isBeingExecutedProperty;
    private PropertyInfo isDeadProperty;
    private PropertyInfo isPostureBrokenProperty;

    // =========================================================
    // ENEMY FIELDS
    // =========================================================

    private FieldInfo executionAnchorField;
    private FieldInfo stunnedStateEndTimeField;

    // =========================================================
    // PLAYER EXECUTION FIELDS
    // =========================================================

    private FieldInfo executionStateManagerField;
    private FieldInfo executionRangeField;
    private FieldInfo enemyLayerMaskField;
    private FieldInfo executionTargetField;

    // =========================================================
    // PLAYER STATE MANAGER FIELDS
    // =========================================================

    private FieldInfo playerCurrentStateField;
    private FieldInfo playerIdleStateField;
    private FieldInfo playerExecutingStateField;
    private FieldInfo playerMovementField;
    private FieldInfo playerExecutionField;

    // =========================================================
    // SETUP / TEARDOWN
    // =========================================================

    [SetUp]
    public void SetUp()
    {
        ResolveTypes();
        ResolveMembers();

        CreateEnemyFixture();
    }

    [TearDown]
    public void TearDown()
    {
        if (playerObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                playerObject
            );
        }

        if (enemyObject != null)
        {
            UnityEngine.Object.DestroyImmediate(
                enemyObject
            );
        }
    }

    // =========================================================
    // DECISION TABLE / NEGATIVE-PATH TESTING
    // =========================================================

    [UnityTest]
    public IEnumerator Execution_NotStunned_IsDenied()
    {
        yield return null;

        bool accepted =
            (bool)requestExecutionMethod.Invoke(
                enemyStateManager,
                null
            );

        Assert.IsFalse(
            accepted,
            "Execution should be denied when the enemy is not in EnemyStunnedState."
        );

        Assert.AreNotSame(
            GetEnemyState(executedStateProperty),
            GetCurrentEnemyState(),
            "A denied execution attempt must not transition the enemy into EnemyExecutedState."
        );

        Assert.IsFalse(
            GetBoolProperty(
                isBeingExecutedProperty,
                enemyCombat
            ),
            "A denied execution attempt must not mark the enemy as being executed."
        );
    }

    [UnityTest]
    public IEnumerator Execution_StunnedButOutsideRange_IsDenied()
    {
        CreatePlayerFixture();

        playerObject.transform.position =
            Vector3.zero;

        enemyObject.transform.position =
            new Vector3(
                ExecutionRange + 5f,
                0f,
                0f
            );

        EnterStunnedState();

        Physics.SyncTransforms();

        yield return null;

        bool accepted =
            (bool)tryExecuteMethod.Invoke(
                playerExecution,
                null
            );

        Assert.IsFalse(
            accepted,
            "PlayerExecution should deny execution when the stunned enemy is outside execution range."
        );

        Assert.AreSame(
            GetEnemyState(stunnedStateProperty),
            GetCurrentEnemyState(),
            "An out-of-range execution attempt should leave the enemy in EnemyStunnedState."
        );

        Assert.IsFalse(
            GetBoolProperty(
                isBeingExecutedProperty,
                enemyCombat
            ),
            "An out-of-range execution attempt must not begin execution."
        );
    }

    // =========================================================
    // DECISION TABLE / VALID STATE TRANSITION
    // =========================================================

    [UnityTest]
    public IEnumerator Execution_StunnedAliveAndInRange_IsAccepted()
    {
        CreatePlayerFixture();

        playerObject.transform.position =
            Vector3.zero;

        enemyObject.transform.position =
            new Vector3(
                1f,
                0f,
                0f
            );

        EnterStunnedState();

        Physics.SyncTransforms();

        yield return null;

        bool accepted =
            (bool)tryExecuteMethod.Invoke(
                playerExecution,
                null
            );

        Assert.IsTrue(
            accepted,
            "Execution should be accepted for a living stunned enemy inside execution range."
        );

        Assert.AreSame(
            GetEnemyState(executedStateProperty),
            GetCurrentEnemyState(),
            "Accepted execution should transition EnemyStunnedState into EnemyExecutedState."
        );

        Assert.IsTrue(
            GetBoolProperty(
                isBeingExecutedProperty,
                enemyCombat
            ),
            "Accepted execution should mark EnemyCombat as being executed."
        );

        object executionTarget =
            executionTargetField.GetValue(
                playerExecution
            );

        Assert.AreSame(
            enemyStateManager,
            executionTarget,
            "PlayerExecution should retain the accepted enemy as its execution target."
        );
    }

    // =========================================================
    // DEAD / INVALID ENEMY NEGATIVE PATH
    // =========================================================

    [UnityTest]
    public IEnumerator Execution_DeadEnemy_IsDenied()
    {
        EnterStunnedState();

        requestDeathMethod.Invoke(
            enemyStateManager,
            null
        );

        yield return null;

        Assert.AreSame(
            GetEnemyState(deathStateProperty),
            GetCurrentEnemyState(),
            "Precondition failed: enemy should be in EnemyDeathState."
        );

        Assert.IsTrue(
            GetBoolProperty(
                isDeadProperty,
                enemyCombat
            ),
            "Precondition failed: EnemyCombat should report the enemy as dead."
        );

        bool accepted =
            (bool)requestExecutionMethod.Invoke(
                enemyStateManager,
                null
            );

        Assert.IsFalse(
            accepted,
            "Execution should be denied for a dead enemy."
        );

        Assert.AreSame(
            GetEnemyState(deathStateProperty),
            GetCurrentEnemyState(),
            "A dead enemy must remain in EnemyDeathState after an execution attempt."
        );
    }

    // =========================================================
    // DUPLICATE EXECUTION NEGATIVE PATH
    // =========================================================

    [Test]
    public void Execution_DuplicateAttempt_IsDenied()
    {
        EnterStunnedState();

        bool firstAccepted =
            (bool)requestExecutionMethod.Invoke(
                enemyStateManager,
                null
            );

        Assert.IsTrue(
            firstAccepted,
            "Precondition failed: first execution request should be accepted from EnemyStunnedState."
        );

        Assert.AreSame(
            GetEnemyState(executedStateProperty),
            GetCurrentEnemyState(),
            "First execution request should transition into EnemyExecutedState."
        );

        bool duplicateAccepted =
            (bool)requestExecutionMethod.Invoke(
                enemyStateManager,
                null
            );

        Assert.IsFalse(
            duplicateAccepted,
            "A second execution request should be rejected once execution is already active."
        );

        Assert.AreSame(
            GetEnemyState(executedStateProperty),
            GetCurrentEnemyState(),
            "Duplicate execution attempts must not change the active EnemyExecutedState."
        );
    }


    // =========================================================
    // RECOVERY STATE TRANSITION / ELIGIBILITY EXPIRATION
    // =========================================================

    [UnityTest]
    public IEnumerator Execution_RecoveryExpires_RemovesEligibility()
    {
        EnterStunnedState();

        Assert.AreSame(
            GetEnemyState(stunnedStateProperty),
            GetCurrentEnemyState(),
            "Precondition failed: enemy should begin in EnemyStunnedState."
        );

        /*
         * Move the existing stun timer into the past.
         *
         * This deterministically represents expiration of the configured
         * stun window without making the test wait four real seconds.
         */

        object stunnedState =
            GetEnemyState(
                stunnedStateProperty
            );

        stunnedStateEndTimeField.SetValue(
            stunnedState,
            Time.time - 1f
        );

        yield return null;

        Assert.AreSame(
            GetEnemyState(neutralStateProperty),
            GetCurrentEnemyState(),
            "Expired stun should transition the enemy back into EnemyNeutralState."
        );

        Assert.IsFalse(
            GetBoolProperty(
                isPostureBrokenProperty,
                enemyPosture
            ),
            "Posture recovery should clear the posture-broken condition."
        );

        bool acceptedAfterRecovery =
            (bool)requestExecutionMethod.Invoke(
                enemyStateManager,
                null
            );

        Assert.IsFalse(
            acceptedAfterRecovery,
            "Execution should no longer be available after stun recovery."
        );

        Assert.AreSame(
            GetEnemyState(neutralStateProperty),
            GetCurrentEnemyState(),
            "A recovered enemy should remain in EnemyNeutralState rather than entering EnemyExecutedState."
        );
    }

    // =========================================================
    // EXECUTION SUPERSEDES RECOVERY / DEATH TRANSITION
    // =========================================================

    [UnityTest]
    public IEnumerator Execution_AcceptedBeforeRecovery_TransitionsToDeathExactlyOnce()
    {
        CreatePlayerFixture();

        playerObject.transform.position =
            Vector3.zero;

        enemyObject.transform.position =
            new Vector3(
                1f,
                0f,
                0f
            );

        EnterStunnedState();

        Physics.SyncTransforms();

        yield return null;

        /*
         * Execution is accepted while the stun window is still active.
         */

        bool accepted =
            (bool)tryExecuteMethod.Invoke(
                playerExecution,
                null
            );

        Assert.IsTrue(
            accepted,
            "Execution should be accepted before the stun recovery window expires."
        );

        Assert.AreSame(
            GetEnemyState(executedStateProperty),
            GetCurrentEnemyState(),
            "Accepted execution should supersede normal stun recovery."
        );

        /*
         * Force the old StunnedState timer into the past.
         *
         * EnemyStateManager should now update EnemyExecutedState instead
         * of the old stunned state, so recovery must not occur.
         */

        object stunnedState =
            GetEnemyState(
                stunnedStateProperty
            );

        stunnedStateEndTimeField.SetValue(
            stunnedState,
            Time.time - 1f
        );

        yield return null;

        Assert.AreSame(
            GetEnemyState(executedStateProperty),
            GetCurrentEnemyState(),
            "Accepted execution must prevent the old stun recovery path from returning the enemy to BlockState."
        );

        /*
         * Complete the execution through the real PlayerExecution path.
         */

        finishExecutionMethod.Invoke(
            playerExecution,
            null
        );

        yield return null;

        object deathStateAfterFirstFinish =
            GetCurrentEnemyState();

        Assert.AreSame(
            GetEnemyState(deathStateProperty),
            deathStateAfterFirstFinish,
            "Finishing an accepted execution should transition the enemy into EnemyDeathState."
        );

        Assert.IsTrue(
            GetBoolProperty(
                isDeadProperty,
                enemyCombat
            ),
            "Finishing execution should mark the enemy as dead."
        );

        /*
         * Attempt execution completion again.
         *
         * PlayerExecution clears executionTarget after the first finish,
         * so the duplicate completion must fail safely and must not
         * produce another death transition.
         */

        finishExecutionMethod.Invoke(
            playerExecution,
            null
        );

        yield return null;

        Assert.AreSame(
            deathStateAfterFirstFinish,
            GetCurrentEnemyState(),
            "Duplicate execution completion must not create another death-state transition."
        );

        Assert.AreSame(
            GetEnemyState(deathStateProperty),
            GetCurrentEnemyState(),
            "Enemy should remain in EnemyDeathState after duplicate completion."
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

        enemyStunnedStateType =
            Type.GetType(
                "EnemyStunnedState, Assembly-CSharp"
            );

        playerExecutionType =
            Type.GetType(
                "PlayerExecution, Assembly-CSharp"
            );

        playerStateManagerType =
            Type.GetType(
                "StateManager, Assembly-CSharp"
            );

        playerMovementType =
            Type.GetType(
                "PlayerMovement, Assembly-CSharp"
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
            enemyStunnedStateType,
            "Could not find EnemyStunnedState in Assembly-CSharp."
        );

        Assert.IsNotNull(
            playerExecutionType,
            "Could not find PlayerExecution in Assembly-CSharp."
        );

        Assert.IsNotNull(
            playerStateManagerType,
            "Could not find StateManager in Assembly-CSharp."
        );

        Assert.IsNotNull(
            playerMovementType,
            "Could not find PlayerMovement in Assembly-CSharp."
        );
    }

    // =========================================================
    // MEMBER RESOLUTION
    // =========================================================

    private void ResolveMembers()
    {
        requestStunMethod =
            enemyStateManagerType.GetMethod(
                "RequestStun",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        requestExecutionMethod =
            enemyStateManagerType.GetMethod(
                "RequestExecution",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        requestDeathMethod =
            enemyStateManagerType.GetMethod(
                "RequestDeath",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        tryExecuteMethod =
            playerExecutionType.GetMethod(
                "TryExecute",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        finishExecutionMethod =
            playerExecutionType.GetMethod(
                "FinishExecution",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        currentEnemyStateProperty =
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

        executedStateProperty =
            enemyStateManagerType.GetProperty(
                "ExecutedState",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        deathStateProperty =
            enemyStateManagerType.GetProperty(
                "DeathState",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        neutralStateProperty =
            enemyStateManagerType.GetProperty(
                "NeutralState",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        isBeingExecutedProperty =
            enemyCombatType.GetProperty(
                "IsBeingExecuted",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        isDeadProperty =
            enemyCombatType.GetProperty(
                "IsDead",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        isPostureBrokenProperty =
            enemyPostureType.GetProperty(
                "IsPostureBroken",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        executionAnchorField =
            enemyCombatType.GetField(
                "executionAnchor",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        stunnedStateEndTimeField =
            enemyStunnedStateType.GetField(
                "stunEndTime",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        executionStateManagerField =
            playerExecutionType.GetField(
                "stateManager",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        executionRangeField =
            playerExecutionType.GetField(
                "executionRange",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        enemyLayerMaskField =
            playerExecutionType.GetField(
                "enemyLayerMask",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        executionTargetField =
            playerExecutionType.GetField(
                "executionTarget",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        playerCurrentStateField =
            playerStateManagerType.GetField(
                "currentState",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        playerIdleStateField =
            playerStateManagerType.GetField(
                "idleState",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        playerExecutingStateField =
            playerStateManagerType.GetField(
                "playerExecutingState",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        playerMovementField =
            playerStateManagerType.GetField(
                "movement",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        playerExecutionField =
            playerStateManagerType.GetField(
                "playerExecution",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        Assert.IsNotNull(requestStunMethod);
        Assert.IsNotNull(requestExecutionMethod);
        Assert.IsNotNull(requestDeathMethod);

        Assert.IsNotNull(tryExecuteMethod);
        Assert.IsNotNull(finishExecutionMethod);

        Assert.IsNotNull(currentEnemyStateProperty);
        Assert.IsNotNull(stunnedStateProperty);
        Assert.IsNotNull(executedStateProperty);
        Assert.IsNotNull(deathStateProperty);
        Assert.IsNotNull(neutralStateProperty);

        Assert.IsNotNull(isBeingExecutedProperty);
        Assert.IsNotNull(isDeadProperty);
        Assert.IsNotNull(isPostureBrokenProperty);

        Assert.IsNotNull(executionAnchorField);
        Assert.IsNotNull(stunnedStateEndTimeField);

        Assert.IsNotNull(executionStateManagerField);
        Assert.IsNotNull(executionRangeField);
        Assert.IsNotNull(enemyLayerMaskField);
        Assert.IsNotNull(executionTargetField);

        Assert.IsNotNull(playerCurrentStateField);
        Assert.IsNotNull(playerIdleStateField);
        Assert.IsNotNull(playerExecutingStateField);
        Assert.IsNotNull(playerMovementField);
        Assert.IsNotNull(playerExecutionField);
    }

    // =========================================================
    // ENEMY FIXTURE
    // =========================================================

    private void CreateEnemyFixture()
    {
        enemyObject =
            new GameObject(
                "ExecutionEligibility_Enemy"
            );

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

        enemyCollider =
            enemyObject.AddComponent<BoxCollider>();

        executionAnchorObject =
            new GameObject(
                "ExecutionEligibility_Anchor"
            );

        executionAnchorObject.transform.SetParent(
            enemyObject.transform
        );

        executionAnchorObject.transform.localPosition =
            Vector3.zero;

        executionAnchorObject.transform.localRotation =
            Quaternion.identity;

        executionAnchorField.SetValue(
            enemyCombat,
            executionAnchorObject.transform
        );

        /*
         * PlayerExecution uses Physics.OverlapSphere with a LayerMask.
         * Default layer is sufficient for this isolated fixture.
         */

        enemyObject.layer = 0;

        Physics.SyncTransforms();
    }

    // =========================================================
    // PLAYER FIXTURE
    // =========================================================

    private void CreatePlayerFixture()
    {
        if (playerObject != null)
            return;

        /*
         * Keep the player inactive while adding components.
         *
         * This prevents PlayerMovement.Awake() from trying to initialize
         * scene-specific InputActionAsset / Animator dependencies.
         *
         * The methods required by this fixture can still be invoked
         * directly through reflection.
         */

        playerObject =
            new GameObject(
                "ExecutionEligibility_Player"
            );

        playerObject.SetActive(false);

        playerStateManager =
            playerObject.AddComponent(
                playerStateManagerType
            );

        playerMovement =
            playerObject.AddComponent(
                playerMovementType
            );

        playerExecution =
            playerObject.AddComponent(
                playerExecutionType
            );

        object idleState =
            playerIdleStateField.GetValue(
                playerStateManager
            );

        playerCurrentStateField.SetValue(
            playerStateManager,
            idleState
        );

        playerMovementField.SetValue(
            playerStateManager,
            playerMovement
        );

        playerExecutionField.SetValue(
            playerStateManager,
            playerExecution
        );

        executionStateManagerField.SetValue(
            playerExecution,
            playerStateManager
        );

        executionRangeField.SetValue(
            playerExecution,
            ExecutionRange
        );

        /*
         * The fixture enemy uses Unity's Default layer.
         */

        enemyLayerMaskField.SetValue(
            playerExecution,
            (LayerMask)(1 << 0)
        );
    }

    // =========================================================
    // STATE HELPERS
    // =========================================================

    private void EnterStunnedState()
    {
        requestStunMethod.Invoke(
            enemyStateManager,
            null
        );

        Assert.AreSame(
            GetEnemyState(stunnedStateProperty),
            GetCurrentEnemyState(),
            "Fixture failed to place the enemy into EnemyStunnedState."
        );
    }

    private object GetCurrentEnemyState()
    {
        return currentEnemyStateProperty.GetValue(
            enemyStateManager
        );
    }

    private object GetEnemyState(
        PropertyInfo stateProperty)
    {
        return stateProperty.GetValue(
            enemyStateManager
        );
    }

    private static bool GetBoolProperty(
        PropertyInfo property,
        object target)
    {
        return (bool)property.GetValue(
            target
        );
    }
}