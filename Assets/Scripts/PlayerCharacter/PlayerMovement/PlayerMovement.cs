using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController _CharacterController;
    [SerializeField] private Animator _Animator;
    [SerializeField] private InputActionAsset _InputActions;
    [SerializeField] private Transform _MainCamera;
    [SerializeField] private CombatDetection _combatDetection;
    [SerializeField] private StateManager stateManager;

    [Header("Movement Settings")]
    [SerializeField] private float _rotationSpeed = 720f;


    private float _gravity = -9.81f;
    private Vector3 _velocity;
    private InputAction _moveAction;

    private InputAction _sprintAction;
    private int _sprintLayerIndex;

    [SerializeField]
    private float _sprintLayerBlendSpeed = 8f;

    void Awake()
    {
        if(stateManager == null) stateManager = GetComponent<StateManager>();
        _CharacterController = GetComponent<CharacterController>();

        InputActionMap playerActions =
            _InputActions.FindActionMap("Player");

        _moveAction =
            playerActions.FindAction("Move");

        _sprintAction =
            playerActions.FindAction("Sprint");

        _sprintLayerIndex =
            _Animator.GetLayerIndex("SprintLayer");

        if (_sprintLayerIndex >= 0)
        {
            _Animator.SetLayerWeight(
                _sprintLayerIndex,
                0f
            );
        }

        if (_MainCamera == null && Camera.main != null)
        {
            Debug.LogWarning(
                "Error: Main Camera may not be assigned to PlayerMovement."
            );

            _MainCamera = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (stateManager != null && stateManager.currentState == stateManager.playerExecutingState)
        {
            Debug.Log("PlayerMovement: Execution guard active");

            _Animator.SetFloat("MoveX", 0f);
            _Animator.SetFloat("MoveY", 0f);
            _Animator.SetBool("IsMoving", false);
            return;
        }

        bool inCombat = _combatDetection != null && _combatDetection.isEnemyNearby;
        _Animator.SetBool("InCombat", inCombat);

        Vector2 input = _moveAction.ReadValue<Vector2>();
        Vector2 normalizedInput = Vector2.ClampMagnitude(input, 1.0f);
        Vector3 moveDirection = Vector3.zero;

        if (normalizedInput.sqrMagnitude > 0.01f && _MainCamera != null)
        {
            Vector3 camForward = _MainCamera.forward;
            Vector3 camRight = _MainCamera.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveDirection = (camForward * normalizedInput.y) + (camRight * normalizedInput.x);
        }

        Vector3 localMoveDir = transform.InverseTransformDirection(moveDirection);
        float maxBlendSpeed = 1.85f;

        if (localMoveDir.sqrMagnitude > 0.01f)
        {
            // normalizes weights for diagonal inputs
            Vector3 normalizedLocal = localMoveDir.normalized * normalizedInput.magnitude;
            localMoveDir.x = normalizedLocal.x;
            localMoveDir.z = normalizedLocal.z;
        }

        float targetX = localMoveDir.x * maxBlendSpeed;
        float targetY = localMoveDir.z * maxBlendSpeed;

        if (normalizedInput.sqrMagnitude < 0.01f)
        {
            targetX = 0f;
            targetY = 0f;
        }

        _Animator.SetFloat("MoveX", targetX, 0.1f, Time.deltaTime);
        _Animator.SetFloat("MoveY", targetY, 0.1f, Time.deltaTime);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Vector3 camForwardFlat = _MainCamera.forward;
            camForwardFlat.y = 0f;

            if (camForwardFlat.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(camForwardFlat);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }

        bool isMoving = normalizedInput.sqrMagnitude > 0.01f;
        if (_Animator.GetBool("IsMoving") != isMoving)
        {
            _Animator.SetBool("IsMoving", isMoving);
        }

        if (_CharacterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        _velocity.y += _gravity * Time.deltaTime;

        // -------------------------
        // SPRINT
        // -------------------------
        bool sprintHeld =
                    _sprintAction != null &&
                    _sprintAction.IsPressed();

        bool validSprintDirection =
            targetY >= -0.05f;

        bool isSprinting = CanSprint(
            isMoving,
            sprintHeld,
            validSprintDirection,
            inCombat
        );

        if (_sprintLayerIndex >= 0)
        {
            float targetSprintWeight =
                isSprinting ? 1f : 0f;

            float currentSprintWeight =
                _Animator.GetLayerWeight(_sprintLayerIndex);

            float newSprintWeight =
                Mathf.MoveTowards(
                    currentSprintWeight,
                    targetSprintWeight,
                    Time.deltaTime * _sprintLayerBlendSpeed
                );

            _Animator.SetLayerWeight(
                _sprintLayerIndex,
                newSprintWeight
            );
        }

    }

    private void OnAnimatorMove()
    {
        if (_Animator == null) return;

        bool isExecuting =
            stateManager != null &&
            stateManager.currentState == stateManager.playerExecutingState;

        if (isExecuting)
        {
            if (_CharacterController != null &&
                _CharacterController.enabled)
            {
                _CharacterController.Move(_Animator.deltaPosition);
            }

            transform.rotation *= _Animator.deltaRotation;

            return;
        }

        if (_CharacterController != null &&
            _CharacterController.enabled)
        {
            Vector3 movement = _Animator.deltaPosition;

            movement.y = _velocity.y * Time.deltaTime;

            _CharacterController.Move(movement);
        }
    }


    private void OnEnable()
    {
        _moveAction?.Enable();
        _sprintAction?.Enable();
    }

    private void OnDisable()
    {
        _moveAction?.Disable();
        _sprintAction?.Disable();
    }

    public Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        if (_MainCamera == null) return new Vector3(input.x, 0, input.y);

        Vector3 camForward = _MainCamera.forward;
        Vector3 camRight = _MainCamera.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        return(camForward * input.y) + (camRight * input.x);
    }

    public bool IsGrounded
    {
        get
        {
            return _CharacterController != null && _CharacterController.isGrounded;
        }
    }

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        bool controllerWasEnabled =
            _CharacterController != null &&
            _CharacterController.enabled;

        if (controllerWasEnabled)
            _CharacterController.enabled = false;

        transform.SetPositionAndRotation(
            position,
            rotation
        );

        // clears accumulated vertical movement so gravity/root motion doesn't affect the execution
        _velocity = Vector3.zero;

        if (controllerWasEnabled)
            _CharacterController.enabled = true;

        Debug.Log(
            $"PlayerMovement teleported to {transform.position}, rotation={transform.rotation.eulerAngles}");
    }

    public static bool CanSprint(
        bool isMoving,
        bool sprintHeld,
        bool validSprintDirection,
        bool inCombat)
    {
    return isMoving &&
           sprintHeld &&
           validSprintDirection &&
           !inCombat;
    }
}
