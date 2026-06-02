using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController _CharacterController;
    [SerializeField] private Animator _Animator;
    [SerializeField] private InputActionAsset _InputActions;
    [SerializeField] private Transform _MainCamera;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _rotationSpeed = 720f;


    private float _gravity = -9.81f;
    private Vector3 _velocity;
    private InputAction _moveAction;

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
        Vector2 input = _moveAction.ReadValue<Vector2>();

        Vector3 moveDirection = Vector3.zero;

        if (input.magnitude > 0.1f && _MainCamera != null) { 
            Vector3 camForward = _MainCamera.forward;
            Vector3 camRight = _MainCamera.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveDirection = (camForward * input.y) + (camRight * input.x);
        }


        if(moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime); // 720f is two 360s, or two rotations per second, scalable

            _CharacterController.Move(moveDirection * _moveSpeed * Time.deltaTime);
        }

        _Animator.SetFloat("Speed", input.magnitude);

        if (_CharacterController.isGrounded && _velocity.y < 0) {
            _velocity.y = -2f;
        }

        _velocity.y += _gravity * Time.deltaTime;
        _CharacterController.Move(_velocity * Time.deltaTime);
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
