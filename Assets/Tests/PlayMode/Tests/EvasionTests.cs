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

        inputManagerType =
            Type.GetType("PlayerInputManager, Assembly-CSharp");

        Assert.IsNotNull(
            inputManagerType,
            "Could not find PlayerInputManager in Assembly-CSharp."
        );

        inputManager =
            testPlayer.AddComponent(inputManagerType);

        Assert.IsNotNull(inputManager);

        moveInputMethod =
            inputManagerType.GetMethod("MoveInput");

        blockInputMethod =
            inputManagerType.GetMethod("BlockInput");

        checkEvadeRequestMethod =
            inputManagerType.GetMethod("CheckEvadeRequest");

        consumeEvadeRequestMethod =
            inputManagerType.GetMethod("ConsumeEvadeRequest");

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


    // ---------------------------------------------------------
    // Initial block behavior
    // ---------------------------------------------------------

    [Test]
    public void WalkingThenBlocking_DoesNotCreateEvadeRequest()
    {
        // Player is already walking.
        Move(Vector2.up);
        CheckEvade();

        // Player presses block while movement is still held.
        Block(true);
        CheckEvade();

        Assert.IsFalse(
            IsEvadeRequested(),
            "Pressing block while already moving should block, not evade."
        );
    }


    [Test]
    public void MovementReinputWhileBlocking_CreatesEvadeRequest()
    {
        // Already walking.
        Move(Vector2.up);
        CheckEvade();

        // Initial block should NOT evade.
        Block(true);
        CheckEvade();

        Assert.IsFalse(IsEvadeRequested());

        // Release movement while continuing to block.
        Move(Vector2.zero);
        CheckEvade();

        // Re-enter movement.
        Move(Vector2.up);
        CheckEvade();

        Assert.IsTrue(
            IsEvadeRequested(),
            "Movement reinput while blocking should create an evade request."
        );
    }


    // ---------------------------------------------------------
    // Direction
    // ---------------------------------------------------------

    [Test]
    public void EvadeDirection_IsNormalized()
    {
        Vector2 inputDirection =
            new Vector2(1f, 1f);

        // Begin blocking.
        Block(true);

        // Neutral movement arms the evade.
        Move(Vector2.zero);
        CheckEvade();

        // Enter diagonal movement.
        Move(inputDirection);
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


    // ---------------------------------------------------------
    // Request consumption
    // ---------------------------------------------------------

    [Test]
    public void ConsumeEvadeRequest_ClearsRequest()
    {
        ArmAndRequestEvade(Vector2.up);

        Assert.IsTrue(IsEvadeRequested());

        ConsumeEvade();

        Assert.IsFalse(IsEvadeRequested());
    }


    // ---------------------------------------------------------
    // Repeated movement behavior
    // ---------------------------------------------------------

    [Test]
    public void ContinuedMovement_DoesNotRequestSecondEvade()
    {
        ArmAndRequestEvade(Vector2.up);

        Assert.IsTrue(IsEvadeRequested());

        ConsumeEvade();

        Assert.IsFalse(IsEvadeRequested());

        // Movement is still held.
        CheckEvade();

        Assert.IsFalse(
            IsEvadeRequested(),
            "Continued movement should not repeatedly request evades."
        );
    }


    [Test]
    public void ReturningMovementToNeutral_RearmsEvade()
    {
        ArmAndRequestEvade(Vector2.up);

        ConsumeEvade();

        // Return movement to neutral while still blocking.
        Move(Vector2.zero);
        CheckEvade();

        // Re-enter movement.
        Move(Vector2.up);
        CheckEvade();

        Assert.IsTrue(
            IsEvadeRequested(),
            "Returning movement to neutral while blocking should rearm evade."
        );
    }


    // ---------------------------------------------------------
    // Deadzone / invalid input
    // ---------------------------------------------------------

    [Test]
    public void MovementBelowDeadzone_DoesNotRequestEvade()
    {
        Block(true);

        Move(new Vector2(0.1f, 0f));
        CheckEvade();

        Assert.IsFalse(IsEvadeRequested());
    }


    [Test]
    public void NoMovement_DoesNotRequestEvade()
    {
        Block(true);

        Move(Vector2.zero);
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


    [Test]
    public void ReleasingBlock_DisarmsEvade()
    {
        // Block + neutral movement arms evade.
        Block(true);
        Move(Vector2.zero);
        CheckEvade();

        // Release block before movement.
        Block(false);
        CheckEvade();

        // Start moving.
        Move(Vector2.up);
        CheckEvade();

        Assert.IsFalse(
            IsEvadeRequested(),
            "Movement should not evade after block has been released."
        );
    }


    // ---------------------------------------------------------
    // Test setup helper
    // ---------------------------------------------------------

    private void ArmAndRequestEvade(Vector2 direction)
    {
        // Start blocking.
        Block(true);

        // Movement must first return to neutral.
        Move(Vector2.zero);
        CheckEvade();

        // Directional input now produces evade.
        Move(direction);
        CheckEvade();
    }


    // ---------------------------------------------------------
    // Reflection helper methods
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