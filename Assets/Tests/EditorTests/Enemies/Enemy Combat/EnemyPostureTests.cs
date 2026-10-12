using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/*
 * EnemyPostureTests
 *
 * Purpose:
 * Provides automated Edit Mode regression coverage for the internal enemy
 * posture lifecycle, including posture accumulation, threshold behavior,
 * duplicate posture-break prevention, recovery, and subsequent break cycles.
 *
 * Testing Methodologies:
 *
 * Unit / Edit Mode Testing
 * - All tests isolate EnemyPosture and its minimum required state-machine
 *   dependencies without requiring a gameplay scene or player input.
 *
 * Boundary Value Analysis
 * - ApplyPostureDamage_BelowThreshold_DoesNotBreakPosture
 * - ApplyPostureDamage_AtThreshold_BreaksPosture
 * - ApplyPostureDamage_AboveThreshold_ClampsAndBreaksPosture
 *
 * State Transition Testing
 * - ApplyPostureDamage_AtThreshold_BreaksPosture
 * - ApplyPostureDamage_WhileAlreadyBroken_DoesNotBreakAgain
 * - RecoverFromPostureBreak_AllowsAnotherBreakCycle
 *
 * Regression Testing
 * - All tests protect the intended enemy posture lifecycle from future
 *   changes to EnemyPosture or EnemyStateManager.
 *
 * Scope:
 * This fixture tests the internal enemy posture lifecycle only.
 * Deflect recognition and the decision to apply posture damage are outside
 * the scope of this test fixture.
 */

public class EnemyPostureTests
{
    private GameObject enemyObject;

    private Component enemyPosture;
    private Type enemyPostureType;

    private Component enemyStateManager;
    private Type enemyStateManagerType;

    private MethodInfo applyPostureDamageMethod;
    private MethodInfo recoverFromPostureBreakMethod;

    private PropertyInfo currentPostureProperty;
    private PropertyInfo isPostureBrokenProperty;

    private PropertyInfo currentStateProperty;
    private PropertyInfo stunnedStateProperty;

    private FieldInfo maxPostureField;
    private FieldInfo stateManagerField;

    private const float MaxPosture = 100f;

