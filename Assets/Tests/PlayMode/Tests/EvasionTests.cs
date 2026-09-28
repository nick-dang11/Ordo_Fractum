using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class EvasionTest
{
    private GameObject testPlayer;

    private Component inputManager;
    private Type inputManagerType;

    private MethodInfo moveInputMethod;
    private MethodInfo blockInputMethod;
    private MethodInfo checkEvadeRequestMethod;
    private MethodInfo consumeEvadeRequestMethod;

    private PropertyInfo isEvadeRequestedProperty;
    private PropertyInfo evadeDirectionProperty;

    [SetUp]
    public void SetUp()
    {
        testPlayer = new GameObject("Test Player");

        // Find PlayerInputManager inside Assembly-CSharp.
        inputManagerType =
            Type.GetType("PlayerInputManager, Assembly-CSharp");

        Assert.IsNotNull(
            inputManagerType,
            "Could not find PlayerInputManager in Assembly-CSharp."
        );

        // Add PlayerInputManager to our temporary test object.
        inputManager =
            testPlayer.AddComponent(inputManagerType);

        Assert.IsNotNull(inputManager);

        // Get methods.
        moveInputMethod =
            inputManagerType.GetMethod("MoveInput");

        blockInputMethod =
            inputManagerType.GetMethod("BlockInput");

        checkEvadeRequestMethod =
            inputManagerType.GetMethod("CheckEvadeRequest");

        consumeEvadeRequestMethod =
            inputManagerType.GetMethod("ConsumeEvadeRequest");

        // Get properties.
        isEvadeRequestedProperty =
            inputManagerType.GetProperty("IsEvadeRequested");

        evadeDirectionProperty =
            inputManagerType.GetProperty("EvadeDirection");

        Assert.IsNotNull(moveInputMethod);
        Assert.IsNotNull(blockInputMethod);
        Assert.IsNotNull(checkEvadeRequestMethod);
        Assert.IsNotNull(consumeEvadeRequestMethod);

        Assert.IsNotNull(isEvadeRequestedProperty);
        Assert.IsNotNull(evadeDirectionProperty);
    }

    [TearDown]
    public void TearDown()
    {
        if (testPlayer != null)
        {
            UnityEngine.Object.DestroyImmediate(testPlayer);
        }
    }

    [Test]
    public void MovementAndBlock_CreatesEvadeRequest()
    {
        Move(Vector2.up);
        Block(true);

        CheckEvade();

        Assert.IsTrue(IsEvadeRequested());
    }

    [Test]
    public void EvadeDirection_IsNormalized()
    {
        Vector2 inputDirection =
            new Vector2(1f, 1f);

        Move(inputDirection);
        Block(true);

        CheckEvade();

        Vector2 expected =
            inputDirection.normalized;

        Vector2 actual =
            GetEvadeDirection();

        Assert.AreEqual(
            expected.x,
            actual.x,
            0.001f
        );

        Assert.AreEqual(
            expected.y,
            actual.y,
            0.001f
        );
    }

    [Test]
    public void ConsumeEvadeRequest_ClearsRequest()
    {
        Move(Vector2.up);
        Block(true);

        CheckEvade();

        Assert.IsTrue(IsEvadeRequested());

        ConsumeEvade();

        Assert.IsFalse(IsEvadeRequested());
    }

    [Test]
    public void ContinuedMovement_DoesNotRequestSecondEvade()
    {
        Move(Vector2.up);
        Block(true);

        // First request.
        CheckEvade();

        Assert.IsTrue(IsEvadeRequested());

        ConsumeEvade();

        Assert.IsFalse(IsEvadeRequested());

        // Still moving and still blocking.
        CheckEvade();

        Assert.IsFalse(IsEvadeRequested());
    }

    [Test]
    public void ReturningMovementToNeutral_RearmsEvade()
    {
        Move(Vector2.up);
        Block(true);

        // First evade.
        CheckEvade();
        ConsumeEvade();

        // Return movement to neutral.
        Move(Vector2.zero);
        CheckEvade();

        // Start moving again.
        Move(Vector2.up);
        CheckEvade();

        Assert.IsTrue(IsEvadeRequested());
    }

    [Test]
    public void MovementBelowDeadzone_DoesNotRequestEvade()
    {
        Move(new Vector2(0.1f, 0f));
        Block(true);

        CheckEvade();

        Assert.IsFalse(IsEvadeRequested());
    }

    [Test]
    public void NoMovement_DoesNotRequestEvade()
    {
        Move(Vector2.zero);
        Block(true);

        CheckEvade();

        Assert.IsFalse(IsEvadeRequested());
    }

    [Test]
    public void MovementWithoutBlock_DoesNotRequestEvade()
    {
        Move(Vector2.up);
        Block(false);

        CheckEvade();

        Assert.IsFalse(IsEvadeRequested());
    }


    // ---------------------------------------------------------
    // Helper methods
    // ---------------------------------------------------------

    private void Move(Vector2 direction)
    {
        moveInputMethod.Invoke(
            inputManager,
            new object[] { direction }
        );
    }

    private void Block(bool value)
    {
        blockInputMethod.Invoke(
            inputManager,
            new object[] { value }
        );
    }

    private void CheckEvade()
    {
        checkEvadeRequestMethod.Invoke(
            inputManager,
            null
        );
    }

    private void ConsumeEvade()
    {
        consumeEvadeRequestMethod.Invoke(
            inputManager,
            null
        );
    }

    private bool IsEvadeRequested()
    {
        return (bool)
            isEvadeRequestedProperty.GetValue(
                inputManager
            );
    }

    private Vector2 GetEvadeDirection()
    {
        return (Vector2)
            evadeDirectionProperty.GetValue(
                inputManager
            );
    }
}