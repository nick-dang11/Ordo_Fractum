using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// this is all essentially unity's third person starter input system, i've just relocated and renamed - N

    public class PlayerInputManager : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        [Header("Combat Inputs")]
        public bool attack;
        public bool block;

#if ENABLE_INPUT_SYSTEM
        public void OnAttack(InputValue value)
        {
            AttackInput(value.isPressed);
        }
#endif
        public void AttackInput(bool newAttackState)
        {
            attack = newAttackState;
            Debug.Log($"Manager thinks attack is {attack}");
        }
#if ENABLE_INPUT_SYSTEM
    public void OnBlock(InputValue value)
    {
        BlockInput(value.isPressed);
    }
#endif
    public void BlockInput(bool newBlockState)
    {
        block = newBlockState;
        Debug.Log($"Manager thinks block is {block}");
    }

#if ENABLE_INPUT_SYSTEM
    public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }
#endif


        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }