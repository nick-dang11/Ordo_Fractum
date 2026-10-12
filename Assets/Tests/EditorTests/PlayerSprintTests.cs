
using System;
using System.Reflection;
using NUnit.Framework;

public class PlayerSprintTests
{
    private MethodInfo canSprintMethod;

    [SetUp]
    public void SetUp()
    {
        Type movementType = Type.GetType(
            "PlayerMovement, Assembly-CSharp"
        );

        Assert.IsNotNull(movementType);

        canSprintMethod = movementType.GetMethod(
            "CanSprint",
            BindingFlags.Public | BindingFlags.Static
        );

        Assert.IsNotNull(canSprintMethod);
    }

    private bool CheckSprint(
        bool isMoving,
        bool sprintHeld,
        bool validSprintDirection,
        bool inCombat)
    {
        return (bool)canSprintMethod.Invoke(
            null,
            new object[]
            {
                isMoving,
                sprintHeld,
                validSprintDirection,
                inCombat
            }
        );
    }

    // TEST 1: Sprinting outside combat
    [Test]
    public void SprintAllowed_WhenOutsideCombat()
    {
        bool result = CheckSprint(
            true,   // Player is moving
            true,   // Shift is held
            true,   // Valid sprint direction
            false   // Not in combat
        );

        Assert.IsTrue(result);
    }

    // TEST 2: Sprinting during combat
    [Test]
    public void SprintDisabled_WhenInCombat()
    {
        bool result = CheckSprint(
            true,   // Player is moving
            true,   // Shift is held
            true,   // Valid sprint direction
            true    // Player is in combat
        );

        Assert.IsFalse(result);
    }

    // TEST 3: Sprinting while standing still
    [Test]
    public void SprintDisabled_WhenNotMoving()
    {
        bool result = CheckSprint(
            false,  // Player is NOT moving
            true,   // Shift is held
            true,   // Valid sprint direction
            false   // Not in combat
        );
    
        Assert.IsFalse(result);
    }

    
    // TEST 4: Sprinting without holding Shift
    [Test]
    public void SprintDisabled_WhenShiftNotHeld()
    {
        bool result = CheckSprint(
            true,   // Player is moving
            false,  // Shift is NOT held
            true,   // Valid sprint direction
            false   // Not in combat
        );
    
        Assert.IsFalse(result);
    }
}
