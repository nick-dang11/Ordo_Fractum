using System;
using System.Collections;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/*
 * SCRUM-195 — Player posture integration and recovery regression tests.
 *
 * Play Mode integration testing using the real PlayerPosture, PlayerCombat,
 * StateManager, and PlayerStunnedState components. Reflection is limited to
 * arranging boundaries and invoking Unity callbacks deterministically when
 * test assembly definitions cannot directly reference gameplay assemblies.
 *
 * Methodologies: boundary value analysis (below / exact / above), state
 * transitions (idle / attack to stun / recovery), negative-path testing
 * (no duplicate stun), and regression testing (re-break after recovery).
 *
 * Runtime dependencies: PlayerCombat.EndBlock() writes Animator "Block";
 * therefore the fixture must provide an Animator even when no controller
 * or scene animation is required. No gameplay scene or player input needed.
 *
 * Current Sprint 4 recovery contract: restore posture to ZERO, not 80%.
 * The recovery test will fail if the shipped state still requests 0.8f.
 */
public class PlayerPostureIntegrationRecoveryTests
{
    private const BindingFlags Flags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private const float MaxPosture = 100f;

    private GameObject player;
    private GameObject ui;
    private Component posture;
    private Component states;
    private Component combat;
    private Animator animator;
    private Slider slider;

    private Type postureType;
    private Type stateType;
    private Type combatType;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        postureType = FindType("PlayerPosture");
        stateType = FindType("StateManager");
        combatType = FindType("PlayerCombat");

        Assert.NotNull(postureType, "Could not resolve PlayerPosture in loaded assemblies.");
        Assert.NotNull(stateType, "Could not resolve StateManager in loaded assemblies.");
        Assert.NotNull(combatType, "Could not resolve PlayerCombat in loaded assemblies.");

        player = new GameObject("SCRUM195_PlayerFixture");
        player.SetActive(false);

        // A real Animator is required by ForceStopBlocking -> EndBlock.
        animator = player.AddComponent<Animator>();
        combat = (Component)player.AddComponent(combatType);
        states = (Component)player.AddComponent(stateType);
        posture = (Component)player.AddComponent(postureType);

        // A valid UI Slider is required by PlayerPosture.Start/Update.
        ui = new GameObject("PostureSlider", typeof(RectTransform), typeof(Slider));
        ui.transform.SetParent(player.transform, false);
        slider = ui.GetComponent<Slider>();

        SetField(combat, "animate", animator);
        SetField(posture, "postureSlider", slider);
        SetField(posture, "maxPosture", MaxPosture);
        SetField(posture, "posture", 0f);
        SetField(posture, "posturedecreaseRate", 0f);
        SetField(posture, "decreaseDelay", 10000f);

        // Initialize required references before Unity can run IdleState.UpdateState.
        player.SetActive(true);
        Invoke(states, "Start");
        Invoke(posture, "Start");

        // Disable automatic updates before yielding; tests invoke methods directly.
        ((Behaviour)states).enabled = false;
        ((Behaviour)posture).enabled = false;
        yield return null;

        Assert.That(GetField(posture, "stateManager"), Is.SameAs(states),
            "PlayerPosture.Start did not resolve its StateManager.");
        Assert.That(GetField(posture, "playerCombat"), Is.SameAs(combat),
            "PlayerPosture.Start did not resolve PlayerCombat.");
        Assert.That(GetField(combat, "animate"), Is.SameAs(animator),
            "PlayerCombat needs a valid Animator for ForceStopBlocking.");