    [SetUp]
    public void SetUp()
    {
        enemyObject =
            new GameObject("EnemyPosture_TestEnemy");

        enemyStateManagerType =
            Type.GetType(
                "EnemyStateManager, Assembly-CSharp"
            );

        Assert.IsNotNull(
            enemyStateManagerType,
            "Could not find EnemyStateManager in Assembly-CSharp."
        );

        enemyPostureType =
            Type.GetType(
                "EnemyPosture, Assembly-CSharp"
            );

        Assert.IsNotNull(
            enemyPostureType,
            "Could not find EnemyPosture in Assembly-CSharp."
        );

        enemyStateManager =
            enemyObject.AddComponent(
                enemyStateManagerType
            );

        enemyPosture =
            enemyObject.AddComponent(
                enemyPostureType
            );

        Assert.IsNotNull(enemyStateManager);
        Assert.IsNotNull(enemyPosture);

        applyPostureDamageMethod =
            enemyPostureType.GetMethod(
                "ApplyPostureDamage",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        recoverFromPostureBreakMethod =
            enemyPostureType.GetMethod(
                "RecoverFromPostureBreak",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        currentPostureProperty =
            enemyPostureType.GetProperty(
                "CurrentPosture",
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

        maxPostureField =
            enemyPostureType.GetField(
                "maxPosture",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        stateManagerField =
            enemyPostureType.GetField(
                "stateManager",
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        Assert.IsNotNull(applyPostureDamageMethod);
        Assert.IsNotNull(recoverFromPostureBreakMethod);

        Assert.IsNotNull(currentPostureProperty);
        Assert.IsNotNull(isPostureBrokenProperty);

        Assert.IsNotNull(currentStateProperty);
        Assert.IsNotNull(stunnedStateProperty);

        Assert.IsNotNull(maxPostureField);
        Assert.IsNotNull(stateManagerField);

        maxPostureField.SetValue(
            enemyPosture,
            MaxPosture
        );

        stateManagerField.SetValue(
            enemyPosture,
            enemyStateManager
        );
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
    public void ApplyPostureDamage_BelowThreshold_DoesNotBreakPosture()
    {
        float damage =
            MaxPosture - 1f;

        applyPostureDamageMethod.Invoke(
            enemyPosture,
            new object[] { damage }
        );

        float currentPosture =
            (float)currentPostureProperty.GetValue(
                enemyPosture
            );

        bool isPostureBroken =
            (bool)isPostureBrokenProperty.GetValue(
                enemyPosture
            );

        Assert.AreEqual(
            MaxPosture - 1f,
            currentPosture,
            0.001f,
            "Posture should accumulate immediately below the break threshold."
        );

        Assert.IsFalse(
            isPostureBroken,
            "Posture should not break below the configured threshold."
        );
    }

    [Test]
    public void ApplyPostureDamage_AtThreshold_BreaksPosture()
    {
        applyPostureDamageMethod.Invoke(
            enemyPosture,
            new object[] { MaxPosture }
        );

        float currentPosture =
            (float)currentPostureProperty.GetValue(
                enemyPosture
            );

        bool isPostureBroken =
            (bool)isPostureBrokenProperty.GetValue(
                enemyPosture
            );

        object currentState =
            currentStateProperty.GetValue(
                enemyStateManager
            );

        object stunnedState =
            stunnedStateProperty.GetValue(
                enemyStateManager
            );

        Assert.AreEqual(
            MaxPosture,
            currentPosture,
            0.001f,
            "Posture should reach the configured threshold exactly."
        );

        Assert.IsTrue(
            isPostureBroken,
            "Posture should break at the exact configured threshold."
        );

        Assert.AreSame(
            stunnedState,
            currentState,
            "Enemy should transition into EnemyStunnedState at the posture threshold."
        );
    }

    [Test]
    public void ApplyPostureDamage_AboveThreshold_ClampsAndBreaksPosture()
    {
        applyPostureDamageMethod.Invoke(
            enemyPosture,
            new object[] { MaxPosture + 25f }
        );

        float currentPosture =
            (float)currentPostureProperty.GetValue(
                enemyPosture
            );

        bool isPostureBroken =
            (bool)isPostureBrokenProperty.GetValue(
                enemyPosture
            );

        Assert.AreEqual(
            MaxPosture,
            currentPosture,
            0.001f,
            "Posture above the threshold should clamp to the configured maximum."
        );

        Assert.IsTrue(
            isPostureBroken,
            "Posture above the threshold should still produce a valid posture break."
        );
    }

    [Test]
    public void ApplyPostureDamage_WhileAlreadyBroken_DoesNotBreakAgain()
    {
        applyPostureDamageMethod.Invoke(
            enemyPosture,
            new object[] { MaxPosture }
        );

        float postureAfterBreak =
            (float)currentPostureProperty.GetValue(
                enemyPosture
            );

        object stateAfterBreak =
            currentStateProperty.GetValue(
                enemyStateManager
            );

        applyPostureDamageMethod.Invoke(
            enemyPosture,
            new object[] { 25f }
        );

        float postureAfterAdditionalDamage =
            (float)currentPostureProperty.GetValue(
                enemyPosture
            );

        object stateAfterAdditionalDamage =
            currentStateProperty.GetValue(
                enemyStateManager
            );

        Assert.AreEqual(
            postureAfterBreak,
            postureAfterAdditionalDamage,
            0.001f,
            "Additional posture damage should be ignored while posture is already broken."
        );

        Assert.AreSame(
            stateAfterBreak,
            stateAfterAdditionalDamage,
            "Additional posture damage should not create another posture-break state transition."
        );
    }

    [Test]
    public void RecoverFromPostureBreak_AllowsAnotherBreakCycle()
    {
        applyPostureDamageMethod.Invoke(
            enemyPosture,
            new object[] { MaxPosture }
        );

        recoverFromPostureBreakMethod.Invoke(
            enemyPosture,
            new object[] { 0.8f }
        );

        bool isPostureBrokenAfterRecovery =
            (bool)isPostureBrokenProperty.GetValue(
                enemyPosture
            );

        float postureAfterRecovery =
            (float)currentPostureProperty.GetValue(
                enemyPosture
            );

        Assert.IsFalse(
            isPostureBrokenAfterRecovery,
            "Recovery should clear the posture-broken state."
        );

        Assert.AreEqual(
            MaxPosture * 0.8f,
            postureAfterRecovery,
            0.001f,
            "Recovery should restore posture to 80 percent of the configured maximum."
        );

        float damageRequiredForSecondBreak =
            MaxPosture - postureAfterRecovery;

        applyPostureDamageMethod.Invoke(
            enemyPosture,
            new object[] { damageRequiredForSecondBreak }
        );

        bool isPostureBrokenAgain =
            (bool)isPostureBrokenProperty.GetValue(
                enemyPosture
            );

        object currentState =
            currentStateProperty.GetValue(
                enemyStateManager
            );

        object stunnedState =
            stunnedStateProperty.GetValue(
                enemyStateManager
            );

        Assert.IsTrue(
            isPostureBrokenAgain,
            "A recovered enemy should be able to enter a new valid posture-break cycle."
        );

        Assert.AreSame(
            stunnedState,
            currentState,
            "The second valid posture break should transition the enemy into EnemyStunnedState again."
        );
    }
}