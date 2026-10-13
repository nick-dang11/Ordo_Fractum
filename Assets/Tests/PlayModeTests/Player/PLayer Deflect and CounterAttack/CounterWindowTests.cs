using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CounterWindowTests
{
    private readonly List<GameObject> createdObjects =
        new List<GameObject>();

    private GameObject playerObject;
    private Component playerCombat;

    private GameObject enemyObject;
    private Component enemyCombat;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        Type playerCombatType =
            FindType("PlayerCombat");

        Type enemyCombatType =
            FindType("EnemyCombat");

        Assert.IsNotNull(
            playerCombatType,
            "PlayerCombat type could not be found."
        );

        Assert.IsNotNull(
            enemyCombatType,
            "EnemyCombat type could not be found."
        );

        // -----------------------------------------------------
        // PLAYER
        // -----------------------------------------------------

        playerObject =
            new GameObject("Sprint3_CounterTest_Player");

        createdObjects.Add(playerObject);

        playerCombat =
            playerObject.AddComponent(
                playerCombatType
            );

        // PlayerCombat.BeginCounter() calls ForceStopBlocking(),
        // which reaches EndBlock() and expects an Animator.
        Animator playerAnimator =
            playerObject.AddComponent<Animator>();

        SetFieldOrProperty(
            playerCombat,
            "animate",
            playerAnimator
        );

        // -----------------------------------------------------
        // ENEMY
        // -----------------------------------------------------

        enemyObject =
            new GameObject("Sprint3_CounterTest_Enemy");

        createdObjects.Add(enemyObject);

        enemyCombat =
            enemyObject.AddComponent(
                enemyCombatType
            );

        // Keep them close enough for a valid counter.
        playerObject.transform.position =
            Vector3.zero;

        enemyObject.transform.position =
            new Vector3(0f, 0f, 1f);

        yield return null;

        // Deterministic values for CI.
        SetFieldOrProperty(
            playerCombat,
            "counterWindowDuration",
            1f
        );

        SetFieldOrProperty(
            playerCombat,
            "counterRange",
            3f
        );

        // Make sure each test begins clean.
        InvokeMethod(
            playerCombat,
            "CloseCounterWindow",
            "test setup"
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
    // 3.5 - COUNTER WINDOW
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator CounterWindow_OpensForValidEnemy()
    {
        InvokeMethod(
            playerCombat,
            "OpenCounterWindow",
            enemyCombat
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                playerCombat,
                "CanCounter"
            ),
            "Opening a counter window for a valid enemy " +
            "should make CanCounter true."
        );
    }

    [UnityTest]
    public IEnumerator CounterWindow_StoresCorrectEnemy()
    {
        InvokeMethod(
            playerCombat,
            "OpenCounterWindow",
            enemyCombat
        );

        yield return null;

        object storedTarget =
            GetPropertyObject(
                playerCombat,
                "CounterTarget"
            );

        Assert.AreSame(
            enemyCombat,
            storedTarget,
            "The stored counter target should be the enemy " +
            "that opened the counter opportunity."
        );
    }

    [UnityTest]
    public IEnumerator CounterWindow_DoesNotReplaceTargetWithUnrelatedEnemy()
    {
        Type enemyCombatType =
            FindType("EnemyCombat");

        GameObject enemyBObject =
            new GameObject(
                "Sprint3_CounterTest_EnemyB"
            );

        createdObjects.Add(enemyBObject);

        Component enemyBCombat =
            enemyBObject.AddComponent(
                enemyCombatType
            );

        enemyBObject.transform.position =
            new Vector3(1f, 0f, 1f);

        yield return null;

        // Only Enemy A opens the counter window.
        InvokeMethod(
            playerCombat,
            "OpenCounterWindow",
            enemyCombat
        );

        yield return null;

        object storedTarget =
            GetPropertyObject(
                playerCombat,
                "CounterTarget"
            );

        Assert.AreSame(
            enemyCombat,
            storedTarget,
            "Enemy A should remain the stored counter target."
        );

        Assert.AreNotSame(
            enemyBCombat,
            storedTarget,
            "A nearby unrelated enemy must not become the counter target."
        );
    }

    [UnityTest]
    public IEnumerator CounterWindow_ExpiresAfterDuration()
    {
        SetFieldOrProperty(
            playerCombat,
            "counterWindowDuration",
            0.05f
        );

        InvokeMethod(
            playerCombat,
            "OpenCounterWindow",
            enemyCombat
        );

        Assert.IsTrue(
            GetProperty<bool>(
                playerCombat,
                "CanCounter"
            ),
            "Counter window should initially be open."
        );

        yield return new WaitForSeconds(0.1f);

        // Update normally calls this, but invoke it directly
        // to keep the PlayMode test deterministic.
        InvokeMethod(
            playerCombat,
            "UpdateCounterWindow"
        );

        Assert.IsFalse(
            GetProperty<bool>(
                playerCombat,
                "CanCounter"
            ),
            "Counter window should close after its duration expires."
        );

        Assert.IsNull(
            GetPropertyObject(
                playerCombat,
                "CounterTarget"
            ),
            "Expired counter window should clear its target."
        );
    }

    [UnityTest]
    public IEnumerator CounterWindow_DeadTargetInvalidatesOpportunity()
    {
        InvokeMethod(
            playerCombat,
            "OpenCounterWindow",
            enemyCombat
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                playerCombat,
                "CanCounter"
            )
        );

        // Simulate enemy death.
        SetPrivateField(
            enemyCombat,
            "isDead",
            true
        );

        InvokeMethod(
            playerCombat,
            "UpdateCounterWindow"
        );

        yield return null;

        Assert.IsFalse(
            GetProperty<bool>(
                playerCombat,
                "CanCounter"
            ),
            "A dead enemy should invalidate the counter opportunity."
        );

        Assert.IsNull(
            GetPropertyObject(
                playerCombat,
                "CounterTarget"
            ),
            "Dead counter target should be cleared."
        );
    }

    [UnityTest]
    public IEnumerator CounterWindow_TargetLeavingRangeClosesOpportunity()
    {
        SetFieldOrProperty(
            playerCombat,
            "counterRange",
            2f
        );

        enemyObject.transform.position =
            new Vector3(0f, 0f, 1f);

        InvokeMethod(
            playerCombat,
            "OpenCounterWindow",
            enemyCombat
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                playerCombat,
                "CanCounter"
            )
        );

        enemyObject.transform.position =
            new Vector3(0f, 0f, 10f);

        InvokeMethod(
            playerCombat,
            "UpdateCounterWindow"
        );

        yield return null;

        Assert.IsFalse(
            GetProperty<bool>(
                playerCombat,
                "CanCounter"
            ),
            "Leaving counter range should close the opportunity."
        );
    }

    [UnityTest]
    public IEnumerator CounterWindow_CloseClearsStoredTarget()
    {
        InvokeMethod(
            playerCombat,
            "OpenCounterWindow",
            enemyCombat
        );

        yield return null;

        InvokeMethod(
            playerCombat,
            "CloseCounterWindow",
            "automated test"
        );

        yield return null;

        Assert.IsFalse(
            GetProperty<bool>(
                playerCombat,
                "CanCounter"
            ),
            "Closing the counter window should make CanCounter false."
        );

        Assert.IsNull(
            GetPropertyObject(
                playerCombat,
                "CounterTarget"
            ),
            "Closing the counter window should clear counterTarget."
        );
    }

    // ---------------------------------------------------------
    // 3.6 - COUNTER EXECUTION
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator Counter_BeginCounterEntersCounterState()
    {
        InvokeMethod(
            playerCombat,
            "OpenCounterWindow",
            enemyCombat
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                playerCombat,
                "CanCounter"
            )
        );

        InvokeMethod(
            playerCombat,
            "BeginCounter"
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                playerCombat,
                "IsCountering"
            ),
            "A valid counter opportunity should enter counter state."
        );
    }

    [UnityTest]
    public IEnumerator Counter_BeginCounterFacesStoredTarget()
    {
        playerObject.transform.position =
            Vector3.zero;

        enemyObject.transform.position =
            new Vector3(5f, 0f, 0f);

        SetFieldOrProperty(
            playerCombat,
            "counterRange",
            10f
        );

        InvokeMethod(
            playerCombat,
            "OpenCounterWindow",
            enemyCombat
        );

        yield return null;

        InvokeMethod(
            playerCombat,
            "BeginCounter"
        );

        yield return null;

        Vector3 forward =
            playerObject.transform.forward;

        Vector3 expectedDirection =
            enemyObject.transform.position -
            playerObject.transform.position;

        expectedDirection.y = 0f;
        expectedDirection.Normalize();

        Assert.Greater(
            Vector3.Dot(
                forward.normalized,
                expectedDirection
            ),
            0.99f,
            "Beginning a counter should face the stored counter target."
        );
    }

    [UnityTest]
    public IEnumerator Counter_ActiveCounterDoesNotExpireMidExecution()
    {
        SetFieldOrProperty(
            playerCombat,
            "counterWindowDuration",
            0.05f
        );

        InvokeMethod(
            playerCombat,
            "OpenCounterWindow",
            enemyCombat
        );

        InvokeMethod(
            playerCombat,
            "BeginCounter"
        );

        Assert.IsTrue(
            GetProperty<bool>(
                playerCombat,
                "IsCountering"
            ),
            "Counter should have started."
        );

        yield return new WaitForSeconds(0.1f);

        InvokeMethod(
            playerCombat,
            "UpdateCounterWindow"
        );

        Assert.IsTrue(
            GetProperty<bool>(
                playerCombat,
                "IsCountering"
            ),
            "Counter-window expiration must not cancel an " +
            "already active counter."
        );

        Assert.AreSame(
            enemyCombat,
            GetPropertyObject(
                playerCombat,
                "CounterTarget"
            ),
            "Active counter should retain its target until cleanup."
        );
    }

    [UnityTest]
    public IEnumerator Counter_EndCounterClearsOpportunity()
    {
        InvokeMethod(
            playerCombat,
            "OpenCounterWindow",
            enemyCombat
        );

        InvokeMethod(
            playerCombat,
            "BeginCounter"
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                playerCombat,
                "IsCountering"
            )
        );

        InvokeMethod(
            playerCombat,
            "EndCounter"
        );

        yield return null;

        Assert.IsFalse(
            GetProperty<bool>(
                playerCombat,
                "IsCountering"
            ),
            "EndCounter should clear active counter state."
        );

        Assert.IsFalse(
            GetProperty<bool>(
                playerCombat,
                "CanCounter"
            ),
            "Completed counter should consume the counter opportunity."
        );

        Assert.IsNull(
            GetPropertyObject(
                playerCombat,
                "CounterTarget"
            ),
            "Completed counter should clear its stored target."
        );
    }

    [UnityTest]
    public IEnumerator Counter_OneOpportunityCannotProduceRepeatedCounters()
    {
        InvokeMethod(
            playerCombat,
            "OpenCounterWindow",
            enemyCombat
        );

        InvokeMethod(
            playerCombat,
            "BeginCounter"
        );

        yield return null;

        Assert.IsTrue(
            GetProperty<bool>(
                playerCombat,
                "IsCountering"
            )
        );

        InvokeMethod(
            playerCombat,
            "EndCounter"
        );

        yield return null;

        Assert.IsFalse(
            GetProperty<bool>(
                playerCombat,
                "CanCounter"
            ),
            "The original counter opportunity should be consumed."
        );

        Assert.IsFalse(
            GetProperty<bool>(
                playerCombat,
                "IsCountering"
            )
        );

        // Attempt to reuse the same already-consumed opportunity.
        InvokeMethod(
            playerCombat,
            "BeginCounter"
        );

        yield return null;

        Assert.IsFalse(
            GetProperty<bool>(
                playerCombat,
                "IsCountering"
            ),
            "A consumed counter opportunity must not start " +
            "another counterattack."
        );
    }

    [UnityTest]
    public IEnumerator Counter_OutOfRangeTargetCannotBeginCounter()
    {
        SetFieldOrProperty(
            playerCombat,
            "counterRange",
            2f
        );

        enemyObject.transform.position =
            new Vector3(0f, 0f, 10f);

        InvokeMethod(
            playerCombat,
            "OpenCounterWindow",
            enemyCombat
        );

        InvokeMethod(
            playerCombat,
            "BeginCounter"
        );

        yield return null;

        Assert.IsFalse(
            GetProperty<bool>(
                playerCombat,
                "IsCountering"
            ),
            "A target outside counter range must not start a counter."
        );

        Assert.IsFalse(
            GetProperty<bool>(
                playerCombat,
                "CanCounter"
            ),
            "Invalid out-of-range target should close " +
            "the counter opportunity."
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

    private static void SetPrivateField(
        object target,
        string fieldName,
        object value)
    {
        Assert.IsNotNull(target);

        FieldInfo field =
            target.GetType().GetField(
                fieldName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        Assert.IsNotNull(
            field,
            $"Could not find field '{fieldName}' on " +
            $"{target.GetType().Name}."
        );

        field.SetValue(
            target,
            ConvertValue(
                value,
                field.FieldType
            )
        );
    }

    private static void SetFieldOrProperty(
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

            return;
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
            $"Could not find field or property '{memberName}' " +
            $"on {type.Name}."
        );

        Assert.IsTrue(
            property.CanWrite,
            $"Property '{memberName}' on {type.Name} " +
            $"is read-only."
        );

        property.SetValue(
            target,
            ConvertValue(
                value,
                property.PropertyType
            )
        );
    }

    private static T GetProperty<T>(
        object target,
        string propertyName)
    {
        object value =
            GetPropertyObject(
                target,
                propertyName
            );

        return (T)value;
    }

    private static object GetPropertyObject(
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
            $"Could not find property '{propertyName}' on " +
            $"{target.GetType().Name}."
        );

        return property.GetValue(target);
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