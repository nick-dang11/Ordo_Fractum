using UnityEngine;

public class PlayerAssetManager : MonoBehaviour
{
    [Header("Core Components")]
    public Animator animator;
    public CharacterController characterController;

    [Header("Camera/Movement")]
    public Transform cameraTransform;
    public Transform mode1Root;

    [Header("Combat Refrences")]
    public Collider weaponHitbox;

    [Header("UI Refrences")]
    public GameObject healthUI;
    public GameObject postureUI;
    public GameObject willUI;

    [Header("PlaceHolder Movement")]
    public float moveSpeed = 5f;
    public float dodgeSpeed = 8f;

    [Header("Placeholder combat Values")]
    public float attackCancelWindowStart = 0.25f;
    public float AttackCancelWindowEnd = 0.65f;
    public float deflectWindow = 0.2f;
    public float stunDuration = 2f;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        characterController = GetComponent<CharacterController>();

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }
}