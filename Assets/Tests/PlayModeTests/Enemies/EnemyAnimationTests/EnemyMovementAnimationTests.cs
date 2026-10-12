
using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

public class EnemyMovementAnimationTests
{
    private GameObject enemy;
    private MonoBehaviour movement;
    private MonoBehaviour animationScript;
    private NavMeshAgent agent;
    private Animator animator;

    private const BindingFlags Flags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    private Type FindType(string name)
    {
        Type type = Type.GetType(
            name + ", Assembly-CSharp"
        );

        Assert.IsNotNull(type,
            "Could not find " + name);

        return type;
    }

    private void SetField(
        object target,
        string name,
        object value)
    {
        FieldInfo field =
            target.GetType().GetField(name, Flags);

        Assert.IsNotNull(field,
            "Field not found: " + name);

        field.SetValue(target, value);
    }

    private T GetField<T>(
        object target,
        string name)
    {
        FieldInfo field =
            target.GetType().GetField(name, Flags);

        Assert.IsNotNull(field,
            "Field not found: " + name);

        return (T)field.GetValue(target);
    }

    private void Call(
        object target,
        string methodName)
    {
        MethodInfo method =
            target.GetType().GetMethod(
                methodName, Flags);

        Assert.IsNotNull(method,
            "Method not found: " + methodName);

        method.Invoke(target, null);
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        enemy = new GameObject("TestEnemy");

        agent = enemy.AddComponent<NavMeshAgent>();
        animator = enemy.AddComponent<Animator>();

        // Add controller because EnemyMovement
        // depends on it.
        Type controllerType =
            FindType("EnemyAIController");

        enemy.AddComponent(controllerType);

        movement = (MonoBehaviour)
            enemy.AddComponent(
                FindType("EnemyMovement"));

        animationScript = (MonoBehaviour)
            enemy.AddComponent(
                FindType("EnemyAIAnimation"));

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (enemy != null)
            UnityEngine.Object.Destroy(enemy);

        yield return null;
    }

    // TEST 1: Movement script is attached.
    [UnityTest]
    public IEnumerator MovementComponent_Exists()
    {
        Assert.IsNotNull(movement);
        yield return null;
    }

    // TEST 2: Animation script is attached.
    [UnityTest]
    public IEnumerator AnimationComponent_Exists()
    {
        Assert.IsNotNull(animationScript);
        yield return null;
    }

    // TEST 3: Movement has NavMeshAgent.
    [UnityTest]
    public IEnumerator Movement_HasAgentReference()
    {
        Assert.AreSame(
            agent,
            GetField<NavMeshAgent>(
                movement, "agent"));

        yield return null;
    }

    // TEST 4: Movement has controller.
    [UnityTest]
    public IEnumerator Movement_HasControllerReference()
    {
        Assert.IsNotNull(
            GetField<object>(
                movement, "controller"));

        yield return null;
    }

    // TEST 5: Patrol speed configured.
    [UnityTest]
    public IEnumerator PatrolSpeed_IsCorrect()
    {
        Assert.AreEqual(
            2f,
            GetField<float>(
                movement, "patrolSpeed"));

        yield return null;
    }

    // TEST 6: Chase speed configured.
    [UnityTest]
    public IEnumerator ChaseSpeed_IsCorrect()
    {
        Assert.AreEqual(
            4f,
            GetField<float>(
                movement, "chaseSpeed"));

        yield return null;
    }

    // TEST 7: Patrol index wraps.
    [UnityTest]
    public IEnumerator PatrolIndex_WrapsCorrectly()
    {
        GameObject a =
            new GameObject("PatrolA");

        GameObject b =
            new GameObject("PatrolB");

        try
        {
            SetField(
                movement,
                "patrolPoints",
                new Transform[]
                {
                    a.transform,
                    b.transform
                });

            Call(movement, "NextPatrolPoint");
            Call(movement, "NextPatrolPoint");

            Assert.AreEqual(
                0,
                GetField<int>(
                    movement,
                    "currentPatrolIndex"));
        }
        finally
        {
            UnityEngine.Object.Destroy(a);
            UnityEngine.Object.Destroy(b);
        }

        yield return null;
    }

    // TEST 8: Movement without NavMesh.
    [UnityTest]
    public IEnumerator MoveTo_WithoutNavMesh_DoesNotCrash()
    {
        Assert.IsFalse(agent.isOnNavMesh);

        MethodInfo method =
            movement.GetType().GetMethod(
                "MoveTo", Flags);

        Assert.IsNotNull(method);

        Assert.DoesNotThrow(() =>
            method.Invoke(
                movement,
                new object[]
                {
                    new Vector3(5, 0, 5)
                }));

        yield return null;
    }

    // TEST 9: Animation references agent.
    [UnityTest]
    public IEnumerator Animation_HasAgentReference()
    {
        Assert.AreSame(
            agent,
            GetField<NavMeshAgent>(
                animationScript,
                "EnemyNavMeshAgent"));

        yield return null;
    }

    // TEST 10: Animation references Animator.
    [UnityTest]
    public IEnumerator Animation_HasAnimatorReference()
    {
        Assert.AreSame(
            animator,
            GetField<Animator>(
                animationScript,
                "EnemyAnimator"));

        yield return null;
    }

    // TEST 11: Animation without NavMesh.
    [UnityTest]
    public IEnumerator Animation_WithoutNavMesh_DoesNotCrash()
    {
        Assert.IsFalse(agent.isOnNavMesh);

        Assert.DoesNotThrow(() =>
            Call(animationScript, "Update"));

        yield return null;
    }
}
