using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController _CharacterController;
    [SerializeField] private Animator _Animator;
    [SerializeField] private InputActionAsset _InputActions;

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
    }

    private void Update()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();

        Vector3 direction = new Vector3(input.x, 0, input.y);

        if(direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime); // 720f is two 360s, or two rotations per second, scalable

            _CharacterController.Move(direction * _moveSpeed * Time.deltaTime);
        }

        _Animator.SetFloat("Speed", direction.magnitude);

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
