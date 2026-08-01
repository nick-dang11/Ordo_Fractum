using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionController : MonoBehaviour
{
    [Header("Core Scripts")]
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private LockOnController lockOnController;
    [SerializeField] private PlayerInputManager inputManager;

    [Header("Transforms")]
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform mainCamera;

    [Header("Directional Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float targetingRadius = 12f;
    [SerializeField] private float maxTargetingAngle = 90f; 
    [SerializeField] private float passiveRotationSpeed = 15f; 


    private Transform currentFocusTarget;
    private bool wasAttackButtonPressed = false;
    private bool wasBlockButtonPressed = false;

    void Start()
    {
        if (mainCamera == null && Camera.main != null) mainCamera = Camera.main.transform;
        if (playerCombat != null && inputManager == null) inputManager = playerCombat.input;
    }

    void Update()
    {
        if (inputManager == null || playerCombat == null) return;

        
        if (lockOnController != null && lockOnController.IsLockedOn)
        {
            wasAttackButtonPressed = inputManager.attack;
            wasBlockButtonPressed = inputManager.block;
            return;
        }

        
        ManageFocusTarget();

        
        bool isAttackButtonPressed = inputManager.attack;
        bool isBlockButtonPressed = inputManager.block;

        
        if (!isAttackButtonPressed && wasAttackButtonPressed)
        {
            PerformDirectionalAttackSnap();
        }
        
        else if (isBlockButtonPressed && !wasBlockButtonPressed)
        {
            
            UpdateFocusToNearestEnemy();
        }

        
        FaceCurrentFocusTarget();

        
        wasAttackButtonPressed = isAttackButtonPressed;
        wasBlockButtonPressed = isBlockButtonPressed;
    }

    private void ManageFocusTarget()
    {
        
        if (currentFocusTarget == null || !currentFocusTarget.gameObject.activeInHierarchy)
        {
            currentFocusTarget = FindNearestEnemy();
        }
    }

    private void PerformDirectionalAttackSnap()
    {
        Vector3 inputDir = GetCameraRelativeInput();
        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, targetingRadius, enemyLayer);

        Transform bestTarget = null;
        float smallestAngle = Mathf.Infinity;

        if (potentialTargets.Length == 1)
        {
            
            bestTarget = potentialTargets[0].transform;
        }
        else if (potentialTargets.Length > 1)
        {
            
            Vector3 referenceDir = inputDir.sqrMagnitude > 0.01f ? inputDir : playerModel.forward;

            foreach (Collider col in potentialTargets)
            {
                Vector3 dirToEnemy = (col.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(referenceDir, dirToEnemy);

                if (angle <= maxTargetingAngle && angle < smallestAngle)
                {
                    smallestAngle = angle;
                    bestTarget = col.transform;
                }
            }
        }

        
        if (bestTarget != null)
        {
            currentFocusTarget = bestTarget;

            SnapRotationToDirection((currentFocusTarget.position - transform.position).normalized);
        }
        else if (inputDir.sqrMagnitude > 0.01f)
        {
            SnapRotationToDirection(inputDir);
        }
    }

    private void FaceCurrentFocusTarget()
    {
        if (currentFocusTarget != null)
        {
            
            Vector3 dirToTarget = (currentFocusTarget.position - transform.position).normalized;
            dirToTarget.y = 0; 

            if (dirToTarget.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dirToTarget);
                playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRot, Time.deltaTime * passiveRotationSpeed);
            }
        }
        else
        {
            
            Vector3 inputDir = GetCameraRelativeInput();
            if (inputDir.sqrMagnitude > 0.01f && !playerCombat.isAttacking)
            {
                Quaternion targetRot = Quaternion.LookRotation(inputDir);
                playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRot, Time.deltaTime * passiveRotationSpeed);
            }
        }
    }

    private void UpdateFocusToNearestEnemy()
    {
        Transform nearest = FindNearestEnemy();
        if (nearest != null)
        {
            currentFocusTarget = nearest;
        }
    }

    private Transform FindNearestEnemy()
    {
        Collider[] allEnemies = Physics.OverlapSphere(transform.position, targetingRadius, enemyLayer);
        if (allEnemies.Length == 0) return null;

        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider col in allEnemies)
        {
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = col.transform;
            }
        }
        return nearest;
    }

    private Vector3 GetCameraRelativeInput()
    {
        Vector2 moveInput = Vector2.zero;

        
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1; 
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1; 
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1; 
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1; 
        }

        Vector3 camForward = mainCamera.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = mainCamera.right;
        camRight.y = 0;
        camRight.Normalize();

        return (camForward * moveInput.y) + (camRight * moveInput.x);
    }

    private void SnapRotationToDirection(Vector3 direction)
    {
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
        {
            playerModel.rotation = Quaternion.LookRotation(direction);
        }
    }
}