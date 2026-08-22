using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    public bool attack;
    public bool heavyAttack;
    public bool block;
    public bool self_heal;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    // input callbacks
    // link these in inspector: PlayerInput via "Invoke Unity Events"
    // see pinned messages in #github-commits for example

    public void OnMove(InputAction.CallbackContext context) => MoveInput(context.ReadValue<Vector2>());
    public void OnLook(InputAction.CallbackContext context)
    {
        if (cursorInputForLook) LookInput(context.ReadValue<Vector2>());
    }
    public void OnJump(InputAction.CallbackContext context) => JumpInput(context.ReadValue<bool>());
    public void OnSprint(InputAction.CallbackContext context) => SprintInput(context.ReadValue<bool>());
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed || context.started)
        {
            AttackInput(true);
        }
        else if (context.canceled)
        {
            AttackInput(false);
        }
    }
    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (context.performed || context.started)
        {
            HeavyAttackInput(true);
        }
        else if (context.canceled)
        {
            HeavyAttackInput(false);
        }
    }
    public void OnBlock(InputAction.CallbackContext context)
    {
        if (context.performed || context.started)
        {
            BlockInput(true);
        }
        else if (context.canceled)
        {
            BlockInput(false);
        }
    }
    public void OnSelfHeal(InputAction.CallbackContext context)
    {
        if(context.performed || context.started)
        {
            Self_HealInput(true);
            Debug.Log("Self heal input detected");
        }
        else if(context.canceled)
        {
            Self_HealInput(false);
        }
    }
     //public void OnSelfHeal(InputAction.CallbackContext context) => self_heal = (context.performed || context.started);


    // input setters

    public void MoveInput(Vector2 newMoveDirection) => move = newMoveDirection;

    public void LookInput(Vector2 newLookDirection) => look = newLookDirection;

    public void JumpInput(bool newJumpState) => jump = newJumpState;

    public void SprintInput(bool newSprintState) => sprint = newSprintState;

    public void AttackInput(bool newAttackState)
    {
        attack = newAttackState;
        Debug.Log($"Manager thinks attack is {attack}");
    }
    
    public void HeavyAttackInput(bool newHeavyAttackState)
    {
        heavyAttack = newHeavyAttackState;
        Debug.Log($"Manager thinks heavyAttack is {heavyAttack}");
    }

    public void BlockInput(bool newBlockState)
    {
        block = newBlockState;
        Debug.Log($"Manager thinks block is {block}");
    }

    // cursor and focus

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void Self_HealInput(bool newSelfHealState)
    {
        self_heal = newSelfHealState;
        //Debug.Log($"Manager thinks self_heal is {self_heal}");
    }
}