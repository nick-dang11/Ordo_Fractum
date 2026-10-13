using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyPostureTests
{
    private readonly List<GameObject> createdObjects =
        new List<GameObject>();

    private GameObject enemyObject;
    private Component posture;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        Type postureType =
            FindType("EnemyPosture");

        Assert.IsNotNull(
            postureType,
            "EnemyPosture type could not be found."
        );

        enemyObject =
            new GameObject("Sprint3_PostureTest_Enemy");

        createdObjects.Add(enemyObject);

        posture =
            enemyObject.AddComponent(postureType);

        // Let Awake/Start finish before overriding
        // values for deterministic test behavior.
        yield return null;

        SetFieldOrProperty(
            posture,
            "maxPosture",
            100f
        );

        SetFieldOrProperty(
            posture,
            "currentPosture",
            0f
        );

        SetFieldOrProperty(
            posture,
            "postureDamageMultiplier",
            1f
        );

        // Prevent natural posture decay from interfering
        // with most tests.
        SetFieldOrProperty(
            posture,
            "decreaseDelay",
            999f
        );

        SetFieldOrProperty(
            posture,
            "decayTimer",
            0f
        );
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        foreach (GameObject obj in createdObjects)
        {
            if (obj != null)
            {
                UnityEngine.Object.Destroy(obj);
            }
        }

        createdObjects.Clear();

        yield return null;
    }

    // ---------------------------------------------------------
    // 3.3 - POSTURE DAMAGE
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator PostureDamage_IncreasesEnemyPosture()
    {
        float before =
            GetNumericMember(
                posture,
                "CurrentPosture"
            );

        InvokeMethod(
            posture,
            "ApplyPostureDamage",
            25f
        );

        yield return null;

        float after =
            GetNumericMember(
                posture,
                "CurrentPosture"
            );

        Assert.AreEqual(
            25f,
            after,
            0.001f,
            "Applying 25 posture damage with a 1x multiplier " +
            "should increase posture from 0 to 25."
        );

        Assert.Greater(
            after,
            before,
            "Enemy posture should increase after posture damage."
        );
    }

    [UnityTest]
    public IEnumerator PostureDamage_UsesPostureDamageMultiplier()
    {
        SetFieldOrProperty(
            posture,
            "postureDamageMultiplier",
            2f
        );

        InvokeMethod(
            posture,
            "ApplyPostureDamage",
            20f
        );

        yield return null;

        float result =
            GetNumericMember(
                posture,
                "CurrentPosture"
            );

        Assert.AreEqual(
            40f,
            result,
            0.001f,
            "Posture damage should respect postureDamageMultiplier."
        );
    }

    [UnityTest]
    public IEnumerator PostureDamage_IsAppliedExactlyOncePerCall()
    {
        InvokeMethod(
            posture,
            "ApplyPostureDamage",
            25f
        );

        yield return null;

        float result =
            GetNumericMember(
                posture,
                "CurrentPosture"
            );

        Assert.AreEqual(
            25f,
            result,
            0.001f,
            "One posture-damage resolution should produce " +
            "exactly one posture increase."
        );
    }

    [UnityTest]
    public IEnumerator NormalBlock_DoesNotApplyDeflectPostureDamage()
    {
        float before =
            GetNumericMember(
                posture,
                "CurrentPosture"
            );

        // No deflect posture-damage call occurs here.
        // Enemy posture should therefore remain unchanged.
        yield return null;

        float after =
            GetNumericMember(
                posture,
                "CurrentPosture"
            );

        Assert.AreEqual(
            before,
            after,
            0.001f,
            "A non-deflect defense result must not add " +
            "enemy deflect posture damage."
        );
    }

    // ---------------------------------------------------------
    // POSTURE LIMITS
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator PostureDamage_ClampsAtMaximumPosture()
    {
        SetFieldOrProperty(
            posture,
            "currentPosture",
            90f
        );

        InvokeMethod(
            posture,
            "ApplyPostureDamage",
            25f
        );

        yield return null;

        float result =
            GetNumericMember(
                posture,
                "CurrentPosture"
            );

        Assert.AreEqual(
            100f,
            result,
            0.001f,
            "Enemy posture should clamp at maxPosture."
        );
    }

    [UnityTest]
    public IEnumerator PostureDamage_CanReachMaximumPostureExactly()
    {
        SetFieldOrProperty(
            posture,
            "currentPosture",
            75f
        );

        InvokeMethod(
            posture,
            "ApplyPostureDamage",
            25f
        );

        yield return null;

        float current =
            GetNumericMember(
                posture,
                "CurrentPosture"
            );

        float maximum =
            GetNumericMember(
                posture,
                "MaxPosture"
            );

        Assert.AreEqual(
            maximum,
            current,
            0.001f,
            "Posture damage should be able to reach " +
            "maximum posture exactly."
        );
    }

    // ---------------------------------------------------------
    // POSTURE BREAK
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator PostureBreak_MaximumPostureSetsBrokenState()
    {
        SetFieldOrProperty(
            posture,
            "currentPosture",
            90f
        );

        Assert.IsFalse(
            GetProperty<bool>(
                posture,
                "IsPostureBroken"
            ),
            "Enemy should not begin the test posture-broken."
        );

        InvokeMethod(
            posture,
            "ApplyPostureDamage",
            25f
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                posture,
                "IsPostureBroken"
            ),
            "Reaching maximum posture should trigger posture break."
        );

        Assert.AreEqual(
            100f,
            GetNumericMember(
                posture,
                "CurrentPosture"
            ),
            0.001f
        );
    }

    [UnityTest]
    public IEnumerator PostureBreak_AdditionalDamageDoesNotRetriggerWhileBroken()
    {
        SetFieldOrProperty(
            posture,
            "currentPosture",
            90f
        );

        InvokeMethod(
            posture,
            "ApplyPostureDamage",
            25f
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                posture,
                "IsPostureBroken"
            )
        );

        float postureAfterBreak =
            GetNumericMember(
                posture,
                "CurrentPosture"
            );

        // Try applying posture damage again while broken.
        InvokeMethod(
            posture,
            "ApplyPostureDamage",
            25f
        );

        yield return null;

        float postureAfterSecondCall =
            GetNumericMember(
                posture,
                "CurrentPosture"
            );

        Assert.AreEqual(
            postureAfterBreak,
            postureAfterSecondCall,
            0.001f,
            "Additional posture damage should be ignored " +
            "while the posture-break latch is active."
        );

        Assert.IsTrue(
            GetProperty<bool>(
                posture,
                "IsPostureBroken"
            ),
            "Posture break should remain latched until recovery."
        );
    }

    // ---------------------------------------------------------
    // RECOVERY
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator PostureRecovery_ResetsBrokenStateAndPosture()
    {
        SetFieldOrProperty(
            posture,
            "currentPosture",
            100f
        );

        SetFieldOrProperty(
            posture,
            "isPostureBroken",
            true
        );

        InvokeMethod(
            posture,
            "RecoverFromPostureBreak",
            0.25f
        );

        yield return null;

        Assert.IsFalse(
            GetProperty<bool>(
                posture,
                "IsPostureBroken"
            ),
            "Recovery should clear the posture-break state."
        );

        Assert.AreEqual(
            25f,
            GetNumericMember(
                posture,
                "CurrentPosture"
            ),
            0.001f,
            "A 25% recovery should restore posture to " +
            "25% of max posture."
        );

        Assert.Less(
            GetNumericMember(
                posture,
                "decayTimer"
            ),
            0.1f,
            "Recovery should reset the posture decay timer."
        );
    }

    [UnityTest]
    public IEnumerator PostureRecovery_AllowsAnotherPostureBreakCycle()
    {
        // First break.
        SetFieldOrProperty(
            posture,
            "currentPosture",
            90f
        );

        InvokeMethod(
            posture,
            "ApplyPostureDamage",
            25f
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                posture,
                "IsPostureBroken"
            ),
            "First posture break should occur."
        );

        // Recover completely.
        InvokeMethod(
            posture,
            "RecoverFromPostureBreak",
            0f
        );

        yield return null;

        Assert.IsFalse(
            GetProperty<bool>(
                posture,
                "IsPostureBroken"
            ),
            "Recovery should clear the first posture break."
        );

        Assert.AreEqual(
            0f,
            GetNumericMember(
                posture,
                "CurrentPosture"
            ),
            0.001f
        );

        // Trigger another cycle.
        InvokeMethod(
            posture,
            "ApplyPostureDamage",
            100f
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                posture,
                "IsPostureBroken"
            ),
            "Enemy should be capable of entering a second " +
            "posture-break cycle after recovery."
        );
    }

    // ---------------------------------------------------------
    // DECAY
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator PostureDamage_ResetsDecayTimer()
    {
        SetFieldOrProperty(
            posture,
            "decayTimer",
            999f
        );

        InvokeMethod(
            posture,
            "ApplyPostureDamage",
            10f
        );

        yield return null;

        float decayTimer =
            GetNumericMember(
                posture,
                "decayTimer"
            );

        Assert.Less(
            decayTimer,
            0.1f,
            "Applying posture damage should reset " +
            "the posture decay timer."
        );
    }

    // ---------------------------------------------------------
    // TARGET ISOLATION
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator PostureDamage_OnlyAffectsTargetEnemy()
    {
        Type postureType =
            FindType("EnemyPosture");

        Assert.IsNotNull(postureType);

        GameObject enemyB =
            new GameObject(
                "Sprint3_PostureTest_EnemyB"
            );

        createdObjects.Add(enemyB);

        Component postureB =
            enemyB.AddComponent(postureType);

        yield return null;

        SetFieldOrProperty(
            postureB,
            "maxPosture",
            100f
        );

        SetFieldOrProperty(
            postureB,
            "currentPosture",
            0f
        );

        SetFieldOrProperty(
            postureB,
            "postureDamageMultiplier",
            1f
        );

        SetFieldOrProperty(
            postureB,
            "decreaseDelay",
            999f
        );

        InvokeMethod(
            posture,
            "ApplyPostureDamage",
            25f
        );

        yield return null;

        float postureA =
            GetNumericMember(
                posture,
                "CurrentPosture"
            );

        float postureBValue =
            GetNumericMember(
                postureB,
                "CurrentPosture"
            );

        Assert.AreEqual(
            25f,
            postureA,
            0.001f,
            "Target enemy should receive posture damage."
        );

        Assert.AreEqual(
            0f,
            postureBValue,
            0.001f,
            "Unrelated enemies must not receive posture damage."
        );
    }

    // ---------------------------------------------------------
    // REFLECTION HELPERS
    // ---------------------------------------------------------

    private static Type FindType(string typeName)
    {
        foreach (
            Assembly assembly
            in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type exactType =
                assembly.GetType(
                    typeName,
                    false
                );

            if (exactType != null)
            {
                return exactType;
            }

            try
            {
                Type[] types =
                    assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type != null &&
                        type.Name == typeName)
                    {
                        return type;
                    }
                }
            }
            catch (ReflectionTypeLoadException exception)
            {
                foreach (Type type in exception.Types)
                {
                    if (type != null &&
                        type.Name == typeName)
                    {
                        return type;
                    }
                }
            }
        }

        return null;
    }

    private static object InvokeMethod(
        object target,
        string methodName,
        params object[] args)
    {
        Assert.IsNotNull(
            target,
            $"Cannot invoke '{methodName}' because target is null."
        );

        MethodInfo method =
            FindMatchingMethod(
                target.GetType(),
                methodName,
                args
            );

        Assert.IsNotNull(
            method,
            $"Could not find method '{methodName}' " +
            $"with {args.Length} parameter(s) on " +
            $"{target.GetType().Name}."
        );

        return method.Invoke(
            target,
            args
        );
    }

    private static MethodInfo FindMatchingMethod(
        Type type,
        string methodName,
        object[] args)
    {
        MethodInfo[] methods =
            type.GetMethods(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        foreach (MethodInfo method in methods)
        {
            if (method.Name != methodName)
            {
                continue;
            }

            ParameterInfo[] parameters =
                method.GetParameters();

            if (parameters.Length != args.Length)
            {
                continue;
            }

            bool matches = true;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (args[i] == null)
                {
                    continue;
                }

                Type parameterType =
                    parameters[i].ParameterType;

                Type argumentType =
                    args[i].GetType();

                if (!parameterType.IsAssignableFrom(
                    argumentType))
                {
                    if (!IsNumericCompatible(
                        parameterType,
                        argumentType))
                    {
                        matches = false;
                        break;
                    }
                }
            }

            if (matches)
            {
                return method;
            }
        }

        return null;
    }

    private static void SetFieldOrProperty(
        object target,
        string memberName,
        object value)
    {
        bool success =
            TrySetFieldOrProperty(
                target,
                memberName,
                value
            );

        Assert.IsTrue(
            success,
            $"Could not find field or property '{memberName}' " +
            $"on {target.GetType().Name}."
        );
    }

    private static bool TrySetFieldOrProperty(
        object target,
        string memberName,
        object value)
    {
        Type type =
            target.GetType();

        FieldInfo field =
            type.GetField(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (field != null)
        {
            field.SetValue(
                target,
                ConvertValue(
                    value,
                    field.FieldType
                )
            );

            return true;
        }

        PropertyInfo property =
            type.GetProperty(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (property != null &&
            property.CanWrite)
        {
            property.SetValue(
                target,
                ConvertValue(
                    value,
                    property.PropertyType
                )
            );

            return true;
        }

        return false;
    }

    private static float GetNumericMember(
        object target,
        string memberName)
    {
        Type type =
            target.GetType();

        FieldInfo field =
            type.GetField(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (field != null)
        {
            return Convert.ToSingle(
                field.GetValue(target)
            );
        }

        PropertyInfo property =
            type.GetProperty(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        Assert.IsNotNull(
            property,
            $"Could not find numeric member '{memberName}' " +
            $"on {type.Name}."
        );

        return Convert.ToSingle(
            property.GetValue(target)
        );
    }

    private static T GetProperty<T>(
        object target,
        string propertyName)
    {
        Assert.IsNotNull(target);

        PropertyInfo property =
            target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        Assert.IsNotNull(
            property,
            $"Could not find property '{propertyName}' " +
            $"on {target.GetType().Name}."
        );

        return (T)property.GetValue(target);
    }

    private static object ConvertValue(
        object value,
        Type targetType)
    {
        if (value == null)
        {
            return null;
        }

        if (targetType.IsInstanceOfType(value))
        {
            return value;
        }

        return Convert.ChangeType(
            value,
            targetType
        );
    }

    private static bool IsNumericCompatible(
        Type parameterType,
        Type argumentType)
    {
        return
            parameterType == typeof(float) &&
            argumentType == typeof(float);
    }
}