        Assert.That(State("playerStunnedState"), Is.Not.Null,
            "StateManager must register PlayerStunnedState.");
        Assert.That(CurrentState(), Is.SameAs(State("idleState")),
            "The fixture must begin in IdleState.");

    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (player != null)
            UnityEngine.Object.Destroy(player);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Posture_BelowThreshold_DoesNotStun()
    {
        SetField(posture, "posture", MaxPosture - 0.01f);
        TickPosture();

        Assert.That(CurrentState(), Is.SameAs(State("idleState")));
        Assert.That((bool)GetField(posture, "postureBroken"), Is.False);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Posture_ExactThreshold_StunsPlayer()
    {
        SetField(posture, "posture", MaxPosture);
        TickPosture();

        Assert.That(CurrentState(), Is.SameAs(State("playerStunnedState")));
        Assert.That((bool)GetField(posture, "postureBroken"), Is.True);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Posture_AboveThreshold_DoesNotRestartStun()
    {
        // Direct value injection tests threshold handling, not damage clamping.
        SetField(posture, "posture", MaxPosture + 25f);
        TickPosture();
        Assert.That(CurrentState(), Is.SameAs(State("playerStunnedState")));

        object stun = State("playerStunnedState");
        float firstEnd = (float)GetField(stun, "stunEndTime");
        yield return null;

        TickPosture();
        Assert.That(CurrentState(), Is.SameAs(stun));
        Assert.That((float)GetField(stun, "stunEndTime"),
            Is.EqualTo(firstEnd).Within(0.0001f),
            "Another posture update must not restart the stun timer.");
    }

    [UnityTest]
    public IEnumerator Posture_Break_InterruptsExistingAttackState()
    {
        // This covers the *state transition* contract. It does not claim to
        // verify attack hitbox shutdown; that requires a separate attack fixture.
        Invoke(states, "SwitchState", State("attackingState"), true);
        Assert.That(CurrentState(), Is.SameAs(State("attackingState")));

        SetField(posture, "posture", MaxPosture);
        TickPosture();

        Assert.That(CurrentState(), Is.SameAs(State("playerStunnedState")),
            "Posture break should override the active attacking state.");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Posture_StunRecovery_RestoresZeroAndIdle()
    {
        BreakPosture();
        object stun = State("playerStunnedState");
        SetField(stun, "stunEndTime", Time.time - 1f);
        Invoke(stun, "UpdateState", states);

        Assert.That(CurrentState(), Is.SameAs(State("idleState")));
        Assert.That((float)GetField(posture, "posture"),
            Is.EqualTo(0f).Within(0.01f),
            "Sprint 4 design: completed stun resets posture to 0%.");
        Assert.That((bool)GetField(posture, "postureBroken"), Is.False);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Posture_Recovery_AllowsAnotherBreak()
    {
        BreakPosture();
        object stun = State("playerStunnedState");
        SetField(stun, "stunEndTime", Time.time - 1f);
        Invoke(stun, "UpdateState", states);

        Assert.That(CurrentState(), Is.SameAs(State("idleState")));
        Assert.That((bool)GetField(posture, "postureBroken"), Is.False);

        SetField(posture, "posture", MaxPosture);
        TickPosture();

        Assert.That(CurrentState(), Is.SameAs(stun));
        Assert.That((bool)GetField(posture, "postureBroken"), Is.True);
        Assert.That((float)GetField(stun, "stunEndTime"),
            Is.GreaterThan(Time.time),
            "Second posture break must start a new stun interval.");
        yield return null;
    }

    private void TickPosture() => Invoke(posture, "Update");

    private void BreakPosture()
    {
        SetField(posture, "posture", MaxPosture);
        TickPosture();
        Assert.That(CurrentState(), Is.SameAs(State("playerStunnedState")),
            "Precondition: posture break must enter PlayerStunnedState.");
    }

    private object CurrentState() => GetField(states, "currentState");
    private object State(string name) => GetField(states, name);

    private static Type FindType(string name)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(name);
            if (type != null) return type;
        }
        return null;
    }

    private static object GetField(object instance, string name)
    {
        Assert.NotNull(instance, "Cannot inspect a null test-fixture object.");
        FieldInfo info = instance.GetType().GetField(name, Flags);
        Assert.NotNull(info, "Missing production field: " + instance.GetType().Name + "." + name);
        return info.GetValue(instance);
    }

    private static void SetField(object instance, string name, object value)
    {
        Assert.NotNull(instance, "Cannot configure a null test-fixture object.");
        FieldInfo info = instance.GetType().GetField(name, Flags);
        Assert.NotNull(info, "Missing production field: " + instance.GetType().Name + "." + name);
        info.SetValue(instance, value);
    }

    private static object Invoke(object instance, string method, params object[] arguments)
    {
        Assert.NotNull(instance, "Cannot invoke a method on a null object.");
        MethodInfo info = instance.GetType().GetMethod(method, Flags);
        Assert.NotNull(info, "Missing production method: " + instance.GetType().Name + "." + method);
        try
        {
            return info.Invoke(instance, arguments);
        }
        catch (TargetInvocationException exception)
        {
            if (exception.InnerException != null)
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }
}
