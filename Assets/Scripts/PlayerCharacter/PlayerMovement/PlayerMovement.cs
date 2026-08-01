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

    [Header("Movement Settings")]
    [SerializeField] private float _rotationSpeed = 720f;


    private float _gravity = -9.81f;
    private Vector3 _velocity;
    private InputAction _moveAction;
    private Vector3 _rootMotionDeltaPosition;

    void Awake()
    {
        _CharacterController = GetComponent<CharacterController>();
        _moveAction = _InputActions.FindActionMap("Player").FindAction("Move");

        if (_MainCamera == null && Camera.main != null)
        {
            Debug.LogWarning("Error: Main Camera may not be assigned to PlayerMovement.");
            _MainCamera = Camera.main.transform;
        }
    }

    private void Update()
    {
        bool inCombat = _combatDetection != null && _combatDetection.isEnemyNearby;
        _Animator.SetBool("InCombat", inCombat);

        Vector2 input = _moveAction.ReadValue<Vector2>();
        Vector3 moveDirection = Vector3.zero;

        if (input.magnitude > 0.1f && _MainCamera != null)
        {
            Vector3 camForward = _MainCamera.forward;
            Vector3 camRight = _MainCamera.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveDirection = (camForward * input.y) + (camRight * input.x);
        }

        Vector3 localMoveDir = transform.InverseTransformDirection(moveDirection);

        _Animator.SetFloat("MoveX", localMoveDir.x, 0.1f, Time.deltaTime);
        _Animator.SetFloat("MoveY", localMoveDir.z, 0.1f, Time.deltaTime);

        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 camForwardFlat = _MainCamera.forward;
            camForwardFlat.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(camForwardFlat);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }

        bool isMoving = input.magnitude > 0.1f;
        if (_Animator.GetBool("IsMoving") != isMoving)
        {
            _Animator.SetBool("IsMoving", isMoving);
        }

        if (_CharacterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        _velocity.y += _gravity * Time.deltaTime;
    }

    private void OnAnimatorMove()
    {
        if (_CharacterController.enabled)
        {
            // takes position delta from the animation and appplies to CharacterController
            Vector3 movement = _Animator.deltaPosition;
            movement.y = _velocity.y * Time.deltaTime;

            // apply root motion rotation so model synchronizes with animations
            transform.rotation *= _Animator.deltaRotation;

            // passes animation's movement delta to CharacterController
            _CharacterController.Move(movement);
        }
    }

    private void OnEnable()
    {
        _moveAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
    }
}
